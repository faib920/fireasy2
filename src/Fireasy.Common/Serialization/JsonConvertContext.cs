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
    /// Json转换器上下文，用于序列化和反序列化时识别属性及值。
    /// </summary>
    public class JsonConvertContext : Scope<JsonConvertContext>
    {
        /// <summary>
        /// 获取序列化的键名称。
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// 获取序列化的值。
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        /// 分配键值，及处理方法。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="action"></param>
        public void Assign(string key, object value, Action action)
        {
            if (action == null)
            {
                return;
            }

            Key = key;
            Value = value;
            action();
            Key = string.Empty;
            Value = null;
        }
    }
}
