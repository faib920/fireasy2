// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using CSRedis;
using Fireasy.Common.Extensions;
using Fireasy.Common.Threading;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Redis
{
    internal class RedisHelper
    {
        private const int LOCK_DELAY = 100;

        internal static void Lock(CSRedisClient client, string token, TimeSpan timeout, Action action)
        {
            CSRedisClientLock locker;
            while ((locker = client.Lock(token, (int)timeout.TotalSeconds)) == null)
            {
                Thread.Sleep(LOCK_DELAY);
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
                Thread.Sleep(LOCK_DELAY);
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
                Thread.Sleep(LOCK_DELAY);
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
                Thread.Sleep(LOCK_DELAY);
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

        internal static void TryLock(CSRedisClient client, string token, TimeSpan timeout, Action action, Action onLocked = null)
        {
            CSRedisClientLock locker;
            if ((locker = client.Lock(token, (int)timeout.TotalSeconds)) == null)
            {
                onLocked?.Invoke();
                return;
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

        internal static T TryLock<T>(CSRedisClient client, string token, TimeSpan timeout, Func<T> func, Func<T> onLocked = null)
        {
            CSRedisClientLock locker;
            if ((locker = client.Lock(token, (int)timeout.TotalSeconds)) == null)
            {
                return onLocked != null ? onLocked() : default;
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

        internal static async Task TryLockAsync(CSRedisClient client, string token, TimeSpan timeout, Func<Task> func, Func<Task> onLocked = null)
        {
            CSRedisClientLock locker;
            if ((locker = client.Lock(token, (int)timeout.TotalSeconds)) == null)
            {
                if (onLocked != null)
                {
                    await onLocked();
                }

#if NET45
                await TaskCompatible.CompletedTask;
#else
                await Task.CompletedTask;
#endif
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

        internal static async Task<T> TryLockAsync<T>(CSRedisClient client, string token, TimeSpan timeout, Func<Task<T>> func, Func<Task<T>> onLocked = null)
        {
            CSRedisClientLock locker;
            if ((locker = client.Lock(token, (int)timeout.TotalSeconds)) == null)
            {
                if (onLocked != null)
                {
                    return await onLocked();
                }

                return await Task.FromResult(default(T));
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
