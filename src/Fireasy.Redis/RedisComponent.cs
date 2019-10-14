// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
#if NETSTANDARD
using CSRedis;
using System.Collections.Generic;
using System.Text;
#else
using StackExchange.Redis;
#endif
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Redis
{
    public class RedisComponent : IConfigurationSettingHostService
    {
#if NETSTANDARD
        private Lazy<CSRedisClient> connectionLazy;
#else
        private Lazy<ConnectionMultiplexer> connectionLazy;

        protected ConfigurationOptions Options { get; private set; }
#endif

        protected RedisConfigurationSetting Setting { get; private set; }

#if NETSTANDARD
        protected void Lock(CSRedisClient client, string token, TimeSpan timeout, Action action)
        {
            CSRedisClientLock locker;
            while ((locker = client.Lock(token, (int)timeout.TotalSeconds)) == null)
            {
                Thread.Sleep(10);
            }

            try
            {
                action();
            }
            catch
            {
                throw;
            }
            finally
            {
                locker.Dispose();
            }
        }

        protected T Lock<T>(CSRedisClient client, string token, TimeSpan timeout, Func<T> func)
        {
            CSRedisClientLock locker;
            while ((locker = client.Lock(token, (int)timeout.TotalSeconds)) == null)
            {
                Thread.Sleep(10);
            }

            T result;
            try
            {
                result = func();
            }
            catch
            {
                throw;
            }
            finally
            {
                locker.Dispose();
            }

            return result;
        }

        protected async Task LockAsync(CSRedisClient client, string token, TimeSpan timeout, Task task)
        {
            CSRedisClientLock locker;
            while ((locker = client.Lock(token, (int)timeout.TotalSeconds)) == null)
            {
                Thread.Sleep(10);
            }

            try
            {
                await task;
            }
            catch
            {
                throw;
            }
            finally
            {
                locker.Dispose();
            }
        }

        protected async Task<T> LockAsync<T>(CSRedisClient client, string token, TimeSpan timeout, Func<Task<T>> func)
        {
            CSRedisClientLock locker;
            while ((locker = client.Lock(token, (int)timeout.TotalSeconds)) == null)
            {
                Thread.Sleep(10);
            }

            T result;
            try
            {
                result = await func();
            }
            catch
            {
                throw;
            }
            finally
            {
                locker.Dispose();
            }

            return result;
        }
#else
        protected void Lock(IDatabase db, string token, TimeSpan timeout, Action action)
        {
            var lockValue = $"{token}:LOCK_TOKEN";

            while (true)
            {
                if (db.LockTake(lockValue, token, timeout))
                {
                    try
                    {
                        action();
                    }
                    finally
                    {
                        db.LockRelease(lockValue, token);
                    }
                }
            }
        }

        protected T Lock<T>(IDatabase db, string token, TimeSpan timeout, Func<T> func)
        {
            var lockValue = $"{token}:LOCK_TOKEN";

            while (true)
            {
                if (db.LockTake(lockValue, token, timeout))
                {
                    try
                    {
                        return func();
                    }
                    finally
                    {
                        db.LockRelease(lockValue, token);
                    }
                }
            }
        }

        protected async Task LockAsync(IDatabase db, string token, TimeSpan timeout, Task task)
        {
            var lockValue = $"{token}:LOCK_TOKEN";

            while (true)
            {
                if (await db.LockTakeAsync(lockValue, token, timeout))
                {
                    try
                    {
                        await task;
                    }
                    finally
                    {
                        await db.LockReleaseAsync(lockValue, token);
                    }
                }
            }
        }

        protected async Task<T> LockAsync<T>(IDatabase db, string token, TimeSpan timeout, Func<Task<T>> func)
        {
            var lockValue = $"{token}:LOCK_TOKEN";

            while (true)
            {
                if (await db.LockTakeAsync(lockValue, token, timeout))
                {
                    try
                    {
                        return await func();
                    }
                    finally
                    {
                        await db.LockReleaseAsync(lockValue, token);
                    }
                }
            }
        }

#endif

        /// <summary>
        /// 序列化对象。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual T Deserialize<T>(string value)
        {
            var serializer = CreateSerializer();
            return serializer.Deserialize<T>(value);
        }

        /// <summary>
        /// 序列化对象。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual object Deserialize(Type type, string value)
        {
            var serializer = CreateSerializer();
            return serializer.Deserialize(type, value);
        }

        /// <summary>
        /// 反序列化。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual string Serialize<T>(T value)
        {
            var serializer = CreateSerializer();
            return serializer.Serialize(value);
        }

        private RedisSerializer CreateSerializer()
        {
            if (Setting.SerializerType != null)
            {
                var serializer = Setting.SerializerType.New<RedisSerializer>();
                if (serializer != null)
                {
                    return serializer;
                }
            }

            return new RedisSerializer();
        }

#if NETSTANDARD
        protected CSRedisClient GetConnection()
        {
            return connectionLazy.Value;
        }

#else
        protected ConnectionMultiplexer GetConnection()
        {
            return connectionLazy.Value;
        }

        protected IDatabase GetDb(IConnectionMultiplexer conn)
        {
            return conn.GetDatabase(Options.DefaultDatabase ?? 0);
        }
#endif

        void IConfigurationSettingHostService.Attach(IConfigurationSettingItem setting)
        {
            this.Setting = (RedisConfigurationSetting)setting;

#if NETSTANDARD
            var connectionStrs = new List<string>();
            if (!string.IsNullOrEmpty(this.Setting.ConnectionString))
            {
                connectionStrs.Add(this.Setting.ConnectionString);
            }
            else
            {

                foreach (var host in this.Setting.Hosts)
                {
                    var connStr = new StringBuilder($"{host.Server}");

#region connection build
                    if (host.Port != 0)
                    {
                        connStr.Append($":{host.Port}");
                    }

                    if (!string.IsNullOrEmpty(this.Setting.Password))
                    {
                        connStr.Append($",password={this.Setting.Password}");
                    }

                    if (this.Setting.DefaultDb != 0)
                    {
                        connStr.Append($",defaultDatabase={this.Setting.DefaultDb}");
                    }

                    if (this.Setting.Ssl)
                    {
                        connStr.Append($",ssl=true");
                    }

                    if (this.Setting.WriteBuffer != null)
                    {
                        connStr.Append($",writeBuffer={this.Setting.WriteBuffer}");
                    }

                    if (this.Setting.PoolSize != null)
                    {
                        connStr.Append($",poolsize={this.Setting.PoolSize}");
                    }

                    connStr.Append(",allowAdmin=true");
#endregion

                    connectionStrs.Add(connStr.ToString());
                }
            }

            if (connectionLazy == null)
            {
                connectionLazy = new Lazy<CSRedisClient>(() => new CSRedisClient(null, connectionStrs.ToArray()));
            }
#else
            if (!string.IsNullOrEmpty(this.Setting.ConnectionString))
            {
                Options = ConfigurationOptions.Parse(this.Setting.ConnectionString);
            }
            else
            {
                Options = new ConfigurationOptions
                {
                    DefaultDatabase = this.Setting.DefaultDb,
                    Password = this.Setting.Password,
                    AllowAdmin = true,
                    Ssl = this.Setting.Ssl,
                    Proxy = this.Setting.Twemproxy ? Proxy.Twemproxy : Proxy.None,
                    AbortOnConnectFail = false
                };

                if (this.Setting.WriteBuffer != null)
                {
                    Options.WriteBuffer = (int)this.Setting.WriteBuffer;
                }

                foreach (var host in this.Setting.Hosts)
                {
                    if (host.Port == 0)
                    {
                        Options.EndPoints.Add(host.Server);
                    }
                    else
                    {
                        Options.EndPoints.Add(host.Server, host.Port);
                    }
                }
            }

            if (connectionLazy == null)
            {
                connectionLazy = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(Options));
            }
#endif
        }

        IConfigurationSettingItem IConfigurationSettingHostService.GetSetting()
        {
            return Setting;
        }
    }
}
