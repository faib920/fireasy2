// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.Extensions;
using Fireasy.Data.Converter;
using Fireasy.Data.Extensions;
using Fireasy.Data.RecordWrapper;
using System;
using System.Data;

namespace Fireasy.Data
{
    /// <summary>
    /// 一个将数据行转换为单一数值或字符串类型的映射器。
    /// </summary>
    /// <typeparam name="T">要转换的类型。该类型一定是实现了 <see cref="IConvertible"/> 或 <see cref="IValueConverter"/> 接口。</typeparam>
    public class SingleValueRowMapper<T> : IDataRowMapper<T>
    {
        /// <summary>
        /// 将一个 <see cref="IDataReader"/> 转换为一个 <typeparamref name="T"/> 的对象。
        /// </summary>
        /// <param name="database">当前的 <see cref="IDatabase"/> 对象。</param>
        /// <param name="reader">一个 <see cref="IDataReader"/> 对象。</param>
        /// <returns>由当前 <see cref="IDataReader"/> 对象中的数据转换成的 <typeparamref name="T"/> 对象实例。</returns>
        public virtual T Map(IDatabase database, IDataReader reader)
        {
            var value = RecordWrapper == null ? reader[0] :
                RecordWrapper.GetValue(reader, 0);

            var converter = ConvertManager.GetConverter(typeof(T));
            return converter != null ? (T)converter.ConvertFrom(value, reader.GetFieldType(0).GetDbType()) :
                value.To<object, T>();
        }

        /// <summary>
        /// 将一个 <see cref="DataRow"/> 转换为一个 <typeparamref name="T"/> 的对象。
        /// </summary>
        /// <param name="database">当前的 <see cref="IDatabase"/> 对象。</param>
        /// <param name="row">一个 <see cref="DataRow"/> 对象。</param>
        /// <returns>由 <see cref="DataRow"/> 中数据转换成的 <typeparamref name="T"/> 对象实例。</returns>
        public virtual T Map(IDatabase database, DataRow row)
        {
            var converter = ConvertManager.GetConverter(typeof(T));
            return converter != null ? (T)converter.ConvertFrom(row[0], row.Table.Columns[0].DataType.GetDbType()) :
                row[0].To<object, T>();
        }

        /// <summary>
        /// 获取或设置 <see cref="IRecordWrapper"/>。
        /// </summary>
        public IRecordWrapper RecordWrapper { get; set; }

        /// <summary>
        /// 获取或设置对象的初始化器。
        /// </summary>
        public Action<object> Initializer { get; set; }

        object IDataRowMapper.Map(IDatabase database, IDataReader reader)
        {
            return Map(database, reader);
        }

        object IDataRowMapper.Map(IDatabase database, DataRow row)
        {
            return Map(database, row);
        }
    }
}
