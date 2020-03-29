// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
#if NETSTANDARD
using Fireasy.Common.Configuration;
using Fireasy.Common.Caching.Configuration;
using CSRedis;
#else
using StackExchange.Redis;
#endif
using System.Threading;
using System.Threading.Tasks;
using Fireasy.Common.Extensions;

namespace Fireasy.Redis
{
    internal class RedisHelper
    {
#if NETSTANDARD
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
#else
        internal static void Lock(IDatabase db, string token, TimeSpan timeout, Action action)
        {
            var lockValue = $"{token}:LOCK_TOKEN";

            while (!db.LockTake(lockValue, token, timeout))
            {
                Thread.Sleep(100);
            }

            try
            {
                action();
            }
            finally
            {
                db.LockRelease(lockValue, token);
            }
        }

        internal static T Lock<T>(IDatabase db, string token, TimeSpan timeout, Func<T> func)
        {
            var lockValue = $"{token}:LOCK_TOKEN";

            while (!db.LockTake(lockValue, token, timeout))
            {
                Thread.Sleep(100);
            }

            try
            {
                return func();
            }
            finally
            {
                db.LockRelease(lockValue, token);
            }
        }

        internal static async Task LockAsync(IDatabase db, string token, TimeSpan timeout, Func<Task> task)
        {
            var lockValue = $"{token}:LOCK_TOKEN";

            while (!await db.LockTakeAsync(lockValue, token, timeout))
            {
                Thread.Sleep(100);
            }

            try
            {
                await task();
            }
            finally
            {
                await db.LockReleaseAsync(lockValue, token);
            }
        }

        internal static async Task<T> LockAsync<T>(IDatabase db, string token, TimeSpan timeout, Func<Task<T>> func)
        {
            var lockValue = $"token_{token}";

            while (!await db.LockTakeAsync(lockValue, token, timeout))
            {
                Thread.Sleep(100);
            }

            try
            {
                return await func();
            }
            finally
            {
                await db.LockReleaseAsync(lockValue, token);
            }
        }

#endif
        internal static void ParseHosts(RedisConfigurationSetting setting, string host)
        {
            if (!string.IsNullOrEmpty(host))
            {
                host.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ForEach(s => setting.Hosts.Add(new RedisHost(s)));
            }
        }
    }
}
