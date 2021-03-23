// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using CSRedis;
using Fireasy.Common;
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
using Fireasy.Common.Ioc;
using Fireasy.Common.Serialization;
using Fireasy.Common.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Fireasy.Redis
{
    /// <summary>
    /// Redis 组件抽象类。
    /// </summary>
    public abstract class RedisComponent : DisposableBase, IConfigurationSettingHostService, IServiceProviderAccessor
    {
        private List<int> _dbRanage;
        private Func<string, string> _captureRule;
        private Func<ISerializer> _serializerFactory;
        private readonly List<string> _connectionStrs = new List<string>();
        private readonly SafetyDictionary<int, CSRedisClient> _clients = new SafetyDictionary<int, CSRedisClient>();

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

        protected virtual CSRedisClient CreateClient(List<string> constrs)
        {
            if (Setting.Sentinels.Count > 0)
            {
                return new CSRedisClient(constrs[0], Setting.Sentinels.Select(s => s.ToString()).ToArray());
            }

            return new CSRedisClient(null, constrs.ToArray());
        }

        protected CSRedisClient GetConnection(string key = null)
        {
            if (string.IsNullOrEmpty(key) || _dbRanage == null)
            {
                return _clients[0] ?? throw new InvalidOperationException("不能初始化 Redis 的连接。");
            }

            var ckey = _captureRule == null ? key : _captureRule(key);
            var index = GetModulus(ckey, _dbRanage.Count);

            Tracer.Debug($"Select redis db{_dbRanage[index]} for the key '{key}'.");

            return _clients.GetOrAdd(index, k =>
            {
                var constrs = _connectionStrs.Select(s => string.Concat(s, $",defaultDatabase={_dbRanage[k]}")).ToList();
                constrs.ForEach(s =>
                {
                    Tracer.Debug($"Connecting to the redis server '{s}'.");
                });
                return CreateClient(constrs);
            });
        }

        protected IEnumerable<CSRedisClient> GetConnections()
        {
            return _clients.Values;
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
                _connectionStrs.Add(Setting.ConnectionString);
            }
            else
            {
                var sb = new StringBuilder();
                if (!string.IsNullOrEmpty(Setting.Password))
                {
                    sb.Append($",password={Setting.Password}");
                }

                if (Setting.DefaultDb != 0 && string.IsNullOrEmpty(Setting.DbRange))
                {
                    sb.Append($",defaultDatabase={Setting.DefaultDb}");
                }

                if (Setting.Ssl)
                {
                    sb.Append($",ssl=true");
                }

                if (Setting.WriteBuffer != null)
                {
                    sb.Append($",writeBuffer={Setting.WriteBuffer}");
                }

                if (Setting.PoolSize != null)
                {
                    sb.Append($",poolsize={Setting.PoolSize}");
                }

                if (!string.IsNullOrEmpty(Setting.Prefix))
                {
                    sb.Append($",prefix={Setting.Prefix}");
                }

                if (Setting.ConnectTimeout.Milliseconds != 5000)
                {
                    sb.Append($",connectTimeout={Setting.ConnectTimeout.Milliseconds}");
                }

                if (Setting.SyncTimeout.Milliseconds != 10000)
                {
                    sb.Append($",syncTimeout={Setting.SyncTimeout.Milliseconds}");
                }

                sb.Append(",allowAdmin=true");

                foreach (var host in Setting.Hosts)
                {
                    var sb1 = new StringBuilder($"{host.Server}");

                    #region connection build
                    if (host.Port != 0)
                    {
                        sb1.Append($":{host.Port}");
                    }

                    if (sb.Length > 0)
                    {
                        sb1.Append(sb.ToString());
                    }
                    #endregion

                    _connectionStrs.Add(sb1.ToString());
                }
            }

            if (string.IsNullOrEmpty(Setting.DbRange))
            {
                _clients.GetOrAdd(0, () => CreateClient(_connectionStrs));
            }
            else
            {
                ParseDbRange(Setting.DbRange);
                if (!string.IsNullOrEmpty(Setting.KeyRule))
                {
                    ParseKeyCapture(Setting.KeyRule);
                }
            }

            OnInitialize();
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

        private ISerializer GetSerializer()
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
            foreach (var client in _clients)
            {
                client.Value.Dispose();
            }

            _clients.Clear();
            return base.Dispose(disposing);
        }
    }
}
