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
        private readonly XmlSerializeOption _option;
        private readonly XmlSerializer _serializer;
        private readonly XmlReader _xmlReader;
        private readonly TypeConverterCache<XmlConverter> _converters = new TypeConverterCache<XmlConverter>();

        private class MethodCache
        {
            internal protected static readonly MethodInfo ToArray = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray), BindingFlags.Public | BindingFlags.Static);
        }

        internal XmlDeserialize(XmlSerializer serializer, XmlReader reader, XmlSerializeOption option)
            : base(option)
        {
            _serializer = serializer;
            _xmlReader = reader;
            _option = option;
        }

        internal T Deserialize<T>()
        {
            _xmlReader.Read();
            XmlReaderHelper.ReadUntilTypeReached(_xmlReader, XmlNodeType.Element);
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
                return new System.Xml.Serialization.XmlSerializer(type).Deserialize(_xmlReader);
            }
#endif

            return DeserializeValue(type);
        }

        private bool WithSerializable(Type type, ref object value)
        {
            if (typeof(ITextSerializable).IsAssignableFrom(type))
            {
                var obj = type.New<ITextSerializable>();
                var rawValue = _xmlReader.ReadInnerXml();
                if (rawValue != null)
                {
                    obj.Deserialize(_serializer, rawValue);
                    value = obj;
                }

                return true;
            }

            return false;
        }

        private bool WithConverter(Type type, ref object value)
        {
            var converter = _converters.GetReadableConverter(type, _option);

            if (converter == null || !converter.CanRead)
            {
                return false;
            }

            value = converter.ReadXml(_serializer, _xmlReader, type);

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

            if (_xmlReader.IsEmptyElement)
            {
                if (_xmlReader.NodeType == XmlNodeType.Element)
                {
                    XmlReaderHelper.ReadAndConsumeMatchingEndElement(_xmlReader);
                }

                return type.GetDefaultValue();
            }

            bool beStartEle;
            if ((beStartEle = (_xmlReader.NodeType == XmlNodeType.Element)))
            {
                XmlReaderHelper.ReadUntilAnyTypesReached(_xmlReader, new[] { XmlNodeType.EndElement, XmlNodeType.Text, XmlNodeType.CDATA });
            }

            var value = _xmlReader.NodeType == XmlNodeType.EndElement ? string.Empty : _xmlReader.ReadContentAsString();
            if (beStartEle)
            {
                XmlReaderHelper.ReadAndConsumeMatchingEndElement(_xmlReader);
            }

            if (type.GetNonNullableType() == typeof(DateTime))
            {
                CheckNullString(value, type);
                return SerializerUtil.ParseDateTime(value, _option.Culture, _option.DateTimeZoneHandling);
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

            while (_xmlReader.Read())
            {
                XmlReaderHelper.ReadUntilAnyTypesReached(_xmlReader, new[] { XmlNodeType.Element, XmlNodeType.EndElement, XmlNodeType.Text, XmlNodeType.CDATA });

                if (_xmlReader.NodeType == XmlNodeType.EndElement)
                {
                    _xmlReader.ReadEndElement();
                    break;
                }
                else if (_xmlReader.NodeType == XmlNodeType.Element)
                {
                    var name = _xmlReader.Name;
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
                else if (_xmlReader.NodeType == XmlNodeType.Text || _xmlReader.NodeType == XmlNodeType.CDATA)
                {
                    throw new AssumeContentException(_xmlReader.ReadContentAsString());
                }
            }

            return dynamicObject;
        }

        private object DeserializeList(Type listType)
        {
            var isReadonly = listType.IsGenericType && listType.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>);

            CreateListContainer(listType, out Type elementType, out IList container);

            _xmlReader.ReadStartElement();

            while (true)
            {
                XmlReaderHelper.ReadUntilAnyTypesReached(_xmlReader, new[] { XmlNodeType.Element, XmlNodeType.Text, XmlNodeType.CDATA, XmlNodeType.EndElement });

                if (_xmlReader.NodeType == XmlNodeType.EndElement)
                {
                    _xmlReader.ReadEndElement();
                    break;
                }
                else if (_xmlReader.NodeType == XmlNodeType.Element)
                {
                    container.Add(Deserialize(elementType));
                }
                else if (_xmlReader.NodeType == XmlNodeType.Text || _xmlReader.NodeType == XmlNodeType.CDATA)
                {
                    foreach (var str in _xmlReader.ReadContentAsString().Split(',', ';'))
                    {
                        container.Add(str.ToType(elementType));
                    }
                }
            }

            if (listType.IsArray)
            {
                var invoker = ReflectionCache.GetInvoker(MethodCache.ToArray.MakeGenericMethod(elementType));
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

            while (_xmlReader.Read())
            {
                XmlReaderHelper.ReadUntilTypeReached(_xmlReader, XmlNodeType.Element);

                if (_xmlReader.NodeType == XmlNodeType.Element)
                {
                    var key = _xmlReader.Name;
                    var value = Deserialize(keyValueTypes[1]);

                    container.Add(Convert.ChangeType(key, keyValueTypes[0]), value);
                }
                else
                {
                    if (_xmlReader.NodeType == XmlNodeType.EndElement)
                    {
                        _xmlReader.ReadEndElement();
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

            if (_xmlReader.AttributeCount > 0)
            {
                for (var i = 0; i < _xmlReader.AttributeCount; i++)
                {
                    _xmlReader.MoveToAttribute(i);

                    if (mappers.TryGetValue(_xmlReader.Name, out SerializerPropertyMetadata metadata))
                    {
                        if (!string.IsNullOrEmpty(_xmlReader.Value))
                        {
                            var value = _xmlReader.Value.ToType(metadata.PropertyInfo.PropertyType);
                            if (processor == null || !processor.SetValue(_xmlReader.Name, value))
                            {
                                metadata.Setter?.Invoke(instance, value);
                            }

                        }
                    }
                }

                _xmlReader.MoveToElement();
            }

            if (_xmlReader.IsEmptyElement)
            {
                _xmlReader.ReadStartElement();
                return instance;
            }

            _xmlReader.ReadStartElement();

            while (true)
            {
                XmlReaderHelper.ReadUntilAnyTypesReached(_xmlReader, new XmlNodeType[] { XmlNodeType.Element, XmlNodeType.EndElement, XmlNodeType.Text, XmlNodeType.CDATA });

                if (_xmlReader.NodeType == XmlNodeType.EndElement)
                {
                    _xmlReader.ReadEndElement();
                    break;
                }
                else if (_xmlReader.NodeType == XmlNodeType.Element)
                {
                    if (mappers.TryGetValue(_xmlReader.Name, out SerializerPropertyMetadata metadata))
                    {
                        var value = Deserialize(metadata.PropertyInfo.PropertyType);
                        if (processor == null || !processor.SetValue(_xmlReader.Name, value))
                        {
                            metadata.Setter?.Invoke(instance, value);
                        }
                    }
                    else
                    {
                        XmlReaderHelper.ReadAndConsumeMatchingEndElement(_xmlReader);
                    }
                }
                else if (_xmlReader.NodeType == XmlNodeType.Text || _xmlReader.NodeType == XmlNodeType.CDATA)
                {
                    if (_xmlReader.HasValue)
                    {
                        if (mappers.TryGetValue(_xmlReader.Name, out SerializerPropertyMetadata metadata))
                        {
                            var value = Deserialize(metadata.PropertyInfo.PropertyType);
                            if (processor == null || !processor.SetValue(_xmlReader.Name, value))
                            {
                                metadata.Setter?.Invoke(instance, value);
                            }
                        }

                        _xmlReader.Read();
                    }
                }
                else
                {
                    if (_xmlReader.NodeType == XmlNodeType.EndElement)
                    {
                        _xmlReader.ReadEndElement();
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
