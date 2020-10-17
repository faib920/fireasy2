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
using System.Xml;

namespace Fireasy.Common.Serialization
{
    /// <summary>
    /// Xml 转换器的抽象类。
    /// </summary>
    public abstract class XmlConverter : ITextConverter
    {
        /// <summary>
        /// 获取当前的属性信息。
        /// </summary>
        public PropertySerialzeInfo SerializeInfo
        {
            get
            {
                return SerializeContext.Current != null ? SerializeContext.Current.SerializeInfo : null;
            }
        }

        /// <summary>
        /// 判断指定的类型是否允许转换。
        /// </summary>
        /// <param name="type">要判断的类型。</param>
        /// <returns>可以转换则为 true。</returns>
        public abstract bool CanConvert(Type type);

        /// <summary>
        /// 获取是否可以使用 ReadXml 方法。
        /// </summary>
        public virtual bool CanRead
        {
            get { return true; }
        }

        /// <summary>
        /// 获取是否可使用 WriteXml 方法。
        /// </summary>
        public virtual bool CanWrite
        {
            get { return true; }
        }

        /// <summary>
        /// 获取是否使用流对象方式，即使用 <see cref="XmlReader"/> 和 <see cref="XmlWriter"/> 对象，默认为 false。
        /// </summary>
        public virtual bool Streaming
        {
            get { return false; }
        }

        /// <summary>
        /// 将对象写为 Xml 文本。
        /// </summary>
        /// <param name="serializer">一个 <see cref="XmlSerializer"/> 对象。</param>
        /// <param name="writer"></param>
        /// <param name="obj">要序列化的对象。</param>
        public virtual void WriteXml(XmlSerializer serializer, XmlWriter writer, object obj)
        {
        }

        string ITextConverter.WriteObject(ITextSerializer serializer, object obj)
        {
            using (var sw = new StringWriter(CultureInfo.InvariantCulture))
            using (var writer = new XmlTextWriter(sw))
            {
                WriteXml((XmlSerializer)serializer, writer, obj);
                return sw.ToString();
            }
        }

        /// <summary>
        /// 从 Xml 中读取对象。
        /// </summary>
        /// <param name="serializer">一个 <see cref="XmlSerializer"/> 对象。</param>
        /// <param name="reader"></param>
        /// <param name="dataType">将要读取的类型。</param>
        /// <returns>反序列化后的对象。</returns>
        public virtual object ReadXml(XmlSerializer serializer, XmlReader reader, Type dataType)
        {
            return null;
        }

        object ITextConverter.ReadObject(ITextSerializer serializer, Type dataType, string text)
        {
            using (var sr = new StringReader(text))
            using (var reader = new XmlTextReader(sr))
            {
                return ReadXml((XmlSerializer)serializer, reader, dataType);
            }
        }
    }

    /// <summary>
    /// 基于 <typeparamref name="T"/> 类型提供的 Xml 转换器。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class XmlConverter<T> : XmlConverter
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