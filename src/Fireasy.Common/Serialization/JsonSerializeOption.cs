// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;

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
            KeyHandling = JsonKeyHandling.Quote;
        }

        /// <summary>
        /// 获取或设置 Key 的输出处理。
        /// </summary>
        public JsonKeyHandling KeyHandling { get; set; }

        /// <summary>
        /// 引用另一个对象的设置属性。
        /// </summary>
        /// <param name="other"></param>
        public override void Reference(SerializeOption other)
        {
            base.Reference(other);

            if (other is JsonSerializeOption joption)
            {
                KeyHandling = joption.KeyHandling;
            }
        }
    }

    /// <summary>
    /// Key 的输出处理。
    /// </summary>
    public enum JsonKeyHandling
    {
        /// <summary>
        /// 无。
        /// </summary>
        None,
        /// <summary>
        /// 名称前后加双引号。
        /// </summary>
        Quote
    }
}
