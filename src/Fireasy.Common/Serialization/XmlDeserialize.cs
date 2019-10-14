using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Fireasy.Common.Extensions;
using System.Collections;
using System.Dynamic;
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
        private TypeConverterCache<XmlConverter> cacheConverter = new TypeConverterCache<XmlConverter>();

        internal XmlDeserialize(XmlSerializer serializer, XmlReader reader, XmlSerializeOption option)
            : base(option)
        {
            this.serializer = serializer;
            xmlReader = reader;
            this.option = option;
        }

        internal T Deserialize<T>()
        {
            xmlReader.Read();
            XmlReaderHelper.ReadUntilTypeReached(xmlReader, XmlNodeType.Element);
            return (T)Deserialize(typeof(T));
        }

        internal object Deserialize(Type type)
        {
            object value = null;
            if (WithSerializable(type, ref value))
            {
                return value;
            }

            if (WithConverter(type, ref value))
            {
                return value;
            }

            if (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(type) ||
                type == typeof(object))
            {
                return DeserializeDynamicObject(type);
            }

            if (typeof(IDictionary).IsAssignableFrom(type) && type != typeof(string))
            {
                return DeserializeDictionary(type);
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
            var converter = cacheConverter.GetReadableConverter(type, option);

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

            if (xmlReader.IsEmptyElement)
            {
                if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    XmlReaderHelper.ReadAndConsumeMatchingEndElement(xmlReader);
                }

                return type.GetDefaultValue();
            }

            bool beStartEle;
            if ((beStartEle = (xmlReader.NodeType == XmlNodeType.Element)))
            {
                XmlReaderHelper.ReadUntilAnyTypesReached(xmlReader, new[] { XmlNodeType.EndElement, XmlNodeType.Text, XmlNodeType.CDATA });
            }

            var value = xmlReader.NodeType == XmlNodeType.EndElement ? string.Empty : xmlReader.ReadContentAsString();
            if (beStartEle)
            {
                XmlReaderHelper.ReadAndConsumeMatchingEndElement(xmlReader);
            }

            return value.ToType(type);
        }

        private object DeserializeDynamicObject(Type type)
        {
            var dynamicObject = type == typeof(object) ? new ExpandoObject() :
                type.New<IDictionary<string, object>>();

            while (xmlReader.Read())
            {
                XmlReaderHelper.ReadUntilTypeReached(xmlReader, XmlNodeType.Element);

                if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    var name = xmlReader.Name;
                    var value = Deserialize(typeof(object));
                    if (value != null)
                    {
                        dynamicObject.Add(name, value);
                    }
                }
                else
                {
                    if (xmlReader.NodeType == XmlNodeType.EndElement)
                    {
                        xmlReader.ReadEndElement();
                    }

                    break;
                }
            }

            return dynamicObject;
        }

        private object DeserializeList(Type listType)
        {
            IList container = null;
            Type elementType = null;
            var isReadonly = listType.IsGenericType && listType.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>);

            CreateListContainer(listType, out elementType, out container);

            xmlReader.ReadStartElement();

            while (true)
            {
                XmlReaderHelper.ReadUntilAnyTypesReached(xmlReader, new[] { XmlNodeType.Element, XmlNodeType.Text, XmlNodeType.CDATA });

                if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    container.Add(Deserialize(elementType));
                }
                else if (xmlReader.NodeType == XmlNodeType.Text || xmlReader.NodeType == XmlNodeType.CDATA)
                {
                    foreach (var str in xmlReader.ReadContentAsString().Split(',', ';'))
                    {
                        container.Add(str.ToType(elementType));
                    }
                }
                else
                {
                    if (xmlReader.NodeType == XmlNodeType.EndElement)
                    {
                        xmlReader.ReadEndElement();
                    }

                    break;
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

        private IDictionary DeserializeDictionary(Type dictType)
        {
            IDictionary container = null;
            Type[] keyValueTypes = null;

            CreateDictionaryContainer(dictType, out keyValueTypes, out container);

            while (xmlReader.Read())
            {
                XmlReaderHelper.ReadUntilTypeReached(xmlReader, XmlNodeType.Element);

                if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    var key = xmlReader.Name;
                    var value = Deserialize(keyValueTypes[1]);

                    container.Add(Convert.ChangeType(key, keyValueTypes[0]), value);
                }
                else
                {
                    if (xmlReader.NodeType == XmlNodeType.EndElement)
                    {
                        xmlReader.ReadEndElement();
                    }
                }
            }

            return container;
        }

        private object ParseObject(Type type)
        {
            return type.IsAnonymousType() ? ParseAnonymousObject(type) : ParseGeneralObject(type);
        }

        private object ParseGeneralObject(Type type)
        {
            var instance = CreateGeneralObject(type);
            var cache = GetAccessorCache(instance.GetType());

            if (xmlReader.AttributeCount > 0)
            {
                for (var i = 0; i < xmlReader.AttributeCount; i++)
                {
                    xmlReader.MoveToAttribute(i);

                    if (cache.TryGetValue(xmlReader.Name, out PropertyAccessor accessor))
                    {
                        if (!string.IsNullOrEmpty(xmlReader.Value))
                        {
                            accessor.SetValue(instance, xmlReader.Value.ToType(accessor.PropertyInfo.PropertyType));
                        }
                    }
                }

                xmlReader.MoveToElement();
            }

            if (xmlReader.IsEmptyElement)
            {
                xmlReader.ReadStartElement();
                return instance;
            }

            xmlReader.ReadStartElement();

            while (true)
            {
                XmlReaderHelper.ReadUntilAnyTypesReached(xmlReader, new XmlNodeType[] { XmlNodeType.Element, XmlNodeType.EndElement, XmlNodeType.Text, XmlNodeType.CDATA });

                if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    if (cache.TryGetValue(xmlReader.Name, out PropertyAccessor accessor))
                    {
                        var value = Deserialize(accessor.PropertyInfo.PropertyType);
                        if (value != null)
                        {
                            accessor.SetValue(instance, value);
                        }
                    }
                    else
                    {
                        XmlReaderHelper.ReadAndConsumeMatchingEndElement(xmlReader);
                    }
                }
                else if (xmlReader.NodeType == XmlNodeType.Text || xmlReader.NodeType == XmlNodeType.CDATA)
                {
                    if (xmlReader.HasValue)
                    {
                        if (cache.TryGetValue(xmlReader.Name, out PropertyAccessor accessor))
                        {
                            var value = Deserialize(accessor.PropertyInfo.PropertyType);
                            if (value != null)
                            {
                                accessor.SetValue(instance, value);
                            }
                        }

                        xmlReader.Read();
                    }
                }
                else
                {
                    if (xmlReader.NodeType == XmlNodeType.EndElement)
                    {
                        xmlReader.ReadEndElement();
                    }

                    break;
                }
            }

            return instance;
        }

        private object ParseAnonymousObject(Type type)
        {
            return null;
        }

        internal class XmlReaderHelper
        {
            public static readonly XmlNodeType[] ElementOrEndElement = { XmlNodeType.Element, XmlNodeType.EndElement };

            public static void ReadAndConsumeMatchingEndElement(XmlReader reader)
            {
                var x = 0;
                var start = reader.Name;

                if (reader.IsEmptyElement)
                {
                    reader.Read();

                    return;
                }

                while (true)
                {
                    if (reader.IsEmptyElement)
                    {
                        reader.Read();
                        continue;
                    }

                    if (reader.NodeType == XmlNodeType.Element && !(x == 0 && start == reader.Name))
                    {
                        x++;
                    }

                    if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        if (x == 0)
                        {
                            reader.ReadEndElement();

                            break;
                        }

                        x--;
                    }

                    if (reader.ReadState != ReadState.Interactive)
                    {
                        break;
                    }

                    reader.Read();
                }
            }

            public static void ReadAndApproachMatchingEndElement(XmlReader reader)
            {
                var x = 0;

                if (reader.IsEmptyElement)
                {
                    reader.Read();

                    return;
                }

                while (true)
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        x++;
                    }

                    if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        if (x == 0)
                        {
                            break;
                        }

                        x--;
                    }

                    if (reader.ReadState != ReadState.Interactive)
                    {
                        break;
                    }

                    reader.Read();
                }
            }

            public static void ReadUntilTypeReached(XmlReader reader, XmlNodeType nodeType)
            {
                ReadUntilAnyTypesReached(reader, new XmlNodeType[] { nodeType });
            }

            public static void ReadUntilAnyTypesReached(XmlReader reader, XmlNodeType[] nodeTypes)
            {
                while (true)
                {
                    if (Array.IndexOf(nodeTypes, reader.NodeType) >= 0)
                    {
                        break;
                    }

                    if (reader.ReadState != ReadState.Interactive)
                    {
                        break;
                    }

                    reader.Read();
                }
            }

            /// <summary>
            /// Reads the current node in the reader's value.
            /// </summary>
            /// <param name="reader"></param>
            /// <returns></returns>
            public static string ReadCurrentNodeValue(XmlReader reader)
            {
                var fromElement = (reader.NodeType == XmlNodeType.Element);

                // If we're deserializing from an element,

                if (fromElement)
                {
                    // read the start node.

                    if (reader.IsEmptyElement)
                    {
                        reader.Read();

                        return "";
                    }

                    reader.ReadStartElement();
                }

                var s = reader.Value;

                // If we're deserializing from an element,

                if (fromElement)
                {
                    // read the end node.

                    ReadAndConsumeMatchingEndElement(reader);
                }

                return s;
            }
        }
    }
}
