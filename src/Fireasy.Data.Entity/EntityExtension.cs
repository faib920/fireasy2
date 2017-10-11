// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common;
using Fireasy.Common.Extensions;
using Fireasy.Common.Linq.Expressions;
using Fireasy.Data.Entity.Properties;
using Fireasy.Data.RecordWrapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

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
            entity.InitializateValue(property, value);
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
        /// 标记指定的属性已被修改。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="entity">当前的实体对象。</param>
        /// <param name="propertyExpression">被修改的属性的表达式。</param>
        /// <returns></returns>
        public static TEntity Modified<TEntity, TProperty>(this TEntity entity, Expression<Func<TEntity, TProperty>> propertyExpression) where TEntity : IEntity
        {
            var lambdaExp = propertyExpression as LambdaExpression;
            if (lambdaExp == null)
            {
                return entity;
            }

            var memberExp = lambdaExp.Body as MemberExpression;
            if (memberExp == null)
            {
                return entity;
            }

            entity.NotifyModified(memberExp.Member.Name);

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
            if (entity.EntityState == EntityState.Attached ||
                entity.EntityState == EntityState.Modified)
            {
                var property = PropertyUnity.GetProperty(entity.GetType(), propertyName);
                if (property != null)
                {
                    return entity.GetOldValue(property);
                }
            }

            return PropertyValue.Empty;
        }

        /// <summary>
        /// 通过主键值使对象正常化。
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="keyValues">主键值数组。</param>
        /// <returns></returns>
        public static T Normalize<T>(this T entity, params PropertyValue[] keyValues) where T : IEntity
        {
            var primaryKeys = PropertyUnity.GetPrimaryProperties(entity.GetType()).ToArray();
            if (primaryKeys.Length != 0 && keyValues == null ||
                primaryKeys.Length != keyValues.Length)
            {
                throw new Exception(SR.GetString(SRKind.DisaccordArgument, primaryKeys.Length, keyValues.Length));
            }

            for (var i = 0; i < primaryKeys.Length; i++)
            {
                entity.InitializateValue(primaryKeys[i], keyValues[i]);
            }

            entity.SetState(EntityState.Modified);

            return entity;
        }

        /// <summary>
        /// 设置实体的状态。
        /// </summary>
        /// <param name="entity">要设置状态的实体。</param>
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
        /// <param name="entity"></param>
        /// <param name="action"></param>
        public static void TryLockModifing(this IEntity entity, Action action)
        {
            if (entity.IsModifyLocked)
            {
                return;
            }

            entity.IsModifyLocked = true;
            action();
            entity.IsModifyLocked = false;
        }

        /// <summary>
        /// 判断实体的属性是否修改。
        /// </summary>
        /// <param name="entity"></param>
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
        /// 获取实体的根实体类型。
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static Type GetRootType(this Type entityType)
        {
            while (true)
            {
                if (!entityType.BaseType.IsDefined<EntityMappingAttribute>() ||
                    !entityType.BaseType.IsDirectImplementInterface(typeof(IEntity)))
                {
                    break;
                }

                entityType = entityType.BaseType;
            }

            return entityType;
        }

        /// <summary>
        /// 获取定义的实体类型。
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static Type GetDefinitionEntityType(this Type entityType)
        {
            while (!entityType.BaseType.IsAbstract && entityType.BaseType != typeof(object))
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
            var column = new DataColumn(property.Info.FieldName, dataType);

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
        /// 通过一个 <see cref="MemberInitExpression"/> 表达式将属性值绑定到实体对象中。
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        public static IEntity InitByExpression(this IEntity entity, LambdaExpression factory)
        {
            if (factory.Body is MemberInitExpression initExp)
            {
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
        /// 执行一个查询，返回一个序列。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="database"></param>
        /// <param name="queryCommand"></param>
        /// <param name="segment"></param>
        /// <param name="parameters"></param>
        /// <param name="initializer"></param>
        /// <param name="setter"></param>
        /// <returns></returns>
        internal static IEnumerable<T> InternalExecuteEnumerable<T>(this IDatabase database, IQueryCommand queryCommand, IDataSegment segment = null, ParameterCollection parameters = null, Action<object> initializer = null, Action<IDataReader, T> setter = null)
        {
            var mapper = RowMapperFactory.CreateMapper<T>();
            Guard.ArgumentNull(mapper, "mapper");

            mapper.RecordWrapper = database.Provider.GetService<IRecordWrapper>();
            mapper.Initializer = initializer;

            using (var reader = database.ExecuteReader(queryCommand, segment, parameters))
            {
                while (reader.Read())
                {
                    var item = mapper.Map(reader);
                    setter?.Invoke(reader, item);

                    yield return item;
                }
            }
        }

        /// <summary>
        /// 执行一个查询，返回一个序列。
        /// </summary>
        /// <param name="database"></param>
        /// <param name="elementType"></param>
        /// <param name="queryCommand"></param>
        /// <param name="segment"></param>
        /// <param name="parameters"></param>
        /// <param name="initializer"></param>
        /// <param name="setter"></param>
        /// <returns></returns>
        internal static IEnumerable InternalExecuteEnumerable(this IDatabase database, Type elementType, IQueryCommand queryCommand, IDataSegment segment = null, ParameterCollection parameters = null, Action<object> initializer = null, Action<IDataReader, object> setter = null)
        {
            var mapper = RowMapperFactory.CreateMapper(elementType);
            Guard.ArgumentNull(mapper, "mapper");

            mapper.RecordWrapper = database.Provider.GetService<IRecordWrapper>();
            mapper.Initializer = initializer;

            using (var reader = database.ExecuteReader(queryCommand, segment, parameters))
            {
                while (reader.Read())
                {
                    var item = mapper.Map(reader);
                    setter?.Invoke(reader, item);

                    yield return item;
                }
            }
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
                return default(T);
            }

            item.As<IEntityPersistentInstanceContainer>(e =>
                    e.InstanceName = instanceName);

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
                return default(T);
            }

            item.As<IEntityPersistentEnvironment>(e =>
                {
                    if (environment != null)
                    {
                        e.Environment = environment;
                    }
                });

            return item;
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
    }
}
