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
        /// <param name="reader">一个 <see cref="IDataReader"/> 对象。</param>
        /// <returns>由当前 <see cref="IDataReader"/> 对象中的数据转换成的 <typeparamref name="T"/> 对象实例。</returns>
        public virtual T Map(IDataReader reader)
        {
            var value = RecordWrapper == null ? reader[0] :
                RecordWrapper.GetValue(reader, 0);

            var converter = ConvertManager.GetConverter(typeof(T));
            return converter != null ? (T)converter.ConvertFrom(value, reader.GetFieldType(0).GetDbType()) :
                value.To<object, T>();
        }

        /// <summary>
        /// 获取或设置 <see cref="IRecordWrapper"/>。
        /// </summary>
        public IRecordWrapper RecordWrapper { get; set; }

        /// <summary>
        /// 获取或设置对象的初始化器。
        /// </summary>
        public Action<object> Initializer { get; set; }

        object IDataRowMapper.Map(IDataReader reader)
        {
            return Map(reader);
        }

        /// <summary>
        /// 创建一个 <see cref="IDataRowMapper"/> 实例。
        /// </summary>
        /// <returns></returns>
        public static IDataRowMapper Create()
        {
            var dataType = typeof(T);

            if (dataType == typeof(byte))
            {
                return new ByteValueRowMapper();
            }
            else if (dataType == typeof(byte?))
            {
                return new ByteNullableRowMapper();
            }
            else if (dataType == typeof(short))
            {
                return new Int16ValueRowMapper();
            }
            else if (dataType == typeof(short?))
            {
                return new Int16NullableRowMapper();
            }
            else if (dataType == typeof(int))
            {
                return new Int32ValueRowMapper();
            }
            else if (dataType == typeof(int?))
            {
                return new Int32NullableRowMapper();
            }
            else if (dataType == typeof(long))
            {
                return new Int64ValueRowMapper();
            }
            else if (dataType == typeof(long?))
            {
                return new Int64NullableRowMapper();
            }
            else if (dataType == typeof(bool))
            {
                return new BooleanValueRowMapper();
            }
            else if (dataType == typeof(bool?))
            {
                return new BooleanNullableRowMapper();
            }
            else if (dataType == typeof(char))
            {
                return new CharValueRowMapper();
            }
            else if (dataType == typeof(char?))
            {
                return new CharNullableRowMapper();
            }
            else if (dataType == typeof(decimal))
            {
                return new DecimalValueRowMapper();
            }
            else if (dataType == typeof(decimal?))
            {
                return new DecimalNullableRowMapper();
            }
            else if (dataType == typeof(float))
            {
                return new FloatValueRowMapper();
            }
            else if (dataType == typeof(float?))
            {
                return new FloatNullableRowMapper();
            }
            else if (dataType == typeof(double))
            {
                return new DoubleValueRowMapper();
            }
            else if (dataType == typeof(double?))
            {
                return new DoubleNullableRowMapper();
            }
            else if (dataType == typeof(string))
            {
                return new StringValueRowMapper();
            }
            else if (dataType == typeof(DateTime))
            {
                return new DateTimeValueRowMapper();
            }
            else if (dataType == typeof(DateTime?))
            {
                return new DateTimeNullableRowMapper();
            }
            else if (dataType == typeof(Guid))
            {
                return new GuidValueRowMapper();
            }

            return new SingleValueRowMapper<T>();
        }

        private class ByteValueRowMapper : SingleValueRowMapper<byte>
        {
            public override byte Map(IDataReader reader)
            {
                return RecordWrapper == null ? reader.GetByte(0) :
                    RecordWrapper.GetByte(reader, 0);
            }
        }

        private class ByteNullableRowMapper : SingleValueRowMapper<byte?>
        {
            public override byte? Map(IDataReader reader)
            {
                if (reader.IsDBNull(0))
                {
                    return null;
                }

                return RecordWrapper == null ? reader.GetByte(0) :
                    RecordWrapper.GetByte(reader, 0);
            }
        }

        private class Int16ValueRowMapper : SingleValueRowMapper<short>
        {
            public override short Map(IDataReader reader)
            {
                return RecordWrapper == null ? reader.GetInt16(0) :
                    RecordWrapper.GetInt16(reader, 0);
            }
        }

        private class Int16NullableRowMapper : SingleValueRowMapper<short?>
        {
            public override short? Map(IDataReader reader)
            {
                if (reader.IsDBNull(0))
                {
                    return null;
                }

                return RecordWrapper == null ? reader.GetInt16(0) :
                    RecordWrapper.GetInt16(reader, 0);
            }
        }

        private class Int32ValueRowMapper : SingleValueRowMapper<int>
        {
            public override int Map(IDataReader reader)
            {
                return RecordWrapper == null ? reader.GetInt32(0) :
                    RecordWrapper.GetInt32(reader, 0);
            }
        }

        private class Int32NullableRowMapper : SingleValueRowMapper<int?>
        {
            public override int? Map(IDataReader reader)
            {
                if (reader.IsDBNull(0))
                {
                    return null;
                }

                return RecordWrapper == null ? reader.GetInt32(0) :
                    RecordWrapper.GetInt32(reader, 0);
            }
        }

        private class Int64ValueRowMapper : SingleValueRowMapper<long>
        {
            public override long Map(IDataReader reader)
            {
                return RecordWrapper == null ? reader.GetInt64(0) :
                    RecordWrapper.GetInt64(reader, 0);
            }
        }

        private class Int64NullableRowMapper : SingleValueRowMapper<long?>
        {
            public override long? Map(IDataReader reader)
            {
                if (reader.IsDBNull(0))
                {
                    return null;
                }

                return RecordWrapper == null ? reader.GetInt64(0) :
                    RecordWrapper.GetInt64(reader, 0);
            }
        }

        private class BooleanValueRowMapper : SingleValueRowMapper<bool>
        {
            public override bool Map(IDataReader reader)
            {
                return RecordWrapper == null ? reader.GetBoolean(0) :
                    RecordWrapper.GetBoolean(reader, 0);
            }
        }

        private class BooleanNullableRowMapper : SingleValueRowMapper<bool?>
        {
            public override bool? Map(IDataReader reader)
            {
                if (reader.IsDBNull(0))
                {
                    return null;
                }

                return RecordWrapper == null ? reader.GetBoolean(0) :
                    RecordWrapper.GetBoolean(reader, 0);
            }
        }

        private class CharValueRowMapper : SingleValueRowMapper<char>
        {
            public override char Map(IDataReader reader)
            {
                return RecordWrapper == null ? reader.GetChar(0) :
                    RecordWrapper.GetChar(reader, 0);
            }
        }

        private class CharNullableRowMapper : SingleValueRowMapper<char?>
        {
            public override char? Map(IDataReader reader)
            {
                if (reader.IsDBNull(0))
                {
                    return null;
                }

                return RecordWrapper == null ? reader.GetChar(0) :
                    RecordWrapper.GetChar(reader, 0);
            }
        }

        private class DecimalValueRowMapper : SingleValueRowMapper<decimal>
        {
            public override decimal Map(IDataReader reader)
            {
                return RecordWrapper == null ? reader.GetDecimal(0) :
                    RecordWrapper.GetDecimal(reader, 0);
            }
        }

        private class DecimalNullableRowMapper : SingleValueRowMapper<decimal?>
        {
            public override decimal? Map(IDataReader reader)
            {
                if (reader.IsDBNull(0))
                {
                    return null;
                }

                return RecordWrapper == null ? reader.GetDecimal(0) :
                    RecordWrapper.GetDecimal(reader, 0);
            }
        }

        private class FloatValueRowMapper : SingleValueRowMapper<float>
        {
            public override float Map(IDataReader reader)
            {
                return RecordWrapper == null ? reader.GetFloat(0) :
                    RecordWrapper.GetFloat(reader, 0);
            }
        }

        private class FloatNullableRowMapper : SingleValueRowMapper<float?>
        {
            public override float? Map(IDataReader reader)
            {
                if (reader.IsDBNull(0))
                {
                    return null;
                }

                return RecordWrapper == null ? reader.GetFloat(0) :
                    RecordWrapper.GetFloat(reader, 0);
            }
        }

        private class DoubleValueRowMapper : SingleValueRowMapper<double>
        {
            public override double Map(IDataReader reader)
            {
                return RecordWrapper == null ? reader.GetDouble(0) :
                    RecordWrapper.GetDouble(reader, 0);
            }
        }

        private class DoubleNullableRowMapper : SingleValueRowMapper<double?>
        {
            public override double? Map(IDataReader reader)
            {
                if (reader.IsDBNull(0))
                {
                    return null;
                }

                return RecordWrapper == null ? reader.GetDouble(0) :
                    RecordWrapper.GetDouble(reader, 0);
            }
        }

        private class StringValueRowMapper : SingleValueRowMapper<string>
        {
            public override string Map(IDataReader reader)
            {
                if (reader.IsDBNull(0))
                {
                    return null;
                }

                return RecordWrapper == null ? reader.GetString(0) :
                    RecordWrapper.GetString(reader, 0);
            }
        }

        private class DateTimeValueRowMapper : SingleValueRowMapper<DateTime>
        {
            public override DateTime Map(IDataReader reader)
            {
                return RecordWrapper == null ? reader.GetDateTime(0) :
                    RecordWrapper.GetDateTime(reader, 0);
            }
        }

        private class DateTimeNullableRowMapper : SingleValueRowMapper<DateTime?>
        {
            public override DateTime? Map(IDataReader reader)
            {
                if (reader.IsDBNull(0))
                {
                    return null;
                }

                return RecordWrapper == null ? reader.GetDateTime(0) :
                    RecordWrapper.GetDateTime(reader, 0);
            }
        }

        private class GuidValueRowMapper : SingleValueRowMapper<Guid>
        {
            public override Guid Map(IDataReader reader)
            {
                if (reader.IsDBNull(0))
                {
                    return Guid.Empty;
                }

                return RecordWrapper == null ? reader.GetGuid(0) :
                    RecordWrapper.GetGuid(reader, 0);
            }
        }
    }
}
