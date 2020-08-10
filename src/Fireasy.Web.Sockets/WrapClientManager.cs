// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace Fireasy.Web.Sockets
{
    internal sealed class WrapClientManager : IClientManager
    {
        private readonly IClientManager _innerMgr;
        private readonly IClientProxy _current;

        public WrapClientManager(IClientManager innerMgr, IClientProxy current)
        {
            _innerMgr = innerMgr;
            _current = current;
        }

        public IClientProxy All => _innerMgr.All;

        public IClientProxy Other
        {
            get
            {
                return new EnumerableClientProxy(() =>
                {
                    var clients = new List<IClientProxy>();

                    foreach (var client in (IEnumerable<IClientProxy>)_innerMgr.All)
                    {
                        if (client != _current)
                        {
                            clients.Add(client);
                        }
                    }

                    return clients;
                });
            }
        }

        public void Add(string connectionId, IClientProxy clientProxy)
        {
            _innerMgr.Add(connectionId, clientProxy);
        }

        public void AddToGroup(string connectionId, string groupName)
        {
            _innerMgr.AddToGroup(connectionId, groupName);
        }

        public IClientProxy Client(string connectionId)
        {
            return _innerMgr.Client(connectionId);
        }

        public IClientProxy Clients(params string[] connectionIds)
        {
            return _innerMgr.Clients(connectionIds);
        }

        public IClientProxy Group(string groupName)
        {
            return _innerMgr.Group(groupName);
        }

        public void Initialize(WebSocketAcceptContext context)
        {
            _innerMgr.Initialize(context);
        }

        public void Remove(string connectionId)
        {
            _innerMgr.Remove(connectionId);
        }

        public void RemoveFromGroup(string connectionId, string groupName)
        {
            _innerMgr.RemoveFromGroup(connectionId, groupName);
        }
    }
}
