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
    /// 指定实体是否使用数据缓存。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class EntityCachableAttribute : Attribute
    {
        public EntityCachableAttribute(bool enabled)
        {
            Enabled = enabled;
        }

        /// <summary>
        /// 获取或设置是否启用缓存。
        /// </summary>
        public bool Enabled { get; set; }
    }
}
