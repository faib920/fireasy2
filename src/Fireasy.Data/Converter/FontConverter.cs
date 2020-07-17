// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if !NETSTANDARD

using Fireasy.Common.Extensions;
using Fireasy.Data.Extensions;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;

namespace Fireasy.Data.Converter
{
    /// <summary>
    /// 字体转换器，用于将一个 <see cref="System.Drawing.Font"/> 对象转换为数据库存储数据，也可以用于反转换。无法继承此类。
    /// </summary>
    public sealed class FontConverter : IValueConverter
    {
        /// <summary>
        /// 将存储的数据转换为指定的类型。
        /// </summary>
        /// <param name="value">要转换的值，格式为 name,size[unit][,style]。</param>
        /// <param name="dbType">数据列类型。</param>
        /// <returns>一个 <see cref="Font"/> 对象。</returns>
        /// <exception cref="ConverterNotSupportedException">如果不支持将 <see cref="Font"/> 类型的对象转换为指定的 dbType 数据时，引发此异常。</exception>
        public object ConvertFrom(object value, DbType dbType = DbType.String)
        {
            if (!dbType.IsStringDbType())
            {
                throw new ConverterNotSupportedException(typeof(Font), dbType);
            }

            if (value.IsNullOrEmpty())
            {
                return null;
            }

            var array = value.ToString().Split(',');
            var style = FontStyle.Regular;
            ParseFontSizeAndUnit(array[1], out float size, out GraphicsUnit unit);

            if (array.Length == 3)
            {
                style = ParseFontStyle(array[2]);
            }

            return new Font(array[0], size, style, unit);
        }

        /// <summary>
        /// 将特殊对象转换为可存储到数据库的类型。
        /// </summary>
        /// <param name="value">要存储的 <see cref="Font"/> 对象。</param>
        /// <param name="dbType">数据列类型。</param>
        /// <returns>表示 <see cref="Font"/> 对象的字符串。</returns>
        /// <exception cref="ConverterNotSupportedException">如果不支持将 dbType 类型的数据转换为 <see cref="Font"/> 类型的对象时，引发此异常。</exception>
        /// <remarks>如宋体12px，粗休删除线，可以表示为 宋体,12px,BU。</remarks>
        public object ConvertTo(object value, DbType dbType = DbType.String)
        {
            if (!dbType.IsStringDbType())
            {
                throw new ConverterNotSupportedException(typeof(Font), dbType);
            }

            if (value == null)
            {
                return string.Empty;
            }

            if (!(value is Font font))
            {
                return string.Empty;
            }

            var num = 3;
            if (font.Style != FontStyle.Regular)
            {
                num++;
            }

            var strArray = new string[num];
            var num2 = 0;
            strArray[num2++] = font.Name;
            strArray[num2++] = font.Size + GetUnitFlag(font.Unit);
            if (font.Style != FontStyle.Regular)
            {
                strArray[num2] = GetStyleFlag(font.Style);
            }

            return string.Join(",", strArray);
        }

        private string GetUnitFlag(GraphicsUnit unit)
        {
            return unit switch
            {
                GraphicsUnit.Inch => "in",
                GraphicsUnit.Pixel => "px",
                GraphicsUnit.Point => "pt",
                GraphicsUnit.Millimeter => "mm",
                _ => string.Empty,
            };
        }

        private void ParseFontSizeAndUnit(string flag, out float size, out GraphicsUnit unit)
        {
            var matches = Regex.Matches(flag, @"(\d+)(px|pt|in|mm)");
            if (matches.Count == 0)
            {
                size = 0;
                unit = GraphicsUnit.Pixel;
                return;
            }

            float.TryParse(matches[0].Groups[1].Value, out size);
            unit = matches[0].Groups[2].Value switch
            {
                "px" => GraphicsUnit.Pixel,
                "pt" => GraphicsUnit.Point,
                "in" => GraphicsUnit.Inch,
                "mm" => GraphicsUnit.Millimeter,
                _ => GraphicsUnit.Pixel,
            };
        }

        private string GetStyleFlag(FontStyle style)
        {
            var sb = new StringBuilder();
            if (style.HasFlag(FontStyle.Bold))
            {
                sb.Append("B");
            }

            if (style.HasFlag(FontStyle.Italic))
            {
                sb.Append("I");
            }

            if (style.HasFlag(FontStyle.Underline))
            {
                sb.Append("U");
            }

            if (style.HasFlag(FontStyle.Strikeout))
            {
                sb.Append("S");
            }

            return sb.ToString();
        }

        private FontStyle ParseFontStyle(string flag)
        {
            var style = FontStyle.Regular;
            foreach (var c in flag)
            {
                switch (char.ToUpper(c))
                {
                    case 'B':
                        style |= FontStyle.Bold;
                        break;
                    case 'U':
                        style |= FontStyle.Underline;
                        break;
                    case 'I':
                        style |= FontStyle.Italic;
                        break;
                    case 'S':
                        style |= FontStyle.Strikeout;
                        break;
                }
            }

            return style;
        }
    }
}
#endif