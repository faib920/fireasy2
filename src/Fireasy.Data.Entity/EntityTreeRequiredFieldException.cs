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
    /// 使用 <see cref="EntityTreeMappingAttribute"/> 建立树映射关系时，如果缺少必要的属性时，将引发此异常。无法继承此类。
    /// </summary>
    public sealed class EntityTreeRequiredFieldException : Exception
    {
        /// <summary>
        /// 初始化 <see cref="EntityTreeRequiredFieldException"/> 类的新实例。
        /// </summary>
        /// <param name="entityType">实体的类型。</param>
        /// <param name="propertyName">所需的属性。</param>
        public EntityTreeRequiredFieldException(Type entityType, string propertyName)
            : base(SR.GetString(SRKind.EntityTreeRequiredField, entityType, propertyName))
        {
            EntityType = entityType;
            PropertyName = propertyName;
        }

        /// <summary>
        /// 获取实体的类型。
        /// </summary>
        public Type EntityType { get; private set; }

        /// <summary>
        /// 获取所需的属性名称。
        /// </summary>
        public string PropertyName { get; private set; }
    }
}
