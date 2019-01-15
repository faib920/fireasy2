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
using Fireasy.Common.Reflection;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Fireasy.Common.Serialization
{
    internal sealed class XmlDeserialize : DeserializeBase
    {
        private readonly XmlSerializeOption option;
        private readonly XmlSerializer serializer;
        private XmlReader xmlReader;
        private bool isDisposed;
        private static MethodInfo mthToArray = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray), BindingFlags.Public | BindingFlags.Static);

        internal XmlDeserialize(XmlSerializer serializer, XmlReader reader, XmlSerializeOption option)
            : base(option)
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
            while (xmlReader.NodeType != XmlNodeType.Element && xmlReader.Read()) ;

            object value = null;
            if (WithSerializable(type, ref value))
            {
                return value;
            }

            if (WithConverter(type, ref value))
            {
                return value;
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
                return DeserializeDynamicObject(type);
            }
#endif

            if (typeof(IDictionary).IsAssignableFrom(type) && type != typeof(string))
            {
                //return DeserializeDictionary(type);
            }

            if (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string))
            {
                return DeserializeList(type);
            }
#if !SILVERLIGHT
            if (type == typeof(DataSet) ||
                type == typeof(DataTable) ||
                type == typeof(DataRow))
            {
                return new System.Xml.Serialization.XmlSerializer(type).Deserialize(xmlReader);
            }
#endif

            return DeserializeValue(type);
        }

        private bool WithSerializable(Type type, ref object value)
        {
            if (typeof(ITextSerializable).IsAssignableFrom(type))
            {
                var obj = type.New<ITextSerializable>();
                var rawValue = xmlReader.ReadInnerXml();
                if (rawValue != null)
                {
                    obj.Deserialize(serializer, rawValue);
                    value = obj;
                }

                return true;
            }

            return false;
        }

        private bool WithConverter(Type type, ref object value)
        {
            XmlConverter converter;
            TextConverterAttribute attr;
            if ((attr = type.GetCustomAttributes<TextConverterAttribute>().FirstOrDefault()) != null &&
                typeof(XmlConverter).IsAssignableFrom(attr.ConverterType))
            {
                converter = attr.ConverterType.New<XmlConverter>();
            }
            else
            {
                converter = option.Converters.GetReadableConverter(type, new[] { typeof(XmlConverter) }) as XmlConverter;
            }

            if (converter == null || !converter.CanRead)
            {
                return false;
            }

            value = converter.ReadXml(serializer, xmlReader, type);

            return true;
        }

        private object DeserializeValue(Type type)
        {
            var stype = type.GetNonNullableType();
            var typeCode = Type.GetTypeCode(stype);
            if (typeCode == TypeCode.Object)
            {
                return ParseObject(type);
            }

            if (type.IsNullableType() && xmlReader.HasValue)
            {
                return null;
            }

            var beStartEle = false;
            if ((beStartEle = (xmlReader.NodeType == XmlNodeType.Element)))
            {
                xmlReader.Read();
            }

            var value = xmlReader.ReadContentAsString();
            if (beStartEle)
            {
                while (xmlReader.NodeType != XmlNodeType.EndElement && xmlReader.Read()) ;
            }

            return value.ToType(type);
        }
#if !NET35

        private object DeserializeDynamicObject(Type type)
        {
            var dynamicObject = type == typeof(object) ? new ExpandoObject() :
                type.New<IDictionary<string, object>>();

            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    var value = Deserialize(typeof(string));
                    if (value != null)
                    {
                        dynamicObject.Add(xmlReader.Name, value);
                    }
                }
                else if (xmlReader.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
            }

            return dynamicObject;
        }
#endif

        private object DeserializeList(Type listType)
        {
            IList container = null;
            Type elementType = null;
#if !NET40 && !NET35
            var isReadonly = listType.IsGenericType && listType.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>);
#else
            var isReadonly = listType.IsGenericType && listType.GetGenericTypeDefinition() == typeof(ReadOnlyCollection<>);
#endif

            CreateListContainer(listType, out elementType, out container);

            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == elementType.Name)
                {
                    container.Add(Deserialize(elementType));
                }
            }

            if (listType.IsArray)
            {
                var invoker = ReflectionCache.GetInvoker(mthToArray.MakeGenericMethod(elementType));
                return invoker.Invoke(null, container);
            }

            if (isReadonly)
            {
                return listType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new[] { container.GetType() }, null).Invoke(new object[] { container });
            }

            return container;
        }

        private object ParseObject(Type type)
        {
            return type.IsAnonymousType() ? ParseAnonymousObject(type) : ParseGeneralObject(type);
        }

        private object ParseGeneralObject(Type type)
        {
            if (xmlReader.NodeType == XmlNodeType.None)
            {
                return null;
            }

            var instance = type.New();

            var cache = GetAccessorCache(instance.GetType());

            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.Element && cache.TryGetValue(xmlReader.Name, out PropertyAccessor accessor))
                {
                    var value = Deserialize(accessor.PropertyInfo.PropertyType);
                    if (value != null)
                    {
                        accessor.SetValue(instance, value);
                    }
                }
                else if (xmlReader.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
            }

            return instance;
        }

        private object ParseAnonymousObject(Type type)
        {
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
    }
}
