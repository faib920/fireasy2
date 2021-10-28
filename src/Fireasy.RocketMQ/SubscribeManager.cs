using Fireasy.Common.ComponentModel;
using Fireasy.Common.Subscribes;
using NewLife.RocketMQ;
using NewLife.RocketMQ.Protocol;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.RocketMQ
{
    public class SubscribeManager : ISubscribeManager
    {
        private SafetyDictionary<string, Producer> _producers = new SafetyDictionary<string, Producer>();
        private SafetyDictionary<string, Consumer> _consumers = new SafetyDictionary<string, Consumer>();

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
            var consumer = _consumers.GetOrAdd(name, () =>
            {
                var mq = new Consumer { Topic = name, NameServerAddress = "" };
                mq.Start();
                return mq;
            });

            consumer.OnConsume = (q, ms) =>
            {
                foreach (var item in ms)
                {
                    subscriber.Invoke(item.Body);
                }

                return true;
            };
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
            var producer = _producers.GetOrAdd(name, () =>
            {
                var mq = new Producer { Topic = name, NameServerAddress = "" };
                mq.Start();
                return mq;
            });

            var msg = new Message
            {
                Body = data,
            };

            producer.Publish(msg);
        }

        public Task PublishAsync<TSubject>(TSubject subject, CancellationToken cancellationToken = default) where TSubject : class
        {
            throw new NotImplementedException();
        }

        public Task PublishAsync<TSubject>(string name, TSubject subject, CancellationToken cancellationToken = default) where TSubject : class
        {
            throw new NotImplementedException();
        }

        public async Task PublishAsync(string name, byte[] data, CancellationToken cancellationToken = default)
        {
            Publish(name, data);
        }

        public void RemoveSubscriber<TSubject>()
        {
            RemoveSubscriber(TopicHelper.GetTopicName(typeof(TSubject)));
        }

        public void RemoveSubscriber(Type subjectType)
        {
            RemoveSubscriber(TopicHelper.GetTopicName(subjectType));
        }

        public void RemoveSubscriber(string name)
        {
            if (_consumers.TryRemove(name, out Consumer consumer))
            {
                consumer.Dispose();
            }
        }
    }
}
