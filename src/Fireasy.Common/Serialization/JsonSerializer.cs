// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Fireasy.Common.Serialization
{
    /// <summary>
    /// Json 文本序列化器。
    /// </summary>
    public class JsonSerializer : ITextSerializer
    {
        /// <summary>
        /// 初始化 <see cref="JsonSerializer"/> 类的新实例。
        /// </summary>
        /// <param name="option">序列化选项。</param>
        public JsonSerializer(JsonSerializeOption option = null)
        {
            Option = option ?? new JsonSerializeOption();
        }

        /// <summary>
        /// 获取序列化选项。
        /// </summary>
        public JsonSerializeOption Option { get; private set; }

        /// <summary>
        /// 将对象转换为使用文本表示。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">要序列化的对象。</param>
        /// <returns>表示对象的 Json 文本。</returns>
        [SuppressMessage("Microsoft.Usage", "CA2202")]
        public string Serialize<T>(T value)
        {
            using (var sw = new StringWriter(CultureInfo.InvariantCulture))
            using (var writer = new JsonWriter(sw))
            using (var ser = new JsonSerialize(this, writer, Option))
            using (var context = new JsonConvertContext())
            {
                if (Option.Indent)
                {
                    writer.Indent = 4;
                }

                ser.Serialize(value);
                return sw.ToString();
            }
        }

        /// <summary>
        /// 将对象转换为使用文本并写入到流中。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="writer"></param>
        public void Serialize<T>(T value, JsonWriter writer)
        {
            var ser = new JsonSerialize(this, writer, Option);
            using (var context = new JsonConvertContext())
            {
                ser.Serialize(value);
            }
        }

        /// <summary>
        /// 从 Json 文本中解析出类型 <typeparamref name="T"/> 的对象。
        /// </summary>
        /// <typeparam name="T">可序列化的对象类型。</typeparam>
        /// <param name="json">表示对象的 Json 文本。</param>
        /// <returns>对象。</returns>
        [SuppressMessage("Microsoft.Usage", "CA2202")]
        public T Deserialize<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return default(T);
            }

            using (var sr = new StringReader(json))
            using (var reader = new JsonReader(sr))
            using (var deser = new JsonDeserialize(this, reader, Option))
            using (var context = new JsonConvertContext())
            {
                return deser.Deserialize<T>();
            }
        }

        /// <summary>
        /// 从流中读取文本，解析出类型 <paramref name="type"/> 的对象。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        public T Deserialize<T>(JsonReader reader)
        {
            var deser = new JsonDeserialize(this, reader, Option);
            using (var context = new JsonConvertContext())
            {
                return deser.Deserialize<T>();
            }
        }

        /// <summary>
        /// 从 Json 文本中解析出类型 <paramref name="type"/> 的对象。
        /// </summary>
        /// <param name="json">表示对象的 Json 文本。</param>
        /// <param name="type">可序列化的对象类型。</param>
        /// <returns>对象。</returns>
        [SuppressMessage("Microsoft.Usage", "CA2202")]
        public object Deserialize(string json, Type type)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            using (var sr = new StringReader(json))
            using (var reader = new JsonReader(sr))
            using (var deser = new JsonDeserialize(this, reader, Option))
            using (var context = new JsonConvertContext())
            {
                return deser.Deserialize(type);
            }
        }

        /// <summary>
        /// 从 Json 文本中解析出类型 <typeparamref name="T"/> 的对象，<typeparamref name="T"/> 可以是匿名类型。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json">表示对象的 Json 文本</param>
        /// <param name="anyObj">可序列化的匿名类型。</param>
        /// <returns>对象。</returns>
        public T Deserialize<T>(string json, T anyObj)
        {
            using (var context = new JsonConvertContext())
            {
                return Deserialize<T>(json);
            }
        }
    }
}
