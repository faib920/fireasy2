// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Fireasy.Web.Sockets
{
    /// <summary>
    /// 客户端管理器。
    /// </summary>
    public class ClientManager
    {
        private SafetyDictionary<string, IClientProxy> clients = new SafetyDictionary<string, IClientProxy>();
        private SafetyDictionary<string, List<string>> groups = new SafetyDictionary<string, List<string>>();
        private readonly Timer timer = null;
        private WebSocketBuildOption option;

        public ClientManager(WebSocketBuildOption option)
        {
            this.option = option;

            //开启一个定时器，一定时间去检查一下有没有死亡而没有释放的连接实例
            if (option.HeartbeatInterval != TimeSpan.Zero)
            {
                timer = new Timer(CheckAlive, null, TimeSpan.FromSeconds(10), option.HeartbeatInterval);
            }
        }

        /// <summary>
        /// 检查没有死亡的连接实例，进行释放。
        /// </summary>
        /// <param name="state"></param>
        private void CheckAlive(object state)
        {
            foreach (var kvp in clients)
            {
                var client = clients[kvp.Key];

                //心跳时间后延
                if (client != null && (DateTime.Now - client.AliveTime).TotalMilliseconds >= option.HeartbeatInterval.TotalMilliseconds * (option.HeartbeatTryTimes + 2) &&
                    clients.TryRemove(kvp.Key, out client))
                {
                    client.TryDispose();
                }
            }
        }

        public virtual void Add(string connectionId, IClientProxy handler)
        {
            clients.TryAdd(connectionId, handler);
        }

        public virtual void AddToGroup(string connectionId, string groupName)
        {
            var group = groups.GetOrAdd(groupName, () => new List<string>());
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
}
