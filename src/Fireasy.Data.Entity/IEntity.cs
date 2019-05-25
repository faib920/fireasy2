// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 实体的接口。
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// 获取实体类型。
        /// </summary>
        Type EntityType { get; }

        /// <summary>
        /// 获取指定属性的值。
        /// </summary>
        /// <param name="propertyName">实体属性名称。</param>
        /// <returns></returns>
        PropertyValue GetValue(string propertyName);

        /// <summary>
        /// 设置指定属性的值。
        /// </summary>
        /// <param name="propertyName">实体属性名称。</param>
        /// <param name="value">要设置的值。</param>
        void SetValue(string propertyName, PropertyValue value);

        /// <summary>
        /// 获取指定属性的值。
        /// </summary>
        /// <param name="property">实体属性。</param>
        /// <returns></returns>
        PropertyValue GetValue(IProperty property);

        /// <summary>
        /// 设置指定属性的值。
        /// </summary>
        /// <param name="property">实体属性。</param>
        /// <param name="value">要设置的值。</param>
        void SetValue(IProperty property, PropertyValue value);

        /// <summary>
        /// 获取实体的状态。
        /// </summary>
        EntityState EntityState { get; }

        /// <summary>
        /// 将实体修改为指定的状态。
        /// </summary>
        /// <param name="state">新的状态。</param>
        void SetState(EntityState state);

        /// <summary>
        /// 重置实体的状态。
        /// </summary>
        void ResetUnchanged();

        /// <summary>
        /// 获取已经修改的属性名称数组。
        /// </summary>
        /// <returns></returns>
        string[] GetModifiedProperties();

        /// <summary>
        /// 获取属性修改前的值。
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        PropertyValue GetOldValue(IProperty property);

        /// <summary>
        /// 直接获取属性的值。
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        PropertyValue GetDirectValue(IProperty property);

        /// <summary>
        /// 初始化属性的值。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        void InitializeValue(IProperty property, PropertyValue value);

        /// <summary>
        /// 是否处于修改锁定状态。
        /// </summary>
        /// <returns></returns>
        bool IsModifyLocked { get; set; }

        /// <summary>
        /// 通知属性已被修改。
        /// </summary>
        /// <param name="propertyName">属性名称。</param>
        /// <param name="modified">是否被修改。默认为 true。</param>
        void NotifyModified(string propertyName, bool modified = true);

        /// <summary>
        /// 判断是否已经修改。
        /// </summary>
        /// <param name="propertyName">属性名称。</param>
        /// <returns></returns>
        bool IsModified(string propertyName);

        /// <summary>
        /// 克隆出一个新的实体对象。
        /// </summary>
        /// <param name="dismodified">如果为 true，将丢弃实体被修改后的属性值，沿用原来的值。</param>
        /// <returns></returns>
        IEntity Clone(bool dismodified = false);
    }

    /// <summary>
    /// 表示可需要编译的实体。
    /// </summary>
    public interface ICompilableEntity
    {
    }

    /// <summary>
    /// 表示动态编译的实体。
    /// </summary>
    public interface ICompiledEntity
    {
    }
}
