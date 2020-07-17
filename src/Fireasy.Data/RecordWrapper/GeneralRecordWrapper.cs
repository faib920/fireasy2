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
using System.IO;
using System.Linq;
using System.Text;

namespace Fireasy.Data.RecordWrapper
{
    /// <summary>
    /// 一个通用的记录包装器。
    /// </summary>
    public class GeneralRecordWrapper : IRecordWrapper
    {
        IProvider IProviderService.Provider { get; set; }

        /// <summary>
        /// 获取指定索引处的字段名称。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="i">字段的索引。</param>
        /// <returns></returns>
        public virtual string GetFieldName(IDataReader reader, int i)
        {
            var fieldName = reader.GetName(i).Trim();

            //处理有别名的情况
            var index = fieldName.IndexOf('.');
            return index != -1 ? fieldName.Substring(index + 1) : fieldName;
        }

        /// <summary>
        /// 返回指定字段的值。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="i">字段的索引。</param>
        /// <returns>该字段包含的对象。</returns>
        public virtual object GetValue(IDataRecord reader, int i)
        {
            return reader.GetValue(i);
        }

        /// <summary>
        /// 返回指定字段的值。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="name">字段的名称。</param>
        /// <returns>该字段包含的对象。</returns>
        public virtual object GetValue(IDataRecord reader, string name)
        {
            return reader[name];
        }

        /// <summary>
        /// 获取指定列的布尔值形式的值。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="i">字段的索引。</param>
        /// <returns>字段的 <see cref="Boolean"/> 值。</returns>
        public virtual bool GetBoolean(IDataRecord reader, int i)
        {
            if (reader.IsDBNull(i))
            {
                return false;
            }

            var type = reader.GetFieldType(i);
            return type == typeof(bool) ? reader.GetBoolean(i) :
                Convert.ToBoolean(GetValue(reader, i));
        }

        /// <summary>
        /// 获取指定列的布尔值形式的值。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="name">字段的名称。</param>
        /// <returns>字段的 <see cref="Boolean"/> 值。</returns>
        public bool GetBoolean(IDataRecord reader, string name)
        {
            var index = reader.GetOrdinal(name);
            return index != -1 ? GetBoolean(reader, index) : false;
        }

        /// <summary>
        /// 获取指定列的 8 位无符号整数值。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="i">字段的索引。</param>
        /// <returns>字段的 <see cref="Byte"/> 值。</returns>
        public virtual byte GetByte(IDataRecord reader, int i)
        {
            if (reader.IsDBNull(i))
            {
                return (byte)0;
            }

            var type = reader.GetFieldType(i);
            return type == typeof(byte) ? reader.GetByte(i) :
                Convert.ToByte(GetValue(reader, i));
        }

        /// <summary>
        /// 获取指定列的 8 位无符号整数值。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="name">字段的名称。</param>
        /// <returns>字段的 <see cref="Boolean"/> 值。</returns>
        public byte GetByte(IDataRecord reader, string name)
        {
            var index = reader.GetOrdinal(name);
            return index != -1 ? GetByte(reader, index) : (byte)0;
        }

        /// <summary>
        /// 返回指定字段的字节数组。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="i">字段的索引。</param>
        /// <returns>字段的 <see cref="Byte"/> 数组值。</returns>
        public virtual byte[] GetBytes(IDataRecord reader, int i)
        {
            if (reader.IsDBNull(i))
            {
                return new byte[0];
            }

            var bufferSize = 1024;
            var outbyte = new byte[bufferSize];
            long startIndex = 0;
            using var stream = new MemoryStream();
            while (true)
            {
                var retval = reader.GetBytes(i, startIndex, outbyte, 0, bufferSize);
                stream.Write(outbyte, 0, (int)retval);
                if (retval < bufferSize)
                {
                    break;
                }

                startIndex += bufferSize;
            }

            return stream.ToArray();
        }

        /// <summary>
        /// 返回指定字段的字节数组。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="name">字段的名称。</param>
        /// <returns>字段的 <see cref="Boolean"/> 值。</returns>
        public byte[] GetBytes(IDataRecord reader, string name)
        {
            var index = reader.GetOrdinal(name);
            return index != -1 ? GetBytes(reader, index) : null;
        }

        /// <summary>
        /// 获取指定列的字符值。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="i">字段的索引。</param>
        /// <returns>字段的 <see cref="Char"/> 值。</returns>
        public virtual char GetChar(IDataRecord reader, int i)
        {
            if (reader.IsDBNull(i))
            {
                return '\0';
            }

            var type = reader.GetFieldType(i);
            return type == typeof(char) ? reader.GetChar(i) :
                Convert.ToChar(GetValue(reader, i));
        }

        /// <summary>
        /// 获取指定列的字符值。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="name">字段的名称。</param>
        /// <returns>字段的 <see cref="Boolean"/> 值。</returns>
        public char GetChar(IDataRecord reader, string name)
        {
            var index = reader.GetOrdinal(name);
            return index != -1 ? GetChar(reader, index) : '\0';
        }

        /// <summary>
        /// 返回指定字段的字符数组。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="i">字段的索引。</param>
        /// <returns>字段的 <see cref="Char"/> 数组值。</returns>
        public virtual char[] GetChars(IDataRecord reader, int i)
        {
            return GetBytes(reader, i).Select(s => (char)s).ToArray();
        }

        /// <summary>
        /// 返回指定字段的 GUID 值。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="i">字段的索引。</param>
        /// <returns>字段的 <see cref="Guid"/> 值。</returns>
        public virtual Guid GetGuid(IDataRecord reader, int i)
        {
            return reader.IsDBNull(i) ? Guid.Empty : reader.GetGuid(i);
        }

        /// <summary>
        /// 获取指定字段的 16 位有符号整数值。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="i">字段的索引。</param>
        /// <returns>字段的 <see cref="Int16"/> 值。</returns>
        public virtual short GetInt16(IDataRecord reader, int i)
        {
            if (reader.IsDBNull(i))
            {
                return 0;
            }

            var type = reader.GetFieldType(i);
            return type == typeof(short) ? reader.GetInt16(i) :
                Convert.ToInt16(GetValue(reader, i));
        }

        /// <summary>
        /// 获取指定列的字符值。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="name">字段的名称。</param>
        /// <returns>字段的 <see cref="Boolean"/> 值。</returns>
        public short GetInt16(IDataRecord reader, string name)
        {
            var index = reader.GetOrdinal(name);
            return index != -1 ? GetInt16(reader, index) : (short)0;
        }

        /// <summary>
        /// 获取指定字段的 32 位有符号整数值。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="i">字段的索引。</param>
        /// <returns>字段的 <see cref="Int32"/> 值。</returns>
        public virtual int GetInt32(IDataRecord reader, int i)
        {
            if (reader.IsDBNull(i))
            {
                return 0;
            }

            var type = reader.GetFieldType(i);
            return type == typeof(int) ? reader.GetInt32(i) :
                Convert.ToInt32(GetValue(reader, i));
        }

        /// <summary>
        /// 获取指定列的字符值。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="name">字段的名称。</param>
        /// <returns>字段的 <see cref="Boolean"/> 值。</returns>
        public int GetInt32(IDataRecord reader, string name)
        {
            var index = reader.GetOrdinal(name);
            return index != -1 ? GetInt32(reader, index) : 0;
        }

        /// <summary>
        /// 获取指定字段的 64 位有符号整数值。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="i">字段的索引。</param>
        /// <returns>字段的 <see cref="Int64"/> 值。</returns>
        public virtual long GetInt64(IDataRecord reader, int i)
        {
            if (reader.IsDBNull(i))
            {
                return 0;
            }

            var type = reader.GetFieldType(i);
            return type == typeof(long) ? reader.GetInt64(i) :
                Convert.ToInt64(GetValue(reader, i));
        }

        /// <summary>
        /// 获取指定列的字符值。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="name">字段的名称。</param>
        /// <returns>字段的 <see cref="Boolean"/> 值。</returns>
        public long GetInt64(IDataRecord reader, string name)
        {
            var index = reader.GetOrdinal(name);
            return index != -1 ? GetInt64(reader, index) : 0;
        }

        /// <summary>
        /// 获取指定字段的单精度浮点数。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="i">字段的索引。</param>
        /// <returns>字段的 <see cref="Single"/> 值。</returns>
        public virtual float GetFloat(IDataRecord reader, int i)
        {
            if (reader.IsDBNull(i))
            {
                return 0;
            }

            var type = reader.GetFieldType(i);
            return type == typeof(float) ? reader.GetFloat(i) :
                Convert.ToSingle(GetValue(reader, i));
        }

        /// <summary>
        /// 获取指定列的字符值。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="name">字段的名称。</param>
        /// <returns>字段的 <see cref="Boolean"/> 值。</returns>
        public float GetFloat(IDataRecord reader, string name)
        {
            var index = reader.GetOrdinal(name);
            return index != -1 ? GetFloat(reader, index) : 0;
        }

        /// <summary>
        /// 获取指定字段的双精度浮点数。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="i">字段的索引。</param>
        /// <returns>字段的 <see cref="Double"/> 值。</returns>
        public virtual double GetDouble(IDataRecord reader, int i)
        {
            if (reader.IsDBNull(i))
            {
                return 0;
            }

            var type = reader.GetFieldType(i);
            return type == typeof(double) ? reader.GetDouble(i) :
                Convert.ToDouble(GetValue(reader, i));
        }

        /// <summary>
        /// 获取指定列的字符值。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="name">字段的名称。</param>
        /// <returns>字段的 <see cref="Boolean"/> 值。</returns>
        public double GetDouble(IDataRecord reader, string name)
        {
            var index = reader.GetOrdinal(name);
            return index != -1 ? GetDouble(reader, index) : 0;
        }

        /// <summary>
        /// 获取指定字段的字符串值。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="i">字段的索引。</param>
        /// <returns>字段的 <see cref="String"/> 值。</returns>
        public virtual string GetString(IDataRecord reader, int i)
        {
            if (reader.IsDBNull(i))
            {
                return string.Empty;
            }

            var type = reader.GetFieldType(i);
            return type == typeof(string) ? reader.GetString(i) :
                Convert.ToString(GetValue(reader, i));
        }

        /// <summary>
        /// 获取指定列的字符值。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="name">字段的名称。</param>
        /// <returns>字段的 <see cref="Boolean"/> 值。</returns>
        public string GetString(IDataRecord reader, string name)
        {
            var index = reader.GetOrdinal(name);
            return index != -1 ? GetString(reader, index) : string.Empty;
        }

        /// <summary>
        /// 获取指定字段的固定位置的数值。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="i">字段的索引。</param>
        /// <returns>字段的 <see cref="Decimal"/> 值。</returns>
        public virtual decimal GetDecimal(IDataRecord reader, int i)
        {
            if (reader.IsDBNull(i))
            {
                return 0;
            }

            var type = reader.GetFieldType(i);
            return type == typeof(decimal) ? reader.GetDecimal(i) :
                Convert.ToDecimal(GetValue(reader, i));
        }

        /// <summary>
        /// 获取指定列的字符值。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="name">字段的名称。</param>
        /// <returns>字段的 <see cref="Boolean"/> 值。</returns>
        public decimal GetDecimal(IDataRecord reader, string name)
        {
            var index = reader.GetOrdinal(name);
            return index != -1 ? GetDecimal(reader, index) : 0;
        }

        /// <summary>
        /// 获取指定字段的固定位置的日期和时间。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="i">字段的索引。</param>
        /// <returns>字段的 <see cref="DateTime"/> 值。</returns>
        public virtual DateTime GetDateTime(IDataRecord reader, int i)
        {
            if (reader.IsDBNull(i))
            {
                return DateTime.MinValue;
            }

            var type = reader.GetFieldType(i);
            if (type == typeof(DateTime))
            {
                return reader.GetDateTime(i);
            }

            if (type == typeof(byte[]))
            {
                var bts = (byte[])GetValue(reader, i);
                if (bts.Length == 8)
                {
                    return DateTime.FromBinary(BitConverter.ToInt64(bts, 0));
                }
                else
                {
                    var str = Encoding.GetEncoding(0).GetString(bts);
                    return Convert.ToDateTime(str);
                }
            }

            return Convert.ToDateTime(GetValue(reader, i));
        }

        /// <summary>
        /// 获取指定字段的固定位置的日期和时间。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataRecord"/> 对象。</param>
        /// <param name="name">字段的名称。</param>
        /// <returns>字段的 <see cref="Boolean"/> 值。</returns>
        public DateTime GetDateTime(IDataRecord reader, string name)
        {
            var index = reader.GetOrdinal(name);
            return index != -1 ? GetDateTime(reader, index) : DateTime.MinValue;
        }

        /// <summary>
        /// 判断指定索引位置的字段值是否为 DBNull。
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        public virtual bool IsDbNull(IDataReader reader, int i)
        {
            return reader.IsDBNull(i);
        }
    }
}
