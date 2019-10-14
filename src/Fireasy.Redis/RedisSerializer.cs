// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Serialization;
using System;

namespace Fireasy.Redis
{
    /// <summary>
    /// 序列化器。
    /// </summary>
    public class RedisSerializer
    {
        /// <summary>
        /// 序列化对象。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public virtual string Serialize<T>(T obj)
        {
            var option = new JsonSerializeOption();
            option.IgnoreNull = false;
            option.Converters.Add(new FullDateTimeJsonConverter());
            var serializer = new JsonSerializer(option);
            return serializer.Serialize(obj);
        }

        /// <summary>
        /// 反序列化。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <returns></returns>
        public virtual T Deserialize<T>(string str)
        {
            return (T)Deserialize(typeof(T), str);
        }

        /// <summary>
        /// 反序列化。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        public virtual object Deserialize(Type type, string str)
        {
            var option = new JsonSerializeOption();
            option.Converters.Add(new FullDateTimeJsonConverter());
            var serializer = new JsonSerializer(option);
            return serializer.Deserialize(str, type);
        }
    }
}
