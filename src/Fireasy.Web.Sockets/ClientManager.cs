// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;
using Fireasy.Common.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fireasy.Web.Sockets
{
    /// <summary>
    /// 客户端管理器。
    /// </summary>
    public class ClientManager
    {
        private static ConcurrentDictionary<Type, ClientManager> managers = new ConcurrentDictionary<Type, ClientManager>();
        private ConcurrentDictionary<string, IClientProxy> clients = new ConcurrentDictionary<string, IClientProxy>();
        private ConcurrentDictionary<string, List<string>> groups = new ConcurrentDictionary<string, List<string>>();

        internal static ClientManager GetManager(Type handlerType, WebSocketBuildOption option)
        {
            return managers.GetOrAdd(handlerType, k => option.Distributed ? new DistributedClientManager(option.AliveKey) : new ClientManager());
        }

        public virtual void Add(string connectionId, IClientProxy handler)
        {
            clients.TryAdd(connectionId, handler);
        }

        public virtual void AddToGroup(string connectionId, string groupName)
        {
            var group = groups.GetOrAdd(groupName, k => new List<string>());
            group.Add(connectionId);
        }

        public virtual void Remove(string connectionId)
        {
            if (clients.TryRemove(connectionId, out IClientProxy client))
            {
                client.TryDispose();
            }
        }

        public virtual void RemoveFromGroup(string connectionId, string groupName)
        {
            if (groups.ContainsKey(groupName))
            {
                groups[groupName].Remove(connectionId);
            }
        }

        /// <summary>
        /// 获取指定客户端连接标识的代理。
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        public virtual IClientProxy Client(string connectionId)
        {
            if (clients.TryGetValue(connectionId, out IClientProxy client))
            {
                return client;
            }

            return NullClientProxy.Instance;
        }

        /// <summary>
        /// 获取指定的多个客户端连接标识的代理。
        /// </summary>
        /// <param name="connectionIds"></param>
        /// <returns></returns>
        public virtual IClientProxy Clients(params string[] connectionIds)
        {
            Guard.ArgumentNull(connectionIds, nameof(connectionIds));

            return new EnumerableClientProxy(() => clients.Where(s => connectionIds.Contains(s.Key)).Select(s => s.Value));
        }

        /// <summary>
        /// 获取所有客户端代理。
        /// </summary>
        public virtual IClientProxy All
        {
            get
            {
                return new EnumerableClientProxy(() => clients.Values);
            }
        }

        /// <summary>
        /// 获取指定分组的所有客户端代理。
        /// </summary>
        /// <param name="groupName">组的名称。</param>
        /// <returns></returns>
        public virtual IClientProxy Group(string groupName)
        {
            if (groups.ContainsKey(groupName))
            {
                return new EnumerableClientProxy(() => clients.Where(s => groups[groupName].Contains(s.Key)).Select(s => s.Value));
            }

            return NullClientProxy.Instance;
        }
    }

    internal class EnumerableClientProxy : IClientProxy
    {
        private Func<IEnumerable<IClientProxy>> proxyFactory;

        public EnumerableClientProxy(Func<IEnumerable<IClientProxy>> proxyFactory)
        {
            this.proxyFactory = proxyFactory;
        }

        public Task SendAsync(string method, params object[] arguments)
        {
            foreach (var proxy in proxyFactory())
            {
                proxy.SendAsync(method, arguments);
            }

#if NETSTANDARD
                return Task.CompletedTask;
#else
            return new Task(null);
#endif
        }
    }

    internal class NullClientProxy : IClientProxy
    {
        public static NullClientProxy Instance = new NullClientProxy();

        public Task SendAsync(string method, params object[] arguments)
        {
#if NETSTANDARD
                return Task.CompletedTask;
#else
            return new Task(null);
#endif
        }
    }
}
