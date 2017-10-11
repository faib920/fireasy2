// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace Fireasy.Common.Serialization
{
    /// <summary>
    /// Json 序列化的控制选项。
    /// </summary>
    public class JsonSerializeOption : SerializeOption
    {
        /// <summary>
        /// 初始化 <see cref="JsonSerializeOption"/> 类的新实例。
        /// </summary>
        public JsonSerializeOption()
        {
            Indent = false;
            Format = JsonFormat.String;
        }

        /// <summary>
        /// 获取或设置Json序列化的格式。
        /// </summary>
        public JsonFormat Format { get; set; }

        /// <summary>
        /// 获取或设置是否忽略为 null 的值。
        /// </summary>
        public bool IgnoreNull { get; set; }
    }
}
