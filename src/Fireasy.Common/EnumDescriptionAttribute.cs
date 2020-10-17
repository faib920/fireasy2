// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.ComponentModel;

namespace Fireasy.Common
{
    /// <summary>
    /// 为枚举值添加文本说明。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class EnumDescriptionAttribute : DescriptionAttribute
    {
        /// <summary>
        /// 初始化 <see cref="EnumDescriptionAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="description">说明文本。</param>
        public EnumDescriptionAttribute(string description)
            : base(description)
        {
        }

        /// <summary>
        /// 初始化 <see cref="EnumDescriptionAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="flags">标志位。</param>
        /// <param name="description">说明文本。</param>
        public EnumDescriptionAttribute(int flags, string description)
            : base(description)
        {
            Flags = flags;
        }

        /// <summary>
        /// 获取或设置标志位。
        /// </summary>
        public int Flags { get; set; }
    }
}
