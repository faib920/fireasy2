// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Configuration;
using Fireasy.Common.Threading;
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
        public void Lock(string token, TimeSpan timeout, Action action)
        {
#if NETSTANDARD
            RedisHelper.Lock(GetConnection(), token, timeout, action);
#else
            RedisHelper.Lock(GetDb(GetConnection()), token, timeout, action);
#endif
        }

        public T Lock<T>(string token, TimeSpan timeout, Func<T> func)
        {
#if NETSTANDARD
            return RedisHelper.Lock(GetConnection(), token, timeout, func);
#else
            return RedisHelper.Lock(GetDb(GetConnection()), token, timeout, func);
#endif
        }

        public async Task LockAsync(string token, TimeSpan timeout, Task task)
        {
#if NETSTANDARD
            await RedisHelper.LockAsync(GetConnection(), token, timeout, task);
#else
            await RedisHelper.LockAsync(GetDb(GetConnection()), token, timeout, task);
#endif
        }

        public async Task<T> LockAsync<T>(string token, TimeSpan timeout, Func<Task<T>> func)
        {
#if NETSTANDARD
            return await RedisHelper.LockAsync(GetConnection(), token, timeout, func);
#else
            return await RedisHelper.LockAsync(GetDb(GetConnection()), token, timeout, func);
#endif
        }
    }
}
