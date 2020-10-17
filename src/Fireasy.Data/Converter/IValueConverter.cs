// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Data;

namespace Fireasy.Data.Converter
{
    /// <summary>
    /// 提供两个方法，用于特殊数据的存储与读取。
    /// </summary>
    public interface IValueConverter
    {
        /// <summary>
        /// 将存储的数据转换为指定的类型。
        /// </summary>
        /// <param name="value">要转换的值。</param>
        /// <param name="dbType">数据列类型。</param>
        /// <returns>特定类型的对象。</returns>
        object ConvertFrom(object value, DbType dbType = DbType.String);

        /// <summary>
        /// 将特殊对象转换为可存储到数据库的类型。
        /// </summary>
        /// <param name="value">要存储的值。</param>
        /// <param name="dbType">数据列类型。</param>
        /// <returns>使用 <paramref name="dbType"/> 类型的数据。</returns>
        object ConvertTo(object value, DbType dbType = DbType.String);
    }

    /// <summary>
    ///  一个泛型转换器的基类。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ValueConverterBase<T> : IValueConverter
    {
        /// <summary>
        /// 将存储的数据转换为指定的类型。
        /// </summary>
        /// <param name="value">要转换的值。</param>
        /// <param name="dbType">数据列类型。</param>
        /// <returns>特定类型的对象。</returns>
        public abstract T ConvertFrom(object value, DbType dbType = DbType.String);

        /// <summary>
        /// 将特殊对象转换为可存储到数据库的类型。
        /// </summary>
        /// <param name="value">要存储的值。</param>
        /// <param name="dbType">数据列类型。</param>
        /// <returns>使用 <paramref name="dbType"/> 类型的数据。</returns>
        public abstract object ConvertTo(T value, DbType dbType = DbType.String);

        object IValueConverter.ConvertFrom(object value, DbType dbType)
        {
            return ConvertFrom(value, dbType);
        }

        object IValueConverter.ConvertTo(object value, DbType dbType)
        {
            return ConvertTo((T)value, dbType);
        }
    }
}
