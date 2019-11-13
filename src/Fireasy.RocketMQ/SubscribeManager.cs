using Fireasy.Common.Subscribes;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.RocketMQ
{
    public class SubscribeManager : ISubscribeManager
    {
        public void AddAsyncSubscriber<TSubject>(Func<TSubject, Task> subscriber) where TSubject : class
        {
            throw new NotImplementedException();
        }

        public void AddAsyncSubscriber<TSubject>(string name, Func<TSubject, Task> subscriber) where TSubject : class
        {
            throw new NotImplementedException();
        }

        public void AddSubscriber<TSubject>(Action<TSubject> subscriber) where TSubject : class
        {
            throw new NotImplementedException();
        }

        public void AddSubscriber<TSubject>(string name, Action<TSubject> subscriber) where TSubject : class
        {
            throw new NotImplementedException();
        }

        public void AddSubscriber(Type subjectType, Delegate subscriber)
        {
            throw new NotImplementedException();
        }

        public void AddSubscriber(string name, Action<byte[]> subscriber)
        {
            throw new NotImplementedException();
        }

        public void Publish<TSubject>(TSubject subject) where TSubject : class
        {
            throw new NotImplementedException();
        }

        public void Publish<TSubject>(string name, TSubject subject) where TSubject : class
        {
            throw new NotImplementedException();
        }

        public void Publish(string name, byte[] data)
        {
            throw new NotImplementedException();
        }

        public Task PublishAsync<TSubject>(TSubject subject, CancellationToken cancellationToken = default) where TSubject : class
        {
            throw new NotImplementedException();
        }

        public Task PublishAsync<TSubject>(string name, TSubject subject, CancellationToken cancellationToken = default) where TSubject : class
        {
            throw new NotImplementedException();
        }

        public Task PublishAsync(string name, byte[] data, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void RemoveSubscriber<TSubject>()
        {
            throw new NotImplementedException();
        }

        public void RemoveSubscriber(Type subjectType)
        {
            throw new NotImplementedException();
        }

        public void RemoveSubscriber(string name)
        {
            throw new NotImplementedException();
        }
    }
}
