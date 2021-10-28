// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Data.Entity.Linq;
using Fireasy.Data.Entity.Query;
#if NETSTANDARD
using Microsoft.Extensions.DependencyInjection;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Data.Entity.Properties
{
    /// <summary>
    /// 属性延迟加载的抽象类。
    /// </summary>
    public abstract class PropertyLazyLoadderBase : IPropertyLazyLoadder
    {
        /// <summary>
        /// 获取实体中指定属性的值。
        /// </summary>
        /// <param name="entity">当前的实体对象。</param>
        /// <param name="property">要获取值的属性。</param>
        /// <returns></returns>
        public abstract PropertyValue GetValue(IEntity entity, IProperty property);

        /// <summary>
        /// 根据关系属性构造查询表达式。
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="relationProperty"></param>
        /// <param name="thisKey"></param>
        /// <param name="otherKey"></param>
        /// <returns></returns>
        protected virtual Expression BuildRelationExpression(IEntity entity, RelationProperty relationProperty, Func<RelationshipKey, IProperty> thisKey, Func<RelationshipKey, IProperty> otherKey)
        {
            var relationKey = RelationshipUnity.GetRelationship(relationProperty);
            if (relationKey == null)
            {
                return null;
            }

            var parExp = Expression.Parameter(relationProperty.RelationalType, "s");
            Expression binExp = null;

            foreach (var key in relationKey.Keys)
            {
                var val = entity.GetValue(thisKey(key));
                if (PropertyValue.IsEmpty(val) || !val.IsValid)
                {
                    continue;
                }

                var member = otherKey(key).Info.ReflectionInfo;
                var mbrExp = Expression.MakeMemberAccess(parExp, member);
                var exp = Expression.Equal(mbrExp, Expression.Constant(val.GetValue()));
                binExp = binExp == null ? exp : Expression.And(binExp, exp);
            }

            if (binExp == null)
            {
                return null;
            }

            return Expression.Lambda(binExp, parExp);
        }

        /// <summary>
        /// 判断是否不需要加载。
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        protected bool CheckWithoutLoading(IInstanceIdentifier identifier)
        {
            if (identifier is EntityContextOptions options && options.LoadBehavior == LoadBehavior.None)
            {
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// 关联实体对象的延迟加载器。
    /// </summary>
    internal class EntityPropertyLazyLoadder : PropertyLazyLoadderBase
    {
        public override PropertyValue GetValue(IEntity entity, IProperty property)
        {
            var entityProperty = property.As<EntityProperty>();

            var instanceName = entity.GetInstanceName();
            var environment = entity.GetEnvironment();

            var identifier = ContextInstanceManager.Default.TryGet(instanceName, entity.EntityType);
            if (identifier == null || CheckWithoutLoading(identifier))
            {
                return PropertyValue.Empty;
            }

            var contextProvider = identifier.Provider.GetService<IContextProvider>();
            using var scope = identifier.ServiceProvider.TryCreateScope();
            using var service = contextProvider.CreateContextService(new ContextServiceContext(scope?.ServiceProvider ?? identifier.ServiceProvider, identifier));
            service.InitializeEnvironment(environment).InitializeInstanceName(instanceName);

            var repProvider = service.CreateRepositoryProvider(entityProperty.RelationalType);
            var expression = BuidRelationExpression(entity, entityProperty);
            if (expression != null)
            {
                var value = Execute(repProvider.Queryable, expression);

                if (value != null)
                {
                    value.As<IEntityRelation>(e => e.Owner = new EntityOwner(entity, property));
                    return PropertyValue.NewValue(value, property.Type);
                }
            }

            return PropertyValue.Empty;
        }

        /// <summary>
        /// 创建关联查询的表达式。
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="relationProperty"></param>
        /// <returns></returns>
        private Expression BuidRelationExpression(IEntity entity, RelationProperty relationProperty)
        {
            return BuildRelationExpression(entity, relationProperty,
                new Func<RelationshipKey, IProperty>(k => k.DependentProperty),
                new Func<RelationshipKey, IProperty>(k => k.PrincipalProperty));
        }

        /// <summary>
        /// 执行查询返回结果。
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private object Execute(IQueryable queryable, Expression predicate)
        {
            var expression = Expression.Call(typeof(Queryable), nameof(Queryable.FirstOrDefault),
                new[] { queryable.ElementType }, queryable.Expression, predicate);

            return queryable.Provider.Execute(expression);
        }
    }

    internal class EntitySetPropertyLazyLoadder : PropertyLazyLoadderBase
    {
        public override PropertyValue GetValue(IEntity entity, IProperty property)
        {
            var entityProperty = property.As<EntitySetProperty>();
            var instanceName = entity.GetInstanceName();
            var environment = entity.GetEnvironment();

            var identifier = ContextInstanceManager.Default.TryGet(instanceName, entity.EntityType);
            if (identifier == null || CheckWithoutLoading(identifier))
            {
                return PropertyValue.Empty;
            }

            var contextProvider = identifier.Provider.GetService<IContextProvider>();
            using var scope = identifier.ServiceProvider.TryCreateScope();
            using var service = contextProvider.CreateContextService(new ContextServiceContext(scope?.ServiceProvider ?? identifier.ServiceProvider, identifier));
            service.InitializeEnvironment(environment).InitializeInstanceName(instanceName);

            var repProvider = service.CreateRepositoryProvider(entityProperty.RelationalType);
            var expression = BuidRelationExpression(entity, entityProperty);
            object result = null;
            if (expression != null)
            {
                result = Execute(repProvider.Queryable, expression);
                var querySetType = typeof(EntitySet<>).MakeGenericType(entityProperty.RelationalType);
                var list = querySetType.New(new[] { result });

                //设置实体集所属的实体Owner
                list.As<IEntityRelation>(e => e.Owner = new EntityOwner(entity, property));

                return PropertyValue.NewValue(list);
            }

            return PropertyValue.Empty;
        }

        /// <summary>
        /// 创建关联查询的表达式。
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="relationProperty"></param>
        /// <returns></returns>
        private Expression BuidRelationExpression(IEntity entity, RelationProperty relationProperty)
        {
            return BuildRelationExpression(entity, relationProperty,
                new Func<RelationshipKey, IProperty>(k => k.PrincipalProperty),
                new Func<RelationshipKey, IProperty>(k => k.DependentProperty));
        }

        /// <summary>
        /// 执行查询返回结果。
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private object Execute(IQueryable queryable, Expression predicate)
        {
            var expression = Expression.Call(typeof(Queryable), nameof(Queryable.Where),
                new[] { queryable.ElementType }, queryable.Expression, predicate);

            return queryable.Provider.Execute(expression);
        }
    }

    internal class ReferencePropertyLazyLoadder : PropertyLazyLoadderBase
    {
        public override PropertyValue GetValue(IEntity entity, IProperty property)
        {
            var referenceProperty = property.As<ReferenceProperty>();
            var instanceName = entity.GetInstanceName();
            var environment = entity.GetEnvironment();

            var identifier = ContextInstanceManager.Default.TryGet(instanceName, entity.EntityType);
            if (identifier == null || CheckWithoutLoading(identifier))
            {
                return PropertyValue.Empty;
            }

            var contextProvider = identifier.Provider.GetService<IContextProvider>();
            using var scope = identifier.ServiceProvider.TryCreateScope();
            using var service = contextProvider.CreateContextService(new ContextServiceContext(scope?.ServiceProvider ?? identifier.ServiceProvider, identifier));
            service.InitializeEnvironment(environment).InitializeInstanceName(instanceName);

            var repProvider = service.CreateRepositoryProvider(referenceProperty.RelationalType);
            var expression = BuidRelationExpression(entity, referenceProperty);
            if (expression != null)
            {
                var value = Execute(repProvider.Queryable, expression);
                return value == null ? PropertyValue.Empty : PropertyValue.NewValue(value);
            }

            return PropertyValue.Empty;
        }

        /// <summary>
        /// 创建关联查询的表达式。
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="relationProperty"></param>
        /// <returns></returns>
        private Expression BuidRelationExpression(IEntity entity, RelationProperty relationProperty)
        {
            return BuildRelationExpression(entity, relationProperty,
                new Func<RelationshipKey, IProperty>(k => k.DependentProperty),
                new Func<RelationshipKey, IProperty>(k => k.PrincipalProperty));
        }

        /// <summary>
        /// 执行查询返回结果。
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        private object Execute(IQueryable queryable, Expression expression)
        {
            var method = typeof(Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(s => s.Name == "Where");

            if (method != null)
            {
                method = method.MakeGenericMethod(queryable.ElementType);
                expression = Expression.Call(null, method,
                    new[] { queryable.Expression, expression });
            }

            method = typeof(Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(s => s.Name == "Select").ElementAt(1);

            if (method != null)
            {
                method = method.MakeGenericMethod(queryable.ElementType);
                expression = Expression.Call(null, method,
                    new[] { queryable.Expression, expression });
            }

            method = typeof(Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(s => s.Name == "FirstOrDefault" && s.GetParameters().Length == 2).FirstOrDefault();

            if (method != null)
            {
                method = method.MakeGenericMethod(queryable.ElementType);
                expression = Expression.Call(null, method,
                    new[] { queryable.Expression, expression });
            }

            return queryable.Provider.Execute(expression);
        }
    }

    internal class EnumPropertyLazyLoadder : PropertyLazyLoadderBase
    {
        public override PropertyValue GetValue(IEntity entity, IProperty property)
        {
            var enumProperty = property.As<EnumProperty>();
            var value = entity.GetValue(enumProperty.Reference);
            return PropertyValue.IsEmpty(value) ? PropertyValue.Empty : ((Enum)value).GetDescription();
        }
    }

    internal class EntityBatchLoadder
    {
        public void Load(IEnumerable<IEntity> entities, Type filterAttribute = null)
        {
            var first = entities.FirstOrDefault();
            if (first == null)
            {
                return;
            }

            var instanceName = first.GetInstanceName();
            var environment = first.GetEnvironment();

            var identifier = ContextInstanceManager.Default.TryGet(instanceName, first.EntityType);
            if (identifier == null)
            {
                return;
            }

            var dict = FindMetadata(entities, filterAttribute);

            var contextProvider = identifier.Provider.GetService<IContextProvider>();
            using var scope = identifier.ServiceProvider.TryCreateScope();
            using var service = contextProvider.CreateContextService(new ContextServiceContext(scope?.ServiceProvider ?? identifier.ServiceProvider, identifier));
            service.InitializeEnvironment(environment).InitializeInstanceName(instanceName);

            foreach (var kvp in dict)
            {
                var expression = BuildRelationExpression(kvp.Key, kvp.Value);
                if (expression == null)
                {
                    continue;
                }

                var repProvider = service.CreateRepositoryProvider(kvp.Key.RelationalType);

                var result = (IEnumerable)Execute(repProvider.Queryable, expression, filterAttribute);
                foreach (IEntity item in result)
                {
                    foreach (var entity in entities)
                    {
                        var value = PropertyValue.NewValue(item, kvp.Key.RelationalType);
                        entity.InitializeValue(kvp.Key, value);
                    }
                }
            }
        }

        private Dictionary<RelationProperty, List<Tuple<IProperty, object>>> FindMetadata(IEnumerable<IEntity> entities, Type filterAttribute)
        {
            var dict = new Dictionary<RelationProperty, List<Tuple<IProperty, object>>>();

            foreach (var entity in entities)
            {
                foreach (RelationProperty property in PropertyUnity.GetRelatedProperties(entity.EntityType))
                {
                    if (property is not EntityProperty p)
                    {
                        continue;
                    }

                    if (filterAttribute != null && !property.Info.ReflectionInfo.IsDefined(filterAttribute))
                    {
                        continue;
                    }

                    var relationKey = RelationshipUnity.GetRelationship(property);

                    var keyValues = relationKey.Keys.Select(s => Tuple.Create(
                        s.PrincipalProperty,
                        entity.GetValue(s.DependentProperty).GetValue())).Where(s => s.Item2 != null).ToList();

                    if (keyValues.Count > 0)
                    {
                        if (!dict.TryGetValue(property, out List<Tuple<IProperty, object>> list))
                        {
                            dict.Add(property, keyValues);
                        }
                        else
                        {
                            list.AddRange(keyValues);
                        }
                    }
                }
            }

            return dict;
        }

        private Expression BuildRelationExpression(RelationProperty relationProperty, List<Tuple<IProperty, object>> properties)
        {
            var relationKey = RelationshipUnity.GetRelationship(relationProperty);
            if (relationKey == null)
            {
                return null;
            }

            var parExp = Expression.Parameter(relationProperty.RelationalType, "s");
            Expression binExp = null;

            foreach (var g in properties.GroupBy(s => s.Item1))
            {
                var member = g.Key.Info.ReflectionInfo;
                var mbrExp = Expression.MakeMemberAccess(parExp, member);
                var exp = g.Select(t => t.Item2).Distinct().ToList()
                    .Select(s => Expression.Equal(mbrExp, Expression.Convert(Expression.Constant(s), mbrExp.Type))).Aggregate(Expression.Or);
                binExp = binExp == null ? exp : Expression.And(binExp, exp);
            }

            if (binExp == null)
            {
                return null;
            }

            return Expression.Lambda(binExp, parExp);
        }

        /// <summary>
        /// 执行查询返回结果。
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private object Execute(IQueryable queryable, Expression predicate, Type filterAttribute = null)
        {
            var expression = BuildQueryExpression(queryable, predicate, filterAttribute);

            return queryable.Provider.Execute(expression);
        }

        private Expression BuildQueryExpression(IQueryable queryable, Expression predicate, Type filterAttribute = null)
        {
            var expression = Expression.Call(typeof(Queryable), nameof(Queryable.Where),
                new[] { queryable.ElementType }, queryable.Expression, predicate);

            if (filterAttribute != null)
            {
                var properties = PropertyUnity.GetProperties(queryable.ElementType).Where(s => s.Info.ReflectionInfo.IsDefined(filterAttribute)).ToList();
                if (properties.Count > 0)
                {
                    properties.AddRange(PropertyUnity.GetPrimaryProperties(queryable.ElementType));

                    var parExp = Expression.Parameter(queryable.ElementType, "s");
                    var binds = properties.Select(s => Expression.Bind(s.Info.ReflectionInfo, CreateSubPropertyExpression(s, () => Expression.MakeMemberAccess(parExp, s.Info.ReflectionInfo), filterAttribute)));
                    var initExp = Expression.MemberInit(Expression.New(queryable.ElementType), binds);

                    expression = Expression.Call(typeof(Queryable), nameof(Queryable.Select),
                        new[] { queryable.ElementType, queryable.ElementType }, expression, Expression.Lambda(initExp, parExp));
                }
            }

            return expression;
        }

        private Expression CreateSubPropertyExpression(IProperty property, Func<Expression> func, Type filterAttribute)
        {
            var mbrExp = func();

            if (filterAttribute != null && property is EntityProperty ep)
            {
                var properties = PropertyUnity.GetProperties(ep.RelationalType).Where(s => s.Info.ReflectionInfo.IsDefined(filterAttribute)).ToList();
                if (properties.Count > 0)
                {
                    properties.AddRange(PropertyUnity.GetPrimaryProperties(ep.RelationalType));

                    var binds = properties.Select(s => Expression.Bind(s.Info.ReflectionInfo, CreateSubPropertyExpression(s, () => Expression.MakeMemberAccess(mbrExp, s.Info.ReflectionInfo), filterAttribute)));
                    return Expression.MemberInit(Expression.New(ep.RelationalType), binds);
                }
            }

            return mbrExp;
        }
    }
}
