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
                await Task.Delay(LOCK_DELAY);
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
                await Task.Delay(LOCK_DELAY);
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
            InternalLock locker;
            var value = Guid.NewGuid().ToString();
            if (!client.Set(token, value, (int)timeout.TotalSeconds, RedisExistence.Nx))
            {
                onLocked?.Invoke();
                return;
            }

            locker = new InternalLock(client, token, value);

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
            InternalLock locker;
            var value = Guid.NewGuid().ToString();
            if (!client.Set(token, value, (int)timeout.TotalSeconds, RedisExistence.Nx))
            {
                return onLocked != null ? onLocked() : default;
            }

            locker = new InternalLock(client, token, value);

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
            InternalLock locker;
            var value = Guid.NewGuid().ToString();
            if (!await client.SetAsync(token, value, (int)timeout.TotalSeconds, RedisExistence.Nx).ConfigureAwait(false))
            {
                if (onLocked != null)
                {
                    await onLocked();
                }

                await TaskCompatible.CompletedTask;
            }

            locker = new InternalLock(client, token, value);

            try
            {
                await func().ConfigureAwait(false);
            }
            finally
            {
                locker.Dispose();
            }
        }

        internal static async Task<T> TryLockAsync<T>(CSRedisClient client, string token, TimeSpan timeout, Func<Task<T>> func, Func<Task<T>> onLocked = null)
        {
            InternalLock locker;
            var value = Guid.NewGuid().ToString();
            if (!await client.SetAsync(token, value, (int)timeout.TotalSeconds, RedisExistence.Nx).ConfigureAwait(false))
            {
                return onLocked != null ? await onLocked() : await Task.FromResult(default(T));
            }

            locker = new InternalLock(client, token, value);

            T result;
            try
            {
                result = await func().ConfigureAwait(false);
            }
            finally
            {
                locker.Dispose();
            }

            return result;
        }

        internal static void ParseHosts(RedisConfigurationSetting setting, string hosts, string sentinels)
        {
            if (!string.IsNullOrEmpty(hosts))
            {
                hosts.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ForEach(s => setting.Hosts.Add(new RedisHost(s)));
            }

            if (!string.IsNullOrEmpty(sentinels))
            {
                sentinels.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ForEach(s => setting.Sentinels.Add(new RedisHost(s)));
            }
        }

        internal class InternalLock : IDisposable
        {
            private CSRedisClient _client;
            private string _name;
            private string _value;

            internal InternalLock(CSRedisClient rds, string name, string value)
            {
                _client = rds;
                _name = name;
                _value = value;
            }

            public bool Unlock()
            {
                return _client.Eval("local gva = redis.call('GET', KEYS[1])\r\nif gva == ARGV[1] then\r\n  redis.call('DEL', KEYS[1])\r\n  return 1\r\nend\r\nreturn 0", _name, _value)?.ToString() == "1";
            }

            public void Dispose()
            {
                Unlock();
            }
        }

    }
}
