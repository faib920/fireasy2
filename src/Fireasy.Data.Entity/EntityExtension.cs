﻿// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.Extensions;
using Fireasy.Common.Linq.Expressions;
using Fireasy.Data.Entity.Properties;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 实体的一些扩展方法。
    /// </summary>
    public static class EntityExtension
    {
        /// <summary>
        /// 为实体加载指定的具有延迟行为的属性的值，该属性为 <see cref="RelationProperty"/> 的子类，且 <see cref="LoadBehavior"/> 属性应设置为 <see cref="LoadBehavior.Lazy"/>。
        /// </summary>
        /// <param name="entity">当前的实体对象。</param>
        /// <param name="property">要进行加载的属性。</param>
        public static void Lazyload(this IEntity entity, IProperty property)
        {
            var value = EntityLazyloader.Load(entity, property);
            entity.InitializeValue(property, value);
        }

        /// <summary>
        /// 为实体集加载所有延迟加载的属性值。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entities">要加载的实体集。</param>
        /// <param name="filterAttribute">使用一个特性来标记需要过滤的属性。</param>
        public static void LazyLoad<TEntity>(this IEnumerable<TEntity> entities, Type filterAttribute = null) where TEntity : IEntity
        {
            EntityLazyloader.LoadEntities(entities.Cast<IEntity>(), filterAttribute);
        }

        /// <summary>
        /// 为实体集加载所有延迟加载的属性值。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity">要加载的实体。</param>
        /// <param name="filterAttribute">使用一个特性来标记需要过滤的属性。</param>
        public static void LazyLoad<TEntity>(this TEntity entity, Type filterAttribute = null) where TEntity : IEntity
        {
            EntityLazyloader.LoadEntities(new[] { entity as IEntity }, filterAttribute);
        }

        /// <summary>
        /// 获取实体被修改的属性列表。
        /// </summary>
        /// <param name="entity">当前的实体对象。</param>
        /// <returns></returns>
        public static string[] GetModifiedProperties(this IEntity entity)
        {
            if (entity.EntityState == EntityState.Attached ||
                entity.EntityState == EntityState.Modified)
            {
                return entity.GetModifiedProperties();
            }

            return new string[0];
        }

        /// <summary>
        /// 标记指定的属性是否已被修改。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="entity">当前的实体对象。</param>
        /// <param name="propertyExpression">被修改的属性的表达式。</param>
        /// <param name="modified">是否被修改。默认为 true。</param>
        /// <returns></returns>
        public static TEntity Modified<TEntity, TProperty>(this TEntity entity, Expression<Func<TEntity, TProperty>> propertyExpression, bool modified = true) where TEntity : IEntity
        {
            if (propertyExpression is LambdaExpression lambdaExp && lambdaExp.Body is MemberExpression memberExp)
            {
                entity.NotifyModified(memberExp.Member.Name, modified);
            }

            return entity;
        }

        /// <summary>
        /// 获取实体指定属性修改前的值。
        /// </summary>
        /// <param name="entity">当前的实体对象。</param>
        /// <param name="propertyName">属性名称。</param>
        /// <returns></returns>
        public static PropertyValue GetOldValue(this IEntity entity, string propertyName)
        {
            var property = PropertyUnity.GetProperty(entity.EntityType, propertyName);
            if (property == null)
            {
                return PropertyValue.Empty;
            }

            if (entity.EntityState == EntityState.Attached ||
                entity.EntityState == EntityState.Modified)
            {
                return entity.GetOldValue(property);
            }

            return entity.GetValue(property);
        }

        /// <summary>
        /// 获取实体指定属性修改前的值。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="entity">当前的实体对象。</param>
        /// <param name="propertyExpression">要获取值的属性表达式。</param>
        /// <returns></returns>
        public static PropertyValue GetOldValue<TEntity, TProperty>(this TEntity entity, Expression<Func<TEntity, TProperty>> propertyExpression) where TEntity : IEntity
        {
            if (propertyExpression is LambdaExpression lambdaExp && lambdaExp.Body is MemberExpression memberExp)
            {
                return GetOldValue(entity, memberExp.Member.Name);
            }

            throw new ArgumentException(nameof(propertyExpression));
        }

        /// <summary>
        /// 通过主键值使对象正常化。
        /// </summary>
        /// <param name="entity">当前的实体对象。</param>
        /// <param name="keyValues">主键值数组。</param>
        /// <returns></returns>
        public static TEntity Normalize<TEntity>(this TEntity entity, params PropertyValue[] keyValues) where TEntity : IEntity
        {
            var primaryKeys = PropertyUnity.GetPrimaryProperties(entity.EntityType).ToArray();
            if (primaryKeys.Length != 0 && keyValues == null ||
                primaryKeys.Length != keyValues.Length)
            {
                throw new Exception(SR.GetString(SRKind.DisaccordArgument, primaryKeys.Length, keyValues.Length));
            }

            for (var i = 0; i < primaryKeys.Length; i++)
            {
                entity.InitializeValue(primaryKeys[i], keyValues[i]);
            }

            entity.SetState(EntityState.Modified);

            return entity;
        }

        /// <summary>
        /// 设置实体的状态。
        /// </summary>
        /// <param name="entity">当前的实体对象。</param>
        /// <param name="state">要设置的状态。</param>
        public static void SetState(this IEntity entity, EntityState state)
        {
            if (state == EntityState.Unchanged)
            {
                entity.ResetUnchanged();
            }
            else
            {
                entity.SetState(state);
            }
        }

        /// <summary>
        /// 锁定实体执行一个方法，即当前实体修改期间，不能再次对该实体进行操作。
        /// </summary>
        /// <param name="entity">当前的实体对象。</param>
        /// <param name="action"></param>
        public static void TryLockModifing(this IEntity entity, Action action)
        {
            if (entity.IsModifyLocked)
            {
                return;
            }

            entity.IsModifyLocked = true;
            action?.Invoke();
            entity.IsModifyLocked = false;
        }

        /// <summary>
        /// 判断实体的属性是否修改。
        /// </summary>
        /// <param name="entity">当前的实体对象。</param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static bool IsModified(this IEntity entity, string propertyName)
        {
            if (entity.EntityState != EntityState.Unchanged)
            {
                return entity.IsModified(propertyName);
            }

            return false;
        }

        /// <summary>
        /// 判断实体的属性是否修改。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="entity">当前的实体对象。</param>
        /// <param name="propertyExpression">要判断的属性表达式。</param>
        /// <returns></returns>
        public static bool IsModified<TEntity, TProperty>(this TEntity entity, Expression<Func<TEntity, TProperty>> propertyExpression) where TEntity : IEntity
        {
            if (propertyExpression is LambdaExpression lambdaExp && lambdaExp.Body is MemberExpression memberExp)
            {
                return IsModified(entity, memberExp.Member.Name);
            }

            throw new ArgumentException(nameof(propertyExpression));
        }

        /// <summary>
        /// 获取定义的实体类型。
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static Type GetDefinitionEntityType(this Type entityType)
        {
            if (typeof(ICompiledEntity).IsAssignableFrom(entityType))
            {
                entityType = entityType.BaseType;
            }

            return entityType;
        }

        /// <summary>
        /// 获取实体的根实体类型。
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static Type GetRootEntityType(this Type entityType)
        {
            while (entityType.BaseType != null && !entityType.BaseType.IsAbstract &&
                !entityType.GetCustomAttributes<EntityMappingAttribute>().Any())
            {
                entityType = entityType.BaseType;
            }

            return entityType;
        }

        /// <summary>
        /// 获取实体的映射的实体类型。
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static Type GetMapEntityType(this Type entityType)
        {
            while (entityType.BaseType != null && !entityType.BaseType.IsAbstract &&
                !entityType.GetCustomAttributes<EntityMappingAttribute>().Any())
            {
                entityType = entityType.BaseType;
            }

            return entityType;
        }

        /// <summary>
        /// 依据一个属性生成一个 <see cref="DataColumn"/> 对象。
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static DataColumn ToDataColumn(this IProperty property)
        {
            if (property is RelationProperty)
            {
                return null;
            }

            var dataType = property.Type.GetNonNullableType();
            var column = new DataColumn(property.Info.ColumnName, dataType);

            //默认值
            if (!PropertyValue.IsEmpty(property.Info.DefaultValue))
            {
                column.DefaultValue = property.Info.DefaultValue.GetValue();
            }

            //长度 
            if (property.Type == typeof(string) && property.Info.Length != null)
            {
                column.MaxLength = (int)property.Info.Length;
            }

            //主键
            //if (property.Info.IsPrimaryKey)
            //{
            //    column.Unique = true;
            //}

            //可空
            column.AllowDBNull = property.Info.IsNullable;

            return column;
        }

        /// <summary>
        /// 通过一个 lambda 表达式将属性值绑定到实体对象中。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <param name="newExpression"></param>
        /// <returns></returns>
        public static TEntity InitByExpression<TEntity>(this TEntity entity, Expression<Func<TEntity>> newExpression) where TEntity : IEntity
        {
            return (TEntity)InitByExpression((IEntity)entity, newExpression);
        }

        /// <summary>
        /// 通过一个 <see cref="MemberInitExpression"/> 表达式将属性值绑定到实体对象中。
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="creator"></param>
        /// <returns></returns>
        public static IEntity InitByExpression(this IEntity entity, LambdaExpression creator)
        {
            if (creator.Body is NewExpression)
            {
                throw new InvalidOperationException(SR.GetString(SRKind.InvalidExpressionInit));
            }

            if (creator.Body is MemberInitExpression initExp)
            {
                if (initExp.NewExpression.Arguments.Count > 0)
                {
                    throw new InvalidOperationException(SR.GetString(SRKind.InvalidExpressionInit));
                }

                foreach (var bind in initExp.Bindings)
                {
                    if (bind as MemberAssignment == null)
                    {
                        continue;
                    }

                    var exp = PartialEvaluator.Eval((bind as MemberAssignment).Expression);
                    if (exp is ConstantExpression constExp)
                    {
                        entity.SetValue((bind as MemberAssignment).Member.Name, PropertyValue.NewValue(constExp.Value, (bind as MemberAssignment).Member.GetMemberType()));
                    }
                }
            }

            return entity;
        }

        /// <summary>
        /// 为实体对象应用默认值。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static TEntity ApplyDefaultValue<TEntity>(this TEntity entity) where TEntity : IEntity
        {
            return (TEntity)ApplyDefaultValue((IEntity)entity);
        }

        /// <summary>
        /// 为实体对象应用默认值。
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static IEntity ApplyDefaultValue(this IEntity entity)
        {
            foreach (var property in PropertyUnity.GetPersistentProperties(entity.EntityType))
            {
                if (!PropertyValue.IsEmpty(property.Info.DefaultValue))
                {
                    entity.InitializeValue(property, property.Info.DefaultValue.TryAllotValue(property.Type, property.Info.DefaultValueFormatter));
                }
            }

            return entity;
        }

        /// <summary>
        /// 如果对象实现了 <see cref="IEntityPersistentInstanceContainer"/> 接口，则会将 <paramref name="instanceName"/> 附加到该对象。
        /// </summary>
        /// <param name="item"></param>
        /// <param name="instanceName"></param>
        internal static T InitializeInstanceName<T>(this T item, string instanceName)
        {
            if (item == null)
            {
                return default;
            }

            if (item is IEntityPersistentInstanceContainer e)
            {
                e.InstanceName = instanceName;
            }

            return item;
        }

        /// <summary>
        /// 如果对象实现了 <see cref="IEntityPersistentEnvironment"/> 接口，则会将 <paramref name="environment"/> 附加到该对象；
        /// </summary>
        /// <param name="item"></param>
        /// <param name="environment"></param>
        internal static T InitializeEnvironment<T>(this T item, EntityPersistentEnvironment environment)
        {
            if (item == null)
            {
                return default;
            }

            if (item is IEntityPersistentEnvironment e && environment != null)
            {
                e.Environment = environment;
            }

            return item;
        }

        /// <summary>
        /// 判断实体是否实现了 <see cref="IEntity"/>。
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        internal static bool IsEntityType(this Type entityType)
        {
            return typeof(IEntity).IsAssignableFrom(entityType);
        }

        /// <summary>
        /// 判断实体类型有没有动态编译成代理类型。
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        internal static bool IsNotCompiled(this Type entityType)
        {
            if (typeof(ICompilableEntity).IsAssignableFrom(entityType) &&
                !typeof(ICompiledEntity).IsAssignableFrom(entityType))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 获取实例上的 <see cref="EntityPersistentEnvironment"/> 实例。
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        internal static EntityPersistentEnvironment GetEnvironment(this IEntity entity)
        {
            var env = entity.As<IEntityPersistentEnvironment>();
            return env?.Environment;
        }

        /// <summary>
        /// 从实体中取出 InstanceName，前提是实体实现了 <see cref="IEntityPersistentInstanceContainer"/> 接口。
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        internal static string GetInstanceName(this IEntity entity)
        {
            var con = entity.As<IEntityPersistentInstanceContainer>();
            return con != null ? con.InstanceName : string.Empty;
        }
    }
}
