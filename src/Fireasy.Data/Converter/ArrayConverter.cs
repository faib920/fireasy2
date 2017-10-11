// -----------------------------------------------------------------------
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
using System.Linq;

namespace Fireasy.Data.Converter
{
    public class ArrayConverter : IValueConverter
    {
        private Type elementType;

        public ArrayConverter(Type elementType)
        {
            this.elementType = elementType;
        }

        /// <summary>
        /// 将存储的数据转换为指定的类型。
        /// </summary>
        /// <param name="value">要转换的值。</param>
        /// <param name="dbType">数据列类型。</param>
        /// <returns>一个数组。</returns>
        /// <exception cref="ConverterNotSupportedException">如果不支持将数组转换为指定的 dbType 数据时，引发此异常。</exception>
        public object ConvertFrom(object value, DbType dbType = DbType.String)
        {
            if (dbType.IsStringDbType())
            {
                var array = value.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.ToType(elementType)).ToArray();
                var result = Array.CreateInstance(elementType, array.Length);
                Array.Copy(array, result, array.Length);
                return result;
            }

            return value;
        }

        /// <summary>
        /// 将特殊对象转换为可存储到数据库的类型。
        /// </summary>
        /// <param name="value">要存储的数组。</param>
        /// <param name="dbType">数据列类型。</param>
        /// <returns></returns>
        /// <exception cref="ConverterNotSupportedException">如果不支持将 dbType 类型的数据转换为数组时，引发此异常。</exception>
        public object ConvertTo(object value, DbType dbType = DbType.String)
        {
            if (dbType.IsStringDbType())
            {
#if !NET35
                if (elementType == typeof(string))
                {
                    return string.Join(",", ((object[])value).Select(s => "'" + s + "'"));
                }
                else
                {
                    return string.Join(",", (object[])value);
                }
#else
                if (elementType == typeof(string))
                {
                    return string.Join(",", ((object[])value).Select(s => "'" + s + "'").ToArray());
                }
                else
                {
                    return string.Join(",", ((object[])value).Select(s => s.ToString()).ToArray());
                }
#endif
            }

            return value;
        }
    }
}
