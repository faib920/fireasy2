// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Data;
using System.Drawing;
using Fireasy.Common.Extensions;
using Fireasy.Data.Extensions;

namespace Fireasy.Data.Converter
{
    /// <summary>
    /// 大小转换器，用于将一个 <see cref="System.Drawing.Size"/> 对象转换为数据库存储数据，也可以用于反转换。无法继承此类。
    /// </summary>
    public sealed class SizeConverter : IValueConverter
    {
        /// <summary>
        /// 将存储的数据转换为指定的类型。
        /// </summary>
        /// <param name="value">要转换的值，格式为 w,h。</param>
        /// <param name="dbType">数据列类型。</param>
        /// <returns>一个 <see cref="Size"/> 对象。</returns>
        /// <exception cref="ConverterNotSupportedException">如果不支持将 <see cref="Size"/> 类型的对象转换为指定的 dbType 数据时，引发此异常。</exception>
        public object ConvertFrom(object value, DbType dbType = DbType.String)
        {
            if (!dbType.IsStringDbType())
            {
                throw new ConverterNotSupportedException(typeof(Size), dbType);
            }

            if (value.IsNullOrEmpty())
            {
                return Size.Empty;
            }

            var array = value.ToString().Split(',');
            return new Size(Convert.ToInt32(array[0]), Convert.ToInt32(array[1]));
        }

        /// <summary>
        /// 将特殊对象转换为可存储到数据库的类型。
        /// </summary>
        /// <param name="value">要存储的 <see cref="Size"/> 对象。</param>
        /// <param name="dbType">数据列类型。</param>
        /// <returns>使用 w,h 表示 <see cref="Size"/> 对象的字符串。</returns>
        /// <exception cref="ConverterNotSupportedException">如果不支持将 dbType 类型的数据转换为 <see cref="Size"/> 类型的对象时，引发此异常。</exception>
        public object ConvertTo(object value, DbType dbType = DbType.String)
        {
            if (!dbType.IsStringDbType())
            {
                throw new ConverterNotSupportedException(typeof(Size), dbType);
            }

            if (value == null || !value.Is<Size>())
            {
                return string.Empty;
            }

            var size = (Size)value;
            return string.Join(",", new[] { size.Width, size.Height });
        }
    }
}
