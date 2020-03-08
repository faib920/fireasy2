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
        internal RedisLocker(RedisDistributedLockerOptions options)
        {
            RedisConfigurationSetting setting;
            if (!string.IsNullOrEmpty(options.ConfigName))
            {
                var section = ConfigurationUnity.GetSection<LockerConfigurationSection>();
                if (section != null && section.GetSetting(options.ConfigName) is ExtendConfigurationSetting extSetting)
                {
                    setting = (RedisConfigurationSetting)extSetting.Extend;
                }
                else
                {
                    throw new InvalidOperationException($"无效的配置节: {options.ConfigName}。");
                }
            }
            else
            {
                setting = new RedisConfigurationSetting
                {
                    Password = options.Password,
                    ConnectionString = options.ConnectionString,
                    DefaultDb = options.DefaultDb,
                    ConnectTimeout = options.ConnectTimeout,
                    LockTimeout = options.LockTimeout,
                    SyncTimeout = options.SyncTimeout,
                    WriteBuffer = options.WriteBuffer,
                    PoolSize = options.PoolSize,
                    SerializerType = options.SerializerType,
                    Ssl = options.Ssl,
                    Twemproxy = options.Twemproxy
                };

                RedisHelper.ParseHosts(setting, options.Hosts);
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
