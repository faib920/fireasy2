// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Serialization;
using System;
using System.Text;

namespace Fireasy.Kafka
{
    /// <summary>
    /// 序列化器。
    /// </summary>
    public class KafkaSerializer
    {
        /// <summary>
        /// 序列化对象。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public virtual byte[] Serialize<T>(T obj)
        {
            var option = new JsonSerializeOption();
            option.Converters.Add(new FullDateTimeJsonConverter());
            var serializer = new JsonSerializer(option);
            return Encoding.UTF8.GetBytes(serializer.Serialize(obj));
        }

        /// <summary>
        /// 反序列化。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual T Deserialize<T>(byte[] data)
        {
            return (T)Deserialize(typeof(T), data);
        }

        /// <summary>
        /// 反序列化。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual object Deserialize(Type type, byte[] data)
        {
            var option = new JsonSerializeOption();
            option.Converters.Add(new FullDateTimeJsonConverter());
            var serializer = new JsonSerializer(option);
            return serializer.Deserialize(Encoding.UTF8.GetString(data), type);
        }
    }
}
