// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;

namespace Fireasy.Common.Serialization
{
    /// <summary>
    /// 序列化选项。
    /// </summary>
    public class SerializeOption
    {
        /// <summary>
        /// 初始化 <see cref="JsonSerializeOption"/> 类的新实例。
        /// </summary>
        public SerializeOption()
        {
            IgnoreType = true;
            Indent = true;
            Converters = new ConverterList();
        }

        /// <summary>
        /// 获取或设置是否对属性名使用 Camel 语法命名规则。默认为 false。
        /// </summary>
        public bool CamelNaming { get; set; }

        /// <summary>
        /// 获取或设置可包含的属性名数组。
        /// </summary>
        public string[] InclusiveNames { get; set; }

        /// <summary>
        /// 获取或设置要排除的属性名数组。
        /// </summary>
        public string[] ExclusiveNames { get; set; }

        /// <summary>
        /// 获取或设置序列化的转换器。
        /// </summary>
        public ConverterList Converters { get; set; }

        /// <summary>
        /// 获取或设置是否忽略 <see cref="Type"/> 类型的属性。
        /// </summary>
        public bool IgnoreType { get; set; }

        /// <summary>
        /// 获取或设置是否缩进。
        /// </summary>
        public bool Indent { get; set; }
    }
}
