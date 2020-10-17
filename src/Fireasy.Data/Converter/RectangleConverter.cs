﻿// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.Extensions;
using Fireasy.Data.Extensions;
using System;
using System.Data;
using System.Drawing;

namespace Fireasy.Data.Converter
{
    /// <summary>
    /// 矩形转换器，用于将一个 <see cref="Rectangle"/> 对象转换为数据库存储数据，也可以用于反转换。无法继承此类。
    /// </summary>
    public sealed class RectangleConverter : IValueConverter
    {
        /// <summary>
        /// 将存储的数据转换为指定的类型。
        /// </summary>
        /// <param name="value">要转换的值，格式为 x,y,w,h。</param>
        /// <param name="dbType">数据列类型。</param>
        /// <returns>一个 <see cref="Rectangle"/> 对象。</returns>
        /// <exception cref="ConverterNotSupportedException">如果不支持将 <see cref="Rectangle"/> 类型的对象转换为指定的 dbType 数据时，引发此异常。</exception>
        public object ConvertFrom(object value, DbType dbType = DbType.String)
        {
            if (!dbType.IsStringDbType())
            {
                throw new ConverterNotSupportedException(typeof(Rectangle), dbType);
            }

            if (value.IsNullOrEmpty())
            {
                return Rectangle.Empty;
            }

            var array = value.ToString().Split(',');
            return new Rectangle(Convert.ToInt32(array[0]), Convert.ToInt32(array[1]), Convert.ToInt32(array[2]), Convert.ToInt32(array[3]));
        }

        /// <summary>
        /// 将特殊对象转换为可存储到数据库的类型。
        /// </summary>
        /// <param name="value">要存储的 <see cref="Rectangle"/> 对象。</param>
        /// <param name="dbType">数据列类型。</param>
        /// <returns>使用 x,y,w,h 表示 <see cref="Rectangle"/> 对象的字符串。</returns>
        /// <exception cref="ConverterNotSupportedException">如果不支持将 dbType 类型的数据转换为 <see cref="Rectangle"/> 类型的对象时，引发此异常。</exception>
        public object ConvertTo(object value, DbType dbType = DbType.String)
        {
            if (!dbType.IsStringDbType())
            {
                throw new ConverterNotSupportedException(typeof(Rectangle), dbType);
            }

            if (value == null)
            {
                return string.Empty;
            }

            var rect = (Rectangle)value;
            return string.Join(",", new[] { rect.X, rect.Y, rect.Width, rect.Height });
        }
    }
}
