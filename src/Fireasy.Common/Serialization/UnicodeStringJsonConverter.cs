// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Globalization;
using System.Text;

namespace Fireasy.Common.Serialization
{
    /// <summary>
    /// 字符串 Unicode 编码的转换器。
    /// </summary>
    public class UnicodeStringJsonConverter : JsonConverter
    {
        /// <summary>
        /// 判断指定的类型是否允许转换。
        /// </summary>
        /// <param name="type">要判断的类型。</param>
        /// <returns>可以转换则为 true。</returns>
        public override bool CanConvert(Type type)
        {
            return type == typeof(string);
        }

        public override bool CanRead => false;

        /// <summary>
        /// 将字符串写为 Json 文本。
        /// </summary>
        /// <param name="serializer">当前的 <see cref="JsonSerializer"/> 对象。</param>
        /// <param name="writer"><see cref="JsonWriter"/>对象。</param>
        /// <param name="obj">要序列化的字符串。</param>
        public override void WriteJson(JsonSerializer serializer, JsonWriter writer, object obj)
        {
            writer.WriteString(obj == null ? "null" : QuoteString(obj.ToString()));
        }

        private string QuoteString(string value)
        {
            StringBuilder builder = null;
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var startIndex = 0;
            var count = 0;
            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                if ((((c == '\r') || (c == '\t')) || ((c == '"') || (c == '\''))) || ((((c == '<') || (c == '>')) || ((c == '\\') || (c == '\n'))) || (((c == '\b') || (c == '\f')) || (c < ' '))))
                {
                    if (builder == null)
                    {
                        builder = new StringBuilder(value.Length + 5);
                    }

                    if (count > 0)
                    {
                        builder.Append(value, startIndex, count);
                    }

                    startIndex = i + 1;
                    count = 0;
                }

                switch (c)
                {
                    case '<':
                    case '>':
                    case '\'':
                    case '"':
                        AppendCharAsUnicode(builder, c);
                        continue;
                    case '\\':
                        builder.Append(@"\\");
                        continue;
                    case '\b':
                        builder.Append(@"\b");
                        continue;
                    case '\t':
                        builder.Append(@"\t");
                        continue;
                    case '\n':
                        builder.Append(@"\n");
                        continue;
                    case '\f':
                        builder.Append(@"\f");
                        continue;
                    case '\r':
                        builder.Append(@"\r");
                        continue;
                }

                if (c < ' ')
                {
                    AppendCharAsUnicode(builder, c);
                }
                else
                {
                    count++;
                }
            }

            if (builder == null)
            {
                return value;
            }

            if (count > 0)
            {
                builder.Append(value, startIndex, count);
            }

            return builder.ToString();
        }

        private static void AppendCharAsUnicode(StringBuilder builder, char c)
        {
            builder.Append(@"\u");
            builder.AppendFormat(CultureInfo.InvariantCulture, "{0:x4}", new object[] { (int)c });
        }
    }
}
