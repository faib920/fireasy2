using System;

namespace Fireasy.Web.Sockets
{
    public class WebSocketHandleException : Exception
    {
        public WebSocketHandleException(string connectionId, string message, Exception innerException)
            : base(message, innerException)
        {
            ConnectionId = connectionId;
        }

        public string ConnectionId { get; private set; }
    }
}
