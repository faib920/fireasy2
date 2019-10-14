using System;
using System.Collections.Generic;
using Confluent.Kafka;
using Fireasy.Common.Subscribes;

namespace Fireasy.Kafka
{
    public class SubscribeManager : ISubscribeManager
    {
        public void AddSubscriber<TSubject>(Action<TSubject> subscriber) where TSubject : class
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = "",
                GroupId = "csharp-consumer",
                EnableAutoCommit = false,
                StatisticsIntervalMs = 5000,
                SessionTimeoutMs = 6000,
                AutoOffsetReset = AutoOffsetResetType.Earliest
            };

            using (var consumer = new Consumer<Ignore, string>(config))
            {
                consumer.OnPartitionsAssigned += Consumer_OnPartitionsAssigned;
            }
        }

        private void Consumer_OnPartitionsAssigned(object sender, List<TopicPartition> e)
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
            throw new NotImplementedException();
        }

        public void Publish(string channel, byte[] data)
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

        public void RemoveSubscriber(string channel)
        {
            throw new NotImplementedException();
        }
    }
}
