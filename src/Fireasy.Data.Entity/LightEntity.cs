// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using System;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 保护的实体类型。
    /// </summary>
    [Serializable]
    public abstract class ProtectedEntity : EntityObject
    {
        /// <summary>
        /// 使用保护方式获取属性的值。
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        protected PropertyValue ProtectGetValue(IProperty property)
        {
            return base.GetValue(property);
        }

        /// <summary>
        /// 使用保护方式设置属性的值。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        protected void ProtectSetValue(IProperty property, PropertyValue value)
        {
            base.SetValue(property, value);
        }

        /// <summary>
        /// 初始化属性的值。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        protected void ProtectInitializeValue(IProperty property, PropertyValue value)
        {
            base.InitializeValue(property, value);
        }
    }

    /// <summary>
    /// 轻量级的数据实体，继承此类不需要显式定义 <see cref="IProperty"/> 。该类型基于 动态编译 实现，属性必须使用 Virtual 声明。
    /// </summary>
    /// <typeparam name="TEntity">实体类型。</typeparam>
    [Serializable]
    public abstract class LightEntity<TEntity> : ProtectedEntity,
        ICompilableEntity
        where TEntity : EntityObject
    {
        /// <summary>
        /// 构造一个代理对象。
        /// </summary>
        /// <returns></returns>
        public static TEntity New()
        {
            return New(false);
        }

        /// <summary>
        /// 构造一个代理对象。
        /// </summary>
        /// <param name="applyDefaultValue">是否应用默认值。</param>
        /// <returns></returns>
        public static TEntity New(bool applyDefaultValue)
        {
            var proxyType = EntityProxyManager.GetType(typeof(TEntity));
            var entity = proxyType.New<TEntity>();

            if (applyDefaultValue)
            {
                return (TEntity)entity.ApplyDefaultValue();
            }

            return entity;
        }

        /// <summary>
        /// 通过 <see cref="MemberInitExpression"/> 表达式来构造一个代理对象。
        /// </summary>
        /// <param name="initExp">一个成员绑定的表达式。</param>
        /// <returns></returns>
        public static TEntity Wrap(Expression<Func<TEntity>> initExp)
        {
            return Wrap(initExp, false);
        }

        /// <summary>
        /// 通过 <see cref="MemberInitExpression"/> 表达式来构造一个代理对象。
        /// </summary>
        /// <param name="initExp">一个成员绑定的表达式。</param>
        /// <param name="applyDefaultValue">是否应用默认值。</param>
        /// <returns></returns>
        public static TEntity Wrap(Expression<Func<TEntity>> initExp, bool applyDefaultValue)
        {
            var entity = New(applyDefaultValue);
            entity.InitByExpression(initExp);
            return entity;
        }

        /// <summary>
        /// 获取属性值。
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public override PropertyValue GetValue(IProperty property)
        {
            var value = property.Info.ReflectionInfo.GetValue(this, null);
            return value == null ? PropertyValue.Empty : PropertyValue.NewValue(value, property.Type);
        }

        /// <summary>
        /// 设置属性值。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        public override void SetValue(IProperty property, PropertyValue value)
        {
            property.Info.ReflectionInfo.SetValue(this, value.GetValue(), null);
        }

        /// <summary>
        /// 初始化属性的值。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        public override void InitializeValue(IProperty property, PropertyValue value)
        {
            this.SetValue(property, value);
        }
    }
}
