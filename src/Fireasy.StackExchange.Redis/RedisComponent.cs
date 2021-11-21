// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Fireasy.Common;
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
using Fireasy.Common.Ioc;
using Fireasy.Common.Serialization;
using Fireasy.Common.Threading;
using StackExchange.Redis;

namespace Fireasy.Redis
{
    /// <summary>
    /// Redis 组件抽象类。
    /// </summary>
    public abstract class RedisComponent : DisposableBase, IConfigurationSettingHostService, IServiceProviderAccessor, IServiceProvider
    {
        private List<int> _dbRanage;
        private Func<string, string> _captureRule;
        private Func<ISerializer> _serializerFactory;
        private Lazy<ConnectionMultiplexer> _connectionLazy;

        protected RedisComponent()
        {
            ServiceProvider = ContainerUnity.GetContainer();
        }

        protected RedisComponent(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        /// <summary>
        /// 获取或设置应用程序服务提供者实例。
        /// </summary>
        public IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// 获取 Redis 的相关配置。
        /// </summary>
        protected RedisConfigurationSetting Setting { get; private set; }

        protected ConfigurationOptions Options { get; private set; }

        object IServiceProvider.GetService(Type serviceType)
        {
            if (serviceType == typeof(ConnectionMultiplexer))
            {
                return _connectionLazy.Value;
            }

            return null;
        }

        /// <summary>
        /// 反序列化。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual T Deserialize<T>(string value)
        {
            var serializer = GetSerializer();
            if (serializer is ITextSerializer txtSerializer)
            {
                return txtSerializer.Deserialize<T>(value);
            }

            return serializer.Deserialize<T>(Encoding.UTF8.GetBytes(value));
        }

        /// <summary>
        /// 反序列化。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <returns></returns>
        protected virtual T Deserialize<T>(byte[] bytes)
        {
            var serializer = GetSerializer();
            return serializer.Deserialize<T>(bytes);
        }

        /// <summary>
        /// 反序列化。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual object Deserialize(Type type, string value)
        {
            var serializer = GetSerializer();
            if (serializer is ITextSerializer txtSerializer)
            {
                return txtSerializer.Deserialize(value, type);
            }

            return serializer.Deserialize(Encoding.UTF8.GetBytes(value), type);
        }

        /// <summary>
        /// 反序列化。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        protected virtual object Deserialize(Type type, byte[] bytes)
        {
            var serializer = GetSerializer();
            return serializer.Deserialize(bytes, type);
        }

        /// <summary>
        /// 序列化。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual string Serialize<T>(T value)
        {
            var serializer = GetSerializer();
            if (serializer is ITextSerializer txtSerializer)
            {
                return txtSerializer.Serialize(value);
            }

            return Encoding.UTF8.GetString(serializer.Serialize(value));
        }

        /// <summary>
        /// 序列化。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual byte[] SerializeToBytes<T>(T value)
        {
            var serializer = GetSerializer();
            return serializer.Serialize(value);
        }

        /// <summary>
        /// 创建 <see cref="ConnectionMultiplexer"/> 实例。
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        protected virtual ConnectionMultiplexer CreateConnection(ConfigurationOptions options)
        {
            return ConnectionMultiplexer.Connect(Options);
        }

        protected IConnectionMultiplexer GetConnection()
        {
            return _connectionLazy.Value ?? throw new InvalidOperationException("不能初始化 Redis 的连接。");
        }

        protected IDatabase GetDatabase(string key)
        {
            var client = _connectionLazy.Value;
            if (_dbRanage == null)
            {
                return client.GetDatabase(Setting.DefaultDb);
            }

            var ckey = _captureRule == null ? key : _captureRule(key);
            var index = GetModulus(ckey, _dbRanage.Count);

            Tracer.Debug($"Select redis db{_dbRanage[index]} for the key '{key}'.");

            return client.GetDatabase(_dbRanage[index]);
        }

        protected IEnumerable<IDatabase> GetDatabases()
        {
            var client = _connectionLazy.Value;
            if (_dbRanage == null)
            {
                yield return client.GetDatabase(Setting.DefaultDb);
            }
            else
            {
                foreach (var index in _dbRanage)
                {
                    yield return client.GetDatabase(index);
                }
            }
        }

        protected virtual void OnInitialize()
        {
        }

        void IConfigurationSettingHostService.Attach(IConfigurationSettingItem setting)
        {
            if (Setting != null)
            {
                return;
            }

            Setting = (RedisConfigurationSetting)setting;

            if (!string.IsNullOrEmpty(Setting.ConnectionString))
            {
                Options = ConfigurationOptions.Parse(Setting.ConnectionString);
            }
            else
            {
                Options = new ConfigurationOptions
                {
                    //DefaultDatabase = Setting.DefaultDb == 0 || !string.IsNullOrEmpty(Setting.DbRange) ? (int?)null : Setting.DefaultDb,
                    Password = Setting.Password,
                    AllowAdmin = true,
                    Ssl = Setting.Ssl,
                    AbortOnConnectFail = false,
                };

                if (Setting.Twemproxy != null)
                {
                    Options.Proxy = Setting.Twemproxy == true ? Proxy.Twemproxy : Proxy.None;
                }

                if (Setting.WriteBuffer != null)
                {
                    Options.WriteBuffer = (int)Setting.WriteBuffer;
                }

                if (Setting.ConnectTimeout.TotalMilliseconds != 5000)
                {
                    Options.ConnectTimeout = (int)Setting.ConnectTimeout.TotalMilliseconds;
                }

                if (Setting.SyncTimeout.TotalMilliseconds != 10000)
                {
                    Options.SyncTimeout = (int)Setting.SyncTimeout.TotalMilliseconds;
                }

                foreach (var host in Setting.Hosts)
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

            if (!string.IsNullOrEmpty(Setting.DbRange))
            {
                ParseDbRange(Setting.DbRange);
            }

            if (!string.IsNullOrEmpty(Setting.KeyRule))
            {
                ParseKeyCapture(Setting.KeyRule);
            }

            if (_connectionLazy == null)
            {
                _connectionLazy = new Lazy<ConnectionMultiplexer>(() => CreateConnection(Options));
            }

            ConvertEndPoints();
            SetMinThreads();
            OnInitialize();
        }

        private void ConvertEndPoints()
        {
            var ipEndPoints = new EndPointCollection();
            foreach (var endpoint in Options.EndPoints)
            {
                if (endpoint is not DnsEndPoint dnsPoint)
                {
                    continue;
                }

                var ipAddresses = Dns.GetHostAddresses(dnsPoint.Host);
                foreach (var ipAddress in ipAddresses)
                {
                    ipEndPoints.Add(new IPEndPoint(ipAddress, dnsPoint.Port));
                }
            }

            if (ipEndPoints.Count > 0)
            {
                Options.EndPoints.Clear();
                foreach (var ipPoint in ipEndPoints)
                {
                    Options.EndPoints.Add(ipPoint);
                }
            }
        }

        private void SetMinThreads()
        {
            if (Setting.MinIoThreads > 0)
            {
                //当最小线程数小于配置，才使用配置
                ThreadPool.GetMinThreads(out int minIoThreads, out int minIOC);
                minIoThreads = Setting.MinIoThreads > minIoThreads ? Setting.MinIoThreads : minIoThreads;
                minIOC = Setting.MinIoThreads > minIOC ? Setting.MinIoThreads : minIOC;
                if (!ThreadPool.SetMinThreads(minIoThreads, minIOC))
                {
                    throw new Exception("Redis thread pool error!");
                }
            }
        }

        IConfigurationSettingItem IConfigurationSettingHostService.GetSetting()
        {
            return Setting;
        }

        private int GetModulus(string str, int mod)
        {
            var hash = 0;
            foreach (var c in str)
            {
                hash = (128 * hash % (mod * 8)) + c;
            }

            return hash % mod;
        }

        private void ParseDbRange(string range)
        {
            _dbRanage = new List<int>();
            foreach (var s1 in range.Split(','))
            {
                int p;
                if ((p = s1.IndexOf('-')) != -1)
                {
                    var start = s1.Substring(0, p).To<int>();
                    var end = s1.Substring(p + 1).To<int>();
                    for (var i = start; i <= end; i++)
                    {
                        _dbRanage.Add(i);
                    }
                }
                else
                {
                    _dbRanage.Add(s1.To<int>());
                }
            }
        }

        private void ParseKeyCapture(string rule)
        {
            if (rule.StartsWith("left", StringComparison.CurrentCultureIgnoreCase))
            {
                var match = Regex.Match(rule, @"(\d+)");
                var length = match.Groups[1].Value.To<int>();
                _captureRule = new Func<string, string>(k => k.Left(length));
            }
            if (rule.StartsWith("right", StringComparison.CurrentCultureIgnoreCase))
            {
                var match = Regex.Match(rule, @"(\d+)");
                var length = match.Groups[1].Value.To<int>();
                _captureRule = new Func<string, string>(k => k.Right(length));
            }
            else if (rule.StartsWith("substr", StringComparison.CurrentCultureIgnoreCase))
            {
                var match = Regex.Match(rule, @"(\d+),\s*(\d+)");
                var start = match.Groups[1].Value.To<int>();
                var length = match.Groups[2].Value.To<int>();
                _captureRule = new Func<string, string>(k => k.Substring(start, start + length > k.Length ? k.Length - start : length));
            }
        }

        protected ISerializer GetSerializer()
        {
            return SingletonLocker.Lock(ref _serializerFactory, this, () =>
            {
                return new Func<ISerializer>(() =>
                {
                    ISerializer _serializer = null;
                    if (Setting.SerializerType != null)
                    {
                        _serializer = Setting.SerializerType.New<ISerializer>();
                    }

                    if (_serializer == null)
                    {
                        _serializer = ServiceProvider.TryGetService(() => SerializerFactory.CreateSerializer());
                    }

                    if (_serializer == null)
                    {
                        var option = new JsonSerializeOption();
                        option.Converters.Add(new FullDateTimeJsonConverter());
                        _serializer = new JsonSerializer(option);
                    }

                    return _serializer;
                });
            })();
        }

        protected override bool Dispose(bool disposing)
        {
            if (_connectionLazy != null && _connectionLazy.IsValueCreated)
            {
                _connectionLazy.Value.Dispose();
            }

            return base.Dispose(disposing);
        }
    }
}
