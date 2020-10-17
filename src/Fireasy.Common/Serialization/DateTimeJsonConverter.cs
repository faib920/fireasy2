// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Globalization;

namespace Fireasy.Common.Serialization
{
    /// <summary>
    /// <see cref="DateTime"/> 及可空类型的转换器。
    /// </summary>
    public class DateTimeJsonConverter : JsonConverter
    {
        /// <summary>
        /// 使用日期输出格式串初始化 <see cref="DateTimeJsonConverter"/> 类的新实例。
        /// </summary>
        /// <param name="formatter">表示日期文本的格式，默认为 yyyy-MM-dd。</param>
        public DateTimeJsonConverter(string formatter = "yyyy-MM-dd")
        {
            Formatter = formatter;
        }

        /// <summary>
        /// 获取或设置表示日期文本的格式，默认为 yyyy-MM-dd。
        /// </summary>
        public string Formatter { get; set; }

        /// <summary>
        /// 判断指定的类型是否允许转换。
        /// </summary>
        /// <param name="type">要判断的类型。</param>
        /// <returns>可以转换则为 true。</returns>
        public override bool CanConvert(Type type)
        {
            return type == typeof(DateTime) ||
                type == typeof(DateTime?);
        }

        /// <summary>
        /// 将 <see cref="DateTime"/> 值写为 Json 文本。
        /// </summary>
        /// <param name="serializer">当前的 <see cref="JsonSerializer"/> 对象。</param>
        /// <param name="writer"><see cref="JsonWriter"/>对象。</param>
        /// <param name="obj">要序列化的 <see cref="DateTime"/> 值。</param>
        public override void WriteJson(JsonSerializer serializer, JsonWriter writer, object obj)
        {
            var value = (DateTime?)obj;
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteString(value.Value.ToString(Formatter, CultureInfo.CurrentCulture));
            }
        }

        /// <summary>
        /// 从 Json 中读取 <see cref="DateTime"/> 对象。
        /// </summary>
        /// <param name="serializer">当前的 <see cref="JsonSerializer"/> 对象。</param>
        /// <param name="reader"><see cref="JsonReader"/>对象。</param>
        /// <param name="dataType">将要读取的类型。</param>
        /// <returns>反序列化后的对象。</returns>
        public override object ReadJson(JsonSerializer serializer, JsonReader reader, Type dataType)
        {
            var json = reader.ReadRaw();

            if (DateTime.TryParseExact(json, Formatter, CultureInfo.CurrentCulture.DateTimeFormat, DateTimeStyles.None, out DateTime time))
            {
                return time;
            }

            return null;
        }
    }
}
