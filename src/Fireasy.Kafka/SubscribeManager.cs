// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Confluent.Kafka;
using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
using Fireasy.Common.Subscribes;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Kafka
{
    [ConfigurationSetting(typeof(KafkaConfigurationSetting))]
    public class SubscribeManager : ISubscribeManager, IConfigurationSettingHostService
    {
        private KafkaConfigurationSetting setting;

        public void Publish<TSubject>(TSubject subject) where TSubject : class
        {
            var name = TopicHelper.GetTopicName(typeof(TSubject));
            Publish(name, subject);
        }

        public async Task PublishAsync<TSubject>(TSubject subject, CancellationToken cancellationToken = default) where TSubject : class
        {
            var name = TopicHelper.GetTopicName(typeof(TSubject));
            await PublishAsync(name, subject, cancellationToken);
        }

        public void Publish<TSubject>(string name, TSubject subject) where TSubject : class
        {
            var data = Serialize(subject);
            Publish(name, data);
        }

        public async Task PublishAsync<TSubject>(string name, TSubject subject, CancellationToken cancellationToken = default) where TSubject : class
        {
            var data = Serialize(subject);
            await PublishAsync(name, data);
        }

        public void Publish(string name, byte[] data)
        {
            var config = GetProducerConfig();
            using (var produre = new ProducerBuilder<Null, byte[]>(config).Build())
            {
                produre.Produce(name, new Message<Null, byte[]> { Value = data });
            }
        }

        public async Task PublishAsync(string name, byte[] data, CancellationToken cancellationToken = default)
        {
            var config = GetProducerConfig();
            using (var produre = new ProducerBuilder<Null, byte[]>(config).Build())
            {
                await produre.ProduceAsync(name, new Message<Null, byte[]> { Value = data });
            }
        }

        public void AddSubscriber<TSubject>(Action<TSubject> subscriber) where TSubject : class
        {
        }

        public void AddAsyncSubscriber<TSubject>(Func<TSubject, Task> subscriber) where TSubject : class
        {
        }

        public void AddSubscriber<TSubject>(string name, Action<TSubject> subscriber) where TSubject : class
        {
        }

        public void AddAsyncSubscriber<TSubject>(string name, Func<TSubject, Task> subscriber) where TSubject : class
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

        void IConfigurationSettingHostService.Attach(IConfigurationSettingItem setting)
        {
            this.setting = (KafkaConfigurationSetting)setting;
        }

        IConfigurationSettingItem IConfigurationSettingHostService.GetSetting()
        {
            return setting;
        }

        /// <summary>
        /// 序列化对象。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual T Deserialize<T>(byte[] value)
        {
            var serializer = CreateSerializer();
            return serializer.Deserialize<T>(value);
        }

        /// <summary>
        /// 序列化对象。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual object Deserialize(Type type, byte[] value)
        {
            var serializer = CreateSerializer();
            return serializer.Deserialize(type, value);
        }

        /// <summary>
        /// 反序列化。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual byte[] Serialize<T>(T value)
        {
            var serializer = CreateSerializer();
            return serializer.Serialize(value);
        }

        private KafkaSerializer CreateSerializer()
        {
            if (setting.SerializerType != null)
            {
                var serializer = setting.SerializerType.New<KafkaSerializer>();
                if (serializer != null)
                {
                    return serializer;
                }
            }

            return new KafkaSerializer();
        }

        private ProducerConfig GetProducerConfig()
        {
            return new ProducerConfig
            {
                BootstrapServers = setting.Server
            };
        }

        private ConsumerConfig GetConsumerConfig()
        {
            return new ConsumerConfig
            {
                BootstrapServers = setting.Server
            };
        }
    }
}
