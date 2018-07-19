// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Caching;
using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fireasy.Redis
{
    /// <summary>
    /// 基于 Redis 的缓存管理器。
    /// </summary>
    [ConfigurationSetting(typeof(RedisCacheSetting))]
    public class CacheManager : IDistributedCacheManager, IConfigurationSettingHostService
    {
        private RedisCacheSetting setting;
        private ConfigurationOptions options;

        /// <summary>
        /// 将对象插入到缓存管理器中。
        /// </summary>
        /// <typeparam name="T">缓存对象的类型。</typeparam>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="value">要插入到缓存的对象。</param>
        /// <param name="expire">对象存放于缓存中的有效时间，到期后将从缓存中移除。如果此值为 null，则默认有效时间为 30 分钟。</param>
        /// <param name="removeCallback">当对象从缓存中移除时，使用该回调方法通知应用程序。(在此类库中无效)</param>
        public T Add<T>(string cacheKey, T value, TimeSpan? expire = default(TimeSpan?), CacheItemRemovedCallback removeCallback = null)
        {
            using (var client = OpenConnection())
            {
                GetDb(client).StringSet(cacheKey, Serialize(value), expire);
                return value;
            }
        }

        /// <summary>
        /// 将对象插入到缓存管理器中。
        /// </summary>
        /// <typeparam name="T">缓存对象的类型。</typeparam>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="value">要插入到缓存的对象。</param>
        /// <param name="expiration">判断对象过期的对象。</param>
        /// <param name="removeCallback">当对象从缓存中移除时，使用该回调方法通知应用程序。</param>
        public T Add<T>(string cacheKey, T value, ICacheItemExpiration expiration, CacheItemRemovedCallback removeCallback = null)
        {
            using (var client = OpenConnection())
            {
                TimeSpan? expiry = null;

                if (expiration is RelativeTime)
                {
                    expiry = ((RelativeTime)expiration).Expiration;
                }

                GetDb(client).StringSet(cacheKey, Serialize(value), expiry);
                return value;
            }
        }

        /// <summary>
        /// 清除所有缓存。
        /// </summary>
        public void Clear()
        {
            using (var client = OpenConnection())
            {
                foreach (var endpoint in client.GetEndPoints())
                {
                    var server = client.GetServer(endpoint);
                    var keys = server.Keys();

                    foreach (var key in keys)
                    {
                        GetDb(client).KeyDelete(key);
                    }
                }
            }
        }

        /// <summary>
        /// 确定缓存中是否包含指定的缓存键的对象。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <returns>如果缓存中包含指定缓存键的对象，则为 true，否则为 false。</returns>
        public bool Contains(string cacheKey)
        {
            using (var client = OpenConnection())
            {
                return GetDb(client).KeyExists(cacheKey);
            }
        }

        /// <summary>
        /// 获取缓存中指定缓存键的对象。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <returns>检索到的缓存对象，未找到时为 null。</returns>
        public object Get(string cacheKey)
        {
            using (var client = OpenConnection())
            {
                var value = GetDb(client).StringGet(cacheKey);
                if (!string.IsNullOrEmpty(value))
                {
                    return Deserialize<dynamic>(value);
                }

                return null;
            }
        }

        /// <summary>
        /// 获取所有的 key。
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetKeys()
        {
            using (var client = OpenConnection())
            {
                var server = client.GetServer(client.GetEndPoints()[0]);
                return server.Keys(options.DefaultDatabase ?? 0).Cast<string>();
            }
        }

        /// <summary>
        /// 从缓存中移除指定缓存键的对象。
        /// </summary>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        public void Remove(string cacheKey)
        {
            using (var client = OpenConnection())
            {
                GetDb(client).KeyDelete(cacheKey);
            }
        }

        /// <summary>
        /// 尝试获取指定缓存键的对象，如果没有则使用工厂函数添加对象到缓存中。
        /// </summary>
        /// <typeparam name="T">缓存对象的类型。</typeparam>
        /// <param name="cacheKey">用于引用对象的缓存键。</param>
        /// <param name="factory">用于添加缓存对象的工厂函数。</param>
        /// <param name="expiration">判断对象过期的对象。</param>
        /// <returns></returns>
        public T TryGet<T>(string cacheKey, Func<T> factory, Func<ICacheItemExpiration> expiration = null)
        {
            using (var client = OpenConnection())
            {
                if (GetDb(client).KeyExists(cacheKey))
                {
                    var redisValue = GetDb(client).StringGet(cacheKey);
                    if (!string.IsNullOrEmpty(redisValue))
                    {
                        return Deserialize<T>(redisValue);
                    }

                    return default(T);
                }
                else
                {
                    TimeSpan? expiry = null;
                    if (expiration != null)
                    {
                        var exValue = expiration();
                        if (exValue is RelativeTime)
                        {
                            expiry = ((RelativeTime)exValue).Expiration;
                        }
                    }

                    var value = factory();
                    GetDb(client).StringSet(cacheKey, Serialize(value), expiry);
                    return value;
                }
            }
        }

        /// <summary>
        /// 尝试获取指定缓存键的对象。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGet<T>(string cacheKey, out T value)
        {
            using (var client = OpenConnection())
            {
                if (GetDb(client).KeyExists(cacheKey))
                {
                    var redisValue = GetDb(client).StringGet(cacheKey);
                    if (!string.IsNullOrEmpty(redisValue))
                    {
                        value = Deserialize<T>(redisValue);
                        return true;
                    }
                }
            }

            value = default(T);
            return false;
        }

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

        private ConnectionMultiplexer OpenConnection()
        {
            try
            {
                return ConnectionMultiplexer.Connect(options);
            }
            catch (RedisConnectionException exp)
            {
                throw new CacheServerException(exp);
            }
        }

        private IDatabase GetDb(IConnectionMultiplexer conn)
        {
            return conn.GetDatabase(options.DefaultDatabase ?? 0);
        }

        void IConfigurationSettingHostService.Attach(IConfigurationSettingItem setting)
        {
            this.setting = (RedisCacheSetting)setting;

            if (!string.IsNullOrEmpty(this.setting.ConnectionString))
            {
                options = ConfigurationOptions.Parse(this.setting.ConnectionString);
            }
            else
            {
                options = new ConfigurationOptions
                {
                    DefaultDatabase = this.setting.DefaultDb,
                    Password = this.setting.Password,
                    ConnectTimeout = this.setting.ConnectTimeout,
                    AllowAdmin = true,
                    Proxy = this.setting.Twemproxy ? Proxy.Twemproxy : Proxy.None
                };

                foreach (var host in this.setting.Hosts)
                {
                    if (host.Port == 0)
                    {
                        options.EndPoints.Add(host.Server);
                    }
                    else
                    {
                        options.EndPoints.Add(host.Server, host.Port);
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
