// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Threading;
using System.Threading.Tasks;
using Fireasy.Common.Extensions;
using StackExchange.Redis;

namespace Fireasy.Redis
{
    internal class RedisHelper
    {
        private const int LOCK_DELAY = 100;

        internal static void Lock(IDatabase db, string token, TimeSpan timeout, Action action)
        {
            var lockValue = $"{token}_{Environment.MachineName}";

            while (!db.LockTake(token, lockValue, timeout))
            {
                Thread.Sleep(LOCK_DELAY);
            }

            try
            {
                action();
            }
            finally
            {
                db.LockRelease(token, lockValue);
            }
        }

        internal static T Lock<T>(IDatabase db, string token, TimeSpan timeout, Func<T> func)
        {
            var lockValue = $"{token}_{Environment.MachineName}";

            while (!db.LockTake(token, lockValue, timeout))
            {
                Thread.Sleep(LOCK_DELAY);
            }

            try
            {
                return func();
            }
            finally
            {
                db.LockRelease(token, lockValue);
            }
        }

        internal static async Task LockAsync(IDatabase db, string token, TimeSpan timeout, Func<Task> func)
        {
            var lockValue = $"{token}_{Environment.MachineName}";

            while (!await db.LockTakeAsync(token, lockValue, timeout).ConfigureAwait(false))
            {
                await Task.Delay(LOCK_DELAY);
            }

            try
            {
                await func().ConfigureAwait(false);
            }
            finally
            {
                await db.LockReleaseAsync(token, lockValue).ConfigureAwait(false);
            }
        }

        internal static async Task<T> LockAsync<T>(IDatabase db, string token, TimeSpan timeout, Func<Task<T>> func)
        {
            var lockValue = $"{token}_{Environment.MachineName}";

            while (!await db.LockTakeAsync(token, lockValue, timeout).ConfigureAwait(false))
            {
                await Task.Delay(LOCK_DELAY);
            }

            try
            {
                return await func();
            }
            finally
            {
                await db.LockReleaseAsync(token, lockValue);
            }
        }

        internal static void TryLock(IDatabase db, string token, TimeSpan timeout, Action action, Action onLocked = null)
        {
            var lockValue = $"{token}_{Environment.MachineName}";

            if (!db.LockTake(token, lockValue, timeout))
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
                db.LockRelease(token, lockValue);
            }
        }

        internal static T TryLock<T>(IDatabase db, string token, TimeSpan timeout, Func<T> func, Func<T> onLocked = null)
        {
            var lockValue = $"{token}_{Environment.MachineName}";

            if (!db.LockTake(token, lockValue, timeout))
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
                db.LockRelease(token, lockValue);
            }

            return result;
        }

        internal static async Task TryLockAsync(IDatabase db, string token, TimeSpan timeout, Func<Task> func, Func<Task> onLocked = null)
        {
            var lockValue = $"{token}_{Environment.MachineName}";

            if (!await db.LockTakeAsync(token, lockValue, timeout).ConfigureAwait(false))
            {
                if (onLocked != null)
                {
                    await onLocked();
                }
                return;
            }

            try
            {
                await func().ConfigureAwait(false);
            }
            finally
            {
                await db.LockReleaseAsync(token, lockValue).ConfigureAwait(false);
            }
        }

        internal static async Task<T> TryLockAsync<T>(IDatabase db, string token, TimeSpan timeout, Func<Task<T>> func, Func<Task<T>> onLocked = null)
        {
            var lockValue = $"{token}_{Environment.MachineName}";

            if (!await db.LockTakeAsync(token, lockValue, timeout).ConfigureAwait(false))
            {
                return onLocked != null ? await onLocked () : default;
            }

            T result;
            try
            {
                result = await func().ConfigureAwait(false);
            }
            finally
            {
                await db.LockReleaseAsync(token, lockValue).ConfigureAwait(false);
            }

            return result;
        }

        internal static void ParseHosts(RedisConfigurationSetting setting, string hosts)
        {
            if (!string.IsNullOrEmpty(hosts))
            {
                hosts.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ForEach(s => setting.Hosts.Add(new RedisHost(s)));
            }
        }
    }
}
