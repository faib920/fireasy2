using System;

namespace Fireasy.Common.Serialization
{
    /// <summary>
    /// Json 转换器的抽象类。
    /// </summary>
    public abstract class JsonConverter : ITextConverter
    {
        /// <summary>
        /// 判断指定的类型是否允许转换。
        /// </summary>
        /// <param name="type">要判断的类型。</param>
        /// <returns>可以转换则为 true。</returns>
        public abstract bool CanConvert(Type type);

        /// <summary>
        /// 获取是否可以使用 ReadJson 方法。
        /// </summary>
        public virtual bool CanRead
        {
            get { return true; }
        }

        /// <summary>
        /// 获取是否可使用 WriteJson 方法。
        /// </summary>
        public virtual bool CanWrite
        {
            get { return true; }
        }

        /// <summary>
        /// 获取是否使用流对象方式，即使用 <see cref="JsonReader"/> 和 <see cref="JsonWriter"/> 对象，默认为 false。
        /// </summary>
        public virtual bool Streaming
        {
            get { return false; }
        }

        /// <summary>
        /// 将对象写为 Json 文本。
        /// </summary>
        /// <param name="serializer">一个 <see cref="JsonSerializer"/> 对象。</param>
        /// <param name="obj">要序列化的对象。</param>
        /// <returns>表示对象的 Json 文本。</returns>
        public virtual string WriteJson(JsonSerializer serializer, object obj)
        {
            return string.Empty;
        }

        /// <summary>
        /// 将对象写为 Json 文本。
        /// </summary>
        /// <param name="serializer">一个 <see cref="JsonSerializer"/> 对象。</param>
        /// <param name="writer"></param>
        public virtual void WriteJson(JsonSerializer serializer, JsonWriter writer, object obj)
        {
        }

        string ITextConverter.WriteObject(ITextSerializer serializer, object obj)
        {
            return WriteJson((JsonSerializer)serializer, obj);
        }

        /// <summary>
        /// 从 Json 中读取对象。
        /// </summary>
        /// <param name="serializer">一个 <see cref="JsonSerializer"/> 对象。</param>
        /// <param name="dataType">将要读取的类型。</param>
        /// <param name="json">表示对象的 Json 文本。</param>
        /// <returns>反序列化后的对象。</returns>
        public virtual object ReadJson(JsonSerializer serializer, Type dataType, string json)
        {
            return null;
        }

        /// <summary>
        /// 从 Json 中读取对象。
        /// </summary>
        /// <param name="serializer">一个 <see cref="JsonSerializer"/> 对象。</param>
        /// <param name="reader"></param>
        /// <param name="dataType">将要读取的类型。</param>
        /// <returns>反序列化后的对象。</returns>
        public virtual object ReadJson(JsonSerializer serializer, JsonReader reader, Type dataType)
        {
            return null;
        }

        object ITextConverter.ReadObject(ITextSerializer serializer, Type dataType, string text)
        {
            return ReadJson((JsonSerializer)serializer, dataType, text);
        }
    }

    /// <summary>
    /// 基于 <typeparamref name="T"/> 类型提供的 Json 转换器。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class JsonConverter<T> : JsonConverter
    {
        /// <summary>
        /// 判断指定的类型是否允许转换。
        /// </summary>
        /// <param name="type">要判断的类型。</param>
        /// <returns>可以转换则为 true。</returns>
        public override bool CanConvert(Type type)
        {
            return typeof(T).IsAssignableFrom(type);
        }
    }
}
