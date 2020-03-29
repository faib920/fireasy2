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
    public class Serializer : ITextSerializer
    {
        private Newton.JsonSerializerSettings settings;

        public Serializer()
        {
        }

#if NETSTANDARD
        public Serializer(IOptions<Newton.JsonSerializerSettings> options)
        {
            settings = options.Value;
        }
#endif

        public T Deserialize<T>(byte[] bytes)
        {
            return Newton.JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(bytes), settings);
        }

        public object Deserialize(byte[] bytes, Type type)
        {
            return Newton.JsonConvert.DeserializeObject(Encoding.UTF8.GetString(bytes), type, settings);
        }

        public T Deserialize<T>(string content)
        {
            return Newton.JsonConvert.DeserializeObject<T>(content, settings);
        }

        public T Deserialize<T>(string content, T anyObj)
        {
            return Newton.JsonConvert.DeserializeObject<T>(content, settings);
        }

        public object Deserialize(string content, Type type)
        {
            return Newton.JsonConvert.DeserializeObject(content, type, settings);
        }

        byte[] ISerializer.Serialize<T>(T value)
        {
            var content = Newton.JsonConvert.SerializeObject(value, settings);
            return Encoding.UTF8.GetBytes(content);
        }

        public string Serialize<T>(T value)
        {
            return Newton.JsonConvert.SerializeObject(value, settings);
        }
    }
}
