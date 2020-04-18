// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using Fireasy.Common.Configuration;
using Fireasy.Common.Caching.Configuration;
using CSRedis;
using System.Threading;
using System.Threading.Tasks;
using Fireasy.Common.Extensions;

namespace Fireasy.Redis
{
    internal class RedisHelper
    {
        internal static void Lock(CSRedisClient client, string token, TimeSpan timeout, Action action)
        {
            CSRedisClientLock locker;
            while ((locker = client.Lock(token, (int)timeout.TotalSeconds)) == null)
            {
                Thread.Sleep(100);
            }

            try
            {
                action();
            }
            finally
            {
                locker.Dispose();
            }
        }

        internal static T Lock<T>(CSRedisClient client, string token, TimeSpan timeout, Func<T> func)
        {
            CSRedisClientLock locker;
            while ((locker = client.Lock(token, (int)timeout.TotalSeconds)) == null)
            {
                Thread.Sleep(100);
            }

            T result;
            try
            {
                result = func();
            }
            finally
            {
                locker.Dispose();
            }

            return result;
        }

        internal static async Task LockAsync(CSRedisClient client, string token, TimeSpan timeout, Func<Task> func)
        {
            CSRedisClientLock locker;
            while ((locker = client.Lock(token, (int)timeout.TotalSeconds)) == null)
            {
                Thread.Sleep(100);
            }

            try
            {
                await func();
            }
            finally
            {
                locker.Dispose();
            }
        }

        internal static async Task<T> LockAsync<T>(CSRedisClient client, string token, TimeSpan timeout, Func<Task<T>> func)
        {
            CSRedisClientLock locker;
            while ((locker = client.Lock(token, (int)timeout.TotalSeconds)) == null)
            {
                Thread.Sleep(100);
            }

            T result;
            try
            {
                result = await func();
            }
            finally
            {
                locker.Dispose();
            }

            return result;
        }

        internal static void ParseHosts(RedisConfigurationSetting setting, string host)
        {
            if (!string.IsNullOrEmpty(host))
            {
                host.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ForEach(s => setting.Hosts.Add(new RedisHost(s)));
            }
        }
    }
}
