// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Extensions;
using System;
using System.Data;

namespace Fireasy.Data.Converter
{
    /// <summary>
    /// 编码数据转换器，用于将一个 <see cref="CodedData"/> 对象转换为数据库存储数据，也可以用于反转换。
    /// </summary>
    public abstract class CodedDataConverter : IValueConverter
    {
        /// <summary>
        /// 将存储的数据转换为指定的类型。
        /// </summary>
        /// <param name="value">要转换的值。</param>
        /// <param name="dbType">数据列类型。</param>
        /// <returns>一个 <see cref="CodedData"/> 对象。</returns>
        /// <exception cref="ConverterNotSupportedException">如果不支持将 <see cref="CodedData"/> 类型的对象转换为指定的 dbType 数据时，引发此异常。</exception>
        public virtual object ConvertFrom(object value, DbType dbType = DbType.String)
        {
            if (value == null || value == DBNull.Value)
            {
                return null;
            }

            if (dbType == DbType.Binary)
            {
                return DecodeDataFromBytes((byte[])value);
            }
            else if (dbType.IsStringDbType())
            {
                return DecodeDataFromString(value.ToString());
            }

            return null;
        }

        /// <summary>
        /// 将特殊对象转换为可存储到数据库的类型。
        /// </summary>
        /// <param name="value">要存储的 <see cref="CodedData"/> 对象。</param>
        /// <param name="dbType">数据列类型。</param>
        /// <returns>表示 <see cref="CodedData"/> 的字节数组。</returns>
        /// <exception cref="ConverterNotSupportedException">如果不支持将 dbType 类型的数据转换为 <see cref="CodedData"/> 类型的对象时，引发此异常。</exception>
        public virtual object ConvertTo(object value, DbType dbType = DbType.String)
        {
            if (dbType != DbType.Binary && !dbType.IsStringDbType())
            {
                throw new ConverterNotSupportedException(typeof(CodedData), dbType);
            }

            var data = value as CodedData;
            if (dbType == DbType.Binary)
            {
                return data == null ? new byte[0] : EncodeDataToBytes(data);
            }
            else if (dbType.IsStringDbType())
            {
                return data == null ? string.Empty : EncodeDataToString(data);
            }

            return null;
        }

        /// <summary>
        /// 对数据编码二进制数据。
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected abstract byte[] EncodeDataToBytes(CodedData data);

        /// <summary>
        /// 对数据编码为字符串。
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected virtual string EncodeDataToString(CodedData data)
        {
            return Convert.ToBase64String(EncodeDataToBytes(data));
        }

        /// <summary>
        /// 对二进制的数据进行解码。
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected abstract CodedData DecodeDataFromBytes(byte[] data);

        /// <summary>
        /// 对字符串类型的数据进行解码。
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected virtual CodedData DecodeDataFromString(string data)
        {
            return DecodeDataFromBytes(Convert.FromBase64String(data));
        }
    }
}
