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
    /// 表示实体的所有者属性。
    /// </summary>
    [Serializable]
    public sealed class EntityOwner
    {
        /// <summary>
        /// 初始化 <see cref="EntityOwner"/> 的新实例 。
        /// </summary>
        /// <param name="parent">实体的父对象。</param>
        /// <param name="property">关联的属性。</param>
        public EntityOwner(object parent, IProperty property)
        {
            Parent = parent;
            Property = property;
        }

        /// <summary>
        /// 获取或设置关联的属性。
        /// </summary>
        public IProperty Property { get; set; }

        /// <summary>
        /// 获取或设置实体的父对象。
        /// </summary>
        public object Parent { get; set; }
    }
}
