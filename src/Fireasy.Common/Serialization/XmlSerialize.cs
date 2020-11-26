// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Dynamic;
using Fireasy.Common.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Dynamic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Fireasy.Common.Serialization
{
    internal sealed class XmlSerialize : DisposableBase
    {
        private readonly XmlSerializeOption _option;
        private readonly XmlSerializer _serializer;
        private readonly XmlTextWriter _xmlWriter;
        private readonly SerializeContext _context;
        private readonly TypeConverterCache<XmlConverter> _converters = new TypeConverterCache<XmlConverter>();

        internal XmlSerialize(XmlSerializer serializer, XmlTextWriter writer, XmlSerializeOption option)
        {
            _serializer = serializer;
            _xmlWriter = writer;
            _option = option;
            _context = new SerializeContext { Option = option };
        }

        /// <summary>
        /// 将对象序列化为文本。
        /// </summary>
        /// <param name="value">要序列化的值。</param>
        /// <param name="startEle"></param>
        /// <param name="type"></param>
        internal void Serialize(object value, bool startEle = false, Type type = null)
        {
            if ((type == null || type == typeof(object)) && value != null)
            {
                type = value.GetType();
            }

            if (type != null && (type.IsTaskReturnType() || typeof(IAsyncResult).IsAssignableFrom(type)))
            {
                throw new SerializationException(SR.GetString(SRKind.NotSerializeTask, type));
            }

            if (WithSerializable(type, value, startEle))
            {
                return;
            }

            if ((value == null) || DBNull.Value.Equals(value))
            {
                _xmlWriter.WriteValue(null);
                return;
            }

            if (WithConverter(type, value, startEle))
            {
                return;
            }

            if (type == typeof(DataSet) ||
                type == typeof(DataTable) ||
                type == typeof(DataRow))
            {
                new System.Xml.Serialization.XmlSerializer(type).Serialize(_xmlWriter, value);
                return;
            }

            if (type == typeof(Color))
            {
                SerializeColor((Color)value, startEle);
                return;
            }

            if (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(type))
            {
                SerializeDynamicObject((IDynamicMetaObjectProvider)value, startEle);
                return;
            }

            if (typeof(IDictionary).IsAssignableFrom(type))
            {
                SerializeDictionary(value as IDictionary, startEle);
                return;
            }

            if (type == typeof(byte[]))
            {
                SerializeBytes((byte[])value, startEle);
                return;
            }

            if (((value is IEnumerable) || typeof(IEnumerable).IsAssignableFrom(type)) && type != typeof(string))
            {
                var eleType = type.IsGenericType || type.IsArray ? type.GetEnumerableElementType() : null;
                SerializeEnumerable(value as IEnumerable, eleType, startEle);
                return;
            }

            SerializeValue(value, startEle);
        }

        private bool WithSerializable(Type type, object value, bool startEle)
        {
            var ser = value.As<ITextSerializable>();
            if (ser != null)
            {
                var result = ser.Serialize(_serializer);

                WriteXmlElement(GetElementName(type), startEle, () =>
                    {
                        _xmlWriter.WriteRaw(result);
                    });

                return true;
            }

            return false;
        }

        private bool WithConverter(Type type, object value, bool startEle)
        {
            var converter = _converters.GetWritableConverter(type, _option);

            if (converter == null || !converter.CanWrite)
            {
                return false;
            }

            converter.WriteXml(_serializer, _xmlWriter, value);

            return true;
        }

        private string GetElementName(Type type)
        {
            if (type == null)
            {
                return "AnyType";
            }

            var sb = new StringBuilder();
            if (type.IsGenericType)
            {
                var name = type.Name;
                sb.Append(name.Substring(0, name.IndexOf('`')));
                sb.Append("Of");

                foreach (var t in type.GetGenericArguments())
                {
                    sb.Append(GetElementName(t));
                }
            }
            else
            {
                sb.Append(GetSimpleTypeName(type));
            }

            return sb.ToString();
        }

        private void SerializeDynamicObject(IDynamicMetaObjectProvider dynamicObject, bool startEle)
        {
            var flag = new AssertFlag();

            var dymgr = new DynamicManager();

            if (startEle)
            {
                _xmlWriter.WriteStartElement("Dynamic");
            }

            var queue = new PriorityActionQueue();

            foreach (var name in dynamicObject.GetDynamicMemberNames())
            {
                dymgr.TryGetMember(dynamicObject, name, out object value);
                if (value == null)
                {
                    continue;
                }

                if (_option.NodeStyle == XmlNodeStyle.Attribute && value.GetType().IsStringable())
                {
                    queue.Add(0, () =>
                        {
                            _context.SerializeInfo = new PropertySerialzeInfo(ObjectType.DynamicObject, typeof(object), name);
                            _xmlWriter.WriteAttributeString(name, value.ToString());
                            _context.SerializeInfo = null;
                        });
                }
                else
                {
                    queue.Add(1, () =>
                        {
                            _context.SerializeInfo = new PropertySerialzeInfo(ObjectType.DynamicObject, typeof(object), name);
                            WriteXmlElement(name, true, () => Serialize(value, type: value.GetType()));
                            _context.SerializeInfo = null;
                        });
                }
            }

            queue.Invoke();

            if (startEle)
            {
                _xmlWriter.WriteEndElement();
            }
        }

        /// <summary>
        /// 释放对象所占用的非托管和托管资源。
        /// </summary>
        /// <param name="disposing">为 true 则释放托管资源和非托管资源；为 false 则仅释放非托管资源。</param>
        protected override bool Dispose(bool disposing)
        {
            _context.Dispose();

            return base.Dispose(disposing);
        }

        private void SerializeEnumerable(IEnumerable enumerable, Type type, bool startEle)
        {
            if (startEle)
            {
                _xmlWriter.WriteStartElement("ArrayOf" + GetElementName(type));
            }

            foreach (var current in enumerable)
            {
                Serialize(current, true);
            }

            if (startEle)
            {
                _xmlWriter.WriteEndElement();
            }
        }

        private void SerializeDictionary(IDictionary dictionary, bool startEle)
        {
            if (!dictionary.GetType().GetGenericArguments()[0].IsStringable())
            {
                throw new InvalidOperationException(SR.GetString(SRKind.KeyMustBeStringable));
            }

            if (startEle)
            {
                _xmlWriter.WriteStartElement("Dictionary");
            }

            foreach (var key in dictionary.Keys)
            {
                _context.SerializeInfo = new PropertySerialzeInfo(ObjectType.Dictionary, typeof(object), key.ToString());

                _xmlWriter.WriteStartElement(key.ToString());
                Serialize(dictionary[key]);
                _xmlWriter.WriteEndElement();

                _context.SerializeInfo = null;
            }

            if (startEle)
            {
                _xmlWriter.WriteEndElement();
            }
        }

        private void SerializeValue(object value, bool startEle)
        {
            var type = value.GetType();
            if (type.IsNullableType())
            {
                type = type.GetNonNullableType();
            }

            if (startEle)
            {
                _xmlWriter.WriteStartElement(GetElementName(type));
            }

            if (type.IsEnum)
            {
                _xmlWriter.WriteValue(((Enum)value).ToString("D"));
            }
            else if (type == typeof(TimeSpan))
            {
                _xmlWriter.WriteString(((TimeSpan)value).ToString());
            }
            else
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Boolean:
                        _xmlWriter.WriteValue((bool)value == true ? "true" : "false");
                        break;
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                    case TypeCode.Decimal:
                    case TypeCode.Single:
                    case TypeCode.Double:
                        _xmlWriter.WriteValue(value.As<IFormattable>().ToString(_context.SerializeInfo?.Formatter ?? "G", CultureInfo.InvariantCulture));
                        break;
                    case TypeCode.DateTime:
                        SerializeDateTime((DateTime)value);
                        break;
                    case TypeCode.Char:
                    case TypeCode.String:
                        SerializeString(value);
                        break;
                    default:
                        _context.TrySerialize(value, value => SerializeObject(value, false));
                        break;
                }
            }

            if (startEle)
            {
                _xmlWriter.WriteEndElement();
            }
        }

        private void SerializeDateTime(DateTime value)
        {
            if (_option.DateFormatHandling == DateFormatHandling.Default)
            {
                _xmlWriter.WriteValue(value.ToString(_context.SerializeInfo?.Formatter ?? "yyyy-MM-dd", CultureInfo.InvariantCulture));
            }
            else if (_option.DateFormatHandling == DateFormatHandling.IsoDateFormat)
            {
                _xmlWriter.WriteValue(value.GetDateTimeFormats('s')[0].ToString(_option.Culture));
            }
        }

        private void SerializeString<T>(T value)
        {
            var str = value.ToString();

            if (_option.CData && Regex.IsMatch(str, "<|>|/|\""))
            {
                _xmlWriter.WriteCData(str);
            }
            else
            {
                _xmlWriter.WriteString(str);
            }
        }

        private void SerializeColor(Color color, bool startEle)
        {
            var value = string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", color.A, color.R, color.G, color.B);
            WriteXmlElement("Color", startEle, () =>
                {
                    SerializeString(value);
                });
        }

        private void SerializeObject(object obj, bool startEle)
        {
            var lazyMgr = obj.As<ILazyManager>();
            var flag = new AssertFlag();
            var type = obj.GetType();

            if (startEle)
            {
                _xmlWriter.WriteStartElement(GetElementName(type));
            }

            var queue = new PriorityActionQueue();

            foreach (var metadata in _context.GetProperties(type, () => _option.ContractResolver.GetProperties(type)))
            {
                if (metadata.Filter(metadata.PropertyInfo, lazyMgr))
                {
                    continue;
                }

                var value = metadata.Getter.Invoke(obj);
                if ((_option.NullValueHandling == NullValueHandling.Ignore && value == null) ||
                    (value == null && _option.NodeStyle == XmlNodeStyle.Attribute))
                {
                    continue;
                }

                var objType = metadata.PropertyInfo.PropertyType == typeof(object) && value != null ? value.GetType() : metadata.PropertyInfo.PropertyType;
                if (_option.NodeStyle == XmlNodeStyle.Attribute && objType.IsStringable())
                {
                    queue.Add(0, () =>
                        {
                            _context.SerializeInfo = new PropertySerialzeInfo(metadata);
                            if (metadata.Converter != null)
                            {
                                _xmlWriter.WriteAttributeString(metadata.PropertyName, metadata.Converter.WriteObject(_serializer, value));
                            }
                            else
                            {
                                _xmlWriter.WriteAttributeString(metadata.PropertyName, value.ToString());
                            }

                            _context.SerializeInfo = null;
                        });
                }
                else
                {
                    queue.Add(1, () =>
                        {
                            _context.SerializeInfo = new PropertySerialzeInfo(metadata);
                            if (metadata.Converter != null && metadata.Converter is XmlConverter converter)
                            {
                                WriteXmlElement(metadata.PropertyName, true, () => converter.WriteXml(_serializer, _xmlWriter, value));
                            }
                            else
                            {
                                WriteXmlElement(metadata.PropertyName, true, () => Serialize(value, type: objType));
                            }

                            _context.SerializeInfo = null;
                        });
                }
            }

            queue.Invoke();

            if (startEle)
            {
                _xmlWriter.WriteEndElement();
            }
        }

        private void SerializeBytes(byte[] bytes, bool startEle)
        {
            WriteXmlElement("Bytes", startEle, () =>
                {
                    _xmlWriter.WriteString(Convert.ToBase64String(bytes));
                });
        }

        private void WriteXmlElement(string name, bool startEle, Action action)
        {
            name = _option.ContractResolver.ResolvePropertyName(name);
            if (startEle)
            {
                _xmlWriter.WriteStartElement(name);
            }

            action?.Invoke();

            if (startEle)
            {
                _xmlWriter.WriteEndElement();
            }
        }

        private IEnumerable<DataColumn> GetDataColumns(DataTable table)
        {
            foreach (DataColumn column in table.Columns)
            {
                if (!SerializerUtil.IsNoSerializable(_option, column.ColumnName))
                {
                    yield return column;
                }
            }
        }

        private string GetSimpleTypeName(Type type)
        {
            return (Type.GetTypeCode(type)) switch
            {
                TypeCode.Boolean => "boolean",
                TypeCode.String => "string",
                TypeCode.DateTime => "dateTime",
                TypeCode.Char => "char",
                TypeCode.SByte => "sbyte",
                TypeCode.Byte => "byte",
                TypeCode.Int16 => "short",
                TypeCode.UInt16 => "ushort",
                TypeCode.Int32 => "int",
                TypeCode.UInt32 => "uint",
                TypeCode.Int64 => "long",
                TypeCode.UInt64 => "ulong",
                TypeCode.Single => "float",
                TypeCode.Decimal => "decimal",
                TypeCode.Double => "double",
                _ => _option.ContractResolver.ResolvePropertyName(type.Name),
            };
        }
    }
}
