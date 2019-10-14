// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)
using Fireasy.Common.Extensions;
using Fireasy.Data.Converter;
using Fireasy.Data.Entity.Linq.Expressions;
using Fireasy.Data.Entity.Linq.Translators;
using Fireasy.Data.Entity.Metadata;
using Fireasy.Data.Extensions;
using Fireasy.Data.Identity;
using Fireasy.Data.RecordWrapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Data.Entity.Linq
{
    /// <summary>
    /// 表达式执行计划的编译器。
    /// </summary>
    public class ExecutionBuilder : DbExpressionVisitor
    {
        private Scope scope;
        private bool isTop = true;
        private bool isAsync = false;
        private MemberInfo receivingMember;
        private int nReaders = 0;
        private int nLookup = 0;
        private ParameterExpression executor;
        private ParameterExpression cancelToken;
        private List<ParameterExpression> variables = new List<ParameterExpression>();
        private List<Expression> initializers = new List<Expression>();
        private Dictionary<string, Expression> variableMap = new Dictionary<string, Expression>();

        private Func<Expression, TranslateResult> translator;
        private static MethodInfo MthExecuteNoQuery = typeof(IDatabase).GetMethod(nameof(IDatabase.ExecuteNonQuery));
        private static MethodInfo MtlExecuteScalar = typeof(IDatabase).GetMethods().FirstOrDefault(s => s.Name == nameof(IDatabase.ExecuteScalar) && !s.IsGenericMethod);
        private static MethodInfo MthExecuteEnumerable = typeof(IDatabase).GetMethods().FirstOrDefault(s => s.Name == nameof(IDatabase.ExecuteEnumerable) && s.IsGenericMethod);
        private static MethodInfo MthExecuteNoQueryAsync = typeof(IDatabase).GetMethod(nameof(IDatabase.ExecuteNonQueryAsync));
        private static MethodInfo MtlExecuteScalarAsync = typeof(IDatabase).GetMethods().FirstOrDefault(s => s.Name == nameof(IDatabase.ExecuteScalarAsync) && s.IsGenericMethod);
        private static MethodInfo MthExecuteEnumerableAsync = typeof(IDatabase).GetMethods().FirstOrDefault(s => s.Name == nameof(IDatabase.ExecuteEnumerableAsync) && s.IsGenericMethod);
        private static MethodInfo MthUpdate = typeof(IDatabase).GetMethods().FirstOrDefault(s => s.Name == nameof(IDatabase.Update) && s.GetParameters().Length == 4);
        private static MethodInfo MthUpdateAsync = typeof(IDatabase).GetMethods().FirstOrDefault(s => s.Name == nameof(IDatabase.UpdateAsync) && s.GetParameters().Length == 5);
        private static MethodInfo MthConstruct = typeof(ExecutionBuilder).GetMethod(nameof(ExecutionBuilder.ConstructEntity), BindingFlags.Static | BindingFlags.NonPublic);
        private static MethodInfo MthNewPropertyValue = typeof(PropertyValue).GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(s => s.Name == nameof(PropertyValue.NewValue));
        private static MethodInfo MthIsDbNull = typeof(IRecordWrapper).GetMethod(nameof(IRecordWrapper.IsDbNull), new[] { typeof(IDataReader), typeof(int) });
        private static MethodInfo MthConvertFrom = typeof(IValueConverter).GetMethod(nameof(IValueConverter.ConvertFrom));
        private static MethodInfo MthConvertTo = typeof(IValueConverter).GetMethod(nameof(IValueConverter.ConvertTo));
        private static MethodInfo MthGetConverter = typeof(ConvertManager).GetMethod(nameof(ConvertManager.GetConverter));
        private static MethodInfo MthGenerateIdentityValue = typeof(ExecutionBuilder).GetMethod(nameof(ExecutionBuilder.GenerateIdentityValue), BindingFlags.Static | BindingFlags.NonPublic);
        private static MethodInfo MthGenerateGuidValue = typeof(ExecutionBuilder).GetMethod(nameof(ExecutionBuilder.GenerateGuidValue), BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// 构造最终执行的表达式。
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="translator">翻译器。</param>
        /// <returns></returns>
        public static Expression Build(Expression expression, Func<Expression, TranslateResult> translator, bool? isAsync = null)
        {
            var builder = new ExecutionBuilder() { executor = Expression.Parameter(typeof(IDatabase), "db"), translator = translator };

            if (isAsync != null)
            {
                builder.isAsync = (bool)isAsync;
            }

            var newExpression = builder.Bind(expression);

            if (builder.isAsync)
            {
                var cancelToken = builder.cancelToken ?? Expression.Parameter(typeof(CancellationToken), "token");
                return Expression.Lambda(newExpression, builder.executor, cancelToken);
            }

            return Expression.Lambda(newExpression, builder.executor);
        }

        private Expression Bind(Expression expression)
        {
            expression = this.Visit(expression);
            expression = this.AddVariables(expression);
            return expression;
        }

        private Expression AddVariables(Expression expression)
        {
            // add variable assignments up front
            if (variables.Count > 0)
            {
                var exprs = new List<Expression>();
                for (int i = 0, n = this.variables.Count; i < n; i++)
                {
                    exprs.Add(MakeAssign(this.variables[i], this.initializers[i]));
                }
                exprs.Add(expression);

                expression = MakeSequence(exprs);  // yields last expression value

                var nulls = variables.Select(v => Expression.Constant(null, v.Type)).ToArray();

                // use invoke/lambda to create variables via parameters in scope
                expression = Expression.Invoke(Expression.Lambda(expression, this.variables.ToArray()), nulls);
            }

            return expression;
        }

        private static Expression MakeSequence(IList<Expression> expressions)
        {
            Expression last = expressions[expressions.Count - 1];
            expressions = expressions.Select(e => e.Type.IsValueType ? Expression.Convert(e, typeof(object)) : e).ToList();
            return Expression.Convert(Expression.Call(typeof(ExecutionBuilder), nameof(ExecutionBuilder.Sequence), null, Expression.NewArrayInit(typeof(object), expressions)), last.Type);
        }

        //不能删除，使用反射调用
        public static object Sequence(params object[] values)
        {
            return values[values.Length - 1];
        }

        private static Expression MakeAssign(ParameterExpression variable, Expression value)
        {
            return Expression.Call(typeof(ExecutionBuilder), nameof(ExecutionBuilder.Assign), new Type[] { variable.Type }, variable, value);
        }

        //不能删除，使用反射调用
        public static T Assign<T>(ref T variable, T value)
        {
            variable = value;
            return value;
        }

        /// <summary>
        /// 浏览 <see cref="InsertCommandExpression"/> 表达式。
        /// </summary>
        /// <param name="insert"></param>
        /// <returns></returns>
        protected override Expression VisitInsert(InsertCommandExpression insert)
        {
            isAsync = isAsync || insert.IsAsync;

            //如果没有更新参数，则返回-1
            if (insert.Assignments.Count == 0)
            {
                return isAsync ? Expression.Constant(Task.FromResult(-1)) : Expression.Constant(-1);
            }

            var arguments = VisitColumnAssignments(insert.Assignments);
            insert = insert.Update(insert.Table, arguments);

            return insert.WithAutoIncrement ? BuildExecuteScalarCommand(insert) :
                BuildExecuteNoQueryCommand(insert);
        }

        /// <summary>
        /// 浏览 <see cref="DeleteCommandExpression"/> 表达式。
        /// </summary>
        /// <param name="delete"></param>
        /// <returns></returns>
        protected override Expression VisitDelete(DeleteCommandExpression delete)
        {
            isAsync = isAsync || delete.IsAsync;

            return BuildExecuteNoQueryCommand(delete);
        }

        /// <summary>
        /// 浏览 <see cref="UpdateCommandExpression"/> 表达式。
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        protected override Expression VisitUpdate(UpdateCommandExpression update)
        {
            isAsync = isAsync || update.IsAsync;

            //如果没有更新参数，则返回-1
            if (update.Assignments.Count == 0)
            {
                return isAsync ? Expression.Constant(Task.FromResult(-1)) : Expression.Constant(-1);
            }

            return BuildExecuteNoQueryCommand(update);
        }

        /// <summary>
        /// 浏览 <see cref="BatchCommandExpression"/> 表达式。
        /// </summary>
        /// <param name="batch"></param>
        /// <returns></returns>
        protected override Expression VisitBatch(BatchCommandExpression batch)
        {
            isAsync = isAsync || batch.IsAsync;

            var operation = batch.Operation;
            if (operation.Body is InsertCommandExpression insert)
            {
                if (batch.Arguments.Count == 0)
                {
                    return isAsync ? Expression.Constant(Task.FromResult(-1)) : Expression.Constant(-1);
                }

                var rewriter = insert.Update(insert.Table, VisitColumnAssignments(insert.Assignments));

                if (rewriter != insert)
                {
                    operation = Expression.Lambda(rewriter, operation.Parameters.ToArray());
                }
            }

            batch = batch.Update(batch.Input, operation, batch.Arguments);

            return BuildExecuteBatch(batch);
        }

        /// <summary>
        /// 浏览 <see cref="ProjectionExpression"/> 表达式。
        /// </summary>
        /// <param name="proj"></param>
        /// <returns></returns>
        protected override Expression VisitProjection(ProjectionExpression proj)
        {
            isAsync = isAsync || proj.IsAsync;

            if (isTop)
            {
                isTop = false;
                return ExecuteProjection(proj, scope != null);
            }
            else
            {
                return BuildInner(proj);
            }
        }

        protected override MemberBinding VisitBinding(MemberBinding binding)
        {
            var save = receivingMember;
            receivingMember = binding.Member;
            var result = base.VisitBinding(binding);
            receivingMember = save;
            return result;
        }

        protected override Expression VisitEntity(EntityExpression entity)
        {
            var mbmInitExp = entity.Expression as MemberInitExpression;

            var properties = new List<Expression>();
            var values = new List<Expression>();
            var bindings = new List<MemberBinding>();

            mbmInitExp.Bindings.ForEach(s =>
                {
                    if (s is MemberAssignment assign)
                    {
                        var expression = Visit(assign.Expression);
                        if (entity.IsNoTracking)
                        {
                            bindings.Add(Expression.Bind(assign.Member, expression));
                        }
                        else
                        {
                            var proprety = PropertyUnity.GetProperty(mbmInitExp.Type, assign.Member.Name);
                            properties.Add(Expression.Constant(proprety));

                            if (PropertyValue.IsSupportedType(proprety.Type))
                            {
                                if (expression.Type.GetNonNullableType().IsEnum)
                                {
                                    expression = Expression.Convert(expression, typeof(Enum));
                                }

                                var pValue = Expression.Convert(expression, typeof(PropertyValue));
                                values.Add(pValue);
                            }
                            else
                            {
                                var pValue = Expression.Call(null, MthNewPropertyValue, expression, Expression.Constant(null, typeof(Type)));
                                values.Add(pValue);
                            }
                        }
                    }
                });

            if (entity.IsNoTracking)
            {
                return Expression.MemberInit(mbmInitExp.NewExpression, bindings);
            }

            var e = TranslateScope.Current.ContextService as IEntityPersistentEnvironment;
            var c = TranslateScope.Current.ContextService as IEntityPersistentInstanceContainer;

            var constCall = Expression.Call(null, MthConstruct,
                Visit(mbmInitExp.NewExpression),
                Expression.NewArrayInit(typeof(IProperty), properties.ToArray()),
                Expression.NewArrayInit(typeof(PropertyValue), values.ToArray()),
                Expression.Constant(c, typeof(IEntityPersistentInstanceContainer)),
                Expression.Constant(e, typeof(IEntityPersistentEnvironment)));

            return Expression.Convert(constCall, entity.Type);
        }

        protected override Expression VisitNew(NewExpression node)
        {
            var elementType = node.Type;
            if (typeof(IEntity).IsAssignableFrom(elementType))
            {
                if (elementType.IsNotCompiled())
                {
                    elementType = EntityProxyManager.GetType(elementType);
                    return Expression.New(elementType);
                }
            }

            return base.VisitNew(node);
        }

        protected override Expression VisitConstant(ConstantExpression constExp)
        {
            if (constExp.Type == typeof(PropertyValue))
            {
                var pv = (PropertyValue)constExp.Value;
                if (!PropertyValue.IsEmpty(pv))
                {
                    var value = pv.GetValue();
                    return Visit(Expression.Constant(value, value.GetType()));
                }
                else
                {
                    return Visit(Expression.Constant(null));
                }
            }

            return base.VisitConstant(constExp);
        }

        protected override Expression VisitOuterJoined(OuterJoinedExpression outer)
        {
            var expr = this.Visit(outer.Expression);
            var column = (ColumnExpression)outer.Test;

            if (this.scope.TryGetValue(column, out ParameterExpression recordWrapper, out ParameterExpression dataReader, out int ordinal))
            {
                return Expression.Condition(
                    Expression.Call(recordWrapper, MthIsDbNull, dataReader, Expression.Constant(ordinal)),
                    Expression.Constant(outer.Type.GetDefaultValue(), outer.Type),
                    expr
                    );
            }

            return expr;
        }

        protected override Expression VisitColumn(ColumnExpression column)
        {
            if (scope != null && scope.TryGetValue(column, out ParameterExpression recordWrapper, out ParameterExpression dataReader, out int ordinal))
            {
                if (!column.Type.IsDbTypeSupported())
                {
                    throw new InvalidCastException(SR.GetString(SRKind.InvalidCastPropertyValue, column.Type.FullName));
                }

                //把 ColumnExpression 换成 RecordWrapper.GetInt32(IDataReader, int) 这样的表达式
                var dbType = column.MapInfo != null && column.MapInfo.DataType != null ?
                    (DbType)column.MapInfo.DataType : column.Type.GetDbType();

                var method = RecordWrapHelper.GetMethodByOrdinal(dbType);
                Expression expression = Expression.Call(recordWrapper, method, dataReader, Expression.Constant(ordinal));

                //先找转换器
                var converter = ConvertManager.GetConverter(column.Type);
                if (converter != null)
                {
                    //调用ConvertManager.GetConverter
                    var mconverter = Expression.Call(null, MthGetConverter, Expression.Constant(column.Type));

                    //调用 IValueConverter.ConvertFrom
                    expression = (Expression)Expression.Convert(
                        Expression.Call(mconverter, MthConvertFrom, expression, Expression.Constant(dbType)),
                        column.Type);
                }
                else
                {
                    if (column.Type.IsNullableType())
                    {
                        //调用 RecordWrapper.IsDbNull 判断值是否为空
                        expression = (Expression)Expression.Condition(
                            Expression.Call(recordWrapper, MthIsDbNull, dataReader, Expression.Constant(ordinal)),
                            Expression.Convert(Expression.Constant(null), column.Type),
                            Expression.Convert(expression, column.Type));
                    }
                    else if (column.Type != method.ReturnType)
                    {
                        expression = (Expression)Expression.Convert(expression, column.Type);
                    }
                }

                return expression;
            }

            return column;
        }

        protected override Expression VisitGenerator(GeneratorExpression generator)
        {
            var property = generator.Property;
            if (property.Info.DataType != null &&
                property.Info.DataType.Value.IsStringDbType())
            {
                return Expression.Call(null, MthGenerateGuidValue, generator.Entity, Expression.Constant(generator.Property));
            }

            return Expression.Call(null, MthGenerateIdentityValue, executor, generator.Entity, Expression.Constant(generator.Property));
        }

        protected override Expression VisitNamedValue(NamedValueExpression value)
        {
            return base.VisitNamedValue(value);
        }

        protected virtual Expression Parameterize(Expression expression)
        {
            if (this.variableMap.Count > 0)
            {
                expression = VariableSubstitutor.Substitute(this.variableMap, expression);
            }

            return Parameterizer.Parameterize(expression);
        }

        protected override Expression VisitClientJoin(ClientJoinExpression join)
        {
            // convert client join into a up-front lookup table builder & replace client-join in tree with lookup accessor

            // 1) lookup = query.Select(e => new KVP(key: inner, value: e)).ToLookup(kvp => kvp.Key, kvp => kvp.Value)
            Expression innerKey = MakeJoinKey(join.InnerKey);
            Expression outerKey = MakeJoinKey(join.OuterKey);

            ConstructorInfo kvpConstructor = typeof(KeyValuePair<,>).MakeGenericType(innerKey.Type, join.Projection.Projector.Type).GetConstructor(new Type[] { innerKey.Type, join.Projection.Projector.Type });
            Expression constructKVPair = Expression.New(kvpConstructor, innerKey, join.Projection.Projector);
            ProjectionExpression newProjection = new ProjectionExpression(join.Projection.Select, constructKVPair, isAsync);

            int iLookup = ++nLookup;
            Expression execution = ExecuteProjection(newProjection, false);

            ParameterExpression kvp = Expression.Parameter(constructKVPair.Type, "kvp");

            // filter out nulls
            if (join.Projection.Projector.NodeType == (ExpressionType)DbExpressionType.OuterJoined)
            {
                var nullType = join.Projection.Projector.Type.GetNullableType();
                LambdaExpression pred = Expression.Lambda(
                    Expression.PropertyOrField(kvp, "Value").NotEqual(Expression.Constant(null, nullType)),
                    kvp
                    );
                execution = Expression.Call(typeof(Enumerable), nameof(Enumerable.Where), new Type[] { kvp.Type }, execution, pred);
            }

            // make lookup
            LambdaExpression keySelector = Expression.Lambda(Expression.PropertyOrField(kvp, "Key"), kvp);
            LambdaExpression elementSelector = Expression.Lambda(Expression.PropertyOrField(kvp, "Value"), kvp);
            Expression toLookup = Expression.Call(typeof(Enumerable), nameof(Enumerable.ToLookup), new Type[] { kvp.Type, outerKey.Type, join.Projection.Projector.Type }, execution, keySelector, elementSelector);

            // 2) agg(lookup[outer])
            ParameterExpression lookup = Expression.Parameter(toLookup.Type, $"lookup{iLookup}");
            PropertyInfo property = lookup.Type.GetProperty("Item");
            Expression access = Expression.Call(lookup, property.GetGetMethod(), this.Visit(outerKey));
            if (join.Projection.Aggregator != null)
            {
                // apply aggregator
                access = DbExpressionReplacer.Replace(join.Projection.Aggregator.Body, join.Projection.Aggregator.Parameters[0], access);
            }

            this.variables.Add(lookup);
            this.initializers.Add(toLookup);

            return access;
        }

        private Expression BuildInner(Expression expression)
        {
            var eb = new ExecutionBuilder();
            eb.executor = executor;
            eb.scope = this.scope;
            eb.receivingMember = receivingMember;
            eb.nReaders = nReaders;
            eb.nLookup = nLookup;
            eb.translator = translator;
            eb.variableMap = variableMap;
            return eb.Visit(expression);
        }

        private Expression ExecuteProjection(ProjectionExpression projection, bool okayToDefer)
        {
            projection = (ProjectionExpression)Parameterize(projection);

            if (scope != null)
            {
                projection = (ProjectionExpression)OuterParameterizer.Parameterize(this.scope.Alias, projection);
            }

            return BuildExecuteEnumerableCommand(projection, okayToDefer);
        }

        private Expression BuildExecuteEnumerableCommand(ProjectionExpression projection, bool okayToDefer)
        {
            var elementType = projection.IsSingleton ? projection.Type : projection.Type.GetEnumerableElementType();

            var saveScope = scope;
            var recordWrapper = Expression.Parameter(typeof(IRecordWrapper), $"rw{nReaders}");
            var dataReader = Expression.Parameter(typeof(IDataReader), $"dr{nReaders}");
            nReaders++;

            scope = new Scope(this.scope, recordWrapper, dataReader, projection.Select.Alias, projection.Select.Columns);

            var projector = this.Visit(projection.Projector);
            var rowMapper = CreateDataRowWrapper(projection, projector, elementType);
            var rowMapperType = typeof(IDataRowMapper<>).MakeGenericType(elementType);
            scope = saveScope;

            var result = translator(projection.Select);

            Expression plan;
            if (isAsync)
            {
                var method = MthExecuteEnumerableAsync.MakeGenericMethod(elementType);
                cancelToken = Expression.Parameter(typeof(CancellationToken), "token");
                plan = Expression.Call(executor, method,
                    Expression.Constant((SqlCommand)result.QueryText),
                    Expression.Constant(result.DataSegment, typeof(IDataSegment)),
                    CreateParameterCollectionExpression(projection.Select),
                    Expression.Constant(rowMapper, rowMapperType),
                    cancelToken
                    );
            }
            else
            {
                var method = MthExecuteEnumerable.MakeGenericMethod(elementType);
                plan = Expression.Call(executor, method,
                    Expression.Constant((SqlCommand)result.QueryText),
                    Expression.Constant(result.DataSegment, typeof(IDataSegment)),
                    CreateParameterCollectionExpression(projection.Select),
                    Expression.Constant(rowMapper, rowMapperType)
                    );
            }

            if (projection.Aggregator != null && (projection.Aggregator.Body as MethodCallExpression).Method.Name != nameof(Queryable.AsQueryable))
            {
                plan = DbExpressionReplacer.Replace(projection.Aggregator.Body, projection.Aggregator.Parameters[0], plan);
            }

            return plan;
        }

        /// <summary>
        /// 构造调用 ExecuteNoQuery 方法的表达式。
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private Expression BuildExecuteNoQueryCommand(CommandExpression command)
        {
            var expression = Parameterize(command);
            var result = translator(expression);

            if (isAsync)
            {
                cancelToken = Expression.Parameter(typeof(CancellationToken), "token");
                return Expression.Call(executor, MthExecuteNoQueryAsync,
                        Expression.Constant((SqlCommand)result.QueryText),
                        CreateParameterCollectionExpression(expression),
                        cancelToken
                    );
            }
            else
            {
                return Expression.Call(executor, MthExecuteNoQuery,
                        Expression.Constant((SqlCommand)result.QueryText),
                        CreateParameterCollectionExpression(expression)
                    );
            }
        }

        /// <summary>
        /// 构造调用 ExecuteScalar 方法的表达式。
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private Expression BuildExecuteScalarCommand(CommandExpression command)
        {
            var expression = Parameterize(command);
            var result = translator(expression);


            if (isAsync)
            {
                cancelToken = Expression.Parameter(typeof(CancellationToken), "token");
                return Expression.Call(executor, MtlExecuteScalarAsync.MakeGenericMethod(command.Type),
                        Expression.Constant((SqlCommand)result.QueryText),
                        CreateParameterCollectionExpression(expression),
                        cancelToken
                    );
            }
            else
            {
                return Expression.Call(executor, MtlExecuteScalar.MakeGenericMethod(command.Type),
                        Expression.Constant((SqlCommand)result.QueryText),
                        CreateParameterCollectionExpression(expression)
                    );
            }
        }

        private Expression BuildExecuteBatch(BatchCommandExpression batch)
        {
            var operation = Parameterize(batch.Operation.Body);

            var result = translator(operation);
            var namedValues = NamedValueGatherer.Gather(operation);

            var table = new DataTable();

            foreach (var nv in namedValues)
            {
                var info = GetPropertyInfoFromExpression(nv.Value);
                if (info != null)
                {
                    table.Columns.Add(nv.Name, info.DataType.Value.FromDbType());
                }
                else
                {
                    table.Columns.Add(nv.Name, DataExpressionColumn.CreateType(nv.Value.Type));
                }
            }

            var parameters = namedValues.ToDictionary(s => s.Name, s =>
                {
                    var expression = s.Value;
                    var info = GetPropertyInfoFromExpression(expression);
                    if (info == null)
                    {
                        return expression;
                    }

                    if (ConvertManager.GetConverter(expression.Type) != null)
                    {
                        var convExp = Expression.Call(null, MthGetConverter, Expression.Constant(expression.Type));
                        expression = Expression.Call(convExp, MthConvertTo, expression, Expression.Constant((DbType)info.DataType));
                    }

                    return (object)Expression.Lambda(expression, batch.Operation.Parameters[1]).Compile();
                });

            var entities = (IEnumerable)((ConstantExpression)batch.Input).Value;
            foreach (IEntity entity in entities)
            {
                var row = table.NewRow();
                foreach (var nv in parameters)
                {
                    if (nv.Value is Delegate del)
                    {
                        row[nv.Key] = del.DynamicInvoke(entity) ?? DBNull.Value;
                    }
                    else if (nv.Value is Expression exp)
                    {
                        exp = ParameterRewriter.Rewrite(exp, batch.Operation.Parameters[1], entity);
                        var setter = Expression.Lambda(exp, executor).Compile();
                        row[nv.Key] = DataExpressionColumn.Create(exp.Type, setter);
                    }
                }

                table.Rows.Add(row);
            }

            Expression plan;
            if (isAsync)
            {
                cancelToken = Expression.Parameter(typeof(CancellationToken), "token");
                plan = Expression.Call(executor, MthUpdateAsync,
                    Expression.Constant(table),
                    Expression.Constant((SqlCommand)result.QueryText),
                    Expression.Constant(null, typeof(SqlCommand)),
                    Expression.Constant(null, typeof(SqlCommand)),
                    cancelToken
                    );
            }
            else
            {
                plan = Expression.Call(executor, MthUpdate,
                    Expression.Constant(table),
                    Expression.Constant((SqlCommand)result.QueryText),
                    Expression.Constant(null, typeof(SqlCommand)),
                    Expression.Constant(null, typeof(SqlCommand))
                    );
            }

            if (operation.NodeType != (ExpressionType)DbExpressionType.Insert)
            {
                return plan;
            }

            return Expression.Call(typeof(ExecutionBuilder),
                isAsync ? nameof(ExecutionBuilder.UpdateEntitiesAsync) : nameof(ExecutionBuilder.UpdateEntities), null,
                plan,
                Expression.Constant(table, typeof(DataTable)),
                Expression.Constant(entities, typeof(IEnumerable)));
        }

        private PropertyMapInfo GetPropertyInfoFromExpression(Expression expression)
        {
            if (expression.NodeType == ExpressionType.MemberAccess)
            {
                var member = (MemberExpression)expression;
                var property = PropertyUnity.GetProperty(member.Member.DeclaringType, member.Member.Name);
                return property?.Info;
            }
            else if ((DbExpressionType)expression.NodeType == DbExpressionType.Column)
            {
                var column = (ColumnExpression)expression;
                return column.MapInfo;
            }

            return null;
        }

        /// <summary>
        /// 收集嵌套的参数并生成 <see cref="ParameterCollection"/> 的表达式。
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private Expression CreateParameterCollectionExpression(Expression expression)
        {
            var namedValues = NamedValueGatherer.Gather(expression);
            var values = namedValues.Select(v => Expression.Convert(this.Visit(v.Value), typeof(object))).ToArray();

            if (values.Length > 0)
            {
                var arguments = new List<Expression>();
                var constructor = typeof(Parameter).GetConstructor(new[] { typeof(string), typeof(object) });

                for (var i = 0; i < namedValues.Count; i++)
                {
                    Expression pv = values[i] is Expression ?
                        (Expression)values[i] :
                        Expression.Constant(values[i], typeof(object));

                    var pExp = Expression.New(constructor, Expression.Constant(namedValues[i].Name), pv);
                    arguments.Add(pExp);
                }

                var listExp = Expression.ListInit(Expression.New(typeof(List<Parameter>)), arguments);
                return Expression.New(typeof(ParameterCollection).GetConstructors()[1], listExp);
            }
            else
            {
                return Expression.Constant(null, typeof(ParameterCollection));
            }
        }

        /// <summary>
        /// 创建 <see cref="IDataRowMapper"/>。
        /// </summary>
        /// <param name="projection"></param>
        /// <param name="projector"></param>
        /// <param name="elementType"></param>
        /// <returns></returns>
        private IDataRowMapper CreateDataRowWrapper(ProjectionExpression projection, Expression projector, Type elementType)
        {
            var funcType = typeof(Func<,>).MakeGenericType(typeof(IDataReader), elementType);
            var lambda = Expression.Lambda(funcType, projector, scope.dataReader);
            return typeof(ProjectionRowMapper<>).MakeGenericType(elementType).New<IDataRowMapper>(executor, lambda, scope.recordWrapper);
        }

        /// <summary>
        /// 构造实体对象。
        /// </summary>
        /// <param name="entity">实体对象。</param>
        /// <param name="properties">属性数组。</param>
        /// <param name="values">值数组。</param>
        /// <param name="instanceContainer"><see cref="IEntityPersistentInstanceContainer"/> 对象。</param>
        /// <param name="environment"><see cref="IEntityPersistentEnvironment"/> 对象。</param>
        /// <returns></returns>
        private static IEntity ConstructEntity(IEntity entity, IProperty[] properties, PropertyValue[] values,
            IEntityPersistentInstanceContainer instanceContainer, IEntityPersistentEnvironment environment)
        {
            entity.As<ISupportInitializeNotification>(s => s.BeginInit());

            //循环所有属性进行赋值
            for (var i = 0; i < properties.Length; i++)
            {
                entity.InitializeValue(properties[i], values[i]);
            }

            //设置状态
            entity.SetState(EntityState.Unchanged);

            //设置 InstanceName 和 Environment
            if (!string.IsNullOrEmpty(instanceContainer.InstanceName) && entity is IEntityPersistentInstanceContainer con)
            {
                con.InstanceName = instanceContainer.InstanceName;
            }

            if (environment.Environment != null && entity is IEntityPersistentEnvironment env)
            {
                env.Environment = environment.Environment;
            }

            entity.As<ISupportInitializeNotification>(s => s.EndInit());

            return entity;
        }

        /// <summary>
        /// 使用 Guid 作为主键。
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        private static string GenerateGuidValue(IEntity entity, IProperty property)
        {
            var value = entity.GetValue(property);
            if (PropertyValue.IsEmpty(value))
            {
                value = Guid.NewGuid().ToString();
                entity.SetValue(property, value);
            }

            return (string)value;
        }

        /// <summary>
        /// 调用 <see cref="IGeneratorProvider"/> 对象生成标识值。在 VisitGenerator 中使用该方法。
        /// </summary>
        /// <param name="database"></param>
        /// <param name="entity"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        private static int GenerateIdentityValue(IDatabase database, IEntity entity, IProperty property)
        {
            var generator = database.Provider.GetService<IGeneratorProvider>();
            if (generator == null)
            {
                return 0;
            }

            var metadata = EntityMetadataUnity.GetEntityMetadata(entity.EntityType);
            var tableName = metadata.TableName;
            var columnName = property.Info.FieldName;

            if (entity is IEntityPersistentEnvironment env && env.Environment != null)
            {
                tableName = env.Environment.GetVariableTableName(metadata);
            }

            var value = generator.GenerateValue(database, tableName, columnName);
            entity.SetValue(property, PropertyValue.NewValue(value, property.Type));

            return value;
        }

        private Expression MakeJoinKey(IList<Expression> key)
        {
            if (key.Count == 1)
            {
                return key[0];
            }
            else
            {
                return Expression.NewArrayInit(typeof(object), key.Select(k => (Expression)Expression.Convert(k, typeof(object))));
            }
        }

        /// <summary>
        /// 执行Batch-Insert后更新主键到实体集。
        /// </summary>
        /// <param name="rows">Update影响的行数。</param>
        /// <param name="table"></param>
        /// <param name="entities"></param>
        /// <returns></returns>
        private static int UpdateEntities(int rows, DataTable table, IEnumerable entities)
        {
            var index = 0;
            IProperty pkProperty = null;
            foreach (IEntity entity in entities)
            {
                if (pkProperty == null)
                {
                    pkProperty = PropertyUnity.GetPrimaryProperties(entity.EntityType)
                        .FirstOrDefault(s => s.Info.GenerateType == IdentityGenerateType.AutoIncrement);
                }

                if (pkProperty == null)
                {
                    return rows;
                }

                var row = table.Rows[index++];
                var pkValue = PropertyValue.NewValue(row.ItemArray[table.Columns.Count - 1], pkProperty.Type);
                entity.InitializeValue(pkProperty, pkValue);
            }

            return rows;
        }

        /// <summary>
        /// 执行Batch-Insert后更新主键到实体集。
        /// </summary>
        /// <param name="rows">Update影响的行数。</param>
        /// <param name="table"></param>
        /// <param name="entities"></param>
        /// <returns></returns>
        private static async Task<int> UpdateEntitiesAsync(Task<int> rowsTask, DataTable table, IEnumerable entities)
        {
            var rows = await rowsTask;
            var index = 0;
            IProperty pkProperty = null;
            foreach (IEntity entity in entities)
            {
                if (pkProperty == null)
                {
                    pkProperty = PropertyUnity.GetPrimaryProperties(entity.EntityType)
                        .FirstOrDefault(s => s.Info.GenerateType == IdentityGenerateType.AutoIncrement);
                }

                if (pkProperty == null)
                {
                    return rows;
                }

                var row = table.Rows[index++];
                var pkValue = PropertyValue.NewValue(row.ItemArray[table.Columns.Count - 1], pkProperty.Type);
                entity.InitializeValue(pkProperty, pkValue);
            }

            return rows;
        }

        private class Scope
        {
            internal Scope outer;
            internal ParameterExpression recordWrapper;
            internal ParameterExpression dataReader;
            internal TableAlias Alias { get; private set; }
            private Dictionary<string, int> nameMap;

            internal Scope(Scope outer, ParameterExpression recordWrapper, ParameterExpression dataReader, TableAlias alias, IEnumerable<ColumnDeclaration> columns)
            {
                this.outer = outer;
                this.recordWrapper = recordWrapper;
                this.dataReader = dataReader;
                this.Alias = alias;
                this.nameMap = columns.Select((c, i) => new { c, i }).ToDictionary(x => x.c.Name, x => x.i);
            }

            internal bool TryGetValue(ColumnExpression column, out ParameterExpression recordWrapper, out ParameterExpression dataReader, out int ordinal)
            {
                for (Scope s = this; s != null; s = s.outer)
                {
                    if (column.Alias == s.Alias && this.nameMap.TryGetValue(column.Name, out ordinal))
                    {
                        recordWrapper = this.recordWrapper;
                        dataReader = this.dataReader;
                        return true;
                    }
                }

                recordWrapper = null;
                dataReader = null;
                ordinal = 0;
                return false;
            }
        }

        /// <summary>
        /// columns referencing the outer alias are turned into special named-value parameters
        /// </summary>
        private class OuterParameterizer : DbExpressionVisitor
        {
            private int iParam;
            private TableAlias outerAlias;
            private Dictionary<ColumnExpression, NamedValueExpression> map = new Dictionary<ColumnExpression, NamedValueExpression>();

            internal static Expression Parameterize(TableAlias outerAlias, Expression expr)
            {
                var op = new OuterParameterizer();
                op.outerAlias = outerAlias;
                return op.Visit(expr);
            }

            protected override Expression VisitProjection(ProjectionExpression proj)
            {
                var select = (SelectExpression)this.Visit(proj.Select);
                return proj.Update(select, proj.Projector, proj.Aggregator);
            }

            protected override Expression VisitColumn(ColumnExpression column)
            {
                if (column.Alias == this.outerAlias)
                {
                    if (!this.map.TryGetValue(column, out NamedValueExpression nv))
                    {
                        nv = QueryUtility.GetNamedValueExpression($"n{(iParam++)}", column, (DbType)column.MapInfo.DataType);
                        this.map.Add(column, nv);
                    }

                    return nv;
                }

                return column;
            }
        }
        class VariableSubstitutor : DbExpressionVisitor
        {
            Dictionary<string, Expression> map;

            private VariableSubstitutor(Dictionary<string, Expression> map)
            {
                this.map = map;
            }

            public static Expression Substitute(Dictionary<string, Expression> map, Expression expression)
            {
                return new VariableSubstitutor(map).Visit(expression);
            }

            protected override Expression VisitVariable(VariableExpression vex)
            {
                if (this.map.TryGetValue(vex.Name, out Expression sub))
                {
                    return sub;
                }
                return vex;
            }
        }

        private class ProjectionRowMapper<T> : ExpressionRowMapper<T>
        {
            private ParameterExpression executor;
            private LambdaExpression lambda;
            private ParameterExpression recordWrapper;

            public ProjectionRowMapper(ParameterExpression executor, LambdaExpression lambda, ParameterExpression recordWrapper)
            {
                this.executor = executor;
                this.lambda = lambda;
                this.recordWrapper = recordWrapper;
            }

            protected override Expression<Func<IDatabase, IDataReader, T>> BuildExpressionForDataReader()
            {
                lambda = (LambdaExpression)DbExpressionReplacer.Replace(lambda, recordWrapper, Expression.Constant(RecordWrapper, typeof(IRecordWrapper)));
                if (lambda.Parameters.Count == 1 && lambda.Parameters[0].Type != typeof(IDatabase))
                {
                    return Expression.Lambda<Func<IDatabase, IDataReader, T>>(lambda.Body, executor, lambda.Parameters[0]);
                }

                return (Expression<Func<IDatabase, IDataReader, T>>)lambda;
            }

            protected override Expression<Func<IDatabase, DataRow, T>> BuildExpressionForDataRow()
            {
                lambda = (LambdaExpression)DbExpressionReplacer.Replace(lambda, recordWrapper, Expression.Constant(RecordWrapper, typeof(IRecordWrapper)));
                if (lambda.Parameters.Count == 1 && lambda.Parameters[0].Type != typeof(IDatabase))
                {
                    return Expression.Lambda<Func<IDatabase, DataRow, T>>(lambda.Body, executor, lambda.Parameters[0]);
                }

                return (Expression<Func<IDatabase, DataRow, T>>)lambda;
            }
        }
    }
}
