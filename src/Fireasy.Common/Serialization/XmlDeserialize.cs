// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Common.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace Fireasy.Common.Serialization
{
    internal sealed class XmlDeserialize : DeserializeBase
    {
        private readonly XmlSerializeOption option;
        private readonly XmlSerializer serializer;
        private readonly XmlReader xmlReader;
        private static readonly MethodInfo MthToArray = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray), BindingFlags.Public | BindingFlags.Static);
        private readonly TypeConverterCache<XmlConverter> cacheConverter = new TypeConverterCache<XmlConverter>();

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
            if (typeCode == TypeCode.Object && stype != typeof(TimeSpan))
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

            if (type.GetNonNullableType() == typeof(DateTime))
            {
                CheckNullString(value, type);
                return SerializerUtil.ParseDateTime(value, option.Culture, option.DateTimeZoneHandling);
            }
            else if (type.GetNonNullableType() == typeof(TimeSpan))
            {
                if (TimeSpan.TryParse(value, out TimeSpan result))
                {
                    return result;
                }
            }

            return value.ToType(type);
        }

        private object DeserializeDynamicObject(Type type)
        {
            var dynamicObject = type == typeof(object) ? new ExpandoObject() :
                type.New<IDictionary<string, object>>();

            while (xmlReader.Read())
            {
                XmlReaderHelper.ReadUntilAnyTypesReached(xmlReader, new[] { XmlNodeType.Element, XmlNodeType.EndElement, XmlNodeType.Text, XmlNodeType.CDATA });

                if (xmlReader.NodeType == XmlNodeType.EndElement)
                {
                    xmlReader.ReadEndElement();
                    break;
                }
                else if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    var name = xmlReader.Name;
                    try
                    {
                        var value = Deserialize(typeof(object));
                        dynamicObject.Add(name, value);
                    }
                    catch (AssumeContentException exp)
                    {
                        dynamicObject.Add(name, exp.Content);
                    }
                }
                else if (xmlReader.NodeType == XmlNodeType.Text || xmlReader.NodeType == XmlNodeType.CDATA)
                {
                    throw new AssumeContentException(xmlReader.ReadContentAsString());
                }
            }

            return dynamicObject;
        }

        private object DeserializeList(Type listType)
        {
            var isReadonly = listType.IsGenericType && listType.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>);

            CreateListContainer(listType, out Type elementType, out IList container);

            xmlReader.ReadStartElement();

            while (true)
            {
                XmlReaderHelper.ReadUntilAnyTypesReached(xmlReader, new[] { XmlNodeType.Element, XmlNodeType.Text, XmlNodeType.CDATA, XmlNodeType.EndElement });

                if (xmlReader.NodeType == XmlNodeType.EndElement)
                {
                    xmlReader.ReadEndElement();
                    break;
                }
                else if (xmlReader.NodeType == XmlNodeType.Element)
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
            }

            if (listType.IsArray)
            {
                var invoker = ReflectionCache.GetInvoker(MthToArray.MakeGenericMethod(elementType));
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
            CreateDictionaryContainer(dictType, out Type[] keyValueTypes, out IDictionary container);

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
            var mappers = GetAccessorMetadataMappers(instance.GetType());
            var processor = instance as IDeserializeProcessor;
            processor?.PreDeserialize();

            if (xmlReader.AttributeCount > 0)
            {
                for (var i = 0; i < xmlReader.AttributeCount; i++)
                {
                    xmlReader.MoveToAttribute(i);

                    if (mappers.TryGetValue(xmlReader.Name, out SerializerPropertyMetadata metadata))
                    {
                        if (!string.IsNullOrEmpty(xmlReader.Value))
                        {
                            var value = xmlReader.Value.ToType(metadata.PropertyInfo.PropertyType);
                            if (processor == null || !processor.SetValue(xmlReader.Name, value))
                            {
                                metadata.Setter?.Invoke(instance, value);
                            }

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

                if (xmlReader.NodeType == XmlNodeType.EndElement)
                {
                    xmlReader.ReadEndElement();
                    break;
                }
                else if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    if (mappers.TryGetValue(xmlReader.Name, out SerializerPropertyMetadata metadata))
                    {
                        var value = Deserialize(metadata.PropertyInfo.PropertyType);
                        if (processor == null || !processor.SetValue(xmlReader.Name, value))
                        {
                            metadata.Setter?.Invoke(instance, value);
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
                        if (mappers.TryGetValue(xmlReader.Name, out SerializerPropertyMetadata metadata))
                        {
                            var value = Deserialize(metadata.PropertyInfo.PropertyType);
                            if (processor == null || !processor.SetValue(xmlReader.Name, value))
                            {
                                metadata.Setter?.Invoke(instance, value);
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

            processor?.PostDeserialize();

            return instance;
        }

        private object ParseAnonymousObject(Type type)
        {
            return null;
        }

        private static void CheckNullString(object value, Type type)
        {
            if ((value == null || value.ToString().Length == 0) && !type.IsNullableType())
            {
                throw new SerializationException(SR.GetString(SRKind.JsonNullableType, type));
            }
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

                        return string.Empty;
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

        private class AssumeContentException : Exception
        {
            public AssumeContentException(string content)
            {
                Content = content;
            }

            public string Content { get; private set; }
        }

    }
}
