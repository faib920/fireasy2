// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Caching;
using Fireasy.Common.Extensions;
using Fireasy.Common.Serialization;
using Fireasy.Common.Subscribes;
using System;
using System.Collections.Generic;
using System.Linq;
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
            : base(option)
        {
            aliveKey = option.AliveKey;

            //开启消息订阅，使用aliveKey作为通道
            var subMgr = SubscribeManagerFactory.CreateManager();

            subMgr.AddSubscriber(aliveKey, bytes =>
                {
                    try
                    {
                        //收到消息后，在本地查找连接，并发送消息
                        var content = Encoding.UTF8.GetString(bytes);
                        var msg = new JsonSerializer().Deserialize<DistributedInvokeMessage>(content);
                        Clients(msg.Connections.ToArray()).SendAsync(msg.Message.Method, msg.Message.Arguments);
                    }
                    catch { }
                });
        }

        public override void Add(string connectionId, IClientProxy handler)
        {
            var cacheMgr = CacheManagerFactory.CreateManager();

            //在redis缓存里存放连接标识对应的aliveKey，即服务标识，以方便后面查找
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
            //暂未实现
            base.AddToGroup(connectionId, groupName);
        }

        public override void RemoveFromGroup(string connectionId, string groupName)
        {
            //暂未实现
            base.RemoveFromGroup(connectionId, groupName);
        }

        /// <summary>
        /// 获取指定客户端连接标识的代理。
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        public override IClientProxy Client(string connectionId)
        {
            //先在本地会话池里查找
            var client = base.Client(connectionId);
            if (client != NullClientProxy.Instance)
            {
                return client;
            }

            //如果没有，则去redis缓存里查找出aliveKey，并使用分布式代理进行传递
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
            if (connectionIds == null || connectionIds.Length == 0)
            {
                return NullClientProxy.Instance;
            }

            var cacheMgr = CacheManagerFactory.CreateManager();
            var clients = new List<IClientProxy>();

            foreach (var connectionId in connectionIds.Distinct())
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

    /// <summary>
    /// 分布式代理。
    /// </summary>
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
            this.connections = new List<string>(connectionIds.Distinct());
        }

        /// <summary>
        /// 发送消息。
        /// </summary>
        /// <param name="method"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public override async Task SendAsync(string method, params object[] arguments)
        {
            //使用消息队列将消息发指定的服务器，即aliveKey对应的服务器
            var subMgr = SubscribeManagerFactory.CreateManager();
            var msg = new DistributedInvokeMessage
            {
                AliveKey = aliveKey,
                Connections = connections,
                Message = new InvokeMessage(method, 0, arguments)
            };

            var bytes = Encoding.UTF8.GetBytes(new JsonSerializer().Serialize(msg));
            await subMgr.PublishAsync(aliveKey, bytes);
        }
    }

    /// <summary>
    /// 分布式消息结构。
    /// </summary>
    internal class DistributedInvokeMessage
    {
        /// <summary>
        /// 对方的服务标识。
        /// </summary>
        public string AliveKey { get; set; }

        /// <summary>
        /// 要通知的客户端连接标识。
        /// </summary>
        public List<string> Connections { get; set; }

        /// <summary>
        /// 消息体。
        /// </summary>
        public InvokeMessage Message { get; set; }
    }
}
