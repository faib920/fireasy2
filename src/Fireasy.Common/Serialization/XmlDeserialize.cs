using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Fireasy.Common.Extensions;
using System.Collections;
#if !NET35
using System.Dynamic;
using Fireasy.Common.Dynamic;
#endif
using System.Drawing;
using System.Data;

namespace Fireasy.Common.Serialization
{
    /*
    internal sealed class XmlDeserialize : IDisposable
    {
        private readonly XmlSerializeOption option;
        private readonly XmlSerializer serializer;
        private XmlTextReader xmlReader;
        private bool isDisposed;

        internal XmlDeserialize(XmlSerializer serializer, XmlTextReader reader, XmlSerializeOption option)
        {
            this.serializer = serializer;
            xmlReader = reader;
            this.option = option;
        }
        internal T Deserialize<T>()
        {
            return (T)Deserialize(typeof(T));
        }

        internal object Deserialize(Type type)
        {
            if (typeof(ITextSerializable).IsAssignableFrom(type))
            {
                var obj = type.New<ITextSerializable>();
                obj.Deserialize(serializer, xmlReader.ReadInnerXml());
                return obj;
            }

            var converter = serializer.GetConverter(type);
            if (converter != null && converter.CanRead)
            {
                return converter.ReadObject(serializer, type, xmlReader.ReadInnerXml());
            }

#if !SILVERLIGHT
            if (type == typeof(Color))
            {
                //return DeserializeColor();
            }
#endif
            if (type == typeof(Type))
            {
                //return DeserializeType();
            }

#if !NET35
            if (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(type) ||
                type == typeof(object))
            {
                //return DeserializeDynamicObject(type);
            }
#endif

            if (typeof(IDictionary).IsAssignableFrom(type) && type != typeof(string))
            {
                //return DeserializeDictionary(type);
            }

            if (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string))
            {
                //return DeserializeList(type);
            }
#if !SILVERLIGHT
            if (type == typeof(DataSet) ||
                type == typeof(DataTable) ||
                type == typeof(DataRow))
            {
                return new System.Xml.Serialization.XmlSerializer(type).Deserialize(xmlReader);
            }
#endif

            //return DeserializeValue(type);
            return null;
        }

        /// <summary>
        /// 释放对象所占用的非托管和托管资源。
        /// </summary>
        /// <param name="disposing">为 true 则释放托管资源和非托管资源；为 false 则仅释放非托管资源。</param>
        private void Dispose(bool disposing)
        {
            if (isDisposed)
            {
                return;
            }

            if (disposing)
            {
                xmlReader.TryDispose();
                xmlReader = null;
            }

            isDisposed = true;
        }

        /// <summary>
        /// 释放对象所占用的所有资源。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
    }
     */
}
