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
    /// 使用 <see cref="EntityTreeMappingAttribute"/> 建立树映射关系时，如果其中属性定义的类型不满足实体树持久要求时，将引发此异常。
    /// 比如属性 Level、Order 只接受 <see cref="Int32"/> 类型的支持。无法继承此类。
    /// </summary>
    public sealed class EntityTreeFieldTypeException : Exception
    {
        /// <summary>
        /// 初始化 <see cref="EntityTreeFieldTypeException"/> 类的新实例。
        /// </summary>
        /// <param name="entityType">实体的类型。</param>
        /// <param name="property">用于标记树映射关系的实体属性。</param>
        /// <param name="requiredType">属性所需的类型。</param>
        public EntityTreeFieldTypeException(Type entityType, IProperty property, Type requiredType)
            : base(SR.GetString(SRKind.FaildInEntityTreeFieldType, entityType, property.Name, requiredType))
        {
            EntityType = entityType;
            Property = property;
            RequiredType = requiredType;
        }

        /// <summary>
        /// 获取实体的类型。
        /// </summary>
        public Type EntityType { get; }

        /// <summary>
        /// 获取用于标记树映射关系的实体属性。
        /// </summary>
        public IProperty Property { get; }

        /// <summary>
        /// 获取所需的属性的类型。
        /// </summary>
        public Type RequiredType { get; }
    }
}
