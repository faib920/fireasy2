// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Extensions;
using Fireasy.Common.Security;
using System;
using System.Linq;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 用于管理上下文实例。
    /// </summary>
    public class ContextInstanceManager
    {
        private static readonly SafetyDictionary<string, IInstanceIdentifier> _instances = new SafetyDictionary<string, IInstanceIdentifier>();

        /// <summary>
        /// 析构函数。
        /// </summary>
        ~ContextInstanceManager()
        {
            foreach (var ident in _instances)
            {
                if (ident.Value.ServiceProvider != null)
                {
                    ident.Value.ServiceProvider.TryDispose();
                }
            }

            _instances.Clear();
        }

        /// <summary>
        /// 尝试从管理器里获取与实例名对应的 <see cref="IInstanceIdentifier"/> 实例。
        /// </summary>
        /// <param name="instanceName"></param>
        /// <returns></returns>
        public static IInstanceIdentifier TryGet(string instanceName)
        {
            if (string.IsNullOrEmpty(instanceName))
            {
                return null;
            }

            if (_instances.TryGetValue(instanceName, out IInstanceIdentifier value))
            {
                return value;
            }

            return null;
        }

        /// <summary>
        /// 尝试将 <see cref="IInstanceIdentifier"/> 实例添加到管理器，并返回一个实例名。如果管理器中已有该实例，则返回原有的实例名。
        /// </summary>
        /// <param name="identification"></param>
        /// <returns></returns>
        public static string TryAdd(IInstanceIdentifier identification)
        {
            lock (_instances)
            {
                var item = _instances.FirstOrDefault(s => s.Value.Equals(identification));
                if (!string.IsNullOrEmpty(item.Key))
                {
                    return item.Key;
                }

                var key = RandomGenerator.Create();
                _instances.TryAdd(key, identification);

                return key;
            }
        }
    }
}
