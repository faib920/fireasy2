// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data;

namespace Fireasy.Data.Schema
{
    /// <summary>
    /// 用于存储限制值的字典。无法继承此类。
    /// </summary>
    public sealed class RestrictionDictionary : Dictionary<string, object>
    {
        public static RestrictionDictionary Empty = new RestrictionDictionary();

        /// <summary>
        /// 使用限制值参数化 <see cref="ParameterCollection"/> 对象。
        /// </summary>
        /// <param name="parameters"><see cref="ParameterCollection"/> 对象。</param>
        /// <param name="parameterName">参数名称。</param>
        /// <param name="propertyName">限制值的名称。</param>
        /// <param name="dbType">数据类型。</param>
        /// <returns></returns>
        public RestrictionDictionary Parameterize(ParameterCollection parameters, string parameterName, string propertyName, DbType dbType = DbType.String)
        {
            parameters.Add(parameterName, string.Empty, ContainsKey(propertyName) ? this[propertyName] : DBNull.Value, dbType, null, ParameterDirection.Input);
            return this;
        }

        /// <summary>
        /// 尝试从字典里拿到不为 null 的字符串值。
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(string name, out string value)
        {
            if (TryGetValue(name, out object obj))
            {
                if (obj != null)
                {
                    value = obj.ToString();
                    return true;
                }
            }

            value = null;
            return false;
        }

        public string GetValue(string name)
        {
            if (TryGetValue(name, out object obj))
            {
                if (obj != null)
                {
                    return obj.ToString();
                }
            }

            return null;
        }
    }
}
