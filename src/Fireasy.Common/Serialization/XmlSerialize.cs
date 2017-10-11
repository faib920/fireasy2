using Fireasy.Common.Extensions;
using System;
using System.Collections;
using System.Data;
using System.Drawing;
using System.Linq;
#if !NET35
using System.Dynamic;
using Fireasy.Common.Dynamic;
#endif
using System.Runtime.InteropServices;
using System.Xml;
using System.Globalization;
using Fireasy.Common.ComponentModel;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Fireasy.Common.Reflection;
using System.Text;

namespace Fireasy.Common.Serialization
{
    internal sealed class XmlSerialize : IDisposable
    {
        private readonly XmlSerializeOption option;
        private readonly XmlSerializer serializer;
        private XmlWriter xmlWriter;
        private readonly SerializeContext context;
        private bool isDisposed;

        internal XmlSerialize(XmlSerializer serializer, XmlWriter writer, XmlSerializeOption option)
        {
            this.serializer = serializer;
            xmlWriter = writer;
            this.option = option;
            context = new SerializeContext();
        }

        /// <summary>
        /// 将对象序列化为文本。
        /// </summary>
        /// <param name="value">要序列化的值。</param>
        /// <param name="startEle"></param>
        internal void Serialize(object value, bool startEle = false)
        {
            if ((value == null) || DBNull.Value.Equals(value))
            {
                xmlWriter.WriteValue(null);
                return;
            }

            var type = value.GetType();
            if (WithSerializable(type, value, startEle))
            {
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

            if (typeof(_Type).IsAssignableFrom(type))
            {
                //SerializeType((Type)(object)value);
                return;
            }

#if !NET35
            if (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(type))
            {
                SerializeDynamicObject((IDynamicMetaObjectProvider)value, startEle);
                return;
            }
#endif

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

            if (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string))
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
            var converter = option.Converters.GetConverter(type);
            if ((converter is XmlConverter || converter is ValueConverter) && converter.CanWrite)
            {
                WriteXmlElement(GetElementName(type), startEle, () =>
                    {
                        var xmlConvert = converter as XmlConverter;
                        if (xmlConvert != null && xmlConvert.Streaming)
                        {
                            xmlConvert.WriteXml(serializer, xmlWriter, value);
                        }
                        else
                        {
                            xmlWriter.WriteRaw(converter.WriteObject(serializer, value));
                        }
                    });

                return true;
            }

            return false;
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
                sb.Append(type.Name);
            }

            return sb.ToString();
        }

#if !NET35
        private void SerializeDynamicObject(IDynamicMetaObjectProvider dynamicObject, bool startEle)
        {
            var flag = new AssertFlag();

            var dymgr = new DynamicManager();

            if (startEle)
            {
                xmlWriter.WriteStartElement("Dynamic");
            }

            foreach (var name in dynamicObject.GetDynamicMemberNames())
            {
                object value;
                dymgr.TryGetMember(dynamicObject, name, out value);
                if (value == null)
                {
                    continue;
                }

                xmlWriter.WriteStartElement(name);
                Serialize(value);
                xmlWriter.WriteEndElement();
            }

            if (startEle)
            {
                xmlWriter.WriteEndElement();
            }
        }
#endif

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
            if (startEle)
            {
                xmlWriter.WriteStartElement("Dictionary");
            }

            foreach (var key in dictionary.Keys)
            {
                xmlWriter.WriteStartElement("Item");
                xmlWriter.WriteStartElement("Key");
                Serialize(key);
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("Value");
                Serialize(dictionary[key]);
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
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
                        xmlWriter.WriteValue(value.ToString());
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
                        xmlWriter.WriteValue(value.As<IFormattable>().ToString("G", CultureInfo.InvariantCulture));
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
            var offset = TimeZone.CurrentTimeZone.GetUtcOffset(value);
            var time = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var ticks = (value.ToUniversalTime().Ticks - time.Ticks) / 10000;
            xmlWriter.WriteValue(value.Year <= 1 ? string.Empty : value.ToString("yyyy-MM-dd"));
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

            var cache = GetAccessorCache(type);

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

                xmlWriter.WriteStartElement(acc.PropertyName);
                Serialize(value);
                xmlWriter.WriteEndElement();
            }

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
                xmlWriter.WriteStartElement(name);
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

        /// <summary>
        /// 获取指定类型的属性访问缓存。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private List<PropertyGetAccessorCache> GetAccessorCache(Type type)
        {
            return context.GetAccessors.TryGetValue(type, () =>
                {
                    return type.GetProperties()
                        .Where(s => s.CanRead && !SerializerUtil.IsNoSerializable(option, s))
                        .Distinct(new SerializerUtil.PropertyEqualityComparer())
                        .Select(s => new PropertyGetAccessorCache
                            {
                                Accessor = ReflectionCache.GetAccessor(s),
                                Filter = (p, l) =>
                                    {
                                        return !SerializerUtil.CheckLazyValueCreate(l, p.Name);
                                    },
                                PropertyInfo = s,
                                PropertyName = SerializerUtil.GetPropertyName(s)
                            })
                        .Where(s => !string.IsNullOrEmpty(s.PropertyName))
                        .ToList();
                });
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

        /// <summary>
        /// 释放对象所占用的所有资源。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
    }
}
