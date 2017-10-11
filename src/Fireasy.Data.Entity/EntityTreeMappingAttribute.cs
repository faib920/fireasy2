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
    /// 一个标识实体支持树型结构所需属性映射的特性。无法继承此类。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class EntityTreeMappingAttribute : Attribute
    {
        /// <summary>
        /// 初始化 <see cref="EntityTreeMappingAttribute"/> 类的新实例。
        /// </summary>
        public EntityTreeMappingAttribute()
        {
            NameSeparator = "\\";
            SignLength = 4;
        }

        /// <summary>
        /// 获取或设置内部标记(必须的)。
        /// </summary>
        public string InnerSign { get; set; }

        /// <summary>
        /// 获取或设置级别(必须的)。
        /// </summary>
        public string Level { get; set; }

        /// <summary>
        /// 获取或设置排序(必须的)。
        /// </summary>
        public string Order { get; set; }

        /// <summary>
        /// 获取或设置名称。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 获取或设置全名。
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// 获取或设置全名的名称分隔符。
        /// </summary>
        public string NameSeparator { get; set; }

        /// <summary>
        /// 获取或设置标记的长度，默认为4位。
        /// </summary>
        public int SignLength { get; set; }

        /// <summary>
        /// 获取或设置每一级标记的位数。
        /// </summary>
        public int[] SignBits { get; set; }

        /// <summary>
        /// 获取或设置标记的编码方式。
        /// </summary>
        public SignStyle SignStyle { get; set; }
    }

    /// <summary>
    /// 标记的编码方式。
    /// </summary>
    public enum SignStyle
    {
        /// <summary>
        /// 固定位数。
        /// </summary>
        Fixed,
        /// <summary>
        /// 每级的位数可变。
        /// </summary>
        Variable
    }
}
