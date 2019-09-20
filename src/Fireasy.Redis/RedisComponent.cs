// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
using CSRedis;
using System.Text;
using System.Collections.Generic;
using System;

namespace Fireasy.Redis
{
    public class RedisComponent : IConfigurationSettingHostService
    {
        private Lazy<CSRedisClient> connectionLazy;

        protected RedisConfigurationSetting Setting { get; private set; }

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

        protected CSRedisClient GetConnection()
        {
            return connectionLazy.Value;
        }

        void IConfigurationSettingHostService.Attach(IConfigurationSettingItem setting)
        {
            this.Setting = (RedisConfigurationSetting)setting;

            var connectionStrs = new List<string>();
            if (!string.IsNullOrEmpty(this.Setting.ConnectionString))
            {
                connectionStrs.Add(this.Setting.ConnectionString);
            }
            else
            {

                foreach (var host in this.Setting.Hosts)
                {
                    var connStr = new StringBuilder($"{host.Server}");

            #region connection build
                    if (host.Port != 0)
                    {
                        connStr.Append($":{host.Port}");
                    }

                    if (!string.IsNullOrEmpty(this.Setting.Password))
                    {
                        connStr.Append($",password={this.Setting.Password}");
                    }

                    if (this.Setting.DefaultDb != 0)
                    {
                        connStr.Append($",defaultDatabase={this.Setting.DefaultDb}");
                    }

                    if (this.Setting.Ssl)
                    {
                        connStr.Append($",ssl=true");
                    }

                    if (this.Setting.WriteBuffer != null)
                    {
                        connStr.Append($",writeBuffer={this.Setting.WriteBuffer}");
                    }

                    if (this.Setting.PoolSize != null)
                    {
                        connStr.Append($",poolsize={this.Setting.PoolSize}");
                    }

                    connStr.Append(",allowAdmin=true");
            #endregion

                    connectionStrs.Add(connStr.ToString());
                }
            }

            if (connectionLazy == null)
            {
                connectionLazy = new Lazy<CSRedisClient>(() => new CSRedisClient(null, connectionStrs.ToArray()));
            }
        }

        IConfigurationSettingItem IConfigurationSettingHostService.GetSetting()
        {
            return Setting;
        }
    }
}
