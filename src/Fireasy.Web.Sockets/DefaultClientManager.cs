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
    public class DefaultClientManager : IClientManager
    {
        private readonly SafetyDictionary<string, IClientProxy> _clients = new SafetyDictionary<string, IClientProxy>();
        private readonly SafetyDictionary<string, List<string>> _groups = new SafetyDictionary<string, List<string>>();
        private Timer _timer = null;
        private WebSocketBuildOption _option;

        public virtual void Initialize(WebSocketAcceptContext acceptContext)
        {
            _option = acceptContext.Option;

            //开启一个定时器，一定时间去检查一下有没有死亡而没有释放的连接实例
            if (_option.HeartbeatInterval != TimeSpan.Zero)
            {
                _timer = new Timer(CheckAlive, null, TimeSpan.FromSeconds(10), _option.HeartbeatInterval);
            }
        }

        /// <summary>
        /// 检查没有死亡的连接实例，进行释放。
        /// </summary>
        /// <param name="state"></param>
        private void CheckAlive(object state)
        {
            foreach (var kvp in _clients)
            {
                var client = _clients[kvp.Key];

                //心跳时间后延
                if (client != null && (DateTime.Now - client.AliveTime).TotalMilliseconds >= _option.HeartbeatInterval.TotalMilliseconds * (_option.HeartbeatTryTimes + 2) &&
                    _clients.TryRemove(kvp.Key, out client))
                {
                    client.TryDispose();
                }
            }
        }

        public virtual void Add(string connectionId, IClientProxy clientProxy)
        {
            _clients.TryAdd(connectionId, clientProxy);
        }

        public virtual void AddToGroup(string connectionId, string groupName)
        {
            var group = _groups.GetOrAdd(groupName, () => new List<string>());
            group.Add(connectionId);
        }

        public virtual void Remove(string connectionId)
        {
            if (_clients.TryRemove(connectionId, out IClientProxy client))
            {
                foreach (var g in _groups)
                {
                    if (g.Value.Contains(connectionId))
                    {
                        g.Value.Remove(connectionId);
                    }
                }
                client.TryDispose();
            }
        }

        public virtual void RemoveFromGroup(string connectionId, string groupName)
        {
            if (_groups.ContainsKey(groupName))
            {
                _groups[groupName].Remove(connectionId);
            }
        }

        public virtual void Refresh(string connectionId)
        {
        }

        /// <summary>
        /// 获取指定客户端连接标识的代理。
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        public virtual IClientProxy Client(string connectionId)
        {
            if (_clients.TryGetValue(connectionId, out IClientProxy client))
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

            return new EnumerableClientProxy(() => _clients.Where(s => connectionIds.Contains(s.Key)).Select(s => s.Value));
        }

        /// <summary>
        /// 获取所有客户端代理。
        /// </summary>
        public virtual IClientProxy All
        {
            get
            {
                return new EnumerableClientProxy(() => _clients.Values);
            }
        }

        /// <summary>
        /// 获取其他客户端代理。
        /// </summary>
        public virtual IClientProxy Other
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 获取指定分组的所有客户端代理。
        /// </summary>
        /// <param name="groupName">组的名称。</param>
        /// <returns></returns>
        public virtual IClientProxy Group(string groupName)
        {
            if (_groups.ContainsKey(groupName))
            {
                return new EnumerableClientProxy(() => _clients.Where(s => _groups[groupName].Contains(s.Key)).Select(s => s.Value));
            }

            return NullClientProxy.Instance;
        }
    }
}
