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
    /// 延迟载入实体的部份属性时引发的异常。无法继承此类。
    /// </summary>
    public sealed class EntityLazyloadException : Exception
    {
        /// <summary>
        /// 初始化 <see cref="EntityLazyloadException"/> 类的新实例。
        /// </summary>
        /// <param name="message">指定此异常的信息。</param>
        /// <param name="entity">当前的实体对象。</param>
        /// <param name="property">当前加载的属性。</param>
        public EntityLazyloadException(string message, IEntity entity, IProperty property)
            : base(message)
        {
            Entity = entity;
            Property = property;
        }

        /// <summary>
        /// 获取当前的实体对象。
        /// </summary>
        public IEntity Entity { get; }

        /// <summary>
        /// 获取当前加载的属性。
        /// </summary>
        public IProperty Property { get; }
    }
}
