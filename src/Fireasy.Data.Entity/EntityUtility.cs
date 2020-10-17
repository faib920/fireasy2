﻿// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.Extensions;
using Fireasy.Data.Entity.Properties;
using System;

namespace Fireasy.Data.Entity
{
    internal static class EntityUtility
    {
        /// <summary>
        /// 改为移除状态，确定该该实体删除。
        /// </summary>
        /// <param name="value"></param>
        internal static void SetEntityToNull(PropertyValue value)
        {
            if (PropertyValue.IsEmpty(value))
            {
                return;
            }
            value.GetValue().As<IEntity>(entity => entity.SetState(EntityState.Detached));
        }

        /// <summary>
        /// 清空并标识，将删除实体集中的所有对象。
        /// </summary>
        /// <param name="value"></param>
        internal static void SetEntitySetToNull(PropertyValue value)
        {
            if (PropertyValue.IsEmpty(value))
            {
                return;
            }
            value.GetValue().As<IEntitySet>(e => e.IsNeedClear = true);
        }

        /// <summary>
        /// 检查关联属性的空值，如果属性值已设置为null，则不应显示给客户端。
        /// </summary>
        /// <param name="property">要检查的属性。</param>
        /// <param name="value">属性的值。</param>
        /// <returns></returns>
        internal static PropertyValue CheckReturnValue(IProperty property, PropertyValue value)
        {
            var relationPro = property.As<RelationProperty>();
            if (relationPro == null || PropertyValue.IsEmpty(value))
            {
                return value;
            }
            switch (relationPro.RelationalPropertyType)
            {
                case RelationPropertyType.Entity:
                    return CheckReturnEntityValue(value.GetValue().As<IEntity>(), value);
                case RelationPropertyType.EntitySet:
                    return CheckReturnEntitySetValue(value.GetValue() as IEntitySet, value);
                default:
                    return value;
            }
        }

        /// <summary>
        /// 检查返回的实体值是否为空值。
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static PropertyValue CheckReturnEntityValue(IEntity entity, PropertyValue value)
        {
            //实体的状态为 Detached，认为是设置为 null
            //var order = new Order();
            //order.Product = null;
            //此时可将对应的实体删除
            return entity != null ? (entity.EntityState == EntityState.Detached ? PropertyValue.Empty : value) : value;
        }

        /// <summary>
        /// 检查返回的实体集的值是否为空值。
        /// </summary>
        /// <param name="entitySet"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static PropertyValue CheckReturnEntitySetValue(IEntitySet entitySet, PropertyValue value)
        {
            //实体集的 SetClearFlag 为true，认为是设置为 null
            //var product = new Product ();
            //product.Orders = null;
            //此时可以将整个实体集清空
            return entitySet != null && entitySet.IsNeedClear ? PropertyValue.Empty : value;
        }

        /// <summary>
        /// 检查主键值是否允许修改。
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="property"></param>
        internal static void CheckPrimaryProperty(IEntity entity, IProperty property)
        {
            //已经持久化后的实体的主键值不能修改
            if (entity.EntityState != EntityState.Attached && property.Info.IsPrimaryKey)
            {
                throw new PrimaryPropertyUpdateException(property);
            }
        }

        /// <summary>
        /// 获取使用主键值设置其他实体外键的方法。
        /// </summary>
        /// <param name="elementType"></param>
        /// <returns></returns>
        private static Action<IEntity, IProperty, PropertyValue> GetAttachPrimaryKeyAction(Type elementType)
        {
            return (e, p, v) => e.SetValue(p, v);
        }

        internal static void UpdateFromReference(IProperty property, IEntity entity, PropertyValue value)
        {
            var relationPro = property.As<RelationProperty>();
            if (relationPro == null || PropertyValue.IsEmpty(value) ||
                relationPro.RelationalPropertyType != RelationPropertyType.Entity)
            {
                return;
            }
            var refEntity = value.GetValue().As<IEntity>();
            var relationKey = RelationshipUnity.GetRelationship(relationPro);
            if (relationKey == null)
            {
                return;
            }
            IEntity descEntity;
            IEntity sourceEntity;
            if (relationKey.Style == RelationshipStyle.One2Many)
            {
                descEntity = entity;
                sourceEntity = refEntity;
            }
            else
            {
                descEntity = refEntity;
                sourceEntity = entity;
            }
            foreach (var key in relationKey.Keys)
            {
                descEntity.SetValue(key.DependentProperty, sourceEntity.GetValue(key.PrincipalProperty));
            }
        }
    }
}
