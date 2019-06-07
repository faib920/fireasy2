// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.Extensions;
using Fireasy.Data.Converter;
using System;
using System.Data;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 使属性能够存储各种类型的值，而不需要进行装箱或拆箱。无法继承此类。
    /// </summary>
    [Serializable]
    public struct PropertyValue : ICloneable
    {
        /// <summary>
        /// 空值。
        /// </summary>
        public readonly static PropertyValue Empty = default(PropertyValue);

        #region char
        /// <summary>
        /// 将 <see cref="Char"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(char value)
        {
            return new PropertyValue { StorageType = StorageType.Char, Char = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Char"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator char(PropertyValue value)
        {
            return value == Empty ? '\0' : value.Char.Value;
        }

        /// <summary>
        /// 将 <see cref="Nullable{Char}"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(char? value)
        {
            return new PropertyValue { StorageType = StorageType.Char, Char = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Nullable{Char}"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator char?(PropertyValue value)
        {
            return value == Empty ? null : value.Char;
        }

        #endregion

        #region bool
        /// <summary>
        /// 将 <see cref="Boolean"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(bool value)
        {
            return new PropertyValue { StorageType = StorageType.Boolean, Boolean = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Boolean"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator bool(PropertyValue value)
        {
            return value == Empty ? false : value.Boolean.Value;
        }

        /// <summary>
        /// 将 <see cref="Nullable{Boolean}"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(bool? value)
        {
            return new PropertyValue { StorageType = StorageType.Boolean, Boolean = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Nullable{Boolean}"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator bool?(PropertyValue value)
        {
            return value == Empty ? null : value.Boolean;
        }

        #endregion

        #region byte
        /// <summary>
        /// 将 <see cref="Byte"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(byte value)
        {
            return new PropertyValue { StorageType = StorageType.Byte, Byte = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Byte"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator byte(PropertyValue value)
        {
            return value == Empty ? (byte)0 : value.Byte.Value;
        }

        /// <summary>
        /// 将 <see cref="Nullable{Byte}"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(byte? value)
        {
            return new PropertyValue { StorageType = StorageType.Byte, Byte = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Nullable{Byte}"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator byte?(PropertyValue value)
        {
            return value == Empty ? null : value.Byte;
        }

        #endregion

        #region bytes
        /// <summary>
        /// 将 <see cref="Byte"/> 数组类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(byte[] value)
        {
            return new PropertyValue { StorageType = StorageType.ByteArray, ByteArray = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Byte"/> 数组类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator byte[](PropertyValue value)
        {
            if (Equals(value, null))
            {
                return new byte[0];
            }
            return value.Object != null ? (byte[])value.Object : value.ByteArray == null ? new byte[0] : value.ByteArray;
        }
        #endregion

        #region DateTime
        /// <summary>
        /// 将 <see cref="DateTime"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(DateTime value)
        {
            return new PropertyValue { StorageType = StorageType.DateTime, DateTime = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="DateTime"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator DateTime(PropertyValue value)
        {
            return value == Empty ? System.DateTime.MinValue : value.DateTime.Value;
        }

        /// <summary>
        /// 将 <see cref="Nullable{DateTime}"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(DateTime? value)
        {
            return new PropertyValue { StorageType = StorageType.DateTime, DateTime = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Nullable{DateTime}"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator DateTime?(PropertyValue value)
        {
            return value == Empty ? null : value.DateTime;
        }

        #endregion

        #region decimal
        /// <summary>
        /// 将 <see cref="Decimal"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(decimal value)
        {
            return new PropertyValue { StorageType = StorageType.Decimal, Decimal = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Decimal"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator decimal(PropertyValue value)
        {
            return value == Empty ? 0 : value.Decimal.Value;
        }

        /// <summary>
        /// 将 <see cref="Nullable{Decimal}"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(decimal? value)
        {
            return new PropertyValue { StorageType = StorageType.Decimal, Decimal = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Nullable{Decimal}"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator decimal?(PropertyValue value)
        {
            return value == Empty ? null : value.Decimal;
        }

        #endregion

        #region double
        /// <summary>
        /// 将 <see cref="Double"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(double value)
        {
            return new PropertyValue { StorageType = StorageType.Double, Double = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Double"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator double(PropertyValue value)
        {
            return value == Empty ? 0 : value.Double.Value;
        }

        /// <summary>
        /// 将 <see cref="Nullable{Double}"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(double? value)
        {
            return new PropertyValue { StorageType = StorageType.Double, Double = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Nullable{Double}"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator double?(PropertyValue value)
        {
            return value == Empty ? null : value.Double;
        }

        #endregion

        #region Guid
        /// <summary>
        /// 将 <see cref="Guid"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(Guid value)
        {
            return new PropertyValue { StorageType = StorageType.Guid, Guid = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Guid"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator Guid(PropertyValue value)
        {
            if (value == Empty)
            {
                return System.Guid.Empty;
            }
            if (value.Object != null)
            {
                return (Guid)value.Object;
            }
            if (value.Guid != null)
            {
                return value.Guid.Value;
            }
            if (value.String != null)
            {
                return new Guid(value.String);
            }
            if (value.ByteArray != null)
            {
                return new Guid(value.ByteArray);
            }
            return System.Guid.Empty;
        }

        /// <summary>
        /// 将 <see cref="Nullable{Guid}"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(Guid? value)
        {
            return new PropertyValue { StorageType = StorageType.Guid, Guid = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Nullable{Guid}"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator Guid?(PropertyValue value)
        {
            if (value == Empty)
            {
                return null;
            }
            return (Guid)value;
        }
        #endregion

        #region int
        /// <summary>
        /// 将 <see cref="Int32"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(int value)
        {
            return new PropertyValue { StorageType = StorageType.Int32, Int32 = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Int32"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator int(PropertyValue value)
        {
            return value == Empty ? 0 : value.Int32.Value;
        }

        /// <summary>
        /// 将 <see cref="Nullable{Int32}"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(int? value)
        {
            return new PropertyValue { StorageType = StorageType.Int32, Int32 = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Nullable{Int32}"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator int?(PropertyValue value)
        {
            return value == Empty ? null : value.Int32;
        }

        #endregion

        #region short
        /// <summary>
        /// 将 <see cref="Int16"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(short value)
        {
            return new PropertyValue { StorageType = StorageType.Int16, Int16 = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Int16"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator short(PropertyValue value)
        {
            return value == Empty ? (short)0 : value.Int16.Value;
        }

        /// <summary>
        /// 将 <see cref="Nullable{Int16}"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(short? value)
        {
            return new PropertyValue { StorageType = StorageType.Int16, Int16 = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Nullable{Int16}"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator short?(PropertyValue value)
        {
            return value == Empty ? null : value.Int16;
        }

        #endregion

        #region long
        /// <summary>
        /// 将 <see cref="Int64"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(long value)
        {
            return new PropertyValue { StorageType = StorageType.Int64, Int64 = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Int64"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator long(PropertyValue value)
        {
            return value == Empty ? 0 : value.Int64.Value;
        }

        /// <summary>
        /// 将 <see cref="Nullable{Int64}"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(long? value)
        {
            return new PropertyValue { StorageType = StorageType.Int64, Int64 = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Nullable{Int64}"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator long?(PropertyValue value)
        {
            return value == Empty ? null : value.Int64;
        }

        #endregion

        #region float
        /// <summary>
        /// 将 <see cref="Single"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(float value)
        {
            return new PropertyValue { StorageType = StorageType.Single, Single = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Single"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator float(PropertyValue value)
        {
            return value == Empty ? 0 : value.Single.Value;
        }

        /// <summary>
        /// 将 <see cref="Nullabl{Single}"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(float? value)
        {
            return new PropertyValue { StorageType = StorageType.Single, Single = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Nullable{Single}"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator float?(PropertyValue value)
        {
            return value == Empty ? null : value.Single;
        }

        #endregion

        #region string
        /// <summary>
        /// 将 <see cref="String"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(string value)
        {
            return new PropertyValue { StorageType = StorageType.String, String = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="String"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator string(PropertyValue value)
        {
            return value == Empty ? string.Empty : value.String;
        }

        #endregion

        #region timespan
        /// <summary>
        /// 将 <see cref="TimeSpan"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(TimeSpan value)
        {
            return new PropertyValue { StorageType = StorageType.TimeSpan, TimeSpan = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="TimeSpan"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator TimeSpan(PropertyValue value)
        {
            if (value == Empty)
            {
                return System.TimeSpan.Zero;
            }
            if (value.Object != null)
            {
                return (TimeSpan)value.Object;
            }
            if (value.TimeSpan != null)
            {
                return value.TimeSpan.Value;
            }
            if (value.Int64 != null)
            {
                return System.TimeSpan.FromTicks(value.Int64.Value);
            }
            return System.TimeSpan.Zero;
        }

        /// <summary>
        /// 将 <see cref="Nullable{TimeSpan}"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(TimeSpan? value)
        {
            return new PropertyValue { StorageType = StorageType.TimeSpan, TimeSpan = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Nullable{TimeSpan}"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator TimeSpan?(PropertyValue value)
        {
            if (value == Empty)
            {
                return null;
            }
            return (TimeSpan)value;
        }
        #endregion

        #region enum
        /// <summary>
        /// 将 <see cref="Enum"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(Enum value)
        {
            return new PropertyValue { StorageType = StorageType.Enum, Enum = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Enum"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator Enum(PropertyValue value)
        {
            return value == Empty ? null : (value.Object != null ? (Enum)value.Object : value.Enum);
        }
        #endregion

        #region ==和!=
        public static bool operator ==(PropertyValue v1, PropertyValue v2)
        {
            if (v1.StorageType != v2.StorageType)
            {
                return false;
            }

            switch (v1.StorageType)
            {
                case StorageType.Boolean: return v1.Boolean == v2.Boolean;
                case StorageType.Byte: return v1.Byte == v2.Byte;
                case StorageType.Char: return v1.Char == v2.Char;
                case StorageType.DateTime: return v1.DateTime == v2.DateTime;
                case StorageType.Decimal: return v1.Decimal == v2.Decimal;
                case StorageType.Double: return v1.Double == v2.Double;
                case StorageType.Enum: return v1.Enum == v2.Enum;
                case StorageType.Guid: return v1.Guid == v2.Guid;
                case StorageType.Int16: return v1.Int16 == v2.Int16;
                case StorageType.Int32: return v1.Int32 == v2.Int32;
                case StorageType.Int64: return v1.Int64 == v2.Int64;
                case StorageType.Single: return v1.Single == v2.Single;
                case StorageType.String: return v1.String == v2.String;
                case StorageType.TimeSpan: return v1.TimeSpan == v2.TimeSpan;
                case StorageType.ByteArray: return ByteEqueals(v1.ByteArray, v2.ByteArray);
                default: return v1.Object == v2.Object;
            }
        }

        private static bool ByteEqueals(byte[] b1, byte[] b2)
        {
            if (b1 == null && b2 == null)
            {
                return true;
            }

            if (b1 == null && b2 != null)
            {
                return false;
            }

            if (b1 != null && b2 == null)
            {
                return false;
            }

            if (b1.Length != b2.Length)
            {
                return false;
            }

            for (var i = 0; i < b1.Length; i++)
            {
                if (b1[i] != b2[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static bool operator !=(PropertyValue v1, PropertyValue v2)
        {
            return !(v1 == v2);
        }

        #region int equals
        public static bool operator == (PropertyValue v1, int v2)
        {
            if (v1.StorageType != StorageType.Int32)
            {
                return false;
            }

            return v1.Int32 == v2;
        }

        public static bool operator !=(PropertyValue v1, int v2)
        {
            if (v1.StorageType != StorageType.Int32)
            {
                return true;
            }

            return v1.Int32 != v2;
        }

        public static bool operator ==(int v1, PropertyValue v2)
        {
            if (v2.StorageType != StorageType.Int32)
            {
                return false;
            }

            return v2.Int32 == v1;
        }

        public static bool operator !=(int v1, PropertyValue v2)
        {
            if (v2.StorageType != StorageType.Int32)
            {
                return true;
            }

            return v2.Int32 != v1;
        }
        #endregion

        #region long equals
        public static bool operator ==(PropertyValue v1, long v2)
        {
            if (v1.StorageType != StorageType.Int64)
            {
                return false;
            }

            return v1.Int64 == v2;
        }

        public static bool operator !=(PropertyValue v1, long v2)
        {
            if (v1.StorageType != StorageType.Int64)
            {
                return true;
            }

            return v1.Int64 != v2;
        }

        public static bool operator ==(long v1, PropertyValue v2)
        {
            if (v2.StorageType != StorageType.Int64)
            {
                return false;
            }

            return v2.Int64 == v1;
        }

        public static bool operator !=(long v1, PropertyValue v2)
        {
            if (v2.StorageType != StorageType.Int64)
            {
                return true;
            }

            return v2.Int64 != v1;
        }
        #endregion

        #region string equals
        public static bool operator ==(PropertyValue v1, string v2)
        {
            if (v1.StorageType != StorageType.String)
            {
                return false;
            }

            return v1.String == v2;
        }

        public static bool operator !=(PropertyValue v1, string v2)
        {
            if (v1.StorageType != StorageType.String)
            {
                return true;
            }

            return v1.String != v2;
        }

        public static bool operator ==(string v1, PropertyValue v2)
        {
            if (v2.StorageType != StorageType.String)
            {
                return false;
            }

            return v2.String == v1;
        }

        public static bool operator !=(string v1, PropertyValue v2)
        {
            if (v2.StorageType != StorageType.String)
            {
                return true;
            }

            return v2.String != v1;
        }
        #endregion

        #endregion

        internal DbType? DataType { get; set; }

        /// <summary>
        /// 获取存储数据的实际类型。
        /// </summary>
        private StorageType StorageType { get; set; }

        private char? Char { get; set; }

        private bool? Boolean { get; set; }

        private byte? Byte { get; set; }

        private byte[] ByteArray { get; set; }

        private DateTime? DateTime { get; set; }

        private decimal? Decimal { get; set; }

        private double? Double { get; set; }

        private Guid? Guid { get; set; }

        private short? Int16 { get; set; }

        private int? Int32 { get; set; }

        private long? Int64 { get; set; }

        private float? Single { get; set; }

        private string String { get; set; }

        private Enum Enum { get; set; }

        private TimeSpan? TimeSpan { get; set; }

        private object Object { get; set; }

        /// <summary>
        /// 获取该对象是否有效，即数字不为 0、字符串不为空字符时有效。
        /// </summary>
        public bool IsValid
        {
            get
            {
                switch (StorageType)
                {
                    case StorageType.Byte: return Byte != 0;
                    case StorageType.Char: return Char != '\0';
                    case StorageType.Decimal: return Decimal != 0;
                    case StorageType.Double: return Double != 0;
                    case StorageType.Int16: return Int16 != 0;
                    case StorageType.Int32: return Int32 != 0;
                    case StorageType.Int64: return Int64 != 0;
                    case StorageType.Single: return Single != 0;
                    case StorageType.String: return String != "";
                    default: return true;
                }
            }
        }

        /// <summary>
        /// 纠正存储值的类型。
        /// </summary>
        /// <param name="correctType">要纠正的实际存储的类型。</param>
        internal void Correct(Type correctType)
        {
            if (correctType.IsEnum)
            {
                if (StorageType != StorageType.Enum)
                {
                    CorrectToEnum(correctType);
                }
            }
            else if (correctType.GetNonNullableType() == typeof(bool))
            {
                if (StorageType != StorageType.Boolean)
                {
                    CorrectToBoolean();
                }
            }
            else if (correctType.GetNonNullableType() == typeof(Guid))
            {
                if (StorageType != StorageType.Guid)
                {
                    CorrectToGuid();
                }
            }
        }

        private void CorrectToEnum(Type correctType)
        {
            switch (StorageType)
            {
                case StorageType.Int32:
                    Enum = (Enum)Enum.Parse(correctType, Int32.ToString());
                    Int32 = null;
                    break;
                case StorageType.Int16:
                    Enum = (Enum)Enum.Parse(correctType, Int16.ToString());
                    Int16 = null;
                    break;
                case StorageType.Int64:
                    Enum = (Enum)Enum.Parse(correctType, Int64.ToString());
                    Int64 = null;
                    break;
            }
            StorageType = StorageType.Enum;
        }

        private void CorrectToBoolean()
        {
            switch (StorageType)
            {
                case StorageType.Int16:
                    Boolean = Int16 == null ? null : (bool?)(Int16.Value != 0);
                    Int16 = null;
                    break;
                case StorageType.Int32:
                    Boolean = Int32 == null ? null : (bool?)(Int32.Value != 0);
                    Int32 = null;
                    break;
                case StorageType.Int64:
                    Boolean = Int64 == null ? null : (bool?)(Int64.Value != 0);
                    Int64 = null;
                    break;
            }
            StorageType = StorageType.Boolean;
        }

        private void CorrectToGuid()
        {
            switch (StorageType)
            {
                case StorageType.String:
                    Guid = new Guid(String);
                    String = null;
                    break;
                case StorageType.ByteArray:
                    Guid = new Guid(ByteArray);
                    ByteArray = null;
                    break;
            }
            StorageType = StorageType.Guid;
        }

        #region object
        /// <summary>
        /// 获取实际存储的值，转换为 <see cref="System.Object"/> 表示。
        /// </summary>
        /// <returns></returns>
        public object GetValue()
        {
            switch (StorageType)
            {
                case StorageType.Boolean: return Boolean;
                case StorageType.Byte: return Byte;
                case StorageType.ByteArray: return ByteArray;
                case StorageType.Char: return Char;
                case StorageType.DateTime: return DateTime;
                case StorageType.Decimal: return Decimal;
                case StorageType.Double: return Double;
                case StorageType.Enum: return Enum;
                case StorageType.Guid: return Guid;
                case StorageType.Int16: return Int16;
                case StorageType.Int32: return Int32;
                case StorageType.Int64: return Int64;
                case StorageType.Object: return Object;
                case StorageType.Single: return Single;
                case StorageType.String: return String;
                case StorageType.TimeSpan: return TimeSpan;
                default: return null;
            }
        }

        /// <summary>
        /// 判断是否与指定的属性值相等。
        /// </summary>
        /// <param name="right"></param>
        /// <returns></returns>
        public override bool Equals(object right)
        {
            if (ReferenceEquals(right, null))
            {
                return false;
            }
            if (ReferenceEquals(this, right))
            {
                return true;
            }
            //var s = right.As<PropertyValue>();
            //if (StorageType != s.StorageType)
            {
            //    return false;
            }
            return GetHashCode() == right.GetHashCode();
        }

        /// <summary>
        /// 获取属性值的哈希码。
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            switch (StorageType)
            {
                case StorageType.Boolean: return Boolean == null ? 0 : Boolean.GetHashCode();
                case StorageType.Byte: return Byte == null ? 0 : Byte.GetHashCode();
                case StorageType.Char: return Char == null ? 0 : Char.GetHashCode();
                case StorageType.DateTime: return DateTime == null ? 0 : DateTime.GetHashCode();
                case StorageType.Decimal: return Decimal == null ? 0 : Decimal.GetHashCode();
                case StorageType.Double: return Double == null ? 0 : Double.GetHashCode();
                case StorageType.Enum: return Enum == null ? 0 : Enum.GetHashCode();
                case StorageType.Guid: return Guid == null ? 0 : Guid.GetHashCode();
                case StorageType.Int16: return Int16 == null ? 0 : Int16.GetHashCode();
                case StorageType.Int32: return Int32 == null ? 0 : Int32.GetHashCode();
                case StorageType.Int64: return Int64 == null ? 0 : Int64.GetHashCode();
                case StorageType.Single: return Single == null ? 0 : Single.GetHashCode();
                case StorageType.String: return String == null ? 0 : String.GetHashCode();
                case StorageType.TimeSpan: return TimeSpan == null ? 0 : TimeSpan.GetHashCode();
            }
            return base.GetHashCode();
        }

        /// <summary>
        /// 克隆该对象副本。
        /// </summary>
        /// <returns></returns>
        public PropertyValue Clone()
        {
            switch (StorageType)
            {
                case StorageType.Boolean: return Boolean;
                case StorageType.Byte: return Byte;
                case StorageType.Char: return Char;
                case StorageType.DateTime: return DateTime;
                case StorageType.Decimal: return Decimal;
                case StorageType.Double: return Double;
                case StorageType.Enum: return Enum;
                case StorageType.Guid: return Guid;
                case StorageType.Int16: return Int16;
                case StorageType.Int32: return Int32;
                case StorageType.Int64: return Int64;
                case StorageType.Single: return Single;
                case StorageType.String: return string.IsNullOrEmpty(String) ? string.Empty : string.Copy(String);
                case StorageType.TimeSpan: return TimeSpan;
                default:
                    if (Object == null)
                    {
                        return Empty;
                    }
                    var staClone = Object.As<IKeepStateCloneable>();
                    if (staClone != null)
                    {
                        return new PropertyValue { StorageType = StorageType.Object, Object = staClone.Clone() };
                    }
                    var cloneable = Object.As<ICloneable>();
                    return new PropertyValue { StorageType = StorageType.Object, Object = cloneable == null ? Object : cloneable.Clone() };
            }
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        /// <summary>
        /// 使用字符串表示该属性。
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return StorageType == StorageType.Empty ? string.Empty : GetValue().ToStringSafely();
        }

        #endregion

        #region 静态方法

        /// <summary>
        /// 获取指定 <see cref="PropertyValue"/> 真实的值。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object GetValue(PropertyValue value)
        {
            return IsEmpty(value) ? null : value.GetValue();
        }

        /// <summary>
        /// 获取指定 <see cref="PropertyValue"/> 真实的值。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TValue GetValue<TValue>(PropertyValue value)
        {
            if (IsEmpty(value))
            {
                return default;
            }

            var val = value.GetValue();
            if (val != null && val is TValue)
            {
                return (TValue)val;
            }

            return default;
        }

        /// <summary>
        /// 判断指定的类型是否受 <see cref="PropertyValue"/> 类型支持。这些类型主要是值类型。
        /// </summary>
        /// <param name="type">要判断的类型。</param>
        /// <returns></returns>
        public static bool IsSupportedType(Type type)
        {
            type = type.GetNonNullableType();
            return (type == typeof(bool) ||
                type == typeof(char) ||
                type == typeof(byte) ||
                type == typeof(byte[]) ||
                type == typeof(DateTime) ||
                type == typeof(decimal) ||
                type == typeof(double) ||
                type == typeof(Guid) ||
                type == typeof(int) ||
                type == typeof(short) ||
                type == typeof(long) ||
                type == typeof(float) ||
                type == typeof(string) ||
                type == typeof(TimeSpan) ||
                type.IsEnum);
        }

        /// <summary>
        /// 判断指定的类型是否显式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="type">要判断的类型。</param>
        /// <returns></returns>
        public static bool IsImplicitType(Type type)
        {
            type = type.GetNonNullableType();
            return (type == typeof(bool) ||
                type == typeof(char) ||
                type == typeof(byte) ||
                type == typeof(byte[]) ||
                type == typeof(DateTime) ||
                type == typeof(decimal) ||
                type == typeof(double) ||
                type == typeof(Guid) ||
                type == typeof(int) ||
                type == typeof(short) ||
                type == typeof(long) ||
                type == typeof(float) ||
                type == typeof(string) ||
                type == typeof(TimeSpan));
        }

        /// <summary>
        /// 使用一个值构造一个 <see cref="PropertyValue"/>。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="valueType"></param>
        /// <returns></returns>
        public static PropertyValue NewValue(object value, Type valueType = null)
        {
            if (value == null)
            {
                return PropertyValue.Empty;
            }

            if (value != null && valueType == null)
            {
                valueType = value.GetType();
            }

            var nonNullType = valueType.GetNonNullableType();

            //如果是受支持的类型
            if (IsSupportedType(valueType))
            {
                //枚举
                if (nonNullType.IsEnum)
                {
                    return value == null && valueType.IsNullableType() ? null : (Enum)Enum.Parse(nonNullType, value.ToString());
                }
                //字节数组
                if (nonNullType.IsArray && valueType.GetElementType() == typeof(byte))
                {
                    return value == null ? new byte[0] : (byte[])value;
                }
                var isNull = value.IsNullOrEmpty();
                switch (nonNullType.FullName)
                {
                    case "System.Boolean": return isNull ? new bool?() : Convert.ToBoolean(value);
                    case "System.Byte": return isNull ? new byte?() : Convert.ToByte(value);
                    case "System.Char": return isNull ? new char?() : Convert.ToChar(value);
                    case "System.DateTime": return isNull ? new DateTime?() : Convert.ToDateTime(value);
                    case "System.Decimal": return isNull ? new decimal?() : Convert.ToDecimal(value);
                    case "System.Double": return isNull ? new double?() : Convert.ToDouble(value);
                    case "System.Guid": return isNull ? new Guid?() : new Guid(value.ToString());
                    case "System.Int16": return isNull ? new short?() : Convert.ToInt16(value);
                    case "System.Int32": return isNull ? new int?() : Convert.ToInt32(value);
                    case "System.Int64": return isNull ? new long?() : Convert.ToInt64(value);
                    case "System.Single": return isNull ? new float?() : Convert.ToSingle(value);
                    case "System.String": return isNull ? null : Convert.ToString(value);
                    case "System.Time": return isNull ? new TimeSpan?() : System.TimeSpan.Parse(value.ToString());
                    default: return Empty;
                }
            }
            return new PropertyValue { StorageType = StorageType.Object, Object = value };
        }

        /// <summary>
        /// 根据指定的<see cref="PropertyValue"/> 来设置 <see cref="Parameter"/> 的值。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static Parameter Parametrization(PropertyValue value, Parameter parameter)
        {
            switch (value.StorageType)
            {
                case StorageType.Boolean:
                    parameter.Value = value.Boolean;
                    parameter.DbType = DbType.Boolean;
                    break;
                case StorageType.Byte:
                    parameter.Value = value.Byte;
                    parameter.DbType = DbType.Byte;
                    break;
                case StorageType.ByteArray:
                    parameter.Value = value.ByteArray;
                    parameter.DbType = DbType.Binary;
                    break;
                case StorageType.Char:
                    parameter.Value = value.Char;
                    parameter.DbType = DbType.AnsiString;
                    break;
                case StorageType.DateTime:
                    parameter.Value = value.DateTime;
                    parameter.DbType = DbType.DateTime;
                    break;
                case StorageType.Decimal:
                    parameter.Value = value.Decimal;
                    parameter.DbType = DbType.Decimal;
                    break;
                case StorageType.Double:
                    parameter.Value = value.Double;
                    parameter.DbType = DbType.Double;
                    break;
                case StorageType.Enum:
                    parameter.Value = value.Enum.To<int>();
                    parameter.DbType = DbType.Int32;
                    break;
                case StorageType.Guid:
                    parameter.Value = value.Guid;
                    parameter.DbType = DbType.Guid;
                    break;
                case StorageType.Int16:
                    parameter.Value = value.Int16;
                    parameter.DbType = DbType.Int16;
                    break;
                case StorageType.Int32:
                    parameter.Value = value.Int32;
                    parameter.DbType = DbType.Int32;
                    break;
                case StorageType.Int64:
                    parameter.Value = value.Int64;
                    parameter.DbType = DbType.Int64;
                    break;
                case StorageType.Single:
                    parameter.Value = value.Single;
                    parameter.DbType = DbType.Single;
                    break;
                case StorageType.String:
                    parameter.Value = value.String;
                    parameter.DbType = DbType.AnsiString;
                    break;
                case StorageType.TimeSpan:
                    parameter.Value = value.TimeSpan;
                    parameter.DbType = DbType.Time;
                    break;
                case StorageType.Object:
                    var converter = ConvertManager.GetConverter(value.Object.GetType());
                    if (converter != null)
                    {
                        var dbType = value.DataType ?? DbType.String;
                        parameter.Value = converter.ConvertTo(value.Object, dbType);
                        parameter.DbType = dbType;
                    }
                    break;
            }

            return parameter;
        }

        /// <summary>
        /// 安全地获取值。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object GetValueSafely(PropertyValue value)
        {
            if (value == Empty || value.StorageType == StorageType.Empty)
            {
                return null;
            }

            return value.GetValue();
        }

        /// <summary>
        /// 判断是否为空。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsEmpty(PropertyValue value)
        {
            return value == Empty || value.StorageType == StorageType.Empty || value.GetValue() == null;
        }

        /// <summary>
        /// 判断是否为空或为缺省值。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsEmptyOrDefault(PropertyValue value)
        {
            switch (value.StorageType)
            {
                case StorageType.Int16:
                    return value.Int16 == 0 || value.Int16 == null;
                case StorageType.Int32:
                    return value.Int32 == 0 || value.Int32 == null;
                case StorageType.Int64:
                    return value.Int64 == 0 || value.Int64 == null;
                case StorageType.Decimal:
                    return value.Decimal == 0 || value.Decimal == null;
                case StorageType.Single:
                    return value.Single == 0 || value.Single == null;
                case StorageType.Double:
                    return value.Double == 0 || value.Double == null;
                case StorageType.Boolean:
                    return value.Boolean == false || value.Boolean == null;
                case StorageType.String:
                    return string.IsNullOrEmpty(value.String);
            }

            return value == Empty || value.StorageType == StorageType.Empty || value.GetValue() == null;
        }
        #endregion
    }

    /// <summary>
    /// 存储数据的类别。
    /// </summary>
    internal enum StorageType
    {
        /// <summary>
        /// 空的，没有存储数据。
        /// </summary>
        Empty,
        /// <summary>
        /// System.Char 类型的数据。
        /// </summary>
        Char,
        /// <summary>
        /// System.Enum 类型的数据。
        /// </summary>
        Enum,
        /// <summary>
        /// System.Boolean 类型的数据。
        /// </summary>
        Boolean,
        /// <summary>
        /// System.Byte 类型的数据。
        /// </summary>
        Byte,
        /// <summary>
        /// System.Byte[] 类型的数据。
        /// </summary>
        ByteArray,
        /// <summary>
        /// System.DateTime 类型的数据。
        /// </summary>
        DateTime,
        /// <summary>
        /// System.Decimal 类型的数据。
        /// </summary>
        Decimal,
        /// <summary>
        /// System.Double 类型的数据。
        /// </summary>
        Double,
        /// <summary>
        /// System.Guid 类型的数据。
        /// </summary>
        Guid,
        /// <summary>
        /// System.Int16 类型的数据。
        /// </summary>
        Int16,
        /// <summary>
        /// System.Int32 类型的数据。
        /// </summary>
        Int32,
        /// <summary>
        /// System.Int64 类型的数据。
        /// </summary>
        Int64,
        /// <summary>
        /// System.Single 类型的数据。
        /// </summary>
        Single,
        /// <summary>
        /// System.String 类型的数据。
        /// </summary>
        String,
        /// <summary>
        /// System.TimeSpan 类型的数据。
        /// </summary>
        TimeSpan,
        /// <summary>
        /// System.Object 类型的数据。
        /// </summary>
        Object
    }
}
