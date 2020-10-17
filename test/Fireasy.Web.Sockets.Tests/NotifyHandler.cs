using System;
using System.Threading.Tasks;

namespace Fireasy.Web.Sockets.Tests
{
    public class NotifyHandler : WebSocketHandler
    {
        public async Task Test(string message)
        {
            Console.WriteLine("received: " + message);
            await Clients.Other.SendAsync("ret", "收到");
        }

        public async Task Ret(string status)
        {
            Console.WriteLine("ret: " + status);
        }

        protected override void OnConnected()
        {
            Console.WriteLine(ConnectionId + " OnConnected");
            base.OnConnected();
        }

        protected override void OnDisconnected()
        {
            Console.WriteLine(ConnectionId + " OnDisconnected");
            base.OnDisconnected();
        }

        protected override void OnReceived(string content)
        {
            Console.WriteLine(ConnectionId + " OnReceived " + content);
            base.OnReceived(content);
        }

        protected override void OnInvokeError(InvokeMessage message, Exception exception)
        {
            Console.WriteLine(ConnectionId + " OnInvokeError " + message.Method);
            base.OnInvokeError(message, exception);
        }

        protected override void OnResolveError(string content, Exception exception)
        {
            Console.WriteLine(ConnectionId + " OnResolveError " + content);
            base.OnResolveError(content, exception);
        }
    }
}
