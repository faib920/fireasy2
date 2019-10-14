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
    internal sealed class XmlSerialize : IDisposable
    {
        private readonly XmlSerializeOption option;
        private readonly XmlSerializer serializer;
        private XmlTextWriter xmlWriter;
        private readonly SerializeContext context;
        private bool isDisposed;
        private TypeConverterCache<XmlConverter> cacheConverter = new TypeConverterCache<XmlConverter>();

        internal XmlSerialize(XmlSerializer serializer, XmlTextWriter writer, XmlSerializeOption option)
        {
            this.serializer = serializer;
            xmlWriter = writer;
            this.option = option;
            context = new SerializeContext { Option = option };
        }

        /// <summary>
        /// 将对象序列化为文本。
        /// </summary>
        /// <param name="value">要序列化的值。</param>
        /// <param name="startEle"></param>
        /// <param name="type"></param>
        internal void Serialize(object value, bool startEle = false, Type type = null)
        {
            if (type == null && value != null)
            {
                type = value.GetType();
            }

            if (WithSerializable(type, value, startEle))
            {
                return;
            }

            if ((value == null) || DBNull.Value.Equals(value))
            {
                xmlWriter.WriteValue(null);
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
                new System.Xml.Serialization.XmlSerializer(type).Serialize(xmlWriter, value);
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
                var result = ser.Serialize(serializer);

                WriteXmlElement(GetElementName(type), startEle, () =>
                    {
                        xmlWriter.WriteRaw(result);
                    });

                return true;
            }

            return false;
        }

        private bool WithConverter(Type type, object value, bool startEle)
        {
            var converter = cacheConverter.GetWritableConverter(type, option);

            if (converter == null || !converter.CanWrite)
            {
                return false;
            }

            converter.WriteXml(serializer, xmlWriter, value);

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
                xmlWriter.WriteStartElement("Dynamic");
            }

            var queue = new PriorityActionQueue();

            foreach (var name in dynamicObject.GetDynamicMemberNames())
            {
                object value;
                dymgr.TryGetMember(dynamicObject, name, out value);
                if (value == null)
                {
                    continue;
                }

                if (option.NodeStyle == XmlNodeStyle.Attribute && value.GetType().IsStringable())
                {
                    queue.Add(0, () =>
                        {
                            context.SerializeInfo = new PropertySerialzeInfo(ObjectType.DynamicObject, typeof(object), name);
                            xmlWriter.WriteAttributeString(name, value.ToString());
                            context.SerializeInfo = null;
                        });
                }
                else
                {
                    queue.Add(1, () =>
                        {
                            context.SerializeInfo = new PropertySerialzeInfo(ObjectType.DynamicObject, typeof(object), name);
                            WriteXmlElement(name, true, () => Serialize(value, type: value.GetType()));
                            context.SerializeInfo = null;
                        });
                }
            }

            queue.Invoke();

            if (startEle)
            {
                xmlWriter.WriteEndElement();
            }
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
                context.Dispose();
                xmlWriter.TryDispose();
                xmlWriter = null;
            }

            isDisposed = true;
        }

        private void SerializeEnumerable(IEnumerable enumerable, Type type, bool startEle)
        {
            if (startEle)
            {
                xmlWriter.WriteStartElement("ArrayOf" + GetElementName(type));
            }

            foreach (var current in enumerable)
            {
                Serialize(current, true);
            }

            if (startEle)
            {
                xmlWriter.WriteEndElement();
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
                xmlWriter.WriteStartElement("Dictionary");
            }

            foreach (var key in dictionary.Keys)
            {
                context.SerializeInfo = new PropertySerialzeInfo(ObjectType.Dictionary, typeof(object), key.ToString());

                xmlWriter.WriteStartElement(key.ToString());
                Serialize(dictionary[key]);
                xmlWriter.WriteEndElement();

                context.SerializeInfo = null;
            }

            if (startEle)
            {
                xmlWriter.WriteEndElement();
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
                xmlWriter.WriteStartElement(GetElementName(type));
            }

            if (type.IsEnum)
            {
                xmlWriter.WriteValue(((Enum)value).ToString("D"));
            }
            else
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Boolean:
                        xmlWriter.WriteValue((bool)value == true ? "true" : "false");
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
                        xmlWriter.WriteValue(value.As<IFormattable>().ToString(context.SerializeInfo?.Formatter ?? "G", CultureInfo.InvariantCulture));
                        break;
                    case TypeCode.DateTime:
                        SerializeDateTime((DateTime)value);
                        break;
                    case TypeCode.Char:
                    case TypeCode.String:
                        SerializeString(value);
                        break;
                    default:
                        context.TrySerialize(value, () => SerializeObject(value, false));
                        break;
                }
            }

            if (startEle)
            {
                xmlWriter.WriteEndElement();
            }
        }

        private void SerializeDateTime(DateTime value)
        {
            if (option.DateFormatHandling == DateFormatHandling.Default)
            {
                xmlWriter.WriteValue(value.ToString(context.SerializeInfo?.Formatter ?? "yyyy-MM-dd", CultureInfo.InvariantCulture));
            }
            else if (option.DateFormatHandling == DateFormatHandling.IsoDateFormat)
            {
                xmlWriter.WriteValue(value.GetDateTimeFormats('s')[0].ToString());
            }
        }

        private void SerializeString<T>(T value)
        {
            var str = value.ToString();

            if (option.CData && Regex.IsMatch(str, "<|>|/|\""))
            {
                xmlWriter.WriteCData(str);
            }
            else
            {
                xmlWriter.WriteString(str);
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
                xmlWriter.WriteStartElement(GetElementName(type));
            }

            var cache = context.GetAccessorCache(type);
            var queue = new PriorityActionQueue();

            foreach (var acc in cache)
            {
                if (acc.Filter(acc.PropertyInfo, lazyMgr))
                {
                    continue;
                }

                var value = acc.Accessor.GetValue(obj);
                if (value == null)
                {
                    continue;
                }

                var objType = acc.PropertyInfo.PropertyType == typeof(object) ? value.GetType() : acc.PropertyInfo.PropertyType;
                if (option.NodeStyle == XmlNodeStyle.Attribute && objType.IsStringable())
                {
                    queue.Add(0, () =>
                        {
                            context.SerializeInfo = new PropertySerialzeInfo(acc);
                            if (acc.Converter != null)
                            {
                                xmlWriter.WriteAttributeString(acc.PropertyName, acc.Converter.WriteObject(serializer, value));
                            }
                            else
                            {
                                xmlWriter.WriteAttributeString(acc.PropertyName, value.ToString());
                            }

                            context.SerializeInfo = null;
                        });
                }
                else
                {
                    queue.Add(1, () =>
                        {
                            context.SerializeInfo = new PropertySerialzeInfo(acc);
                            if (acc.Converter != null && acc.Converter is XmlConverter converter)
                            {
                                WriteXmlElement(acc.PropertyName, true, () => converter.WriteXml(serializer, xmlWriter, value));
                            }
                            else
                            {
                                WriteXmlElement(acc.PropertyName, true, () => Serialize(value, type: objType));
                            }

                            context.SerializeInfo = null;
                        });
                }
            }

            queue.Invoke();

            if (startEle)
            {
                xmlWriter.WriteEndElement();
            }
        }

        private void SerializeBytes(byte[] bytes, bool startEle)
        {
            WriteXmlElement("Bytes", startEle, () =>
                {
                    xmlWriter.WriteString(Convert.ToBase64String(bytes));
                });
        }

        private void WriteXmlElement(string name, bool startEle, Action action)
        {
            if (startEle)
            {
                xmlWriter.WriteStartElement(option.CamelNaming ? char.ToLower(name[0]) + name.Substring(1) : name);
            }

            if (action != null)
            {
                action();
            }

            if (startEle)
            {
                xmlWriter.WriteEndElement();
            }
        }

        private IEnumerable<DataColumn> GetDataColumns(DataTable table)
        {
            foreach (DataColumn column in table.Columns)
            {
                if (!SerializerUtil.IsNoSerializable(option, column.ColumnName))
                {
                    yield return column;
                }
            }
        }

        private string GetSimpleTypeName(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return "boolean";
                case TypeCode.String:
                    return "string";
                case TypeCode.DateTime:
                    return "dateTime";
                case TypeCode.Char:
                    return "char";
                case TypeCode.SByte:
                    return "sbyte";
                case TypeCode.Byte:
                    return "byte";
                case TypeCode.Int16:
                    return "short";
                case TypeCode.UInt16:
                    return "ushort";
                case TypeCode.Int32:
                    return "int";
                case TypeCode.UInt32:
                    return "uint";
                case TypeCode.Int64:
                    return "long";
                case TypeCode.UInt64:
                    return "ulong";
                case TypeCode.Single:
                    return "float";
                case TypeCode.Decimal:
                    return "decimal";
                case TypeCode.Double:
                    return "double";
                default:
                    return option.CamelNaming ? char.ToLower(type.Name[0]) + type.Name.Substring(1) : type.Name;
            }
        }

        /// <summary>
        /// 释放对象所占用的所有资源。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
    }
}
