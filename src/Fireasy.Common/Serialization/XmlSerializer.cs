using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;

namespace Fireasy.Common.Serialization
{
    public class XmlSerializer : ITextSerializer
    {
        /// <summary>
        /// 初始化 <see cref="XmlSerializer"/> 类的新实例。
        /// </summary>
        /// <param name="option">序列化选项。</param>
        public XmlSerializer(XmlSerializeOption option = null)
        {
            Option = option ?? new XmlSerializeOption();
        }

        /// <summary>
        /// 获取序列化选项。
        /// </summary>
        public XmlSerializeOption Option { get; private set; }

        /// <summary>
        /// 将对象转换为使用文本表示。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">要序列化的对象。</param>
        /// <returns>表示对象的 Json 文本。</returns>
        [SuppressMessage("Microsoft.Usage", "CA2202")]
        public string Serialize<T>(T value)
        {
            var formatting = Option.Indent ? Formatting.Indented : Formatting.None;
            using (var sw = new StringWriter(CultureInfo.InvariantCulture))
            using (var writer = new XmlTextWriter(sw) { Formatting = formatting })
            using (var ser = new XmlSerialize(this, writer, Option))
            {
                if (Option.Declaration)
                {
                    writer.WriteStartDocument();
                }

                ser.Serialize(value, Option.StartElement);

                if (Option.Declaration)
                {
                    writer.WriteEndDocument();
                }

                return sw.ToString();
            }
        }

        /// <summary>
        /// 将对象转换为使用文本并写入到流中。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="writer"></param>
        public void Serialize<T>(T value, XmlWriter writer)
        {
            var ser = new XmlSerialize(this, writer, Option);
            if (Option.Declaration)
            {
                writer.WriteStartDocument();
            }

            ser.Serialize(value, Option.StartElement);

            if (Option.Declaration)
            {
                writer.WriteEndDocument();
            }
        }

        /// <summary>
        /// 从 Xml 文本中解析出类型 <typeparamref name="T"/> 的对象。
        /// </summary>
        /// <typeparam name="T">可序列化的对象类型。</typeparam>
        /// <param name="xml">表示对象的 Xml 文本。</param>
        /// <returns>对象。</returns>
        [SuppressMessage("Microsoft.Usage", "CA2202")]
        public T Deserialize<T>(string xml)
        {
            throw new NotImplementedException();
            /*
            if (string.IsNullOrEmpty(xml))
            {
                return default(T);
            }

            using (var sr = new StringReader(xml))
            using (var reader = new XmlTextReader(sr))
            using (var deser = new XmlDeserialize(this, reader, Option))
            {
                return deser.Deserialize<T>();
            }
             */
        }

        /// <summary>
        /// 从流中读取文本，解析出类型 <paramref name="type"/> 的对象。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        public T Deserialize<T>(XmlReader reader)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 从 Xml 文本中解析出类型 <paramref name="type"/> 的对象。
        /// </summary>
        /// <param name="xml">表示对象的 Xml 文本。</param>
        /// <param name="type">可序列化的对象类型。</param>
        /// <returns>对象。</returns>
        [SuppressMessage("Microsoft.Usage", "CA2202")]
        public object Deserialize(string xml, Type type)
        {
            if (string.IsNullOrEmpty(xml))
            {
                return null;
            }

            throw new NotImplementedException();
            /*
            using (var sr = new StringReader(xml))
            using (var reader = new XmlTextReader(sr))
            using (var deser = new XmlDeserialize(this, reader, Option))
            {
                return deser.Deserialize(type);
            }
             */
        }

        /// <summary>
        /// 从 Xml 文本中解析出类型 <typeparamref name="T"/> 的对象，<typeparamref name="T"/> 可以是匿名类型。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xml">表示对象的 Xml 文本</param>
        /// <param name="anyObj">可序列化的匿名类型。</param>
        /// <returns>对象。</returns>
        public T Deserialize<T>(string xml, T anyObj)
        {
            return Deserialize<T>(xml);
        }
    }
}
