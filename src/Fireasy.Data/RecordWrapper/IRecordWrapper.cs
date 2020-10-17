// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Data.Provider;
using System;
using System.Data;

namespace Fireasy.Data.RecordWrapper
{
    /// <summary>
    /// 记录包装器，由于各 <see cref="IDataReader"/> 对数据类型的支持可能不一样，因此需要处理以使数据达到兼容。
    /// </summary>
    public interface IRecordWrapper : IProviderService
    {
        /// <summary>
        /// 获取指定索引处的字段名称。
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        string GetFieldName(IDataReader reader, int i);

        /// <summary>
        /// 返回指定字段的值。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="i">字段的索引。</param>
        /// <returns>该字段包含的对象。</returns>
        object GetValue(IDataRecord reader, int i);

        /// <summary>
        /// 返回指定字段的值。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="name">字段的名称。</param>
        /// <returns>该字段包含的对象。</returns>
        object GetValue(IDataRecord reader, string name);

        /// <summary>
        /// 获取指定列的布尔值形式的值。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="i">字段的索引。</param>
        /// <returns>字段的 <see cref="Boolean"/> 值。</returns>
        bool GetBoolean(IDataRecord reader, int i);

        /// <summary>
        /// 获取指定列的布尔值形式的值。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="name">字段的名称。</param>
        /// <returns>字段的 <see cref="Boolean"/> 值。</returns>
        bool GetBoolean(IDataRecord reader, string name);

        /// <summary>
        /// 获取指定列的 8 位无符号整数值。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="i">字段的索引。</param>
        /// <returns>字段的 <see cref="Byte"/> 值。</returns>
        byte GetByte(IDataRecord reader, int i);

        /// <summary>
        /// 获取指定列的 8 位无符号整数值。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="name">字段的名称。</param>
        /// <returns>字段的 <see cref="Byte"/> 值。</returns>
        byte GetByte(IDataRecord reader, string name);

        /// <summary>
        /// 返回指定字段的字节数组。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="i">字段的索引。</param>
        /// <returns>字段的 <see cref="Byte"/> 数组值。</returns>
        byte[] GetBytes(IDataRecord reader, int i);

        /// <summary>
        /// 返回指定字段的字节数组。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="name">字段的名称。</param>
        /// <returns>字段的 <see cref="Byte"/> 数组值。</returns>
        byte[] GetBytes(IDataRecord reader, string name);

        /// <summary>
        /// 获取指定列的字符值。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="i">字段的索引。</param>
        /// <returns>字段的 <see cref="Char"/> 值。</returns>
        char GetChar(IDataRecord reader, int i);

        /// <summary>
        /// 获取指定列的字符值。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="name">字段的名称。</param>
        /// <returns>字段的 <see cref="Char"/> 值。</returns>
        char GetChar(IDataRecord reader, string name);

        /// <summary>
        /// 返回指定字段的字符数组。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="i">字段的索引。</param>
        /// <returns>字段的 <see cref="Char"/> 数组值。</returns>
        char[] GetChars(IDataRecord reader, int i);

        /// <summary>
        /// 返回指定字段的 GUID 值。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="i">字段的索引。</param>
        /// <returns>字段的 <see cref="Guid"/> 值。</returns>
        Guid GetGuid(IDataRecord reader, int i);

        /// <summary>
        /// 获取指定字段的 16 位有符号整数值。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="i">字段的索引。</param>
        /// <returns>字段的 <see cref="Int16"/> 值。</returns>
        short GetInt16(IDataRecord reader, int i);

        /// <summary>
        /// 获取指定字段的 16 位有符号整数值。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="name">字段的名称。</param>
        /// <returns>字段的 <see cref="Int16"/> 值。</returns>
        short GetInt16(IDataRecord reader, string name);

        /// <summary>
        /// 获取指定字段的 32 位有符号整数值。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="i">字段的索引。</param>
        /// <returns>字段的 <see cref="Int32"/> 值。</returns>
        int GetInt32(IDataRecord reader, int i);

        /// <summary>
        /// 获取指定字段的 32 位有符号整数值。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="name">字段的名称。</param>
        /// <returns>字段的 <see cref="Int32"/> 值。</returns>
        int GetInt32(IDataRecord reader, string name);

        /// <summary>
        /// 获取指定字段的 64 位有符号整数值。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="i">字段的索引。</param>
        /// <returns>字段的 <see cref="Int64"/> 值。</returns>
        long GetInt64(IDataRecord reader, int i);

        /// <summary>
        /// 获取指定字段的 64 位有符号整数值。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="name">字段的名称。</param>
        /// <returns>字段的 <see cref="Int64"/> 值。</returns>
        long GetInt64(IDataRecord reader, string name);

        /// <summary>
        /// 获取指定字段的单精度浮点数。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="i">字段的索引。</param>
        /// <returns>字段的 <see cref="Single"/> 值。</returns>
        float GetFloat(IDataRecord reader, int i);

        /// <summary>
        /// 获取指定字段的单精度浮点数。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="name">字段的名称。</param>
        /// <returns>字段的 <see cref="Single"/> 值。</returns>
        float GetFloat(IDataRecord reader, string name);

        /// <summary>
        /// 获取指定字段的双精度浮点数。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="i">字段的索引。</param>
        /// <returns>字段的 <see cref="Double"/> 值。</returns>
        double GetDouble(IDataRecord reader, int i);

        /// <summary>
        /// 获取指定字段的双精度浮点数。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="name">字段的名称。</param>
        /// <returns>字段的 <see cref="Double"/> 值。</returns>
        double GetDouble(IDataRecord reader, string name);

        /// <summary>
        /// 获取指定字段的字符串值。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="i">字段的索引。</param>
        /// <returns>字段的 <see cref="String"/> 值。</returns>
        string GetString(IDataRecord reader, int i);

        /// <summary>
        /// 获取指定字段的字符串值。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="name">字段的名称。</param>
        /// <returns>字段的 <see cref="String"/> 值。</returns>
        string GetString(IDataRecord reader, string name);

        /// <summary>
        /// 获取指定字段的固定位置的数值。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="i">字段的索引。</param>
        /// <returns>字段的 <see cref="Decimal"/> 值。</returns>
        decimal GetDecimal(IDataRecord reader, int i);

        /// <summary>
        /// 获取指定字段的固定位置的数值。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="name">字段的名称。</param>
        /// <returns>字段的 <see cref="Decimal"/> 值。</returns>
        decimal GetDecimal(IDataRecord reader, string name);

        /// <summary>
        /// 获取指定字段的固定位置的日期和时间。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="i">字段的索引。</param>
        /// <returns>字段的 <see cref="DateTime"/> 值。</returns>
        DateTime GetDateTime(IDataRecord reader, int i);

        /// <summary>
        /// 获取指定字段的固定位置的日期和时间。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="name">字段的名称。</param>
        /// <returns>字段的 <see cref="DateTime"/> 值。</returns>
        DateTime GetDateTime(IDataRecord reader, string name);

        /// <summary>
        /// 判断指定索引位置的字段值是否为 DBNull。
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        bool IsDbNull(IDataReader reader, int i);
    }
}
