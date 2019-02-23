// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Data.Entity.Linq;
using Fireasy.Data.Entity.Metadata;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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
        /// 获取实例上的 <see cref="EntityPersistentEnvironment"/> 实例。
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected EntityPersistentEnvironment GetEnvironment(IEntity entity)
        {
            var env = entity.As<IEntityPersistentEnvironment>();
            return env != null ? env.Environment : null;
        }

        /// <summary>
        /// 从实体中取出 InstanceName，前提是实体实现了 <see cref="IEntityPersistentInstanceContainer"/> 接口。
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="relationType"></param>
        /// <returns></returns>
        protected string GetInstanceName(IEntity entity, Type relationType)
        {
            var con = entity.As<IEntityPersistentInstanceContainer>();
            return con != null ? con.InstanceName : string.Empty;
        }

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
    }

    /// <summary>
    /// 关联实体对象的延迟加载器。
    /// </summary>
    internal class EntityPropertyLazyLoadder : PropertyLazyLoadderBase
    {
        public override PropertyValue GetValue(IEntity entity, IProperty property)
        {
            var entityProperty = property.As<EntityProperty>();

            var instanceName = GetInstanceName(entity, entityProperty.RelationalType);
            var environment = GetEnvironment(entity);

            using (var context = new InternalContext(instanceName))
            {
                context.As<IEntityPersistentEnvironment>(s => s.Environment = environment);
                context.As<IEntityPersistentInstanceContainer>(s => s.InstanceName = instanceName);

                var provider = context.CreateRepositoryProvider(entityProperty.RelationalType);
                var expression = BuidRelationExpression(entity, entityProperty);
                if (expression != null)
                {
                    var value = Execute(provider.Queryable, expression);

                    //设置实体所属的实体Owner
                    value.As<IEntityRelation>(e => e.Owner = new EntityOwner(entity, property));

                    return value == null ? PropertyValue.Empty : PropertyValue.NewValue(value);
                }

                return PropertyValue.Empty;
            }
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
                new Func<RelationshipKey, IProperty>(k => k.OtherProperty),
                new Func<RelationshipKey, IProperty>(k => k.ThisProperty));
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

    internal class EntitySetPropertyLazyLoadder : PropertyLazyLoadderBase
    {
        public override PropertyValue GetValue(IEntity entity, IProperty property)
        {
            var entityProperty = property.As<EntitySetProperty>();
            var instanceName = GetInstanceName(entity, entityProperty.RelationalType);
            var environment = GetEnvironment(entity);

            using (var context = new InternalContext(instanceName))
            {
                context.Environment = environment;
                context.InstanceName = instanceName;

                var provider = context.CreateRepositoryProvider(entityProperty.RelationalType);
                var expression = BuidRelationExpression(entity, entityProperty);
                object result = null;
                if (expression != null)
                {
                    result = Execute(provider.Queryable, expression);
                    var querySetType = typeof(EntitySet<>).MakeGenericType(entityProperty.RelationalType);
                    var list = querySetType.New(new[] { result });

                    //设置实体集所属的实体Owner
                    list.As<IEntityRelation>(e => e.Owner = new EntityOwner(entity, property));

                    return PropertyValue.NewValue(list);
                }

                return PropertyValue.Empty;
            }
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
                new Func<RelationshipKey, IProperty>(k => k.ThisProperty),
                new Func<RelationshipKey, IProperty>(k => k.OtherProperty));
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

            return queryable.Provider.Execute(expression);
        }
    }

    internal class ReferencePropertyLazyLoadder : PropertyLazyLoadderBase
    {
        public override PropertyValue GetValue(IEntity entity, IProperty property)
        {
            var referenceProperty = property.As<ReferenceProperty>();
            var instanceName = GetInstanceName(entity, referenceProperty.RelationalType);
            var environment = GetEnvironment(entity);

            using (var context = new InternalContext(instanceName))
            {
                context.As<IEntityPersistentEnvironment>(s => s.Environment = environment);
                context.As<IEntityPersistentInstanceContainer>(s => s.InstanceName = instanceName);

                var provider = context.CreateRepositoryProvider(referenceProperty.RelationalType);
                var expression = BuidRelationExpression(entity, referenceProperty);
                if (expression != null)
                {
                    var value = Execute(provider.Queryable, expression);
                    return value == null ? PropertyValue.Empty : PropertyValue.NewValue(value);
                }

                return PropertyValue.Empty;
            }
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
                new Func<RelationshipKey, IProperty>(k => k.OtherProperty),
                new Func<RelationshipKey, IProperty>(k => k.ThisProperty));
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
}
