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
    public class ContextInstanceManager : DisposableBase
    {
        public readonly static ContextInstanceManager Default = new ContextInstanceManager();

        private readonly SafetyDictionary<string, IInstanceIdentifier> _instances = new SafetyDictionary<string, IInstanceIdentifier>();

        protected override bool Dispose(bool disposing)
        {
            foreach (var ident in _instances)
            {
                if (ident.Value.ServiceProvider != null)
                {
                    ident.Value.ServiceProvider.TryDispose();
                }
            }

            _instances.Clear();

            return base.Dispose(disposing);
        }

        /// <summary>
        /// 尝试从管理器里获取与实例名对应的 <see cref="IInstanceIdentifier"/> 实例。
        /// </summary>
        /// <param name="instanceName"></param>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public IInstanceIdentifier TryGet(string instanceName, Type entityType = null)
        {
            if (string.IsNullOrEmpty(instanceName))
            {
                if (entityType != null)
                {
                    var match = _instances.FirstOrDefault(s => EntityRepositoryDiscoveryService.TryGetInitializerPair(s.Value.ContextType)?.Contains(entityType) == true); 
                    if (match.Value != null)
                    {
                        return match.Value;
                    }
                }

                return _instances.Count == 1 ? _instances.First().Value : null;
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
        public string TryAdd(IInstanceIdentifier identification)
        {
            lock (_instances)
            {
                var item = _instances.FirstOrDefault(s => s.Value.Equals(identification));
                if (!string.IsNullOrEmpty(item.Key))
                {
                    return item.Key;
                }

                var key = RandomGenerator.Create();
                _instances.TryAdd(key, identification.Clone());

                return key;
            }
        }
    }
}
