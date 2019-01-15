using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Web.Sockets
{
    public class DefaultLifetimeManager : ILifetimeManager
    {
        private static ConcurrentDictionary<Type, ClientManager> managers = new ConcurrentDictionary<Type, ClientManager>();
        private ConcurrentDictionary<string, IClientProxy> clients = new ConcurrentDictionary<string, IClientProxy>();
        private ConcurrentDictionary<string, List<string>> groups = new ConcurrentDictionary<string, List<string>>();

        private WebSocketAcceptContext context;

        public DefaultLifetimeManager(WebSocketAcceptContext context, ClientManager  d)
        {
            this.context = context;
        }

        public void AddUser()
        {
            throw new NotImplementedException();
        }

        public Task LisitenAsync()
        {
            throw new NotImplementedException();
        }

        public async Task SendAsync(InvokeMessage message)
        {
            var json = context.Option.Formatter.FormatMessage(message);
            var bytes = context.Option.Encoding.GetBytes(json);

            await context.WebSocket.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public Task SendToGroup(InvokeMessage message)
        {
            throw new NotImplementedException();
        }

        public Task SendToUser(InvokeMessage message)
        {
            throw new NotImplementedException();
        }

        public Task SendToUsers(InvokeMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
