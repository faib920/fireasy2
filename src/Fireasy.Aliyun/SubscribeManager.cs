using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Profile;
using Aliyun.Acs.Ons.Model.V20190214;
using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
using Fireasy.Common.Subscribes;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Aliyun
{
    [ConfigurationSetting(typeof(SubscribeConfigurationSetting))]
    public class SubscribeManager : ISubscribeManager, IConfigurationSettingHostService
    {
        private DefaultAcsClient client;
        private SubscribeConfigurationSetting setting;

        public void Publish<TSubject>(TSubject subject) where TSubject : class
        {
            var name = TopicHelper.GetTopicName(typeof(TSubject));
            var data = Serialize(subject);
            
            Publish(name, data);
        }

        public void Publish<TSubject>(string name, TSubject subject) where TSubject : class
        {
            var data = Serialize(subject);

            Publish(name, data);
        }

        public void Publish(string name, byte[] data)
        {
            TryCreateTopic(name);

            var request = new OnsMessageSendRequest();
            request.Topic = name;
            request.Message = Convert.ToBase64String(data);
            request.InstanceId = setting.InstanceId;
            
            try
            {
                var response = client.GetAcsResponse(request);
            }
            catch
            {
            }
        }

        public async Task PublishAsync<TSubject>(TSubject subject, CancellationToken cancellationToken = default) where TSubject : class
        {
            await Task.Run(() => Publish(subject));
        }

        public async Task PublishAsync<TSubject>(string name, TSubject subject, CancellationToken cancellationToken = default) where TSubject : class
        {
            await Task.Run(() => Publish(name, subject));
        }

        public async Task PublishAsync(string name, byte[] data, CancellationToken cancellationToken = default)
        {
            await Task.Run(() => Publish(name, data));
        }

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

        private AliyunSerializer CreateSerializer()
        {
            if (setting.SerializerType != null)
            {
                var serializer = setting.SerializerType.New<AliyunSerializer>();
                if (serializer != null)
                {
                    return serializer;
                }
            }

            return new AliyunSerializer();
        }

        private void TryCreateTopic(string name)
        {
            var rqTopicList = new OnsTopicListRequest 
            {
                InstanceId = setting.InstanceId, 
                Topic = name
            };

            try
            {
                var resTopicList = client.GetAcsResponse(rqTopicList);
                if (! resTopicList.Data.Any(s => s.Topic == name))
                {
                    var rqTopicCreate = new OnsTopicCreateRequest
                    {
                        Topic = name,
                        InstanceId = setting.InstanceId,
                        MessageType = 0 
                    };

                    var resTopicCreate = client.GetAcsResponse(rqTopicCreate);
                }
            }
            catch
            {
            }
        }

        void IConfigurationSettingHostService.Attach(IConfigurationSettingItem setting)
        {
            this.setting = (SubscribeConfigurationSetting)setting;
            var profile = DefaultProfile.GetProfile(this.setting.RegionId, this.setting.AccessId, this.setting.AccessSecret);
            client = new DefaultAcsClient(profile);
        }

        IConfigurationSettingItem IConfigurationSettingHostService.GetSetting()
        {
            return setting;
        }
    }
}
