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
    /// 用于描述实体属性的信息。
    /// </summary>
    public interface IProperty
    {
        /// <summary>
        /// 获取或设置属性的名称。
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// 获取或设置属性的类型。
        /// </summary>
        Type Type { get; set; }

        /// <summary>
        /// 获取或设置实体类型。
        /// </summary>
        Type EntityType { get; set; }

        /// <summary>
        /// 获取或设置属性的映射信息。
        /// </summary>
        PropertyMapInfo Info { get; set; }
    }
}
