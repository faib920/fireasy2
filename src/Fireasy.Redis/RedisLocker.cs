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
    [ConfigurationSetting(typeof(RedisConfigurationSetting))]
    public class RedisLocker : RedisComponent, IDistributedLocker
    {
        public void Lock(string token, TimeSpan timeout, Action action)
        {
#if NETSTANDARD
            Lock(GetConnection(), token, timeout, action);
#else
            Lock(GetDb(GetConnection()), token, timeout, action);
#endif
        }

        public T Lock<T>(string token, TimeSpan timeout, Func<T> func)
        {
#if NETSTANDARD
            return Lock(GetConnection(), token, timeout, func);
#else
            return Lock(GetDb(GetConnection()), token, timeout, func);
#endif
        }

        public async Task LockAsync(string token, TimeSpan timeout, Task task)
        {
#if NETSTANDARD
            await LockAsync(GetConnection(), token, timeout, task);
#else
            await LockAsync(GetDb(GetConnection()), token, timeout, task);
#endif
        }

        public async Task<T> LockAsync<T>(string token, TimeSpan timeout, Func<Task<T>> func)
        {
#if NETSTANDARD
            return await LockAsync(GetConnection(), token, timeout, func);
#else
            return await LockAsync(GetDb(GetConnection()), token, timeout, func);
#endif
        }
    }
}
