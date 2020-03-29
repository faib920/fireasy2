// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Configuration;
using Fireasy.Common.Threading;
#if NETSTANDARD
using Fireasy.Common.Threading.Configuration;
using Microsoft.Extensions.Options;
#endif
using System;
using System.Threading.Tasks;

namespace Fireasy.Redis
{
    /// <summary>
    /// 基于 Redis 的分布式锁。
    /// </summary>
    [ConfigurationSetting(typeof(RedisConfigurationSetting))]
    public class RedisLocker : RedisComponent, IDistributedLocker
    {
        /// <summary>
        /// 初始化 <see cref="RedisLocker"/> 类的新实例。
        /// </summary>
        public RedisLocker()
        {
        }

#if NETSTANDARD
        /// <summary>
        /// 初始化 <see cref="RedisLocker"/> 类的新实例。
        /// </summary>
        /// <param name="options"></param>
        public RedisLocker(IOptions<RedisDistributedLockerOptions> options)
        {
            var _options = options.Value;
            RedisConfigurationSetting setting;
            if (!string.IsNullOrEmpty(_options.ConfigName))
            {
                var section = ConfigurationUnity.GetSection<LockerConfigurationSection>();
                if (section != null && section.GetSetting(_options.ConfigName) is ExtendConfigurationSetting extSetting)
                {
                    setting = (RedisConfigurationSetting)extSetting.Extend;
                }
                else
                {
                    throw new InvalidOperationException($"无效的配置节: {_options.ConfigName}。");
                }
            }
            else
            {
                setting = new RedisConfigurationSetting
                {
                    Password = _options.Password,
                    ConnectionString = _options.ConnectionString,
                    DefaultDb = _options.DefaultDb,
                    ConnectTimeout = _options.ConnectTimeout,
                    LockTimeout = _options.LockTimeout,
                    SyncTimeout = _options.SyncTimeout,
                    WriteBuffer = _options.WriteBuffer,
                    PoolSize = _options.PoolSize,
                    SerializerType = _options.SerializerType,
                    Ssl = _options.Ssl,
                    Twemproxy = _options.Twemproxy
                };

                RedisHelper.ParseHosts(setting, _options.Hosts);
            }

            if (setting != null)
            {
                (this as IConfigurationSettingHostService).Attach(setting);
            }
        }
#endif

        public void Lock(string token, TimeSpan timeout, Action action)
        {
#if NETSTANDARD
            RedisHelper.Lock(GetConnection(token), token, timeout, action);
#else
            RedisHelper.Lock(GetDatabase(token), token, timeout, action);
#endif
        }

        public T Lock<T>(string token, TimeSpan timeout, Func<T> func)
        {
#if NETSTANDARD
            return RedisHelper.Lock(GetConnection(token), token, timeout, func);
#else
            return RedisHelper.Lock(GetDatabase(token), token, timeout, func);
#endif
        }

        public async Task LockAsync(string token, TimeSpan timeout, Func<Task> func)
        {
#if NETSTANDARD
            await RedisHelper.LockAsync(GetConnection(token), token, timeout, func);
#else
            await RedisHelper.LockAsync(GetDatabase(token), token, timeout, func);
#endif
        }

        public async Task<T> LockAsync<T>(string token, TimeSpan timeout, Func<Task<T>> func)
        {
#if NETSTANDARD
            return await RedisHelper.LockAsync(GetConnection(token), token, timeout, func);
#else
            return await RedisHelper.LockAsync(GetDatabase(token), token, timeout, func);
#endif
        }
    }
}
