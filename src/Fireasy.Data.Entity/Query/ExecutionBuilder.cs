// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)
using Fireasy.Common.Extensions;
using Fireasy.Common.Reflection;
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

namespace Fireasy.Data.Entity.Query
{
    /// <summary>
    /// 表达式执行计划的编译器。
    /// </summary>
    public class ExecutionBuilder : DbExpressionVisitor
    {
        private Scope scope;
        private bool isTop = true;
        private bool isAsync = false;
        private bool isNoTracking = false;
        private MemberInfo receivingMember;
        private int nReaders = 0;
        private int nLookup = 0;
        private ParameterExpression executor;
        private ParameterExpression cancelToken;
        private readonly List<ParameterExpression> variables = new List<ParameterExpression>();
        private readonly List<Expression> initializers = new List<Expression>();
        private Dictionary<string, Expression> variableMap = new Dictionary<string, Expression>();

        private class MethodCache
        {
            internal static readonly MethodInfo DbExecuteNoQuery = typeof(IDatabase).GetMethod(nameof(IDatabase.ExecuteNonQuery));
            internal static readonly MethodInfo DbExecuteScalar = typeof(IDatabase).GetMethods().FirstOrDefault(s => s.Name == nameof(IDatabase.ExecuteScalar) && s.IsGenericMethod);
            internal static readonly MethodInfo DbExecuteEnumerable = typeof(IDatabase).GetMethods().FirstOrDefault(s => s.Name == nameof(IDatabase.ExecuteEnumerable) && s.IsGenericMethod);
            internal static readonly MethodInfo DbExecuteNoQueryAsync = typeof(IDatabase).GetMethod(nameof(IDatabase.ExecuteNonQueryAsync));
            internal static readonly MethodInfo DbExecuteScalarAsync = typeof(IDatabase).GetMethods().FirstOrDefault(s => s.Name == nameof(IDatabase.ExecuteScalarAsync) && s.IsGenericMethod);
            internal static readonly MethodInfo DbExecuteEnumerableAsync = typeof(IDatabase).GetMethods().FirstOrDefault(s => s.Name == nameof(IDatabase.ExecuteEnumerableAsync) && s.IsGenericMethod);
            internal static readonly MethodInfo DbUpdate = typeof(IDatabase).GetMethods().FirstOrDefault(s => s.Name == nameof(IDatabase.Update) && s.GetParameters().Length == 4);
            internal static readonly MethodInfo DbUpdateAsync = typeof(IDatabase).GetMethods().FirstOrDefault(s => s.Name == nameof(IDatabase.UpdateAsync) && s.GetParameters().Length == 5);
            internal static readonly MethodInfo ConstructEntity = typeof(ExecutionBuilder).GetMethod(nameof(ExecutionBuilder.ConstructEntity), BindingFlags.Static | BindingFlags.NonPublic);
            internal static readonly MethodInfo NewPropertyValue = typeof(PropertyValue).GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(s => s.Name == nameof(PropertyValue.NewValue));
            internal static readonly MethodInfo IsDbNull = typeof(IRecordWrapper).GetMethod(nameof(IRecordWrapper.IsDbNull), new[] { typeof(IDataReader), typeof(int) });
            internal static readonly MethodInfo ConvertFrom = typeof(IValueConverter).GetMethod(nameof(IValueConverter.ConvertFrom));
            internal static readonly MethodInfo ConvertTo = typeof(IValueConverter).GetMethod(nameof(IValueConverter.ConvertTo));
            internal static readonly MethodInfo GetConverter = typeof(ConvertManager).GetMethod(nameof(ConvertManager.GetConverter));
            internal static readonly MethodInfo GenerateIdentityValue = typeof(ExecutionBuilder).GetMethod(nameof(ExecutionBuilder.GenerateIdentityValue), BindingFlags.Static | BindingFlags.NonPublic);
            internal static readonly MethodInfo GenerateGuidValue = typeof(ExecutionBuilder).GetMethod(nameof(ExecutionBuilder.GenerateGuidValue), BindingFlags.Static | BindingFlags.NonPublic);
            internal static readonly Expression ExpNullParameter = Expression.Constant(null, typeof(ParameterCollection));
        }

        private TranslatorBase translator;

        /// <summary>
        /// 构造最终执行的表达式。
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="translator">翻译器。</param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static Expression Build(Expression expression, BuildOptions options)
        {
            var scope = TranslateScope.Current;
            var translator = scope.TranslateProvider.CreateTranslator();

            var builder = new ExecutionBuilder()
            {
                executor = Expression.Parameter(typeof(IDatabase), "db"),
                translator = translator
            };

            if (options.IsAsync != null)
            {
                builder.isAsync = (bool)options.IsAsync;
            }

            if (options.IsNoTracking == true)
            {
                builder.isNoTracking = true;
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
            expression = Visit(expression);
            expression = AddVariables(expression);
            return expression;
        }

        private Expression AddVariables(Expression expression)
        {
            // add variable assignments up front
            if (variables.Count > 0)
            {
                var exprs = new List<Expression>();
                for (int i = 0, n = variables.Count; i < n; i++)
                {
                    exprs.Add(MakeAssign(variables[i], initializers[i]));
                }

                exprs.Add(expression);

                expression = MakeSequence(exprs);  // yields last expression value

                var nulls = variables.Select(v => Expression.Constant(null, v.Type)).ToArray();

                // use invoke/lambda to create variables via parameters in scope
                expression = Expression.Invoke(Expression.Lambda(expression, variables.ToArray()), nulls);
            }

            return expression;
        }

        private static Expression MakeSequence(IList<Expression> expressions)
        {
            var last = expressions[expressions.Count - 1];
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
        /// 访问 <see cref="InsertCommandExpression"/> 表达式。
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
        /// 访问 <see cref="DeleteCommandExpression"/> 表达式。
        /// </summary>
        /// <param name="delete"></param>
        /// <returns></returns>
        protected override Expression VisitDelete(DeleteCommandExpression delete)
        {
            isAsync = isAsync || delete.IsAsync;

            return BuildExecuteNoQueryCommand(delete);
        }

        /// <summary>
        /// 访问 <see cref="UpdateCommandExpression"/> 表达式。
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
        /// 访问 <see cref="BatchCommandExpression"/> 表达式。
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
        /// 访问 <see cref="ProjectionExpression"/> 表达式。
        /// </summary>
        /// <param name="proj"></param>
        /// <returns></returns>
        protected override Expression VisitProjection(ProjectionExpression proj)
        {
            if (isTop)
            {
                isTop = false;
                isAsync = isAsync || proj.IsAsync;
                isNoTracking = isNoTracking || proj.IsNoTracking;

                return ExecuteProjection(proj, scope != null);
            }
            else
            {
                return BuildInner(proj);
            }
        }

        protected override Expression VisitMemberInit(MemberInitExpression mbmInitExp)
        {
            if (typeof(IEntity).IsAssignableFrom(mbmInitExp.Type))
            {
                return ConvertEntityExpression(mbmInitExp);
            }

            return base.VisitMemberInit(mbmInitExp);
        }

        protected override Expression VisitEntity(EntityExpression entity)
        {
            var mbrInitExp = entity.Expression as MemberInitExpression;

            return ConvertEntityExpression(mbrInitExp);
        }

        protected override Expression VisitNew(NewExpression node)
        {
            var elementType = node.Type;
            if (typeof(IEntity).IsAssignableFrom(elementType))
            {
                if (elementType.IsNotCompiled())
                {
                    var provider = TranslateScope.Current?.Provider;
                    elementType = EntityProxyManager.GetType(provider, elementType);
                    return Expression.New(elementType);
                }
            }

            return base.VisitNew(node);
        }

        protected override MemberBinding VisitBinding(MemberBinding binding)
        {
            var save = receivingMember;
            receivingMember = binding.Member;
            var result = base.VisitBinding(binding);
            receivingMember = save;
            return result;
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
            var expr = Visit(outer.Expression);
            var column = (ColumnExpression)outer.Test;

            if (scope.TryGetValue(column, out ParameterExpression recordWrapper, out ParameterExpression dataReader, out int ordinal))
            {
                return Expression.Condition(
                    Expression.Call(recordWrapper, MethodCache.IsDbNull, dataReader, Expression.Constant(ordinal)),
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
                    var mconverter = Expression.Call(null, MethodCache.GetConverter, Expression.Constant(column.Type));

                    //调用 IValueConverter.ConvertFrom
                    expression = Expression.Convert(
                        Expression.Call(mconverter, MethodCache.ConvertFrom, expression, Expression.Constant(dbType)),
                        column.Type);
                }
                else
                {
                    if (column.Type.IsNullableType())
                    {
                        //调用 RecordWrapper.IsDbNull 判断值是否为空
                        expression = Expression.Condition(
                            Expression.Call(recordWrapper, MethodCache.IsDbNull, dataReader, Expression.Constant(ordinal)),
                            Expression.Convert(Expression.Constant(null), column.Type),
                            Expression.Convert(expression, column.Type));
                    }
                    else if (column.Type != method.ReturnType)
                    {
                        expression = Expression.Convert(expression, column.Type);
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
                return Expression.Call(null, MethodCache.GenerateGuidValue, generator.Entity, Expression.Constant(generator.Property));
            }

            return Expression.Call(null, MethodCache.GenerateIdentityValue, executor, generator.Entity, Expression.Constant(generator.Property));
        }

        protected override Expression VisitNamedValue(NamedValueExpression value)
        {
            return base.VisitNamedValue(value);
        }

        protected virtual Expression Parameterize(Expression expression)
        {
            if (variableMap.Count > 0)
            {
                expression = VariableSubstitutor.Substitute(variableMap, expression);
            }

            return Parameterizer.Parameterize(expression);
        }

        protected override Expression VisitClientJoin(ClientJoinExpression join)
        {
            // convert client join into a up-front lookup table builder & replace client-join in tree with lookup accessor

            // 1) lookup = query.Select(e => new KVP(key: inner, value: e)).ToLookup(kvp => kvp.Key, kvp => kvp.Value)
            Expression innerKey = MakeJoinKey(join.InnerKey);
            Expression outerKey = MakeJoinKey(join.OuterKey);

            var kvpConstructor = ReflectionCache.GetMember("KeyValueConstructor", new [] { innerKey.Type, join.Projection.Projector.Type }, pars => typeof(KeyValuePair<,>).MakeGenericType(pars).GetConstructor(pars));
            Expression constructKVPair = Expression.New(kvpConstructor, innerKey, join.Projection.Projector);
            ProjectionExpression newProjection = new ProjectionExpression(join.Projection.Select, constructKVPair, isAsync, join.Projection.IsNoTracking);

            int iLookup = ++nLookup;
            Expression execution = ExecuteProjection(newProjection, false);

            ParameterExpression kvp = Expression.Parameter(constructKVPair.Type, "kvp");

            // filter out nulls
            if (join.Projection.Projector.NodeType == (ExpressionType)DbExpressionType.OuterJoined)
            {
                var nullType = ReflectionCache.GetMember("NullableType", join.Projection.Projector.Type, k => k.GetNullableType());
                var pred = Expression.Lambda(
                    Expression.PropertyOrField(kvp, "Value").NotEqual(Expression.Constant(null, nullType)),
                    kvp
                    );
                execution = Expression.Call(typeof(Enumerable), nameof(Enumerable.Where), new Type[] { kvp.Type }, execution, pred);
            }

            // make lookup
            var keySelector = Expression.Lambda(Expression.PropertyOrField(kvp, "Key"), kvp);
            var elementSelector = Expression.Lambda(Expression.PropertyOrField(kvp, "Value"), kvp);
            var toLookup = Expression.Call(typeof(Enumerable), nameof(Enumerable.ToLookup), new Type[] { kvp.Type, outerKey.Type, join.Projection.Projector.Type }, execution, keySelector, elementSelector);

            // 2) agg(lookup[outer])
            var lookup = Expression.Parameter(toLookup.Type, $"lookup{iLookup}");
            var property = lookup.Type.GetProperty("Item");
            Expression access = Expression.Call(lookup, property.GetGetMethod(), Visit(outerKey));
            if (join.Projection.Aggregator != null)
            {
                // apply aggregator
                access = DbExpressionReplacer.Replace(join.Projection.Aggregator.Body, join.Projection.Aggregator.Parameters[0], access);
            }

            variables.Add(lookup);
            initializers.Add(toLookup);

            return access;
        }

        private Expression BuildInner(Expression expression)
        {
            var eb = new ExecutionBuilder
            {
                executor = executor,
                scope = scope,
                receivingMember = receivingMember,
                nReaders = nReaders,
                nLookup = nLookup,
                translator = translator,
                variableMap = variableMap
            };
            return eb.Visit(expression);
        }

        private Expression ExecuteProjection(ProjectionExpression projection, bool okayToDefer)
        {
            projection = (ProjectionExpression)Parameterize(projection);

            if (scope != null)
            {
                projection = (ProjectionExpression)OuterParameterizer.Parameterize(scope.Alias, projection);
            }

            return BuildExecuteEnumerableCommand(projection, okayToDefer);
        }

        protected override Expression VisitTable(TableExpression table)
        {
            return base.VisitTable(table);
        }

        private Expression BuildExecuteEnumerableCommand(ProjectionExpression projection, bool okayToDefer)
        {
            var elementType = projection.IsSingleton ? projection.Type : ReflectionCache.GetMember("EnumerableElementType", projection.Type, k => k.GetEnumerableElementType());

            var saveScope = scope;
            var recordWrapper = Expression.Parameter(typeof(IRecordWrapper), $"rw{nReaders}");
            var dataReader = Expression.Parameter(typeof(IDataReader), $"dr{nReaders}");
            nReaders++;

            scope = new Scope(scope, recordWrapper, dataReader, projection.Select.Alias, projection.Select.Columns);

            var projector = Visit(projection.Projector);
            var rowMapper = CreateDataRowWrapper(projector, elementType);
            scope = saveScope;

            var result = translator.Translate(projection.Select);

            Expression plan;
            if (isAsync)
            {
                var method = ReflectionCache.GetMember("ExecuteEnumerableAsync", elementType, k => MethodCache.DbExecuteEnumerableAsync.MakeGenericMethod(k));
                cancelToken = Expression.Parameter(typeof(CancellationToken), "token");
                plan = Expression.Call(executor, method,
                    Expression.Constant((SqlCommand)result.QueryText),
                    Expression.Constant(result.DataSegment, typeof(IDataSegment)),
                    CreateParameterCollectionExpression(projection.Select),
                    Expression.Constant(rowMapper),
                    cancelToken
                    );
            }
            else
            {
                var method = ReflectionCache.GetMember("ExecuteEnumerable", elementType, k => MethodCache.DbExecuteEnumerable.MakeGenericMethod(k));
                plan = Expression.Call(executor, method,
                    Expression.Constant((SqlCommand)result.QueryText),
                    Expression.Constant(result.DataSegment, typeof(IDataSegment)),
                    CreateParameterCollectionExpression(projection.Select),
                    Expression.Constant(rowMapper)
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
            var result = translator.Translate(expression);

            if (isAsync)
            {
                cancelToken = Expression.Parameter(typeof(CancellationToken), "token");
                return Expression.Call(executor, MethodCache.DbExecuteNoQueryAsync,
                        Expression.Constant((SqlCommand)result.QueryText),
                        CreateParameterCollectionExpression(expression),
                        cancelToken
                    );
            }
            else
            {
                return Expression.Call(executor, MethodCache.DbExecuteNoQuery,
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
            var result = translator.Translate(expression);

            if (isAsync)
            {
                var method = ReflectionCache.GetMember("ExecuteScalarAsync", command.Type, k => MethodCache.DbExecuteScalarAsync.MakeGenericMethod(k));
                cancelToken = Expression.Parameter(typeof(CancellationToken), "token");
                return Expression.Call(executor, method,
                        Expression.Constant((SqlCommand)result.QueryText),
                        CreateParameterCollectionExpression(expression),
                        cancelToken
                    );
            }
            else
            {
                var method = ReflectionCache.GetMember("ExecuteScalar", command.Type, k => MethodCache.DbExecuteScalar.MakeGenericMethod(k));
                return Expression.Call(executor, method,
                        Expression.Constant((SqlCommand)result.QueryText),
                        CreateParameterCollectionExpression(expression)
                    );
            }
        }

        private Expression BuildExecuteBatch(BatchCommandExpression batch)
        {
            var operation = Parameterize(batch.Operation.Body);

            var result = translator.Translate(operation);
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
                    table.Columns.Add(nv.Name, DataExpressionRow.CreateType(nv.Value.Type));
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
                        var convExp = Expression.Call(null, MethodCache.GetConverter, Expression.Constant(expression.Type));
                        expression = Expression.Call(convExp, MethodCache.ConvertTo, expression, Expression.Constant((DbType)info.DataType));
                    }

                    return (object)Expression.Lambda(expression, batch.Operation.Parameters[1]).Compile();
                });

            var entities = (IEnumerable)batch.Input.Value;
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
                        row[nv.Key] = DataExpressionRow.Create(exp.Type, setter);
                    }
                }

                table.Rows.Add(row);
            }

            Expression plan;
            if (isAsync)
            {
                cancelToken = Expression.Parameter(typeof(CancellationToken), "token");
                plan = Expression.Call(executor, MethodCache.DbUpdateAsync,
                    Expression.Constant(table),
                    Expression.Constant((SqlCommand)result.QueryText),
                    Expression.Constant(null, typeof(SqlCommand)),
                    Expression.Constant(null, typeof(SqlCommand)),
                    cancelToken
                    );
            }
            else
            {
                plan = Expression.Call(executor, MethodCache.DbUpdate,
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
            var values = namedValues.Select(v => Expression.Convert(Visit(v.Value), typeof(object))).ToArray();

            if (values.Length > 0)
            {
                var arguments = new List<Expression>();
                var consPar = ReflectionCache.GetMember("ParameterConstructor", typeof(Parameter), k => k.GetConstructor(new[] { typeof(string), typeof(object) }));

                for (var i = 0; i < namedValues.Count; i++)
                {
                    Expression pv = values[i] is Expression ?
                        (Expression)values[i] :
                        Expression.Constant(values[i], typeof(object));

                    var pExp = Expression.New(consPar, Expression.Constant(namedValues[i].Name), pv);
                    arguments.Add(pExp);
                }

                var initExp = Expression.ListInit(Expression.New(typeof(List<Parameter>)), arguments);
                var consParas = ReflectionCache.GetMember("ParameterCollectionConstructor1", typeof(ParameterCollection), k => k.GetConstructor(new[] { typeof(IEnumerable<Parameter>) }));
                return Expression.New(consParas, initExp);
            }
            else
            {
                return MethodCache.ExpNullParameter;
            }
        }

        /// <summary>
        /// 创建 <see cref="IDataRowMapper"/>。
        /// </summary>
        /// <param name="projector"></param>
        /// <param name="elementType"></param>
        /// <returns></returns>
        private IDataRowMapper CreateDataRowWrapper(Expression projector, Type elementType)
        {
            var funcType = ReflectionCache.GetMember("FuncType", elementType, k => typeof(Func<,>).MakeGenericType(typeof(IDataReader), k));
            var mapperType = ReflectionCache.GetMember("ProjectionRowMapperType", elementType, k => typeof(ProjectionRowMapper<>).MakeGenericType(k));
            var lambda = Expression.Lambda(funcType, projector, scope.dataReader);
            return mapperType.New<IDataRowMapper>(executor, lambda, scope.recordWrapper);
        }

        /// <summary>
        /// 构造实体对象。
        /// </summary>
        /// <param name="entity">实体对象。</param>
        /// <param name="properties">属性数组。</param>
        /// <param name="values">值数组。</param>
        /// <param name="instanceName"></param>
        /// <param name="environment"></param>
        /// <returns></returns>
        private static IEntity ConstructEntity(IEntity entity, IProperty[] properties, PropertyValue[] values,
            string instanceName, EntityPersistentEnvironment environment)
        {
            entity.As<ISupportInitializeNotification>(s => s.BeginInit());

            //循环所有属性进行赋值
            for (int i = 0, n = properties.Length; i < n; i++)
            {
                entity.InitializeValue(properties[i], values[i]);
            }

            //设置状态
            entity.SetState(EntityState.Unchanged);

            //设置 InstanceName 和 Environment
            entity.InitializeEnvironment(environment).InitializeInstanceName(instanceName);

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

        private Expression ConvertEntityExpression(MemberInitExpression mbmInitExp)
        {
            if (isNoTracking == true)
            {
                return mbmInitExp.Update(mbmInitExp.NewExpression, mbmInitExp.Bindings.Select(s => VisitBinding(s)));
            }

            var properties = new List<Expression>();
            var values = new List<Expression>();
            var bindings = new List<MemberBinding>();

            mbmInitExp.Bindings.ForEach(s =>
            {
                if (s is MemberAssignment assign)
                {
                    var expression = Visit(assign.Expression);
                    var proprety = PropertyUnity.GetProperty(mbmInitExp.Type, assign.Member.Name);
                    if (proprety == null)
                    {
                        bindings.Add(Expression.Bind(assign.Member, expression));
                    }
                    else
                    {
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
                            var pValue = Expression.Call(null, MethodCache.NewPropertyValue, expression, Expression.Constant(null, typeof(Type)));
                            values.Add(pValue);
                        }
                    }
                }
            });

            var newExp = Visit(mbmInitExp.NewExpression);
            if (bindings.Count > 0)
            {
                newExp = Expression.MemberInit((NewExpression)newExp, bindings.ToArray());
            }

            var constCall = Expression.Call(null, MethodCache.ConstructEntity,
                newExp,
                Expression.NewArrayInit(typeof(IProperty), properties.ToArray()),
                Expression.NewArrayInit(typeof(PropertyValue), values.ToArray()),
                Expression.Constant(TranslateScope.Current.InstanceName, typeof(string)),
                Expression.Constant(TranslateScope.Current.PersistentEnvironment, typeof(EntityPersistentEnvironment)));

            return Expression.Convert(constCall, mbmInitExp.Type);
        }

        private class Scope
        {
            internal Scope outer;
            internal ParameterExpression recordWrapper;
            internal ParameterExpression dataReader;
            internal TableAlias Alias { get; private set; }
            private readonly Dictionary<string, int> nameMap;

            internal Scope(Scope outer, ParameterExpression recordWrapper, ParameterExpression dataReader, TableAlias alias, IEnumerable<ColumnDeclaration> columns)
            {
                this.outer = outer;
                this.recordWrapper = recordWrapper;
                this.dataReader = dataReader;
                Alias = alias;
                nameMap = columns.Select((c, i) => new { c, i }).ToDictionary(x => x.c.Name, x => x.i);
            }

            internal bool TryGetValue(ColumnExpression column, out ParameterExpression recordWrapper, out ParameterExpression dataReader, out int ordinal)
            {
                for (Scope s = this; s != null; s = s.outer)
                {
                    if (column.Alias == s.Alias && nameMap.TryGetValue(column.Name, out ordinal))
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
            private readonly Dictionary<ColumnExpression, NamedValueExpression> map = new Dictionary<ColumnExpression, NamedValueExpression>();

            internal static Expression Parameterize(TableAlias outerAlias, Expression expr)
            {
                var op = new OuterParameterizer
                {
                    outerAlias = outerAlias
                };
                return op.Visit(expr);
            }

            protected override Expression VisitProjection(ProjectionExpression proj)
            {
                var select = (SelectExpression)Visit(proj.Select);
                return proj.Update(select, proj.Projector, proj.Aggregator);
            }

            protected override Expression VisitColumn(ColumnExpression column)
            {
                if (column.Alias == outerAlias)
                {
                    if (!map.TryGetValue(column, out NamedValueExpression nv))
                    {
                        nv = QueryUtility.GetNamedValueExpression($"n{(iParam++)}", column, (DbType)column.MapInfo.DataType);
                        map.Add(column, nv);
                    }

                    return nv;
                }

                return column;
            }
        }
        class VariableSubstitutor : DbExpressionVisitor
        {
            private readonly Dictionary<string, Expression> map;

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
                if (map.TryGetValue(vex.Name, out Expression sub))
                {
                    return sub;
                }
                return vex;
            }
        }

        private class ProjectionRowMapper<T> : ExpressionRowMapper<T>
        {
            private readonly ParameterExpression executor;
            private readonly LambdaExpression expression;
            private readonly ParameterExpression recordWrapper;

            public ProjectionRowMapper(ParameterExpression executor, LambdaExpression expression, ParameterExpression recordWrapper)
            {
                this.executor = executor;
                this.expression = expression;
                this.recordWrapper = recordWrapper;
            }

            protected override Expression<Func<IDatabase, IDataReader, T>> BuildExpressionForDataReader()
            {
                var exp = (LambdaExpression)DbExpressionReplacer.Replace(expression, recordWrapper, Expression.Constant(RecordWrapper, typeof(IRecordWrapper)));
                if (exp.Parameters.Count == 1 && exp.Parameters[0].Type != typeof(IDatabase))
                {
                    return Expression.Lambda<Func<IDatabase, IDataReader, T>>(exp.Body, executor, exp.Parameters[0]);
                }

                return (Expression<Func<IDatabase, IDataReader, T>>)exp;
            }

            protected override Expression<Func<IDatabase, DataRow, T>> BuildExpressionForDataRow()
            {
                var exp = (LambdaExpression)DbExpressionReplacer.Replace(expression, recordWrapper, Expression.Constant(RecordWrapper, typeof(IRecordWrapper)));
                if (exp.Parameters.Count == 1 && exp.Parameters[0].Type != typeof(IDatabase))
                {
                    return Expression.Lambda<Func<IDatabase, DataRow, T>>(exp.Body, executor, exp.Parameters[0]);
                }

                return (Expression<Func<IDatabase, DataRow, T>>)exp;
            }
        }

        public class BuildOptions
        {
            public bool? IsAsync { get; set; }

            public bool? IsNoTracking { get; set; }
        }
    }
}
