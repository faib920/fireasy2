// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
using StackExchange.Redis;
using System;

namespace Fireasy.Redis
{
    public class RedisComponent : IConfigurationSettingHostService
    {
        private RedisConfigurationSetting setting;
        private static ConnectionMultiplexer connection;
        private static object locker = new object();

        protected ConfigurationOptions Options { get; private set; }

        /// <summary>
        /// 序列化对象。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual T Deserialize<T>(string value)
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
        protected virtual object Deserialize(Type type, string value)
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
        protected virtual string Serialize<T>(T value)
        {
            var serializer = CreateSerializer();
            return serializer.Serialize(value);
        }

        private RedisSerializer CreateSerializer()
        {
            if (setting.SerializerType != null)
            {
                var serializer = setting.SerializerType.New<RedisSerializer>();
                if (serializer != null)
                {
                    return serializer;
                }
            }

            return new RedisSerializer();
        }

        protected ConnectionMultiplexer GetConnection()
        {
            if (connection == null)
            {
                lock (locker)
                {
                    connection = ConnectionMultiplexer.Connect(Options);
                }
            }

            return connection;
        }

        void IConfigurationSettingHostService.Attach(IConfigurationSettingItem setting)
        {
            this.setting = (RedisConfigurationSetting)setting;

            if (!string.IsNullOrEmpty(this.setting.ConnectionString))
            {
                Options = ConfigurationOptions.Parse(this.setting.ConnectionString);
            }
            else
            {
                Options = new ConfigurationOptions
                {
                    DefaultDatabase = this.setting.DefaultDb,
                    Password = this.setting.Password,
                    AllowAdmin = true,
                    Proxy = this.setting.Twemproxy ? Proxy.Twemproxy : Proxy.None,
                    AbortOnConnectFail = false
                };

                if (this.setting.ConnectTimeout != null)
                {
                    Options.ConnectTimeout = (int)this.setting.ConnectTimeout;
                }

                foreach (var host in this.setting.Hosts)
                {
                    if (host.Port == 0)
                    {
                        Options.EndPoints.Add(host.Server);
                    }
                    else
                    {
                        Options.EndPoints.Add(host.Server, host.Port);
                    }
                }
            }
        }

        IConfigurationSettingItem IConfigurationSettingHostService.GetSetting()
        {
            return setting;
        }
    }
}
