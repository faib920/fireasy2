// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Data;

namespace Fireasy.Data.RecordWrapper
{
    /// <summary>
    /// Oracle 的记录包装器。无法继承此类。
    /// </summary>
    public sealed class OracleRecordWrapper : GeneralRecordWrapper
    {
        /// <summary>
        /// 获取布尔值。由于 Oracle 没有该数据类型，只能使用整型进行转换。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="i">字段的索引。</param>
        /// <returns>字段的 <see cref="Boolean"/> 值。</returns>
        public override bool GetBoolean(IDataRecord reader, int i)
        {
            return !reader.IsDBNull(i) && Convert.ToBoolean(GetInt32(reader, i));
        }

        /// <summary>
        /// 获取字节值。由于 Oracle 没有该数据类型，只能使用整型进行转换。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="i">字段的索引。</param>
        /// <returns>字段的 <see cref="Byte"/> 值。</returns>
        public override byte GetByte(IDataRecord reader, int i)
        {
            return reader.IsDBNull(i) ? (byte)0 : Convert.ToByte(GetInt32(reader, i));
        }

        /// <summary>
        /// 获取字符值。由于 Oracle 没有该数据类型，只能使用字符串进行转换。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="i">字段的索引。</param>
        /// <returns>字段的 <see cref="Char"/> 值。</returns>
        public override char GetChar(IDataRecord reader, int i)
        {
            return reader.IsDBNull(i) ? '\0' : Convert.ToChar(GetString(reader, i));
        }

        /// <summary>
        /// 获取短整型值。由于 Oracle 没有该数据类型，只能使用整型进行转换。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="i">字段的索引。</param>
        /// <returns>字段的 <see cref="Int16"/> 值。</returns>
        public override short GetInt16(IDataRecord reader, int i)
        {
            return reader.IsDBNull(i) ? (short)0 : Convert.ToInt16(GetInt32(reader, i));
        }
    }
}
