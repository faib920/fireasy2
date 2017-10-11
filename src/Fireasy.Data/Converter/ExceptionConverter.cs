// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Data;
using Fireasy.Common.Extensions;
using Fireasy.Common.Serialization;

namespace Fireasy.Data.Converter
{
    /// <summary>
    /// 异常转换器，用于将一个 <see cref="Exception"/> 对象转换为数据库存储数据，也可以用于反转换。无法继承此类。
    /// </summary>
    public sealed class ExceptionConverter : IValueConverter
    {
        /// <summary>
        /// 将存储的数据转换为指定的类型。
        /// </summary>
        /// <param name="value">要转换的值，为 <see cref="Exception"/> 对象序列化后的字节数组。</param>
        /// <param name="dbType">数据列类型。</param>
        /// <returns>一个 <see cref="Exception"/> 对象。</returns>
        /// <exception cref="ConverterNotSupportedException">如果不支持将 <see cref="Exception"/> 类型的对象转换为指定的 dbType 数据时，引发此异常。</exception>
        public object ConvertFrom(object value, DbType dbType = DbType.String)
        {
            if (dbType != DbType.Binary)
            {
                throw new ConverterNotSupportedException(typeof(Exception), dbType);
            }

            if (value.IsNullOrEmpty())
            {
                return null;
            }

            return new BinaryCompressSerializer().Deserialize<object>((byte[])value);
        }

        /// <summary>
        /// 将特殊对象转换为可存储到数据库的类型。
        /// </summary>
        /// <param name="value">要存储的 <see cref="Exception"/> 对象。</param>
        /// <param name="dbType">数据列类型。</param>
        /// <returns>一个 <see cref="Exception"/> 对象序列化后的字节数组。</returns>
        /// <exception cref="ConverterNotSupportedException">如果不支持将 dbType 类型的数据转换为 <see cref="Exception"/> 类型的对象时，引发此异常。</exception>
        public object ConvertTo(object value, DbType dbType = DbType.String)
        {
            if (dbType != DbType.Binary)
            {
                throw new ConverterNotSupportedException(typeof(Exception), dbType);
            }

            return new BinaryCompressSerializer().Serialize(value);
        }
    }
}
