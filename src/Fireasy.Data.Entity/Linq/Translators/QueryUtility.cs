// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using Fireasy.Common.Extensions;
using Fireasy.Data.Converter;
using Fireasy.Data.Entity.Linq.Expressions;
using Fireasy.Data.Entity.Metadata;
using Fireasy.Data.Entity.Properties;
using Fireasy.Data.Extensions;
using Fireasy.Data.Syntax;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Fireasy.Data.Entity.Linq.Translators
{
    internal sealed class QueryUtility
    {
        const string REFERENCE_MPROS_KEY = "MPSKEY";

        internal static LambdaExpression GetAggregator(Type expectedType, Type actualType)
        {
            var actualElementType = actualType.GetEnumerableElementType();
            if (!expectedType.IsAssignableFrom(actualType))
            {
                var expectedElementType = expectedType.GetEnumerableElementType();
                var p = Expression.Parameter(actualType, "p");
                Expression body = null;

                if (expectedType.IsAssignableFrom(actualElementType))
                {
                    body = Expression.Call(typeof(Enumerable), nameof(Enumerable.SingleOrDefault), new[] { actualElementType }, p);
                }
                else if (expectedType.IsGenericType && expectedType.GetGenericTypeDefinition() == typeof(IQueryable<>))
                {
                    body = Expression.Call(typeof(Queryable), nameof(Queryable.AsQueryable), new[] { expectedElementType }, CastElementExpression(expectedElementType, p));
                }
                else if (expectedType.IsArray && expectedType.GetArrayRank() == 1)
                {
                    body = Expression.Call(typeof(Enumerable), nameof(Enumerable.ToArray), new[] { expectedElementType }, CastElementExpression(expectedElementType, p));
                }
                else if (expectedType.IsGenericType && expectedType.GetGenericTypeDefinition() == typeof(EntitySet<>))
                {
                    body = Expression.Call(typeof(EnumerableExtension), nameof(EnumerableExtension.ToEntitySet), new[] { expectedElementType }, CastElementExpression(expectedElementType, p));
                }
                else if (expectedType.IsAssignableFrom(typeof(List<>).MakeGenericType(actualElementType)))
                {
                    body = Expression.Call(typeof(Enumerable), nameof(Enumerable.ToList), new[] { expectedElementType }, CastElementExpression(expectedElementType, p));
                }
                else
                {
                    var ci = expectedType.GetConstructor(new[] { actualType });
                    if (ci != null)
                    {
                        body = Expression.New(ci, p);
                    }
                }

                if (body != null)
                {
                    return Expression.Lambda(body, p);
                }
            }

            return null;
        }

        /// <summary>
        /// 判断表达式是否可用为 Column 使用。
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        internal static bool CanBeColumnExpression(Expression expression)
        {
            if (expression is ProjectionExpression proj && proj.IsSingleton && proj.Type.IsDbTypeSupported())
            {
                return true;
            }

            switch (expression.NodeType)
            {
                case (ExpressionType)DbExpressionType.Column:
                case (ExpressionType)DbExpressionType.Scalar:
                case (ExpressionType)DbExpressionType.Exists:
                case (ExpressionType)DbExpressionType.AggregateSubquery:
                case (ExpressionType)DbExpressionType.Aggregate:
                    return true;
                default:
                    return false;
            }
        }

        private static Expression CastElementExpression(Type expectedElementType, Expression expression)
        {
            var elementType = expression.Type.GetEnumerableElementType();
            if (expectedElementType != elementType &&
                (expectedElementType.IsAssignableFrom(elementType) ||
                  elementType.IsAssignableFrom(expectedElementType)))
            {
                return Expression.Call(typeof(Enumerable), nameof(Enumerable.Cast), new[] { expectedElementType }, expression);
            }

            return expression;
        }

        internal static ProjectionExpression GetTableQuery(EntityMetadata entity, bool isNoTracking, bool isAsync)
        {
            var tableAlias = new TableAlias();
            var selectAlias = new TableAlias();
            var entityType = entity.EntityType;
            var table = new TableExpression(tableAlias, entity.TableName, entityType);

            var projector = GetTypeProjection(table, entity, isNoTracking);
            var pc = ColumnProjector.ProjectColumns(CanBeColumnExpression, projector, null, selectAlias, tableAlias);

            var proj = new ProjectionExpression(
                new SelectExpression(selectAlias, pc.Columns, table, null),
                pc.Projector, isAsync
                );

            return (ProjectionExpression)ApplyPolicy(proj, entityType);
        }

        internal static Expression GetTypeProjection(Expression root, EntityMetadata entity, bool isNoTracking)
        {
            var entityType = entity.EntityType;
            var bindings = new List<MemberBinding>();

            //获取实体中定义的所有依赖属性
            var properties = PropertyUnity.GetLoadedProperties(entityType);
            foreach (var property in properties)
            {
                var mbrExpression = GetMemberExpression(root, property);
                if (mbrExpression != null)
                {
                    bindings.Add(Expression.Bind(property.Info.ReflectionInfo, mbrExpression));
                }
            }

            return new EntityExpression(entity, Expression.MemberInit(Expression.New(entityType), bindings), isNoTracking);
        }

        internal static Expression GetMemberExpression(Expression root, IProperty property)
        {
            if (property is RelationProperty relationProprety)
            {
                //所关联的实体类型
                var relMetadata = EntityMetadataUnity.GetEntityMetadata(relationProprety.RelationalType);
                var projection = GetTableQuery(relMetadata, false, false);

                Expression parentExp = null, childExp = null;
                var ship = RelationshipUnity.GetRelationship(relationProprety);

                if (ship.ThisType != relationProprety.EntityType)
                {
                    parentExp = projection.Projector;
                    childExp = root;
                }
                else
                {
                    parentExp = root;
                    childExp = projection.Projector;
                }

                Expression where = null;
                for (int i = 0, n = ship.Keys.Count; i < n; i++)
                {
                    var equal = GetMemberExpression(parentExp, ship.Keys[i].ThisProperty)
                        .Equal(GetMemberExpression(childExp, ship.Keys[i].OtherProperty));
                    where = (where != null) ? Expression.And(where, equal) : equal;
                }

                var newAlias = new TableAlias();
                var pc = ColumnProjector.ProjectColumns(CanBeColumnExpression, projection.Projector, null, newAlias, projection.Select.Alias);

                var aggregator = GetAggregator(property.Type, typeof(IEnumerable<>).MakeGenericType(pc.Projector.Type));
                var result = new ProjectionExpression(
                    new SelectExpression(newAlias, pc.Columns, projection.Select, where),
                    pc.Projector, aggregator, projection.IsAsync
                    );

                return ApplyPolicy(result, property.Info.ReflectionInfo);
            }

            if (root is TableExpression table)
            {
                if (property is SubqueryProperty sqProperty)
                {
                    return new SubqueryColumnExpression(property.Type, table.Alias, property.Info.FieldName, sqProperty.Subquery);
                }
                else if (property is ISavedProperty)
                {
                    return new ColumnExpression(property.Type, table.Alias, property.Name, property.Info);
                }
            }

            return QueryBinder.BindMember(root, property.Info.ReflectionInfo);
        }

        internal static Expression GetDeleteExpression(EntityMetadata metadata, LambdaExpression predicate, bool replace, bool isAsync)
        {
            var table = new TableExpression(new TableAlias(), metadata.TableName, metadata.EntityType);
            Expression where = null;

            if (predicate != null)
            {
                if (replace)
                {
                    var row = GetTypeProjection(table, metadata, false);
                    where = DbExpressionReplacer.Replace(predicate.Body, predicate.Parameters[0], row);
                }
                else
                {
                    where = predicate.Body;
                }
            }

            return new DeleteCommandExpression(table, where, isAsync);
        }

        internal static Expression GetLogicalDeleteExpression(EntityMetadata metadata, LambdaExpression predicate, bool isAsync)
        {
            var table = new TableExpression(new TableAlias(), metadata.TableName, metadata.EntityType);
            Expression where = null;

            var delflag = (ColumnExpression)GetMemberExpression(table, metadata.DeleteProperty);
            var assignments = new List<ColumnAssignment>
                {
                    new ColumnAssignment(delflag, Expression.Constant(true))
                };

            if (predicate != null)
            {
                var row = GetTypeProjection(table, metadata, false);
                where = DbExpressionReplacer.Replace(predicate.Body, predicate.Parameters[0], row);
            }

            return new UpdateCommandExpression(table, where, assignments, isAsync);
        }

        internal static Expression GetUpdateExpression(Expression instance, LambdaExpression predicate, bool isAsync)
        {
            if (instance is ParameterExpression)
            {
                return GetUpdateExpressionByParameter((ParameterExpression)instance, predicate, isAsync);
            }
            else if (instance is ConstantExpression)
            {
                return GetUpdateExpressionByEntity((ConstantExpression)instance, predicate, isAsync);
            }

            var lambda = instance as LambdaExpression;
            if (lambda == null && instance.NodeType == ExpressionType.Quote)
            {
                lambda = ((UnaryExpression)instance).Operand as LambdaExpression;
            }

            if (lambda != null)
            {
                return GetUpdateExpressionByCalculator(lambda, predicate, isAsync);
            }

            return null;
        }

        internal static LambdaExpression GetPrimaryKeyExpression(ParameterExpression parExp)
        {
            var metadata = EntityMetadataUnity.GetEntityMetadata(parExp.Type);
            var table = new TableExpression(new TableAlias(), metadata.TableName, parExp.Type);
            var primaryKeys = PropertyUnity.GetPrimaryProperties(parExp.Type);
            Expression where = null;

            foreach (var pk in primaryKeys)
            {
                var eq = GetMemberExpression(table, pk).Equal(Expression.MakeMemberAccess(parExp, pk.Info.ReflectionInfo));
                where = where == null ? eq : Expression.And(where, eq);
            }

            return Expression.Lambda(where, parExp);
        }

        private static Expression GetUpdateExpressionByEntity(ConstantExpression constant, LambdaExpression predicate, bool isAsync)
        {
            var entity = constant.Value as IEntity;
            var metadata = EntityMetadataUnity.GetEntityMetadata(entity.EntityType);
            var table = new TableExpression(new TableAlias(), metadata.TableName, metadata.EntityType);
            Expression where = null;

            if (predicate != null)
            {
                var row = GetTypeProjection(table, metadata, false);
                where = DbExpressionReplacer.Replace(predicate.Body, predicate.Parameters[0], row);
            }

            return new UpdateCommandExpression(table, where, GetUpdateArguments(table, entity), isAsync);
        }

        private static Expression GetUpdateExpressionByParameter(ParameterExpression parExp, LambdaExpression predicate, bool isAsync)
        {
            var metadata = EntityMetadataUnity.GetEntityMetadata(parExp.Type);
            var table = new TableExpression(new TableAlias(), metadata.TableName, parExp.Type);

            return new UpdateCommandExpression(table, predicate.Body, GetUpdateArguments(table, parExp), isAsync);
        }

        private static Expression GetUpdateExpressionByCalculator(LambdaExpression lambda, LambdaExpression predicate, bool isAsync)
        {
            var initExp = lambda.Body as MemberInitExpression;
            var newExp = initExp.NewExpression;
            var entityType = newExp.Type;
            var metadata = EntityMetadataUnity.GetEntityMetadata(entityType);
            var table = new TableExpression(new TableAlias(), metadata.TableName, metadata.EntityType);
            Expression where = null;
            var row = GetTypeProjection(table, metadata, false);

            if (predicate != null)
            {
                where = DbExpressionReplacer.Replace(predicate.Body, predicate.Parameters[0], row);
            }

            var list = new List<MemberAssignment>();
            foreach (MemberAssignment ass in initExp.Bindings)
            {
                list.Add(Expression.Bind(ass.Member, DbExpressionReplacer.Replace(ass.Expression, lambda.Parameters[0], row)));
            }

            return new UpdateCommandExpression(table, where, GetUpdateArguments(table, list), isAsync);
        }

        internal static Expression GetInsertExpression(ISyntaxProvider syntax, Expression instance, bool isAsync)
        {
            InsertCommandExpression insertExp;
            var entityType = instance.Type;
            Func<TableExpression, IEnumerable<ColumnAssignment>> func;

            if (instance is ParameterExpression parExp)
            {
                func = new Func<TableExpression, IEnumerable<ColumnAssignment>>(t => GetInsertArguments(syntax, t, parExp));
            }
            else
            {
                var entity = instance.As<ConstantExpression>().Value as IEntity;
                func = new Func<TableExpression, IEnumerable<ColumnAssignment>>(t => GetInsertArguments(syntax, t, entity));
            }

            var metadata = EntityMetadataUnity.GetEntityMetadata(entityType);
            var table = new TableExpression(new TableAlias(), metadata.TableName, entityType);
            insertExp = new InsertCommandExpression(table, func(table), isAsync)
                {
                    WithAutoIncrement = !string.IsNullOrEmpty(syntax.IdentitySelect) && HasAutoIncrement(instance.Type),
                    WithGenerateValue = HasGenerateValue(instance.Type)
                };

            return insertExp;
        }

        internal static NamedValueExpression GetNamedValueExpression(string name, Expression value, DbType dbType)
        {
            var exp = value;
            if (exp.NodeType == ExpressionType.Convert)
            {
                exp = ((UnaryExpression)value).Operand;
            }

            if (exp.NodeType != ExpressionType.Constant)
            {
                return new NamedValueExpression(name, exp);
            }

            IValueConverter converter;
            if ((converter = ConvertManager.GetConverter(exp.Type)) != null)
            {
                exp = Expression.Constant(converter.ConvertTo(((ConstantExpression)exp).Value, dbType));
                exp = Expression.Convert(exp, typeof(object));
            }

            return new NamedValueExpression(name, exp, dbType);
        }

        /// <summary>
        /// 将被修改的属性集附加到 <see cref="TranslateScope"/> 对象中。
        /// </summary>
        /// <param name="instances"></param>
        internal static List<string> AttachModifiedProperties(Expression instances)
        {
            var properties = GetModifiedProperties(instances);

            //在序列中查找被修改的属性列表
            TranslateScope.Current.SetData(REFERENCE_MPROS_KEY, properties);

            return properties;
        }

        /// <summary>
        /// 从 <see cref="TranslateScope"/> 中移除标记的属性集。
        /// </summary>
        internal static void ReleaseModifiedProperties()
        {
            //从上下文中移除
            TranslateScope.Current.RemoveData(REFERENCE_MPROS_KEY);
        }

        private static bool HasAutoIncrement(Type entityType)
        {
            return PropertyUnity.GetPrimaryProperties(entityType).Any(s => s.Info.GenerateType == IdentityGenerateType.AutoIncrement);
        }

        private static bool HasGenerateValue(Type entityType)
        {
            return PropertyUnity.GetPrimaryProperties(entityType).Any(s => s.Info.GenerateType == IdentityGenerateType.Generator);
        }

        private static IEnumerable<ColumnAssignment> GetUpdateArguments(TableExpression table, IEntity entity)
        {
            var properties = GetModifiedProperties(entity).ToArray();

            return properties
                .Select(m => new ColumnAssignment(
                       (ColumnExpression)GetMemberExpression(table, m),
                       Expression.Constant(GetConvertableValue(entity, m))
                       )).ToList();
        }

        private static IEnumerable<ColumnAssignment> GetUpdateArguments(TableExpression table, ParameterExpression parExp)
        {
            IEnumerable<IProperty> properties = null;
            List<string> modifiedNames = null;
            if ((modifiedNames = GetReferenceModifiedProperties()) != null)
            {
                properties = GetModifiedProperties(table.Type, modifiedNames);
            }

            return properties
                .Select(m => new ColumnAssignment(
                       (ColumnExpression)GetMemberExpression(table, m),
                       Expression.MakeMemberAccess(parExp, m.Info.ReflectionInfo)
                       ));
        }

        private static IEnumerable<ColumnAssignment> GetUpdateArguments(TableExpression table, IEnumerable<MemberAssignment> bindings)
        {
            return from m in bindings
                   let property = PropertyUnity.GetProperty(table.Type, m.Member.Name)
                   select new ColumnAssignment(
                       (ColumnExpression)GetMemberExpression(table, property),
                       m.Expression
                       );
        }

        /// <summary>
        /// 返回插入具体实体时的 <see cref="ColumnAssignment"/> 集合。
        /// </summary>
        /// <param name="syntax"></param>
        /// <param name="table"></param>
        /// <param name="entity">具体的实体。</param>
        /// <returns></returns>
        private static IEnumerable<ColumnAssignment> GetInsertArguments(ISyntaxProvider syntax, TableExpression table, IEntity entity)
        {
            var properties = GetModifiedProperties(entity);

            var assignments = properties
                .Select(m => new ColumnAssignment(
                       (ColumnExpression)GetMemberExpression(table, m),
                       Expression.Constant(GetConvertableValue(entity, m))
                       )).ToList();

            assignments.AddRange(GetAssignmentsForPrimaryKeys(syntax, table, null, entity));

            return assignments;
        }

        /// <summary>
        /// 返回插入具体实体时的 <see cref="ColumnAssignment"/> 集合。
        /// </summary>
        /// <param name="syntax"></param>
        /// <param name="table"></param>
        /// <param name="parExp"></param>
        /// <returns></returns>
        private static IEnumerable<ColumnAssignment> GetInsertArguments(ISyntaxProvider syntax, TableExpression table, ParameterExpression parExp)
        {
            IEnumerable<IProperty> properties = null;
            List<string> modifiedNames = null;

            if ((modifiedNames = GetReferenceModifiedProperties()) != null)
            {
                properties = GetModifiedProperties(table.Type, modifiedNames);
            }

            var assignments = properties
                .Select(m => new ColumnAssignment(
                       (ColumnExpression)GetMemberExpression(table, m),
                       Expression.MakeMemberAccess(parExp, m.Info.ReflectionInfo)
                       )).ToList();

            assignments.AddRange(GetAssignmentsForPrimaryKeys(syntax, table, parExp, null));

            return assignments;
        }

        private static IEnumerable<ColumnAssignment> GetAssignmentsForPrimaryKeys(ISyntaxProvider syntax, TableExpression table, ParameterExpression parExp, IEntity entity)
        {
            var entityType = parExp != null ? parExp.Type : entity.EntityType;
            var assignments = new List<ColumnAssignment>();

            foreach (var p in PropertyUnity.GetPrimaryProperties(entityType))
            {
                var pvExp = GetPrimaryValueExpression(syntax, table, parExp, entity, p);

                if (pvExp != null)
                {
                    var columnExp = (ColumnExpression)GetMemberExpression(table, p);
                    assignments.Add(new ColumnAssignment(columnExp, pvExp));
                }
            }

            return assignments;
        }

        private static Expression ApplyPolicy(Expression expression, MemberInfo member)
        {
            if (TranslateScope.Current != null)
            {
                if (TranslateScope.Current.ContextService is IQueryPolicy policy)
                {
                    return policy.ApplyPolicy(expression, member);
                }
            }

            return expression;
        }

        /// <summary>
        /// 获取实体可插入或更新的非主键属性列表。
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private static IEnumerable<IProperty> GetModifiedProperties(IEntity entity)
        {
            var properties = PropertyUnity.GetPersistentProperties(entity.EntityType)
                .Where(m => !m.Info.IsPrimaryKey ||
                    (m.Info.IsPrimaryKey && m.Info.GenerateType == IdentityGenerateType.None));

            //判断实体类型有是不是编译的代理类型，如果不是，取非null的属性，否则使用IsModified判断
            var isNotCompiled = entity.GetType().IsNotCompiled();
            return properties.Where(m => isNotCompiled ?
                    !PropertyValue.IsEmpty(entity.GetValue(m)) :
                    entity.IsModified(m.Name));
        }

        /// <summary>
        /// 获取实体可插入或更新的非主键属性列表。
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="names"></param>
        /// <returns></returns>
        private static IEnumerable<IProperty> GetModifiedProperties(Type entityType, List<string> names)
        {
            return PropertyUnity.GetPersistentProperties(entityType)
                .Where(s => names.Contains(s.Name));
        }

        /// <summary>
        /// 获取主键值的表达式。
        /// </summary>
        /// <param name="syntax"></param>
        /// <param name="table"></param>
        /// <param name="parExp"></param>
        /// <param name="entity"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        private static Expression GetPrimaryValueExpression(ISyntaxProvider syntax, TableExpression table, ParameterExpression parExp, IEntity entity, IProperty property)
        {
            //如果不支持自增量，则使用 IGenerateProvider生成
            if (property.Info.GenerateType == IdentityGenerateType.Generator ||
                (property.Info.GenerateType == IdentityGenerateType.AutoIncrement &&
                string.IsNullOrEmpty(syntax.IdentitySelect)))
            {
                return new GeneratorExpression(table, parExp ?? (Expression)Expression.Constant(entity), property);
            }

            return null;
        }

        /// <summary>
        /// 获取受 <see cref="IValueConverter"/> 支持的数据转换值。
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        private static object GetConvertableValue(IEntity entity, IProperty property)
        {
            var value = entity.GetValue(property);
            if (!PropertyValue.IsEmpty(value))
            {
                //查找属性类型对应的转换器
                var converter = ConvertManager.GetConverter(property.Type);
                if (converter != null && property.Info.DataType != null)
                {
                    var pvValue = value.GetValue();
                    return converter.ConvertTo(pvValue, (DbType)property.Info.DataType);
                }

                return value.GetValue();
            }

            return property.Type.GetDefaultValue();
        }

        /// <summary>
        /// 在批处理的指令中获取被修改的实体属性名称列表。
        /// </summary>
        /// <returns></returns>
        private static List<string> GetReferenceModifiedProperties()
        {
            List<string> names;
            if (TranslateScope.Current != null &&
                (names = TranslateScope.Current.GetData<object>(REFERENCE_MPROS_KEY) as List<string>) != null)
            {
                return names;
            }

            return null;
        }

        /// <summary>
        /// 获取插入或更新所参照的实体。
        /// </summary>
        /// <param name="instances"></param>
        /// <returns></returns>
        private static List<string> GetModifiedProperties(Expression instances)
        {
            var entities = ((ConstantExpression)instances).Value as IEnumerable;
            if (entities == null)
            {
                return null;
            }

            var maxProperties = new List<string>();
            IEntity retEntity = null;
            Type entityType = null;
            IEnumerable<IProperty> properties = null;

            //循环列表查找属性修改最多的那个实体作为参照
            foreach (IEntity entity in entities)
            {
                if (entityType == null)
                {
                    entityType = entity.GetType();
                    properties = PropertyUnity.GetPersistentProperties(entityType)
                        .Where(m => !m.Info.IsPrimaryKey ||
                            (m.Info.IsPrimaryKey && m.Info.GenerateType == IdentityGenerateType.None));
                }

                //判断实体类型有是不是编译的代理类型，如果不是，取非null的属性，否则使用IsModified判断
                var isNotCompiled = entityType.IsNotCompiled();
                var modified = isNotCompiled ?
                    properties.Where(s => !PropertyValue.IsEmpty(entity.GetValue(s))).Select(s => s.Name).ToArray() :
                    entity.GetModifiedProperties();

                if (!modified.All(s => maxProperties.Contains(s)))
                {
                    retEntity = entity;
                    maxProperties.AddRange(modified.Except(maxProperties));
                }
            }

            return maxProperties;
        }
    }
}