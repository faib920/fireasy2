using Fireasy.Common.Subscribes;
using System.Threading.Tasks;

namespace Fireasy.Web.Sockets
{
    public class SubscribeLifetimeManager : ILifetimeManager
    {
        private WebSocketAcceptContext context;

        public SubscribeLifetimeManager(WebSocketAcceptContext context)
        {
            this.context = context;
        }

        public void AddUser()
        {
            throw new System.NotImplementedException();
        }

        public Task LisitenAsync()
        {
            var sub = SubscribeManagerFactory.CreateManager();
            sub.AddSubscriber<Subject>(subject =>
            {
                if (subject.Target == context.ConnectionId)
                {

                }
            });

            return null;
        }

        public Task SendAsync(InvokeMessage message)
        {
            var sub = SubscribeManagerFactory.CreateManager();
            sub.Publish(new Subject { Source = context.ConnectionId, Message = message });
            return null;
        }

        public Task SendToGroup(InvokeMessage message)
        {
            throw new System.NotImplementedException();
        }

        public Task SendToUser(InvokeMessage message)
        {
            throw new System.NotImplementedException();
        }

        public Task SendToUsers(InvokeMessage message)
        {
            throw new System.NotImplementedException();
        }
    }

    public class Subject
    {
        public string Source { get; set; }

        public string Target { get; set; }

        public InvokeMessage Message { get; set; }
    }
}
