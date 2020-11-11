// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Serialization;
#if NETSTANDARD
using Microsoft.Extensions.Options;
#endif
using System;
using System.Text;
using Newton = Newtonsoft.Json;

namespace Fireasy.Newtonsoft
{
    public class JsonSerializer : ITextSerializer
    {
        private Newton.JsonSerializerSettings _settings;

        public JsonSerializer()
        {
        }

#if NETSTANDARD
        public JsonSerializer(IOptions<Newton.JsonSerializerSettings> options)
        {
            _settings = options.Value;
        }
#endif

        public T Deserialize<T>(byte[] bytes)
        {
            return Newton.JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(bytes), _settings);
        }

        public object Deserialize(byte[] bytes, Type type)
        {
            return Newton.JsonConvert.DeserializeObject(Encoding.UTF8.GetString(bytes), type, _settings);
        }

        public T Deserialize<T>(string content)
        {
            return Newton.JsonConvert.DeserializeObject<T>(content, _settings);
        }

        public T Deserialize<T>(string content, T anyObj)
        {
            return Newton.JsonConvert.DeserializeObject<T>(content, _settings);
        }

        public object Deserialize(string content, Type type)
        {
            return Newton.JsonConvert.DeserializeObject(content, type, _settings);
        }

        byte[] ISerializer.Serialize<T>(T value)
        {
            var content = Newton.JsonConvert.SerializeObject(value, _settings);
            return Encoding.UTF8.GetBytes(content);
        }

        public string Serialize<T>(T value)
        {
            return Newton.JsonConvert.SerializeObject(value, _settings);
        }
    }
}
