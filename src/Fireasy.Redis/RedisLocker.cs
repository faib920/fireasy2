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
using Fireasy.Common.Options;
using Fireasy.Common.Threading.Configuration;
using Microsoft.Extensions.Options;
#endif
using System;
using System.Linq;
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
            : base()
        {
        }

        /// <summary>
        /// 初始化 <see cref="RedisLocker"/> 类的新实例。
        /// </summary>
        /// <param name="serviceProvider"></param>
        public RedisLocker(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

#if NETSTANDARD
        /// <summary>
        /// 初始化 <see cref="RedisLocker"/> 类的新实例。
        /// </summary>
        /// <param name="options"></param>
        public RedisLocker(IServiceProvider serviceProvider, IOptionsMonitor<RedisDistributedLockerOptions> options)
            : this(serviceProvider)
        {
            RedisConfigurationSetting setting = null;
            var optValue = options.CurrentValue;
            if (!optValue.IsConfigured())
            {
                var section = ConfigurationUnity.GetSection<LockerConfigurationSection>();
                var matchSetting = section.Settings.FirstOrDefault(s => s.Value.LockerType == typeof(RedisLocker)).Value;
                if (matchSetting != null && section.GetSetting(matchSetting.Name) is ExtendConfigurationSetting extSetting)
                {
                    setting = (RedisConfigurationSetting)extSetting.Extend;
                }
                else
                {
                    throw new InvalidOperationException($"未发现与 {typeof(RedisLocker).FullName} 相匹配的配置。");
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(optValue.ConfigName))
                {
                    var section = ConfigurationUnity.GetSection<LockerConfigurationSection>();
                    if (section != null && section.GetSetting(optValue.ConfigName) is ExtendConfigurationSetting extSetting)
                    {
                        setting = (RedisConfigurationSetting)extSetting.Extend;
                    }
                    else
                    {
                        throw new InvalidOperationException($"无效的配置节: {optValue.ConfigName}。");
                    }
                }
                else
                {
                    setting = new RedisConfigurationSetting
                    {
                        Password = optValue.Password,
                        ConnectionString = optValue.ConnectionString,
                        DefaultDb = optValue.DefaultDb,
                        ConnectTimeout = optValue.ConnectTimeout,
                        LockTimeout = optValue.LockTimeout,
                        SyncTimeout = optValue.SyncTimeout,
                        WriteBuffer = optValue.WriteBuffer,
                        PoolSize = optValue.PoolSize,
                        SerializerType = optValue.SerializerType,
                        Ssl = optValue.Ssl,
                        Twemproxy = optValue.Twemproxy
                    };

                    RedisHelper.ParseHosts(setting, optValue.Hosts);
                }
            }

            if (setting != null)
            {
                (this as IConfigurationSettingHostService).Attach(setting);
            }
        }
#endif

        public void Lock(string token, TimeSpan timeout, Action action)
        {
            RedisHelper.Lock(GetConnection(token), token, timeout, action);
        }

        public T Lock<T>(string token, TimeSpan timeout, Func<T> func)
        {
            return RedisHelper.Lock(GetConnection(token), token, timeout, func);
        }

        public async Task LockAsync(string token, TimeSpan timeout, Func<Task> func)
        {
            await RedisHelper.LockAsync(GetConnection(token), token, timeout, func);
        }

        public async Task<T> LockAsync<T>(string token, TimeSpan timeout, Func<Task<T>> func)
        {
            return await RedisHelper.LockAsync(GetConnection(token), token, timeout, func);
        }
    }
}
