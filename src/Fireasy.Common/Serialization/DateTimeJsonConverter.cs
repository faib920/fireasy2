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
        /// <param name="serializer">一个 <see cref="JsonSerializer"/> 对象。</param>
        /// <param name="obj">要序列化的 <see cref="DateTime"/> 值。</param>
        /// <returns>表示值的 Json 文本。</returns>
        public override string WriteJson(JsonSerializer serializer, object obj)
        {
            var value = (DateTime?)obj;
            if (value == null || ((DateTime)value).Year <= 1900)
            {
                return "\"\"";
            }

            return string.Format("\"{0}\"", value.Value.ToString(Formatter));
        }

        /// <summary>
        /// 从 Json 中读取 <see cref="DateTime"/> 对象。
        /// </summary>
        /// <param name="serializer">一个 <see cref="JsonSerializer"/> 对象。</param>
        /// <param name="type">要读取的对象的类型。</param>
        /// <param name="json">表示对象的 Json 文本。</param>
        /// <returns>反序列化后的 <see cref="DateTime"/> 值。</returns>
        public override object ReadJson(JsonSerializer serializer, Type type, string json)
        {
            if ((string.IsNullOrEmpty(json) || json == "\"\"" || json == "null") && type == typeof(DateTime?))
            {
                return null;
            }

            json = json.Replace("\"", "");
            if (DateTime.TryParse(json, out DateTime value))
            {
                return value;
            }

            return DateTime.MinValue;
        }
    }
}
