// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.Extensions;
using System.ComponentModel;
using System.Data;
using System.Drawing;

namespace Fireasy.Data.Converter
{
    /// <summary>
    /// 颜色转换器，用于将一个 <see cref="Color"/> 对象转换为数据库存储数据，也可以用于反转换。无法继承此类。
    /// </summary>
    public sealed class ColorConverter : IValueConverter
    {
        /// <summary>
        /// 将存储的数据转换为指定的类型。
        /// </summary>
        /// <param name="value">要转换的值，如果 <paramref name="dbType"/> 为 String，则格式为ARGB十六进制字符串；如果 <paramref name="dbType"/> 为 Binary，则为 byte 的数组。</param>
        /// <param name="dbType">数据列类型。</param>
        /// <returns>一个 <see cref="Color"/> 对象。</returns>
        /// <exception cref="ConverterNotSupportedException">如果不支持将 <see cref="Color"/> 类型的对象转换为指定的 dbType 数据时，引发此异常。</exception>
        public object ConvertFrom(object value, DbType dbType = DbType.String)
        {
            switch (dbType)
            {
                case DbType.String:
                case DbType.StringFixedLength:
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                    return ConvertFromString(value);
                case DbType.Binary:
                    return ConvertFromBinary(value);
                default:
                    throw new ConverterNotSupportedException(typeof(Color), dbType);
            }
        }

        /// <summary>
        /// 将特殊对象转换为可存储到数据库的类型。
        /// </summary>
        /// <param name="value">要存储的 <see cref="Color"/> 对象。</param>
        /// <param name="dbType">数据列类型。</param>
        /// <returns>如果 <paramref name="dbType"/> 为 String，则为ARGB十六进制字符串，如FF0088；如果 <paramref name="dbType"/> 为 Binary，则为长度为 4 的字节数组。</returns>
        /// <exception cref="ConverterNotSupportedException">如果不支持将 dbType 类型的数据转换为 <see cref="Color"/> 类型的对象时，引发此异常。</exception>
        public object ConvertTo(object value, DbType dbType = DbType.String)
        {
            switch (dbType)
            {
                case DbType.String:
                case DbType.StringFixedLength:
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                    return ConvertToString(value);
                case DbType.Binary:
                    return ConvertToBinary(value);
                default:
                    throw new ConverterNotSupportedException(typeof(Color), dbType);
            }
        }

        /// <summary>
        /// 从一个十六进制字符串中解析颜色。
        /// </summary>
        /// <param name="value">一个字符串对象。</param>
        /// <returns>一个 <see cref="Color"/> 对象。</returns>
        private object ConvertFromString(object value)
        {
            if (value.IsNullOrEmpty())
            {
                return Color.Empty;
            }

            var converter = TypeDescriptor.GetConverter(typeof(int));
            if (converter == null)
            {
                return Color.Empty;
            }

            var val = converter.ConvertFromString("0x" + value);
            return val == null ? Color.Empty : Color.FromArgb(-16777216 | (int)val);
        }

        /// <summary>
        /// 从一个数组中解析颜色。
        /// </summary>
        /// <param name="value">字节数组。</param>
        /// <returns>一个 <see cref="Color"/> 对象。</returns>
        private object ConvertFromBinary(object value)
        {
            var bytes = (byte[])value;
            if (bytes == null)
            {
                return Color.Empty;
            }

            return bytes.Length switch
            {
                4 => Color.FromArgb(bytes[0], bytes[1], bytes[2], bytes[3]),
                3 => Color.FromArgb(bytes[0], bytes[1], bytes[2]),
                _ => Color.Empty,
            };
        }

        /// <summary>
        /// 转换为十六进制字符串。如F9AC00
        /// </summary>
        /// <param name="value">一个 <see cref="Color"/> 对象。</param>
        /// <returns>字符串对象。</returns>
        private object ConvertToString(object value)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return string.Empty;
            }

            var color = (Color)value;
            return string.Format(
                "{0}{1}{2}{3}",
                color.A.ToString("X2"),
                color.R.ToString("X2"),
                color.G.ToString("X2"),
                color.B.ToString("X2"));
        }

        /// <summary>
        /// 转换为数组表示。
        /// </summary>
        /// <param name="value">一个 <see cref="Color"/> 对象。</param>
        /// <returns>字节数组。</returns>
        private object ConvertToBinary(object value)
        {
            if (value == null)
            {
                return new byte[0];
            }

            var color = (Color)value;
            return new[] { color.A, color.R, color.G, color.B };
        }
    }
}
