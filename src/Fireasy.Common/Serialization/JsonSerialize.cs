// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections;
using System.Data;
using System.Linq;
using Fireasy.Common.Extensions;
using System.Dynamic;
using Fireasy.Common.ComponentModel;
using System.Collections.Generic;
using System.Text;

namespace Fireasy.Common.Serialization
{
    internal sealed class JsonSerialize : DisposeableBase
    {
        private readonly JsonSerializeOption option;
        private readonly JsonSerializer serializer;
        private JsonWriter jsonWriter;
        private readonly SerializeContext context;
        private readonly TypeConverterCache<JsonConverter> cacheConverter = new TypeConverterCache<JsonConverter>();

        internal JsonSerialize(JsonSerializer serializer, JsonWriter writer, JsonSerializeOption option)
        {
            this.serializer = serializer;
            jsonWriter = writer;
            this.option = option;
            context = new SerializeContext { Option = option };
        }

        /// <summary>
        /// 将对象序列化为文本。
        /// </summary>
        /// <param name="value">要序列化的值。</param>
        /// <param name="type"></param>
        internal void Serialize(object value, Type type = null)
        {
            if ((type == null || type == typeof(object)) && value != null)
            {
                type = value.GetType();
            }

            if (type != null && WithConverter(type, value))
            {
                return;
            }

            if ((value == null) || DBNull.Value.Equals(value))
            {
                jsonWriter.WriteNull();
                return;
            }

            if (WithSerializable(value))
            {
                return;
            }


            if (type == typeof(DataSet))
            {
                SerializeDataSet(value as DataSet);
                return;
            }

            if (type == typeof(DataTable))
            {
                SerializeDataTable(value as DataTable);
                return;
            }

            if (type == typeof(DataRow))
            {
                SerializeDataRow(value as DataRow, null);
                return;
            }

            if (typeof(Type).IsAssignableFrom(type))
            {
                SerializeType((Type)value);
                return;
            }

            if (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(type))
            {
                SerializeDynamicObject((IDictionary<string, object>)value);
                return;
            }

            if (typeof(IDictionary).IsAssignableFrom(type))
            {
                SerializeDictionary(value as IDictionary);
                return;
            }

            if (type == typeof(byte[]))
            {
                SerializeBytes((byte[])value);
                return;
            }

            if (((value is IEnumerable) || typeof(IEnumerable).IsAssignableFrom(type)) && type != typeof(string))
            {
                SerializeEnumerable(value as IEnumerable);
                return;
            }

            SerializeValue(value);
        }

        private bool WithSerializable(object value)
        {
            var ser = value.As<ITextSerializable>();
            if (ser != null)
            {
                var result = ser.Serialize(serializer);
                jsonWriter.WriteValue(result);
                return true;
            }

            return false;
        }

        private bool WithConverter(Type type, object value)
        {
            var converter = cacheConverter.GetWritableConverter(type, option);

            if (converter == null || !converter.CanWrite)
            {
                return false;
            }

            converter.WriteJson(serializer, jsonWriter, value);

            return true;
        }

        private void SerializeDataSet(DataSet set)
        {
            var flag = new AssertFlag();
            jsonWriter.WriteStartObject();
            var index = 0;
            foreach (DataTable table in set.Tables)
            {
                if (!flag.AssertTrue())
                {
                    jsonWriter.WriteComma();
                }

                var name = string.IsNullOrEmpty(table.TableName) ? "Table" + (index++) : table.TableName;
                jsonWriter.WriteKey(SerializeName(name));
                SerializeDataTable(table);
            }

            jsonWriter.WriteEndObject();
        }

        private void SerializeDataTable(DataTable table)
        {
            var flag = new AssertFlag();
            jsonWriter.WriteStartArray();
            var columns = GetDataColumns(table).ToList();
            foreach (DataRow row in table.Rows)
            {
                if (!flag.AssertTrue())
                {
                    jsonWriter.WriteComma();
                }

                SerializeDataRow(row, columns);
            }

            jsonWriter.WriteEndArray();
        }

        private void SerializeDataRow(DataRow row, List<DataColumn> columns)
        {
            if (columns == null)
            {
                columns = GetDataColumns(row.Table).ToList();
            }

            var flag = new AssertFlag();
            jsonWriter.WriteStartObject();
            foreach (var column in columns)
            {
                if (!flag.AssertTrue())
                {
                    jsonWriter.WriteComma();
                }

                jsonWriter.WriteKey(SerializeName(column.ColumnName));
                SerializeValue(row[column]);
            }

            jsonWriter.WriteEndObject();
        }

        private void SerializeEnumerable(IEnumerable enumerable)
        {
            var flag = new AssertFlag();
            jsonWriter.WriteStartArray();

            foreach (var current in enumerable)
            {
                if (!flag.AssertTrue())
                {
                    jsonWriter.WriteComma();
                }

                Serialize(current);
            }

            jsonWriter.WriteEndArray();
        }

        private void SerializeDictionary(IDictionary dictionary)
        {
            var flag = new AssertFlag();
            jsonWriter.WriteStartObject();
            foreach (DictionaryEntry entry in dictionary)
            {
                if (!flag.AssertTrue())
                {
                    jsonWriter.WriteComma();
                }

                context.SerializeInfo = new PropertySerialzeInfo(ObjectType.Dictionary, typeof(object), entry.Key.ToString());

                SerializeKeyValue(entry.Key, entry.Value);

                context.SerializeInfo = null;

            }

            jsonWriter.WriteEndObject();
        }

        private void SerializeDynamicObject(IDictionary<string, object> dynamicObject)
        {
            var flag = new AssertFlag();

            jsonWriter.WriteStartObject();
            foreach (var name in dynamicObject.Keys)
            {
                dynamicObject.TryGetValue(name, out object value);
                if (option.NullValueHandling == NullValueHandling.Ignore && value == null)
                {
                    continue;
                }

                if (!flag.AssertTrue())
                {
                    jsonWriter.WriteComma();
                }

                context.SerializeInfo = new PropertySerialzeInfo(ObjectType.DynamicObject, typeof(object), name);

                jsonWriter.WriteKey(SerializeName(name));
                Serialize(value);

                context.SerializeInfo = null;
            }

            jsonWriter.WriteEndObject();
        }

        private void SerializeObject(object obj)
        {
            var lazyMgr = obj.As<ILazyManager>();
            var flag = new AssertFlag();
            var type = obj.GetType();
            jsonWriter.WriteStartObject();

            foreach (var acc in context.GetProperties(type, () => option.ContractResolver.GetProperties(type)))
            {
                if (acc.Filter(acc.PropertyInfo, lazyMgr))
                {
                    continue;
                }

                var value = acc.Getter.Invoke(obj);
                if (option.NullValueHandling == NullValueHandling.Ignore && value == null)
                {
                    continue;
                }

                if (!flag.AssertTrue())
                {
                    jsonWriter.WriteComma();
                }

                jsonWriter.WriteKey(SerializeName(acc.PropertyName));

                context.SerializeInfo = new PropertySerialzeInfo(acc);

                if (value == null)
                {
                    jsonWriter.WriteNull();
                }
                else
                {
                    //如果在属性上指定了 JsonConverter
                    if (acc.Converter is JsonConverter converter)
                    {
                        converter.WriteJson(serializer, jsonWriter, value);
                    }
                    else
                    {
                        var objType = acc.PropertyInfo.PropertyType == typeof(object) ? value.GetType() : acc.PropertyInfo.PropertyType;
                        Serialize(value, objType);
                    }
                }

                context.SerializeInfo = null;
            }

            jsonWriter.WriteEndObject();
        }

        private void SerializeBytes(byte[] bytes)
        {
            jsonWriter.WriteString(Convert.ToBase64String(bytes, 0, bytes.Length));
        }

        private void SerializeValue(object value)
        {
            var type = value.GetType();
            if (type.IsNullableType())
            {
                type = type.GetNonNullableType();
            }

            if (type.IsEnum)
            {
                jsonWriter.WriteValue(((Enum)value).ToString(context.SerializeInfo?.Formatter ?? "D"));
                return;
            }
            else if (type == typeof(TimeSpan))
            {
                jsonWriter.WriteString(((TimeSpan)value).ToString());
                return;
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    jsonWriter.WriteValue((bool)value ? "true" : "false");
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
                    SerializeNumeric(value);
                    break;
                case TypeCode.DateTime:
                    SerializeDateTime((DateTime)value);
                    break;
                case TypeCode.Char:
                case TypeCode.String:
                    SerializeString(value);
                    break;
                default:
                    context.TrySerialize(value, value => SerializeObject(value));
                    break;
            }
        }

        private void SerializeDateTime(DateTime value)
        {
            value = SerializerUtil.EnsureDateTime(value, option.DateTimeZoneHandling);

            if (option.DateFormatHandling == DateFormatHandling.Default)
            {
                jsonWriter.WriteValue(string.Concat(JsonTokens.StringDelimiter, (value.Year <= 1 ? string.Empty : value.ToString(context.SerializeInfo?.Formatter ?? "yyyy-MM-dd")), JsonTokens.StringDelimiter));
            }
            else if (option.DateFormatHandling == DateFormatHandling.IsoDateFormat)
            {
                jsonWriter.WriteValue(string.Concat(JsonTokens.StringDelimiter, value.GetDateTimeFormats('s')[0].ToString(option.Culture), JsonTokens.StringDelimiter));
            }
            else if (option.DateFormatHandling == DateFormatHandling.MicrosoftDateFormat)
            {
                var offset = TimeZoneInfo.Local.GetUtcOffset(value);
                var time = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var ticks = (value.ToUniversalTime().Ticks - time.Ticks) / 10000;

                var sb = new StringBuilder();
                sb.Append("\"\\/Date(" + ticks);

                if (value.Kind == DateTimeKind.Local)
                {
                    sb.Append(offset.Ticks >= 0 ? "+" : "-");
                    var h = Math.Abs(offset.Hours);
                    var m = Math.Abs(offset.Minutes);
                    if (h < 10)
                    {
                        sb.Append(0);
                    }

                    sb.Append(h);
                    if (m < 10)
                    {
                        sb.Append(0);
                    }

                    sb.Append(m);
                }

                sb.Append(")\\/\"");
                jsonWriter.WriteValue(sb);
            }
        }

        private void SerializeNumeric(object value)
        {
            if (context.SerializeInfo == null || string.IsNullOrEmpty(context.SerializeInfo.Formatter))
            {
                jsonWriter.WriteValue(value);
            }
            else
            {
                jsonWriter.WriteValue(string.Concat(JsonTokens.StringDelimiter, value.As<IFormattable>().ToString(context.SerializeInfo.Formatter, option.Culture), JsonTokens.StringDelimiter));
            }
        }

        private void SerializeString<T>(T value)
        {
            var str = value.ToString();
            jsonWriter.WriteString(str);
        }

        private void SerializeKeyValue(object key, object value)
        {
            if (option.NullValueHandling == NullValueHandling.Ignore && value == null)
            {
                return;
            }

            jsonWriter.WriteKey(SerializeName(key.ToString()));
            Serialize(value);
        }

        private string SerializeName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }

            name = option.ContractResolver.ResolvePropertyName(name);

            if (option.KeyHandling == JsonKeyHandling.None)
            {
                return name;
            }

            return string.Concat(JsonTokens.StringDelimiter, name, JsonTokens.StringDelimiter);
        }

        private void SerializeType(Type type)
        {
            jsonWriter.WriteString(type.FullName + ", " + type.Assembly.FullName);
        }

        /// <summary>
        /// 释放对象所占用的非托管和托管资源。
        /// </summary>
        /// <param name="disposing">为 true 则释放托管资源和非托管资源；为 false 则仅释放非托管资源。</param>
        protected override bool Dispose(bool disposing)
        {
            context.Dispose();

            return base.Dispose(disposing);
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
    }
}
