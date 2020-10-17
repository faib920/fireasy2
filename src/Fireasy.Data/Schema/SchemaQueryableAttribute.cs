// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Data.Schema
{
    /// <summary>
    /// 指示属性在架构限制数组查询中的索引。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public sealed class SchemaQueryableAttribute : Attribute
    {
        /// <summary>
        /// 初始化 <see cref="SchemaQueryableAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="index">索引值。</param>
        /// <param name="providerType">所支持的数据库提供者类型。</param>
        public SchemaQueryableAttribute(int index, string providerType)
        {
            ProviderType = providerType;
            Index = index;
        }

        /// <summary>
        /// 获取或设置数据提供者类别。
        /// </summary>
        public string ProviderType { get; set; }

        /// <summary>
        /// 获取或设置索引值。
        /// </summary>
        public int Index { get; set; }
    }
}
