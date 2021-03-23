// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Caching;
using Fireasy.Common.Extensions;
using Fireasy.Common.Subscribes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fireasy.Web.Sockets
{
    /// <summary>
    /// 分布式的客户端管理器。
    /// </summary>
    public class DistributedClientManager : DefaultClientManager
    {
        private string _aliveCacheKey;
        private string _aliveKey;
        private ISubscribeManager _subscribeMgr;
        private IDistributedCacheManager _cacheMgr;
        private TimeSpan _errorExpire;

        public override void Initialize(WebSocketAcceptContext acceptContext)
        {
            base.Initialize(acceptContext);

            _aliveKey = acceptContext.Option.AliveKey;
            _aliveCacheKey = $"{acceptContext.Option.AppKey}:alive_keys";
            _errorExpire = TimeSpan.FromMilliseconds(acceptContext.Option.HeartbeatInterval.TotalMilliseconds * 5);

            //开启消息订阅，使用aliveKey作为通道
            _subscribeMgr = acceptContext.Option.SubscribeManager ?? 
                acceptContext.ServiceProvider.TryGetService<ISubscribeManager>();

            _cacheMgr = acceptContext.Option.CacheManager ??
                acceptContext.ServiceProvider.TryGetService<IDistributedCacheManager>();

            if (_subscribeMgr == null)
            {
                throw new NotSupportedException("必须使用分布式订阅组件。");
            }

            if (_cacheMgr == null)
            {
                throw new NotSupportedException("必须使用分布式缓存组件。");
            }

            _subscribeMgr.AddSubscriber<DistributedInvokeMessage>(_aliveKey, msg =>
            {
                try
                {
                    //收到消息后，在本地查找连接，并发送消息
                    Clients(msg.Connections.ToArray()).SendAsync(msg.Message.Method, msg.Message.Arguments);
                }
                catch { }
            });
        }

        public override void Add(string connectionId, IClientProxy clientProxy)
        {
            var hashSet = _cacheMgr.GetHashSet<string, string>(_aliveCacheKey);

            //在分布式缓存里存放连接标识对应的aliveKey，即服务标识，以方便后面查找
            hashSet.Add(connectionId, _aliveKey, new RelativeTime(_errorExpire));

            base.Add(connectionId, clientProxy);
        }

        public override void Remove(string connectionId)
        {
            var hashSet = _cacheMgr.GetHashSet<string, string>(_aliveCacheKey);

            hashSet.Remove(connectionId);

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

        public override void Refresh(string connectionId)
        {
            var hashSet = _cacheMgr.GetHashSet<string, string>(_aliveCacheKey);

            //在分布式缓存里存放连接标识对应的aliveKey，即服务标识，以方便后面查找
            hashSet.Add(connectionId, _aliveKey, new RelativeTime(TimeSpan.FromMinutes(5)));
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

            //如果没有，则去分布式缓存里查找出aliveKey，并使用分布式代理进行传递
            var hashSet = _cacheMgr.GetHashSet<string, string>(_aliveCacheKey);

            if (hashSet.TryGet(connectionId, out string aliveKey))
            {
                return new DistributedUserClientProxy(_subscribeMgr, aliveKey, connectionId);
            }
            
            return NullClientProxy.Instance;
        }

        public override IClientProxy All
        {
            get
            {
                var clients = new List<IClientProxy>();
                var hashSet = _cacheMgr.GetHashSet<string, string>(_aliveCacheKey);
                foreach (var connectionId in hashSet.GetKeys())
                {
                    if (hashSet.TryGet(connectionId, out string aliveKey))
                    {
                        var client = base.Client(connectionId);
                        if (client != NullClientProxy.Instance)
                        {
                            clients.Add(client);
                        }
                        else
                        {
                            clients.Add(new DistributedUserClientProxy(_subscribeMgr, aliveKey, connectionId));
                        }
                    }
                }

                return new EnumerableClientProxy(() => clients);
            }
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

            var hashSet = _cacheMgr.GetHashSet<string, string>(_aliveCacheKey);

            var clients = new List<IClientProxy>();

            foreach (var connectionId in connectionIds.Distinct())
            {
                var client = base.Client(connectionId);
                if (client != NullClientProxy.Instance)
                {
                    clients.Add(client);
                }
                else if (hashSet.TryGet(connectionId, out string aliveKey))
                {
                    clients.Add(new DistributedUserClientProxy(_subscribeMgr, aliveKey, connectionId));
                }
            }

            return new EnumerableClientProxy(() => clients);
        }
    }

    /// <summary>
    /// 分布式代理。
    /// </summary>
    public class DistributedUserClientProxy : BaseClientProxy
    {
        private readonly ISubscribeManager _subscribeMgr;
        private readonly string _aliveKey;
        private readonly List<string> _connections;

        public DistributedUserClientProxy(ISubscribeManager _subscribeMgr, string aliveKey, string connectionId)
        {
            this._subscribeMgr = _subscribeMgr;
            _aliveKey = aliveKey;
            _connections = new List<string> { connectionId };
        }

        public DistributedUserClientProxy(string aliveKey, IEnumerable<string> connectionIds)
        {
            _aliveKey = aliveKey;
            _connections = new List<string>(connectionIds.Distinct());
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
            var msg = new DistributedInvokeMessage
            {
                AliveKey = _aliveKey,
                Connections = _connections,
                Message = new InvokeMessage(method, 0, arguments)
            };

            await _subscribeMgr.PublishAsync(_aliveKey, msg);
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
