// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using System;
using System.Globalization;

namespace Fireasy.Common.Serialization
{
    /// <summary>
    /// 完整的日期时间格式转换器。
    /// </summary>
    public sealed class FullDateTimeJsonConverter : JsonConverter
    {
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
            if (obj == null)
            {
                return "null";
            }

            var time = (DateTime)obj;
            return string.Format("\"{0}\"", time.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture));
        }

        /// <summary>
        /// 从 Json 中读取 <see cref="DateTime"/> 对象。
        /// </summary>
        /// <param name="serializer">一个 <see cref="JsonSerializer"/> 对象。</param>
        /// <param name="dataType">要读取的对象的类型。</param>
        /// <param name="json">表示对象的 Json 文本。</param>
        /// <returns>反序列化后的 <see cref="DateTime"/> 值。</returns>
        public override object ReadJson(JsonSerializer serializer, Type dataType, string json)
        {
            if ((string.IsNullOrEmpty(json) || json == "null") && dataType.IsNullableType())
            {
                return (DateTime?)null;
            }

            DateTime time;
            if (DateTime.TryParse(json, CultureInfo.CurrentCulture, DateTimeStyles.AdjustToUniversal, out time))
            {
                return time;
            }

            return (DateTime?)null;
        }
    }
}
