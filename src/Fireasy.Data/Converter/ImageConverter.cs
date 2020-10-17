﻿// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Fireasy.Data.Converter
{
    /// <summary>
    /// 图像转换器，用于将一个 <see cref="System.Drawing.Image"/> 对象转换为数据库存储数据，也可以用于反转换。无法继承此类。
    /// </summary>
    public sealed class ImageConverter : IValueConverter
    {
        /// <summary>
        /// 将存储的数据转换为指定的类型。
        /// </summary>
        /// <param name="value">要转换的值。</param>
        /// <param name="dbType">数据列类型。</param>
        /// <returns>一个 <see cref="Image"/> 对象。</returns>
        /// <exception cref="ConverterNotSupportedException">如果不支持将 <see cref="Image"/> 类型的对象转换为指定的 dbType 数据时，引发此异常。</exception>
        public object ConvertFrom(object value, DbType dbType = DbType.String)
        {
            if (value == null || value == DBNull.Value)
            {
                return null;
            }

            if (dbType != DbType.Binary)
            {
                throw new ConverterNotSupportedException(typeof(Image), dbType);
            }

            if (!(value is byte[] bytes))
            {
                return null;
            }

            using var stream = new MemoryStream(bytes);
            try
            {
                return Image.FromStream(stream);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 将特殊对象转换为可存储到数据库的类型。
        /// </summary>
        /// <param name="value">要存储的 <see cref="Image"/> 对象。</param>
        /// <param name="dbType">数据列类型。</param>
        /// <returns>表示 <see cref="Image"/> 的字节数组。</returns>
        /// <exception cref="ConverterNotSupportedException">如果不支持将 dbType 类型的数据转换为 <see cref="Image"/> 类型的对象时，引发此异常。</exception>
        public object ConvertTo(object value, DbType dbType = DbType.String)
        {
            if (dbType != DbType.Binary)
            {
                throw new ConverterNotSupportedException(typeof(Image), dbType);
            }

            if (!(value is Image image))
            {
                return new byte[0];
            }

            using var stream = new MemoryStream();
            image.Save(stream, ImageFormat.Png);
            return stream.ToArray();
        }
    }
}