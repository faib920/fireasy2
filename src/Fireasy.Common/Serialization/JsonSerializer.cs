// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Fireasy.Common.Serialization
{
    /// <summary>
    /// Json 文本序列化器。
    /// </summary>
    public class JsonSerializer : ITextSerializer<JsonSerializeOption>
    {
        /// <summary>
        /// 初始化 <see cref="JsonSerializer"/> 类的新实例。
        /// </summary>
        /// <param name="option">序列化选项。</param>
        public JsonSerializer(JsonSerializeOption option = null)
        {
            Option = option ?? JsonSerializeOption.Default;
        }

        /// <summary>
        /// 获取或设置序列化选项。
        /// </summary>
        public JsonSerializeOption Option { get; set; }

        SerializeOption ISerializer.Option { get => Option; set => Option = (JsonSerializeOption)value; }

        /// <summary>
        /// 将对象转换为使用文本表示。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">要序列化的对象。</param>
        /// <returns>表示对象的 Json 文本。</returns>
        public string Serialize<T>(T value)
        {
            using var sw = new StringWriter(CultureInfo.InvariantCulture);
            using var writer = new JsonWriter(sw);
            using var ser = new JsonSerialize(this, writer, Option);
            if (Option.Indent)
            {
                writer.Indent = 4;
            }

            ser.Serialize(value);
            return sw.ToString();
        }

        byte[] ISerializer.Serialize<T>(T value)
        {
            return Encoding.UTF8.GetBytes(Serialize(value));
        }

        /// <summary>
        /// 将对象转换为使用文本并写入到流中。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="writer"></param>
        public void Serialize<T>(T value, JsonWriter writer)
        {
            using var ser = new JsonSerialize(this, writer, Option);
            ser.Serialize(value);
        }

        /// <summary>
        /// 从 Json 文本中解析出类型 <typeparamref name="T"/> 的对象。
        /// </summary>
        /// <typeparam name="T">可序列化的对象类型。</typeparam>
        /// <param name="json">表示对象的 Json 文本。</param>
        /// <returns>对象。</returns>
        public T Deserialize<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return default;
            }

            using var sr = new StringReader(json);
            using var reader = new JsonReader(sr);
            using var deser = new JsonDeserialize(this, reader, Option);
            return deser.Deserialize<T>();
        }

        T ISerializer.Deserialize<T>(byte[] bytes)
        {
            return Deserialize<T>(Encoding.UTF8.GetString(bytes));
        }

        object ISerializer.Deserialize(byte[] bytes, Type type)
        {
            return Deserialize(Encoding.UTF8.GetString(bytes), type);
        }

        /// <summary>
        /// 从流中读取文本，解析出类型 <paramref name="type"/> 的对象。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        public T Deserialize<T>(JsonReader reader)
        {
            using var deser = new JsonDeserialize(this, reader, Option);
            return deser.Deserialize<T>();
        }

        /// <summary>
        /// 从 Json 文本中解析出类型 <paramref name="type"/> 的对象。
        /// </summary>
        /// <param name="json">表示对象的 Json 文本。</param>
        /// <param name="type">可序列化的对象类型。</param>
        /// <returns>对象。</returns>
        public object Deserialize(string json, Type type)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            using var sr = new StringReader(json);
            using var reader = new JsonReader(sr);
            using var deser = new JsonDeserialize(this, reader, Option);
            return deser.Deserialize(type);
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
            return Deserialize<T>(json);
        }
    }
}
