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
        private Lazy<ConnectionMultiplexer> connectionLazy;

        protected RedisConfigurationSetting Setting { get; private set; }

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
            if (Setting.SerializerType != null)
            {
                var serializer = Setting.SerializerType.New<RedisSerializer>();
                if (serializer != null)
                {
                    return serializer;
                }
            }

            return new RedisSerializer();
        }

        protected ConnectionMultiplexer GetConnection()
        {
            return connectionLazy.Value;
        }

        void IConfigurationSettingHostService.Attach(IConfigurationSettingItem setting)
        {
            this.Setting = (RedisConfigurationSetting)setting;

            if (!string.IsNullOrEmpty(this.Setting.ConnectionString))
            {
                Options = ConfigurationOptions.Parse(this.Setting.ConnectionString);
            }
            else
            {
                Options = new ConfigurationOptions
                {
                    DefaultDatabase = this.Setting.DefaultDb,
                    Password = this.Setting.Password,
                    AllowAdmin = true,
                    Proxy = this.Setting.Twemproxy ? Proxy.Twemproxy : Proxy.None,
                    AbortOnConnectFail = false
                };

                if (this.Setting.ConnectTimeout != null)
                {
                    Options.ConnectTimeout = (int)this.Setting.ConnectTimeout;
                }

                foreach (var host in this.Setting.Hosts)
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

            connectionLazy = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(Options));
        }

        IConfigurationSettingItem IConfigurationSettingHostService.GetSetting()
        {
            return Setting;
        }
    }
}
