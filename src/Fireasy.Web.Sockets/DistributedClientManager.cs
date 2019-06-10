// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Caching;
using Fireasy.Common.Serialization;
using Fireasy.Common.Subscribes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fireasy.Web.Sockets
{
    /// <summary>
    /// 分布式的客户端管理器。
    /// </summary>
    public class DistributedClientManager : ClientManager
    {
        private const string prefix = "ws_client_";
        private string aliveKey;

        /// <summary>
        /// 初始化 <see cref="DistributedClientManager"/> 类的新实例。
        /// </summary>
        /// <param name="option"></param>
        public DistributedClientManager(WebSocketBuildOption option)
            : base (option)
        {
            aliveKey = option.AliveKey;
            var subMgr = SubscribeManagerFactory.CreateManager();
            subMgr.AddSubscriber(aliveKey, bytes =>
            {
                var msg = new JsonSerializer().Deserialize<DistributedInvokeMessage>(Encoding.UTF8.GetString(bytes));
                Clients(msg.Connections.ToArray()).SendAsync(msg.Message.Method, msg.Message.Arguments);
            });
        }

        public override void Add(string connectionId, IClientProxy handler)
        {
            var cacheMgr = CacheManagerFactory.CreateManager();
            cacheMgr.Add(prefix + connectionId, aliveKey, new RelativeTime(TimeSpan.FromDays(5)));

            base.Add(connectionId, handler);
        }

        public override void Remove(string connectionId)
        {
            var cacheMgr = CacheManagerFactory.CreateManager();
            cacheMgr.Remove(prefix + connectionId);

            base.Remove(connectionId);
        }

        public override void AddToGroup(string connectionId, string groupName)
        {
            base.AddToGroup(connectionId, groupName);
        }

        public override void RemoveFromGroup(string connectionId, string groupName)
        {
            base.RemoveFromGroup(connectionId, groupName);
        }

        /// <summary>
        /// 获取指定客户端连接标识的代理。
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        public override IClientProxy Client(string connectionId)
        {
            var client = base.Client(connectionId);
            if (client != NullClientProxy.Instance)
            {
                return client;
            }

            var cacheMgr = CacheManagerFactory.CreateManager();
            if (cacheMgr.TryGet(prefix + connectionId, out string aliveKey))
            {
                return new DistributedUserClientProxy(aliveKey, connectionId);
            }

            return NullClientProxy.Instance;
        }

        /// <summary>
        /// 获取指定的多个客户端连接标识的代理。
        /// </summary>
        /// <param name="connectionIds"></param>
        /// <returns></returns>
        public override IClientProxy Clients(params string[] connectionIds)
        {
            var cacheMgr = CacheManagerFactory.CreateManager();
            var clients = new List<IClientProxy>();

            foreach (var connectionId in connectionIds)
            {
                var client = base.Client(connectionId);
                if (client != NullClientProxy.Instance)
                {
                    clients.Add(client);
                }
                else if (cacheMgr.TryGet(prefix + connectionId, out string aliveKey))
                {
                    clients.Add(new DistributedUserClientProxy(aliveKey, connectionId));
                }
            }

            return new EnumerableClientProxy(() => clients);
        }
    }

    internal class DistributedUserClientProxy : InternalClientProxy
    {
        private string aliveKey;
        private List<string> connections;

        public DistributedUserClientProxy(string aliveKey, string connectionId)
        {
            this.aliveKey = aliveKey;
            this.connections = new List<string> { connectionId };
        }

        public DistributedUserClientProxy(string aliveKey, IEnumerable<string> connectionIds)
        {
            this.aliveKey = aliveKey;
            this.connections = new List<string>(connectionIds);
        }

        public override Task SendAsync(string method, params object[] arguments)
        {
            var subMgr = SubscribeManagerFactory.CreateManager();
            var msg = new DistributedInvokeMessage
            {
                AliveKey = aliveKey,
                Connections = connections,
                Message = new InvokeMessage(method, 0, arguments)
            };

            var bytes = Encoding.UTF8.GetBytes(new JsonSerializer().Serialize(msg));
            subMgr.Publish(aliveKey, bytes);

#if NETSTANDARD
            return Task.CompletedTask;
#else
            return new Task(null);
#endif
        }
    }

    internal class DistributedInvokeMessage
    {
        public string AliveKey { get; set; }

        public List<string> Connections { get; set; }

        public InvokeMessage Message { get; set; }
    }
}
