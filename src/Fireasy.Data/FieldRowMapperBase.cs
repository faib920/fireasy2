// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.RecordWrapper;
using System;
using System.Data;

namespace Fireasy.Data
{
    /// <summary>
    /// 一个抽象类，使用 <see cref="IDataReader"/> 或 <see cref="DataRow"/> 的相关列索引位置进行映射。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class FieldRowMapperBase<T> : IDataRowMapper<T>
    {
        /// <summary>
        /// 将一个 <see cref="IDataReader"/> 转换为一个 <typeparamref name="T"/> 的对象。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataReader"/> 对象。</param>
        /// <returns>由当前 <see cref="IDataReader"/> 对象中的数据转换成的 <typeparamref name="T"/> 对象实例。</returns>
        public abstract T Map(IDataReader reader);

        /// <summary>
        /// 获取或设置 <see cref="IRecordWrapper"/>。
        /// </summary>
        public IRecordWrapper RecordWrapper { get; set; }

        object IDataRowMapper.Map(IDataReader reader)
        {
            return Map(reader);
        }

        /// <summary>
        /// 获取 <see cref="IDataRecord"/> 对象中的所有列。
        /// </summary>
        /// <param name="reader">当前实例中的 <see cref="IDataRecord"/> 对象。</param>
        /// <returns><see cref="IDataRecord"/> 对象中所包含的字段名称的数组。</returns>
        protected string[] GetDataReaderFields(IDataRecord reader)
        {
            var fieldLength = reader.FieldCount;
            var fields = new string[fieldLength];
            for (var i = 0; i < fieldLength; i++)
            {
                fields[i] = reader.GetName(i);
            }

            return fields;
        }

        /// <summary>
        /// 获取列在列数组中的索引位置。
        /// </summary>
        /// <param name="fields">包含的列数组。</param>
        /// <param name="fieldName">要检查的列名称。</param>
        /// <returns>如果列存在于数组中，则为大于或等于 0 的值，否则为 -1。</returns>
        protected int IndexOf(string[] fields, string fieldName)
        {
            for (int i = 0, n = fields.Length; i < n; i++)
            {
                var fieldName1 = fields[i];
                var p = fieldName1.IndexOf('.');
                if (p != -1)
                {
                    fieldName1 = fieldName1.Substring(p + 1);
                }

                if (fieldName.Equals(fieldName1, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
