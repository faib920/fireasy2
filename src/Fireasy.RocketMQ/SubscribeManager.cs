using Fireasy.Common.Subscribes;
using System;

namespace Fireasy.RocketMQ
{
    public class SubscribeManager : ISubscribeManager
    {
        public void AddSubscriber<TSubject>(Action<TSubject> subscriber) where TSubject : class
        {
        }

        public void AddSubscriber(Type subjectType, Delegate subscriber)
        {
            throw new NotImplementedException();
        }

        public void AddSubscriber(string channel, Action<byte[]> subscriber)
        {
            throw new NotImplementedException();
        }

        public void Publish<TSubject>(TSubject subject) where TSubject : class
        {
        }

        public void Publish(string channel, byte[] data)
        {
        }

        public void RemoveSubscriber<TSubject>()
        {
            throw new NotImplementedException();
        }

        public void RemoveSubscriber(Type subjectType)
        {
            throw new NotImplementedException();
        }

        public void RemoveSubscriber(string channel)
        {
            throw new NotImplementedException();
        }
    }
}
