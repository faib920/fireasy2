// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace Fireasy.Common.Configuration
{
    /// <summary>
    /// 表示配置项的集合。无法继承此类。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public sealed class ConfigurationSettings<T> : Dictionary<string, T> where T : IConfigurationSettingItem
    {
        private readonly IDictionary<string, Exception> _errors = new Dictionary<string, Exception>();

        /// <summary>
        /// 添加一个无效的配置项，并保存其异常信息。
        /// </summary>
        /// <param name="key">配置项的键名。</param>
        /// <param name="exception">异常信息。</param>
        public void AddInvalidSetting(string key, Exception exception)
        {
            Add(key, new InvalidInstanceWrap<T>().Instance);
            _errors.Add(key, exception);
        }

        /// <summary>
        /// 索引器，通过键名访问集合中的配置项。
        /// </summary>
        /// <param name="key">键名。</param>
        /// <returns></returns>
        public new T this[string key]
        {
            get
            {
                if (!ContainsKey(key))
                {
                    return default;
                }

                if (_errors.ContainsKey(key))
                {
                    throw _errors[key];
                }

                return base[key];
            }
        }
    }
}