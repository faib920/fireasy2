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
    /// 使属性能够存储各种类型的值，而不需要进行装箱或拆箱。
    /// </summary>
    [Serializable]
    public struct PropertyValue : ICloneable
    {
        /// <summary>
        /// 空值。
        /// </summary>
        public readonly static PropertyValue Empty = default;

        /// <summary>
        /// 当前日期。
        /// </summary>
        private readonly static PropertyValue Today = new PropertyValue { _dateTime = new DateTime(1, 1, 1), _storageType = StorageType.DateTime, _isConstant = true };

        /// <summary>
        /// 当前时间。
        /// </summary>
        private readonly static PropertyValue Now = new PropertyValue { _dateTime = new DateTime(2, 2, 2, 2, 2, 2), _storageType = StorageType.DateTime, _isConstant = true };

        /// <summary>
        /// 当前时间。
        /// </summary>
        private readonly static PropertyValue NewGuid = new PropertyValue { _guid = Guid.Empty, _storageType = StorageType.Guid, _isConstant = true };

        /// <summary>
        /// 默认值常量。
        /// </summary>
        public static class Constants
        {
            /// <summary>
            /// 当前系统日期。
            /// </summary>
            public const string Today = ":TODAY";

            /// <summary>
            /// 当前系统时间。
            /// </summary>
            public const string Now = ":NOW";

            /// <summary>
            /// 新的 Guid。
            /// </summary>
            public const string Guid = ":GUID";
        }

        #region 存储
        internal DbType? _dataType;

        private bool _isConstant;

        /// <summary>
        /// 获取存储数据的实际类型。
        /// </summary>
        private StorageType _storageType;

        private char? _char;

        private bool? _boolean;

        private byte? _byte;

        private sbyte? _sbyte;

        private byte[] _byteArray;

        private DateTime? _dateTime;

        private decimal? _decimal;

        private double? _double;

        private Guid? _guid;

        private short? _int16;

        private int? _int32;

        private long? _int64;

        private ushort? _uint16;

        private uint? _uint32;

        private ulong? _uint64;

        private float? _single;

        private string _string;

        private Enum _enum;

        private object _object;
        #endregion

        #region char
        /// <summary>
        /// 将 <see cref="Char"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(char value)
        {
            return new PropertyValue { _storageType = StorageType.Char, _char = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="_char"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator char(PropertyValue value)
        {
            if (IsEmpty(value))
            {
                return '\0';
            }

            return value._storageType switch
            {
                StorageType.Byte => (char)value._byte,
                StorageType.SByte => (char)value._sbyte,
                _ => throw new InvalidCastException(SR.GetString(SRKind.InvalidCastPropertyValue, typeof(bool).FullName)),
            };
        }

        /// <summary>
        /// 将 <see cref="Nullable{Char}"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(char? value)
        {
            return new PropertyValue { _storageType = StorageType.Char, _char = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Nullable{Char}"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator char?(PropertyValue value)
        {
            if (IsEmpty(value))
            {
                return null;
            }

            return value._storageType switch
            {
                StorageType.Byte => (char?)value._byte,
                StorageType.SByte => (char?)value._sbyte,
                _ => throw new InvalidCastException(SR.GetString(SRKind.InvalidCastPropertyValue, typeof(bool).FullName)),
            };
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
            return new PropertyValue { _storageType = StorageType.Boolean, _boolean = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Boolean"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator bool(PropertyValue value)
        {
            if (IsEmpty(value))
            {
                return false;
            }

            return value._storageType switch
            {
                StorageType.Byte => value._byte > 0,
                StorageType.SByte => value._sbyte > 0,
                StorageType.Boolean => (bool)value._boolean,
                StorageType.Int16 => value._int16 > 0,
                StorageType.Int32 => value._int32 > 0,
                StorageType.Int64 => value._int64 > 0,
                StorageType.UInt16 => value._uint16 > 0,
                StorageType.UInt32 => value._uint32 > 0,
                StorageType.UInt64 => value._uint64 > 0,
                StorageType.Single => value._single > 0,
                StorageType.Decimal => value._decimal > 0,
                StorageType.Double => value._double > 0,
                _ => throw new InvalidCastException(SR.GetString(SRKind.InvalidCastPropertyValue, typeof(bool).FullName)),
            };
        }

        /// <summary>
        /// 将 <see cref="Nullable{Boolean}"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(bool? value)
        {
            return new PropertyValue { _storageType = StorageType.Boolean, _boolean = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Nullable{Boolean}"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator bool?(PropertyValue value)
        {
            if (IsEmpty(value))
            {
                return null;
            }

            return value._storageType switch
            {
                StorageType.Byte => value._byte > 0,
                StorageType.SByte => value._sbyte > 0,
                StorageType.Boolean => value._boolean,
                StorageType.Int16 => value._int16 > 0,
                StorageType.Int32 => value._int32 > 0,
                StorageType.Int64 => value._int64 > 0,
                StorageType.UInt16 => value._uint16 > 0,
                StorageType.UInt32 => value._uint32 > 0,
                StorageType.UInt64 => value._uint64 > 0,
                StorageType.Single => value._single > 0,
                StorageType.Decimal => value._decimal > 0,
                StorageType.Double => value._double > 0,
                _ => throw new InvalidCastException(SR.GetString(SRKind.InvalidCastPropertyValue, typeof(bool?).FullName)),
            };
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
            return new PropertyValue { _storageType = StorageType.Byte, _byte = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Byte"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator byte(PropertyValue value)
        {
            if (IsEmpty(value))
            {
                return 0;
            }

            return value._storageType switch
            {
                StorageType.Byte => (byte)value._byte,
                StorageType.SByte => (byte)value._sbyte,
                StorageType.Boolean => (byte)(value._boolean == true ? 1 : 0),
                StorageType.Int16 => (byte)value._int16,
                StorageType.Int32 => (byte)value._int32,
                StorageType.Int64 => (byte)value._int64,
                StorageType.UInt16 => (byte)value._uint16,
                StorageType.UInt32 => (byte)value._uint32,
                StorageType.UInt64 => (byte)value._uint64,
                StorageType.Single => (byte)value._single,
                StorageType.Decimal => (byte)value._decimal,
                StorageType.Double => (byte)value._double,
                StorageType.Enum => ((IConvertible)value._enum).ToByte(null),
                _ => throw new InvalidCastException(SR.GetString(SRKind.InvalidCastPropertyValue, typeof(byte).FullName)),
            };
        }

        /// <summary>
        /// 将 <see cref="Nullable{Byte}"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(byte? value)
        {
            return new PropertyValue { _storageType = StorageType.Byte, _byte = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Nullable{Byte}"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator byte?(PropertyValue value)
        {
            if (IsEmpty(value))
            {
                return null;
            }

            return value._storageType switch
            {
                StorageType.Byte => (byte?)value._byte,
                StorageType.SByte => (byte?)value._sbyte,
                StorageType.Boolean => (byte?)(value._boolean == true ? 1 : 0),
                StorageType.Int16 => (byte?)value._int16,
                StorageType.Int32 => (byte?)value._int32,
                StorageType.Int64 => (byte?)value._int64,
                StorageType.UInt16 => (byte?)value._uint16,
                StorageType.UInt32 => (byte?)value._uint32,
                StorageType.UInt64 => (byte?)value._uint64,
                StorageType.Single => (byte?)value._single,
                StorageType.Decimal => (byte?)value._decimal,
                StorageType.Double => (byte?)value._double,
                StorageType.Enum => ((IConvertible)value._enum).ToByte(null),
                _ => throw new InvalidCastException(SR.GetString(SRKind.InvalidCastPropertyValue, typeof(byte?).FullName)),
            };
        }
        #endregion

        #region sbyte
        /// <summary>
        /// 将 <see cref="SByte"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(sbyte value)
        {
            return new PropertyValue { _storageType = StorageType.SByte, _sbyte = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="SByte"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator sbyte(PropertyValue value)
        {
            if (IsEmpty(value))
            {
                return 0;
            }

            return value._storageType switch
            {
                StorageType.Byte => (sbyte)value._byte,
                StorageType.SByte => (sbyte)value._sbyte,
                StorageType.Boolean => (sbyte)(value._boolean == true ? 1 : 0),
                StorageType.Int16 => (sbyte)value._int16,
                StorageType.Int32 => (sbyte)value._int32,
                StorageType.Int64 => (sbyte)value._int64,
                StorageType.UInt16 => (sbyte)value._uint16,
                StorageType.UInt32 => (sbyte)value._uint32,
                StorageType.UInt64 => (sbyte)value._uint64,
                StorageType.Single => (sbyte)value._single,
                StorageType.Decimal => (sbyte)value._decimal,
                StorageType.Double => (sbyte)value._double,
                StorageType.Enum => ((IConvertible)value._enum).ToSByte(null),
                _ => throw new InvalidCastException(SR.GetString(SRKind.InvalidCastPropertyValue, typeof(sbyte).FullName)),
            };
        }

        /// <summary>
        /// 将 <see cref="Nullable{SByte}"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(sbyte? value)
        {
            return new PropertyValue { _storageType = StorageType.SByte, _sbyte = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Nullable{SByte}"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator sbyte?(PropertyValue value)
        {
            if (IsEmpty(value))
            {
                return null;
            }

            return value._storageType switch
            {
                StorageType.Byte => (sbyte?)value._byte,
                StorageType.SByte => value._sbyte,
                StorageType.Boolean => (sbyte?)(value._boolean == true ? 1 : 0),
                StorageType.Int16 => (sbyte?)value._int16,
                StorageType.Int32 => (sbyte?)value._int32,
                StorageType.Int64 => (sbyte?)value._int64,
                StorageType.UInt16 => (sbyte?)value._uint16,
                StorageType.UInt32 => (sbyte?)value._uint32,
                StorageType.UInt64 => (sbyte?)value._uint64,
                StorageType.Single => (sbyte?)value._single,
                StorageType.Decimal => (sbyte?)value._decimal,
                StorageType.Double => (sbyte?)value._double,
                StorageType.Enum => ((IConvertible)value._enum).ToSByte(null),
                _ => throw new InvalidCastException(SR.GetString(SRKind.InvalidCastPropertyValue, typeof(sbyte).FullName)),
            };
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
            return new PropertyValue { _storageType = StorageType.ByteArray, _byteArray = value };
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
            return value._object != null ? (byte[])value._object : value._byteArray == null ? new byte[0] : value._byteArray;
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
            return new PropertyValue { _storageType = StorageType.DateTime, _dateTime = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="DateTime"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator DateTime(PropertyValue value)
        {
            return value == Empty ? DateTime.MinValue : value._dateTime.Value;
        }

        /// <summary>
        /// 将 <see cref="Nullable{DateTime}"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(DateTime? value)
        {
            return new PropertyValue { _storageType = StorageType.DateTime, _dateTime = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Nullable{DateTime}"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator DateTime?(PropertyValue value)
        {
            return value == Empty ? null : value._dateTime;
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
            return new PropertyValue { _storageType = StorageType.Guid, _guid = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Guid"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator Guid(PropertyValue value)
        {
            if (IsEmpty(value))
            {
                return Guid.Empty;
            }

            if (value._object != null)
            {
                return (Guid)value._object;
            }
            if (value._guid != null)
            {
                return value._guid.Value;
            }
            if (value._string != null)
            {
                return new Guid(value._string);
            }
            if (value._byteArray != null)
            {
                return new Guid(value._byteArray);
            }

            throw new InvalidCastException(SR.GetString(SRKind.InvalidCastPropertyValue, typeof(Guid).FullName));
        }

        /// <summary>
        /// 将 <see cref="Nullable{Guid}"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(Guid? value)
        {
            return new PropertyValue { _storageType = StorageType.Guid, _guid = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Nullable{Guid}"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator Guid?(PropertyValue value)
        {
            if (IsEmpty(value))
            {
                return null;
            }

            if (value._object != null)
            {
                return (Guid)value._object;
            }
            if (value._guid != null)
            {
                return value._guid.Value;
            }
            if (value._string != null)
            {
                return new Guid(value._string);
            }
            if (value._byteArray != null)
            {
                return new Guid(value._byteArray);
            }

            throw new InvalidCastException(SR.GetString(SRKind.InvalidCastPropertyValue, typeof(Guid).FullName));
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
            return new PropertyValue { _storageType = StorageType.Int16, _int16 = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Int16"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator short(PropertyValue value)
        {
            if (IsEmpty(value))
            {
                return 0;
            }

            return value._storageType switch
            {
                StorageType.Byte => (short)value._byte,
                StorageType.SByte => (short)value._sbyte,
                StorageType.Boolean => (short)(value._boolean == true ? 1 : 0),
                StorageType.Int16 => (short)value._int16,
                StorageType.Int32 => (short)value._int32,
                StorageType.Int64 => (short)value._int64,
                StorageType.UInt16 => (short)value._uint16,
                StorageType.UInt32 => (short)value._uint32,
                StorageType.UInt64 => (short)value._uint64,
                StorageType.Single => (short)value._single,
                StorageType.Decimal => (short)value._decimal,
                StorageType.Double => (short)value._double,
                StorageType.Enum => ((IConvertible)value._enum).ToInt16(null),
                _ => throw new InvalidCastException(SR.GetString(SRKind.InvalidCastPropertyValue, typeof(short).FullName)),
            };
        }

        /// <summary>
        /// 将 <see cref="Nullable{Int16}"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(short? value)
        {
            return new PropertyValue { _storageType = StorageType.Int16, _int16 = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Nullable{Int16}"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator short?(PropertyValue value)
        {
            if (IsEmpty(value))
            {
                return null;
            }

            return value._storageType switch
            {
                StorageType.Byte => value._byte,
                StorageType.SByte => value._sbyte,
                StorageType.Boolean => (short?)(value._boolean == true ? 1 : 0),
                StorageType.Int16 => value._int16,
                StorageType.Int32 => (short?)value._int32,
                StorageType.Int64 => (short?)value._int64,
                StorageType.UInt16 => (short?)value._uint16,
                StorageType.UInt32 => (short?)value._uint32,
                StorageType.UInt64 => (short?)value._uint64,
                StorageType.Single => (short?)value._single,
                StorageType.Decimal => (short?)value._decimal,
                StorageType.Double => (short?)value._double,
                StorageType.Enum => ((IConvertible)value._enum).ToInt16(null),
                _ => throw new InvalidCastException(SR.GetString(SRKind.InvalidCastPropertyValue, typeof(short?).FullName)),
            };
        }
        #endregion

        #region ushort
        /// <summary>
        /// 将 <see cref="UInt16"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(ushort value)
        {
            return new PropertyValue { _storageType = StorageType.UInt16, _uint16 = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="UInt16"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator ushort(PropertyValue value)
        {
            if (IsEmpty(value))
            {
                return 0;
            }

            return value._storageType switch
            {
                StorageType.Byte => (ushort)value._byte,
                StorageType.SByte => (ushort)value._sbyte,
                StorageType.Boolean => (ushort)(value._boolean == true ? 1 : 0),
                StorageType.Int16 => (ushort)value._int16,
                StorageType.Int32 => (ushort)value._int32,
                StorageType.Int64 => (ushort)value._int64,
                StorageType.UInt16 => (ushort)value._uint16,
                StorageType.UInt32 => (ushort)value._uint32,
                StorageType.UInt64 => (ushort)value._uint64,
                StorageType.Single => (ushort)value._single,
                StorageType.Decimal => (ushort)value._decimal,
                StorageType.Double => (ushort)value._double,
                StorageType.Enum => ((IConvertible)value._enum).ToUInt16(null),
                _ => throw new InvalidCastException(SR.GetString(SRKind.InvalidCastPropertyValue, typeof(ushort).FullName)),
            };
        }

        /// <summary>
        /// 将 <see cref="Nullable{UInt16}"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(ushort? value)
        {
            return new PropertyValue { _storageType = StorageType.UInt16, _uint16 = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Nullable{UInt16}"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator ushort?(PropertyValue value)
        {
            if (IsEmpty(value))
            {
                return null;
            }

            return value._storageType switch
            {
                StorageType.Byte => value._byte,
                StorageType.SByte => (ushort?)value._sbyte,
                StorageType.Boolean => (ushort?)(value._boolean == true ? 1 : 0),
                StorageType.Int16 => (ushort?)value._int16,
                StorageType.Int32 => (ushort?)value._int32,
                StorageType.Int64 => (ushort?)value._int64,
                StorageType.UInt16 => value._uint16,
                StorageType.UInt32 => (ushort?)value._uint32,
                StorageType.UInt64 => (ushort?)value._uint64,
                StorageType.Single => (ushort?)value._single,
                StorageType.Decimal => (ushort?)value._decimal,
                StorageType.Double => (ushort?)value._double,
                StorageType.Enum => ((IConvertible)value._enum).ToUInt16(null),
                _ => throw new InvalidCastException(SR.GetString(SRKind.InvalidCastPropertyValue, typeof(ushort?).FullName)),
            };
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
            return new PropertyValue { _storageType = StorageType.Int32, _int32 = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Int32"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator int(PropertyValue value)
        {
            if (IsEmpty(value))
            {
                return 0;
            }

            return value._storageType switch
            {
                StorageType.Byte => (int)value._byte,
                StorageType.SByte => (int)value._sbyte,
                StorageType.Boolean => value._boolean == true ? 1 : 0,
                StorageType.Int16 => (int)value._int16,
                StorageType.Int32 => (int)value._int32,
                StorageType.Int64 => (int)value._int64,
                StorageType.UInt16 => (int)value._uint16,
                StorageType.UInt32 => (int)value._uint32,
                StorageType.UInt64 => (int)value._uint64,
                StorageType.Single => (int)value._single,
                StorageType.Decimal => (int)value._decimal,
                StorageType.Double => (int)value._double,
                StorageType.Enum => ((IConvertible)value._enum).ToInt32(null),
                _ => throw new InvalidCastException(SR.GetString(SRKind.InvalidCastPropertyValue, typeof(int).FullName)),
            };
        }

        /// <summary>
        /// 将 <see cref="Nullable{Int32}"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(int? value)
        {
            return new PropertyValue { _storageType = StorageType.Int32, _int32 = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Nullable{Int32}"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator int?(PropertyValue value)
        {
            if (IsEmpty(value))
            {
                return null;
            }

            return value._storageType switch
            {
                StorageType.Byte => value._byte,
                StorageType.SByte => value._sbyte,
                StorageType.Boolean => value._boolean == true ? 1 : 0,
                StorageType.Int16 => value._int16,
                StorageType.Int32 => value._int32,
                StorageType.Int64 => (int?)value._int64,
                StorageType.UInt16 => value._uint16,
                StorageType.UInt32 => (int?)value._uint32,
                StorageType.UInt64 => (int?)value._uint64,
                StorageType.Single => (int?)value._single,
                StorageType.Decimal => (int?)value._decimal,
                StorageType.Double => (int?)value._double,
                StorageType.Enum => ((IConvertible)value._enum).ToInt32(null),
                _ => throw new InvalidCastException(SR.GetString(SRKind.InvalidCastPropertyValue, typeof(int?).FullName)),
            };
        }
        #endregion

        #region uint
        /// <summary>
        /// 将 <see cref="UInt32"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(uint value)
        {
            return new PropertyValue { _storageType = StorageType.UInt32, _uint32 = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="UInt32"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator uint(PropertyValue value)
        {
            if (IsEmpty(value))
            {
                return 0;
            }

            return value._storageType switch
            {
                StorageType.Byte => (uint)value._byte,
                StorageType.SByte => (uint)value._sbyte,
                StorageType.Boolean => (uint)(value._boolean == true ? 1 : 0),
                StorageType.Int16 => (uint)value._int16,
                StorageType.Int32 => (uint)value._int32,
                StorageType.Int64 => (uint)value._int64,
                StorageType.UInt16 => (uint)value._uint16,
                StorageType.UInt32 => (uint)value._uint32,
                StorageType.UInt64 => (uint)value._uint64,
                StorageType.Single => (uint)value._single,
                StorageType.Decimal => (uint)value._decimal,
                StorageType.Double => (uint)value._double,
                StorageType.Enum => ((IConvertible)value._enum).ToUInt32(null),
                _ => throw new InvalidCastException(SR.GetString(SRKind.InvalidCastPropertyValue, typeof(uint).FullName)),
            };
        }

        /// <summary>
        /// 将 <see cref="Nullable{UInt32}"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(uint? value)
        {
            return new PropertyValue { _storageType = StorageType.UInt32, _uint32 = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Nullable{UInt32}"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator uint?(PropertyValue value)
        {
            if (IsEmpty(value))
            {
                return null;
            }

            return value._storageType switch
            {
                StorageType.Byte => value._byte,
                StorageType.SByte => (uint?)value._sbyte,
                StorageType.Boolean => (uint?)(value._boolean == true ? 1 : 0),
                StorageType.Int16 => (uint?)value._int16,
                StorageType.Int32 => (uint?)value._int32,
                StorageType.Int64 => (uint?)value._int64,
                StorageType.UInt16 => value._uint16,
                StorageType.UInt32 => value._uint32,
                StorageType.UInt64 => (uint?)value._uint64,
                StorageType.Single => (uint?)value._single,
                StorageType.Decimal => (uint?)value._decimal,
                StorageType.Double => (uint?)value._double,
                StorageType.Enum => ((IConvertible)value._enum).ToUInt32(null),
                _ => throw new InvalidCastException(SR.GetString(SRKind.InvalidCastPropertyValue, typeof(uint?).FullName)),
            };
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
            return new PropertyValue { _storageType = StorageType.Int64, _int64 = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Int64"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator long(PropertyValue value)
        {
            if (IsEmpty(value))
            {
                return 0L;
            }

            return value._storageType switch
            {
                StorageType.Byte => (long)value._byte,
                StorageType.SByte => (long)value._sbyte,
                StorageType.Boolean => value._boolean == true ? 1L : 0L,
                StorageType.Int16 => (long)value._int16,
                StorageType.Int32 => (long)value._int32,
                StorageType.Int64 => (long)value._int64,
                StorageType.UInt16 => (long)value._uint16,
                StorageType.UInt32 => (long)value._uint32,
                StorageType.UInt64 => (long)value._uint64,
                StorageType.Single => (long)value._single,
                StorageType.Decimal => (long)value._decimal,
                StorageType.Double => (long)value._double,
                StorageType.Enum => ((IConvertible)value._enum).ToInt64(null),
                _ => throw new InvalidCastException(SR.GetString(SRKind.InvalidCastPropertyValue, typeof(long).FullName)),
            };
        }

        /// <summary>
        /// 将 <see cref="Nullable{Int64}"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(long? value)
        {
            return new PropertyValue { _storageType = StorageType.Int64, _int64 = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Nullable{Int64}"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator long?(PropertyValue value)
        {
            if (IsEmpty(value))
            {
                return null;
            }

            return value._storageType switch
            {
                StorageType.Byte => value._byte,
                StorageType.SByte => value._sbyte,
                StorageType.Boolean => value._boolean == true ? 1L : 0L,
                StorageType.Int16 => value._int16,
                StorageType.Int32 => value._int32,
                StorageType.Int64 => value._int64,
                StorageType.UInt16 => value._uint16,
                StorageType.UInt32 => value._uint32,
                StorageType.UInt64 => (long?)value._uint64,
                StorageType.Single => (long?)value._single,
                StorageType.Decimal => (long?)value._decimal,
                StorageType.Double => (long?)value._double,
                StorageType.Enum => ((IConvertible)value._enum).ToInt64(null),
                _ => throw new InvalidCastException(SR.GetString(SRKind.InvalidCastPropertyValue, typeof(long?).FullName)),
            };
        }
        #endregion

        #region ulong
        /// <summary>
        /// 将 <see cref="UInt64"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(ulong value)
        {
            return new PropertyValue { _storageType = StorageType.UInt64, _uint64 = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="UInt64"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator ulong(PropertyValue value)
        {
            if (IsEmpty(value))
            {
                return 0L;
            }

            return value._storageType switch
            {
                StorageType.Byte => (ulong)value._byte,
                StorageType.SByte => (ulong)value._sbyte,
                StorageType.Boolean => (ulong)(value._boolean == true ? 1L : 0L),
                StorageType.Int16 => (ulong)value._int16,
                StorageType.Int32 => (ulong)value._int32,
                StorageType.Int64 => (ulong)value._int64,
                StorageType.UInt16 => (ulong)value._uint16,
                StorageType.UInt32 => (ulong)value._uint32,
                StorageType.UInt64 => (ulong)value._uint64,
                StorageType.Single => (ulong)value._single,
                StorageType.Decimal => (ulong)value._decimal,
                StorageType.Double => (ulong)value._double,
                StorageType.Enum => ((IConvertible)value._enum).ToUInt64(null),
                _ => throw new InvalidCastException(SR.GetString(SRKind.InvalidCastPropertyValue, typeof(ulong).FullName)),
            };
        }

        /// <summary>
        /// 将 <see cref="Nullable{UInt64}"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(ulong? value)
        {
            return new PropertyValue { _storageType = StorageType.UInt64, _uint64 = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Nullable{Int64}"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator ulong?(PropertyValue value)
        {
            if (IsEmpty(value))
            {
                return null;
            }

            return value._storageType switch
            {
                StorageType.Byte => value._byte,
                StorageType.SByte => (ulong?)value._sbyte,
                StorageType.Boolean => (ulong?)(value._boolean == true ? 1L : 0L),
                StorageType.Int16 => (ulong?)value._int16,
                StorageType.Int32 => (ulong?)value._int32,
                StorageType.Int64 => (ulong?)value._int64,
                StorageType.UInt16 => value._uint16,
                StorageType.UInt32 => value._uint32,
                StorageType.UInt64 => value._uint64,
                StorageType.Single => (ulong?)value._single,
                StorageType.Decimal => (ulong?)value._decimal,
                StorageType.Double => (ulong?)value._double,
                StorageType.Enum => ((IConvertible)value._enum).ToUInt64(null),
                _ => throw new InvalidCastException(SR.GetString(SRKind.InvalidCastPropertyValue, typeof(ulong?).FullName)),
            };
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
            return new PropertyValue { _storageType = StorageType.Decimal, _decimal = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Decimal"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator decimal(PropertyValue value)
        {
            if (IsEmpty(value))
            {
                return 0m;
            }

            return value._storageType switch
            {
                StorageType.Byte => (decimal)value._byte,
                StorageType.SByte => (decimal)value._sbyte,
                StorageType.Boolean => value._boolean == true ? 1 : 0,
                StorageType.Int16 => (decimal)value._int16,
                StorageType.Int32 => (decimal)value._int32,
                StorageType.Int64 => (decimal)value._int64,
                StorageType.UInt16 => (decimal)value._uint16,
                StorageType.UInt32 => (decimal)value._uint32,
                StorageType.UInt64 => (decimal)value._uint64,
                StorageType.Single => (decimal)value._single,
                StorageType.Decimal => (decimal)value._decimal,
                StorageType.Double => (decimal)value._double,
                StorageType.Enum => ((IConvertible)value._enum).ToDecimal(null),
                _ => throw new InvalidCastException(SR.GetString(SRKind.InvalidCastPropertyValue, typeof(decimal).FullName)),
            };
        }

        /// <summary>
        /// 将 <see cref="Nullable{Decimal}"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(decimal? value)
        {
            return new PropertyValue { _storageType = StorageType.Decimal, _decimal = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Nullable{Decimal}"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator decimal?(PropertyValue value)
        {
            if (IsEmpty(value))
            {
                return null;
            }

            return value._storageType switch
            {
                StorageType.Byte => value._byte,
                StorageType.SByte => value._sbyte,
                StorageType.Boolean => value._boolean == true ? 1 : 0,
                StorageType.Int16 => value._int16,
                StorageType.Int32 => value._int32,
                StorageType.Int64 => value._int64,
                StorageType.UInt16 => value._uint16,
                StorageType.UInt32 => value._uint32,
                StorageType.UInt64 => value._uint64,
                StorageType.Single => (decimal?)value._single,
                StorageType.Decimal => value._decimal,
                StorageType.Double => (decimal?)value._double,
                StorageType.Enum => ((IConvertible)value._enum).ToDecimal(null),
                _ => throw new InvalidCastException(SR.GetString(SRKind.InvalidCastPropertyValue, typeof(decimal?).FullName)),
            };
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
            return new PropertyValue { _storageType = StorageType.Single, _single = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Single"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator float(PropertyValue value)
        {
            if (IsEmpty(value))
            {
                return 0;
            }

            return value._storageType switch
            {
                StorageType.Byte => (float)value._byte,
                StorageType.SByte => (float)value._sbyte,
                StorageType.Boolean => value._boolean == true ? 1 : 0,
                StorageType.Int16 => (float)value._int16,
                StorageType.Int32 => (float)value._int32,
                StorageType.Int64 => (float)value._int64,
                StorageType.UInt16 => (float)value._uint16,
                StorageType.UInt32 => (float)value._uint32,
                StorageType.UInt64 => (float)value._uint64,
                StorageType.Single => (float)value._single,
                StorageType.Decimal => (float)value._decimal,
                StorageType.Double => (float)value._double,
                StorageType.Enum => ((IConvertible)value._enum).ToSingle(null),
                _ => throw new InvalidCastException(SR.GetString(SRKind.InvalidCastPropertyValue, typeof(float).FullName)),
            };
        }

        /// <summary>
        /// 将 <see cref="Nullabl{Single}"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(float? value)
        {
            return new PropertyValue { _storageType = StorageType.Single, _single = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Nullable{Single}"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator float?(PropertyValue value)
        {
            if (IsEmpty(value))
            {
                return null;
            }

            return value._storageType switch
            {
                StorageType.Byte => (float)value._byte,
                StorageType.SByte => (float)value._sbyte,
                StorageType.Boolean => value._boolean == true ? 1 : 0,
                StorageType.Int16 => (float)value._int16,
                StorageType.Int32 => (float)value._int32,
                StorageType.Int64 => (float)value._int64,
                StorageType.UInt16 => (float)value._uint16,
                StorageType.UInt32 => (float)value._uint32,
                StorageType.UInt64 => (float)value._uint64,
                StorageType.Single => (float)value._single,
                StorageType.Decimal => (float)value._decimal,
                StorageType.Double => (float?)value._double,
                StorageType.Enum => ((IConvertible)value._enum).ToSingle(null),
                _ => throw new InvalidCastException(SR.GetString(SRKind.InvalidCastPropertyValue, typeof(float?).FullName)),
            };
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
            return new PropertyValue { _storageType = StorageType.Double, _double = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Double"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator double(PropertyValue value)
        {
            if (IsEmpty(value))
            {
                return 0d;
            }

            return value._storageType switch
            {
                StorageType.Byte => (double)value._byte,
                StorageType.SByte => (double)value._sbyte,
                StorageType.Boolean => value._boolean == true ? 1 : 0,
                StorageType.Int16 => (double)value._int16,
                StorageType.Int32 => (double)value._int32,
                StorageType.Int64 => (double)value._int64,
                StorageType.UInt16 => (double)value._uint16,
                StorageType.UInt32 => (double)value._uint32,
                StorageType.UInt64 => (double)value._uint64,
                StorageType.Single => (double)value._single,
                StorageType.Decimal => (double)value._decimal,
                StorageType.Double => (double)value._double,
                StorageType.Enum => ((IConvertible)value._enum).ToDouble(null),
                _ => throw new InvalidCastException(SR.GetString(SRKind.InvalidCastPropertyValue, typeof(double).FullName)),
            };
        }

        /// <summary>
        /// 将 <see cref="Nullable{Double}"/> 类型隐式转换为 <see cref="PropertyValue"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PropertyValue(double? value)
        {
            return new PropertyValue { _storageType = StorageType.Double, _double = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Nullable{Double}"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator double?(PropertyValue value)
        {
            if (IsEmpty(value))
            {
                return null;
            }

            return value._storageType switch
            {
                StorageType.Byte => value._byte,
                StorageType.SByte => value._sbyte,
                StorageType.Boolean => value._boolean == true ? 1 : 0,
                StorageType.Int16 => value._int16,
                StorageType.Int32 => value._int32,
                StorageType.Int64 => value._int64,
                StorageType.UInt16 => value._uint16,
                StorageType.UInt32 => value._uint32,
                StorageType.UInt64 => value._uint64,
                StorageType.Single => value._single,
                StorageType.Decimal => (double?)value._decimal,
                StorageType.Double => value._double,
                StorageType.Enum => ((IConvertible)value._enum).ToDouble(null),
                _ => throw new InvalidCastException(SR.GetString(SRKind.InvalidCastPropertyValue, typeof(double?).FullName)),
            };
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
            return new PropertyValue { _storageType = StorageType.String, _string = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="String"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator string(PropertyValue value)
        {
            return value == Empty ? null : value._string;
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
            return new PropertyValue { _storageType = StorageType.Enum, _enum = value };
        }

        /// <summary>
        /// 将 <see cref="PropertyValue"/> 类型显示转换为 <see cref="Enum"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator Enum(PropertyValue value)
        {
            if (IsEmpty(value))
            {
                return null;
            }

            return value._storageType switch
            {
                StorageType.Enum => value._enum,
                StorageType.Object => (Enum)value._object,
                _ => throw new InvalidCastException(SR.GetString(SRKind.InvalidCastPropertyValue, "enum")),
            };
        }
        #endregion

        #region ==和!=
        public static bool operator ==(PropertyValue v1, PropertyValue v2)
        {
            if (v1._storageType != v2._storageType)
            {
                return false;
            }

            return v1._storageType switch
            {
                StorageType.Boolean => v1._boolean == v2._boolean,
                StorageType.Byte => v1._byte == v2._byte,
                StorageType.SByte => v1._sbyte == v2._sbyte,
                StorageType.Char => v1._char == v2._char,
                StorageType.DateTime => v1._dateTime == v2._dateTime,
                StorageType.Decimal => v1._decimal == v2._decimal,
                StorageType.Double => v1._double == v2._double,
                StorageType.Enum => v1._enum == v2._enum,
                StorageType.Guid => v1._guid == v2._guid,
                StorageType.Int16 => v1._int16 == v2._int16,
                StorageType.Int32 => v1._int32 == v2._int32,
                StorageType.Int64 => v1._int64 == v2._int64,
                StorageType.UInt16 => v1._uint16 == v2._uint16,
                StorageType.UInt32 => v1._uint32 == v2._uint32,
                StorageType.UInt64 => v1._uint64 == v2._uint64,
                StorageType.Single => v1._single == v2._single,
                StorageType.String => v1._string == v2._string,
                StorageType.ByteArray => ByteEqueals(v1._byteArray, v2._byteArray),
                _ => v1._object == v2._object,
            };
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

            for (int i = 0, n = b1.Length; i < n; i++)
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
        public static bool operator ==(PropertyValue v1, int v2)
        {
            if (v1._storageType != StorageType.Int32)
            {
                return false;
            }

            return v1._int32 == v2;
        }

        public static bool operator !=(PropertyValue v1, int v2)
        {
            if (v1._storageType != StorageType.Int32)
            {
                return true;
            }

            return v1._int32 != v2;
        }

        public static bool operator ==(int v1, PropertyValue v2)
        {
            if (v2._storageType != StorageType.Int32)
            {
                return false;
            }

            return v2._int32 == v1;
        }

        public static bool operator !=(int v1, PropertyValue v2)
        {
            if (v2._storageType != StorageType.Int32)
            {
                return true;
            }

            return v2._int32 != v1;
        }
        #endregion

        #region uint equals
        public static bool operator ==(PropertyValue v1, uint v2)
        {
            if (v1._storageType != StorageType.UInt32)
            {
                return false;
            }

            return v1._uint32 == v2;
        }

        public static bool operator !=(PropertyValue v1, uint v2)
        {
            if (v1._storageType != StorageType.UInt32)
            {
                return true;
            }

            return v1._uint32 != v2;
        }

        public static bool operator ==(uint v1, PropertyValue v2)
        {
            if (v2._storageType != StorageType.UInt32)
            {
                return false;
            }

            return v2._uint32 == v1;
        }

        public static bool operator !=(uint v1, PropertyValue v2)
        {
            if (v2._storageType != StorageType.UInt32)
            {
                return true;
            }

            return v2._uint32 != v1;
        }
        #endregion

        #region long equals
        public static bool operator ==(PropertyValue v1, long v2)
        {
            if (v1._storageType != StorageType.Int64)
            {
                return false;
            }

            return v1._int64 == v2;
        }

        public static bool operator !=(PropertyValue v1, long v2)
        {
            if (v1._storageType != StorageType.Int64)
            {
                return true;
            }

            return v1._int64 != v2;
        }

        public static bool operator ==(long v1, PropertyValue v2)
        {
            if (v2._storageType != StorageType.Int64)
            {
                return false;
            }

            return v2._int64 == v1;
        }

        public static bool operator !=(long v1, PropertyValue v2)
        {
            if (v2._storageType != StorageType.Int64)
            {
                return true;
            }

            return v2._int64 != v1;
        }
        #endregion

        #region ulong equals
        public static bool operator ==(PropertyValue v1, ulong v2)
        {
            if (v1._storageType != StorageType.UInt64)
            {
                return false;
            }

            return v1._uint64 == v2;
        }

        public static bool operator !=(PropertyValue v1, ulong v2)
        {
            if (v1._storageType != StorageType.UInt64)
            {
                return true;
            }

            return v1._uint64 != v2;
        }

        public static bool operator ==(ulong v1, PropertyValue v2)
        {
            if (v2._storageType != StorageType.UInt64)
            {
                return false;
            }

            return v2._uint64 == v1;
        }

        public static bool operator !=(ulong v1, PropertyValue v2)
        {
            if (v2._storageType != StorageType.UInt64)
            {
                return true;
            }

            return v2._uint64 != v1;
        }
        #endregion

        #region string equals
        public static bool operator ==(PropertyValue v1, string v2)
        {
            if (v1._storageType != StorageType.String)
            {
                return false;
            }

            return v1._string == v2;
        }

        public static bool operator !=(PropertyValue v1, string v2)
        {
            if (v1._storageType != StorageType.String)
            {
                return true;
            }

            return v1._string != v2;
        }

        public static bool operator ==(string v1, PropertyValue v2)
        {
            if (v2._storageType != StorageType.String)
            {
                return false;
            }

            return v2._string == v1;
        }

        public static bool operator !=(string v1, PropertyValue v2)
        {
            if (v2._storageType != StorageType.String)
            {
                return true;
            }

            return v2._string != v1;
        }
        #endregion
        #endregion

        /// <summary>
        /// 获取该对象是否有效，即数字不为 0、字符串不为空字符时有效。
        /// </summary>
        public bool IsValid => _storageType switch
        {
            StorageType.Byte => _byte != 0,
            StorageType.SByte => _sbyte != 0,
            StorageType.Char => _char != '\0',
            StorageType.Decimal => _decimal != 0,
            StorageType.Double => _double != 0,
            StorageType.Int16 => _int16 != 0,
            StorageType.Int32 => _int32 != 0,
            StorageType.Int64 => _int64 != 0,
            StorageType.UInt16 => _uint16 != 0,
            StorageType.UInt32 => _uint32 != 0,
            StorageType.UInt64 => _uint64 != 0,
            StorageType.Single => _single != 0,
            StorageType.String => !string.IsNullOrEmpty(_string),
            _ => true
        };

        /// <summary>
        /// 纠正存储值的类型。
        /// </summary>
        /// <param name="correctType">要纠正的实际存储的类型。</param>
        public void CorrectValue(Type correctType)
        {
            if (correctType.IsEnum && _storageType != StorageType.Enum)
            {
                CorrectEnumValue(correctType);
            }
            else if (correctType.GetNonNullableType() == typeof(Guid) && _storageType != StorageType.Guid)
            {
                CorrectGuidValue();
            }
        }

        private void CorrectEnumValue(Type correctType)
        {
            switch (_storageType)
            {
                case StorageType.Byte:
                    _enum = (Enum)Enum.Parse(correctType, _byte.ToString());
                    _byte = null;
                    break;
                case StorageType.Int32:
                    _enum = (Enum)Enum.Parse(correctType, _int32.ToString());
                    _int32 = null;
                    break;
                case StorageType.Int16:
                    _enum = (Enum)Enum.Parse(correctType, _int16.ToString());
                    _int16 = null;
                    break;
                case StorageType.Int64:
                    _enum = (Enum)Enum.Parse(correctType, _int64.ToString());
                    _int64 = null;
                    break;
            }

            _storageType = StorageType.Enum;
        }

        private void CorrectGuidValue()
        {
            switch (_storageType)
            {
                case StorageType.String:
                    _guid = new Guid(_string);
                    _string = null;
                    break;
                case StorageType.ByteArray:
                    _guid = new Guid(_byteArray);
                    _byteArray = null;
                    break;
            }

            _storageType = StorageType.Guid;
        }

        #region object
        /// <summary>
        /// 获取实际存储的值，转换为 <see cref="System.Object"/> 表示。
        /// </summary>
        /// <returns></returns>
        public object GetValue()
        {
            return _storageType switch
            {
                StorageType.Boolean => _boolean,
                StorageType.Byte => _byte,
                StorageType.SByte => _sbyte,
                StorageType.ByteArray => _byteArray,
                StorageType.Char => _char,
                StorageType.DateTime => _dateTime,
                StorageType.Decimal => _decimal,
                StorageType.Double => _double,
                StorageType.Enum => _enum,
                StorageType.Guid => _guid,
                StorageType.Int16 => _int16,
                StorageType.Int32 => _int32,
                StorageType.Int64 => _int64,
                StorageType.UInt16 => _uint16,
                StorageType.UInt32 => _uint32,
                StorageType.UInt64 => _uint64,
                StorageType.Object => _object,
                StorageType.Single => _single,
                StorageType.String => _string,
                _ => null,
            };
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
            return _storageType switch
            {
                StorageType.Boolean => _boolean == null ? 0 : _boolean.GetHashCode(),
                StorageType.Byte => _byte == null ? 0 : _byte.GetHashCode(),
                StorageType.SByte => _sbyte == null ? 0 : _sbyte.GetHashCode(),
                StorageType.Char => _char == null ? 0 : _char.GetHashCode(),
                StorageType.DateTime => _dateTime == null ? 0 : _dateTime.GetHashCode(),
                StorageType.Decimal => _decimal == null ? 0 : _decimal.GetHashCode(),
                StorageType.Double => _double == null ? 0 : _double.GetHashCode(),
                StorageType.Enum => _enum == null ? 0 : _enum.GetHashCode(),
                StorageType.Guid => _guid == null ? 0 : _guid.GetHashCode(),
                StorageType.Int16 => _int16 == null ? 0 : _int16.GetHashCode(),
                StorageType.Int32 => _int32 == null ? 0 : _int32.GetHashCode(),
                StorageType.Int64 => _int64 == null ? 0 : _int64.GetHashCode(),
                StorageType.UInt16 => _uint16 == null ? 0 : _uint16.GetHashCode(),
                StorageType.UInt32 => _uint32 == null ? 0 : _uint32.GetHashCode(),
                StorageType.UInt64 => _uint64 == null ? 0 : _uint64.GetHashCode(),
                StorageType.Single => _single == null ? 0 : _single.GetHashCode(),
                StorageType.String => _string == null ? 0 : _string.GetHashCode(),
                _ => base.GetHashCode(),
            };
        }

        /// <summary>
        /// 克隆该对象副本。
        /// </summary>
        /// <returns></returns>
        public PropertyValue Clone()
        {
            switch (_storageType)
            {
                case StorageType.Boolean: return _boolean;
                case StorageType.Byte: return _byte;
                case StorageType.SByte: return _sbyte;
                case StorageType.Char: return _char;
                case StorageType.DateTime: return _dateTime;
                case StorageType.Decimal: return _decimal;
                case StorageType.Double: return _double;
                case StorageType.Enum: return _enum;
                case StorageType.Guid: return _guid;
                case StorageType.Int16: return _int16;
                case StorageType.Int32: return _int32;
                case StorageType.Int64: return _int64;
                case StorageType.UInt16: return _uint16;
                case StorageType.UInt32: return _uint32;
                case StorageType.UInt64: return _uint64;
                case StorageType.Single: return _single;
                case StorageType.String: return string.IsNullOrEmpty(_string) ? string.Empty : string.Copy(_string);
                default:
                    if (_object == null)
                    {
                        return Empty;
                    }
                    var cloneable = _object.As<ICloneable>();
                    return new PropertyValue { _storageType = StorageType.Object, _object = cloneable == null ? _object : cloneable.Clone() };
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
            return _storageType == StorageType.Empty ? string.Empty : GetValue().ToStringSafely();
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
            return type == typeof(bool) ||
                type == typeof(char) ||
                type == typeof(byte) ||
                type == typeof(sbyte) ||
                type == typeof(byte[]) ||
                type == typeof(DateTime) ||
                type == typeof(decimal) ||
                type == typeof(double) ||
                type == typeof(Guid) ||
                type == typeof(short) ||
                type == typeof(int) ||
                type == typeof(long) ||
                type == typeof(ushort) ||
                type == typeof(uint) ||
                type == typeof(ulong) ||
                type == typeof(float) ||
                type == typeof(string) ||
                type.IsEnum;
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
                return Empty;
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
                return Type.GetTypeCode(nonNullType) switch
                {
                    TypeCode.Boolean => isNull ? new bool?() : Convert.ToBoolean(value),
                    TypeCode.Byte => isNull ? new byte?() : Convert.ToByte(value),
                    TypeCode.SByte => isNull ? new sbyte?() : Convert.ToSByte(value),
                    TypeCode.Char => isNull ? new char?() : Convert.ToChar(value),
                    TypeCode.DateTime => isNull ? new DateTime?() : ProcessNewDateTime(value),
                    TypeCode.Decimal => isNull ? new decimal?() : Convert.ToDecimal(value),
                    TypeCode.Double => isNull ? new double?() : Convert.ToDouble(value),
                    TypeCode.Int16 => isNull ? new short?() : Convert.ToInt16(value),
                    TypeCode.Int32 => isNull ? new int?() : Convert.ToInt32(value),
                    TypeCode.Int64 => isNull ? new long?() : Convert.ToInt64(value),
                    TypeCode.UInt16 => isNull ? new ushort?() : Convert.ToUInt16(value),
                    TypeCode.UInt32 => isNull ? new uint?() : Convert.ToUInt32(value),
                    TypeCode.UInt64 => isNull ? new ulong?() : Convert.ToUInt64(value),
                    TypeCode.Single => isNull ? new float?() : Convert.ToSingle(value),
                    TypeCode.String => isNull ? (string)null : ProcessNewString(value),
                    _ => nonNullType.FullName switch
                    {
                        "System.Guid" => isNull ? new Guid?() : ProcessNewGuid(value),
                        _ => Empty
                    }
                };
            }

            return new PropertyValue { _storageType = StorageType.Object, _object = value };
        }

        private static PropertyValue ProcessNewDateTime(object value)
        {
            if (value is string)
            {
                if (value.Equals(Constants.Now))
                {
                    return Now;
                }
                else if (value.Equals(Constants.Today))
                {
                    return Today;
                }
            }

            return Convert.ToDateTime(value);
        }

        private static PropertyValue ProcessNewString(object value)
        {
            if (value is string)
            {
                if (value.Equals(Constants.Guid))
                {
                    return NewGuid;
                }
            }

            return Convert.ToString(value);
        }

        private static PropertyValue ProcessNewGuid(object value)
        {
            if (value is string)
            {
                if (value.Equals(Constants.Guid))
                {
                    return NewGuid;
                }
            }

            return new Guid(value.ToString());
        }

        /// <summary>
        /// 根据指定的<see cref="PropertyValue"/> 来设置 <see cref="Parameter"/> 的值。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static Parameter Parametrization(PropertyValue value, Parameter parameter)
        {
            switch (value._storageType)
            {
                case StorageType.Boolean:
                    parameter.Value = value._boolean;
                    parameter.DbType = DbType.Boolean;
                    break;
                case StorageType.Byte:
                    parameter.Value = value._byte;
                    parameter.DbType = DbType.Byte;
                    break;
                case StorageType.SByte:
                    parameter.Value = value._sbyte;
                    parameter.DbType = DbType.SByte;
                    break;
                case StorageType.ByteArray:
                    parameter.Value = value._byteArray;
                    parameter.DbType = DbType.Binary;
                    break;
                case StorageType.Char:
                    parameter.Value = value._char;
                    parameter.DbType = DbType.AnsiString;
                    break;
                case StorageType.DateTime:
                    parameter.Value = value._dateTime;
                    parameter.DbType = DbType.DateTime;
                    break;
                case StorageType.Decimal:
                    parameter.Value = value._decimal;
                    parameter.DbType = DbType.Decimal;
                    break;
                case StorageType.Double:
                    parameter.Value = value._double;
                    parameter.DbType = DbType.Double;
                    break;
                case StorageType.Enum:
                    parameter.Value = value._enum.To<int>();
                    parameter.DbType = DbType.Int32;
                    break;
                case StorageType.Guid:
                    parameter.Value = value._guid;
                    parameter.DbType = DbType.Guid;
                    break;
                case StorageType.Int16:
                    parameter.Value = value._int16;
                    parameter.DbType = DbType.Int16;
                    break;
                case StorageType.Int32:
                    parameter.Value = value._int32;
                    parameter.DbType = DbType.Int32;
                    break;
                case StorageType.Int64:
                    parameter.Value = value._int64;
                    parameter.DbType = DbType.Int64;
                    break;
                case StorageType.UInt16:
                    parameter.Value = value._uint16;
                    parameter.DbType = DbType.UInt16;
                    break;
                case StorageType.UInt32:
                    parameter.Value = value._uint32;
                    parameter.DbType = DbType.UInt32;
                    break;
                case StorageType.UInt64:
                    parameter.Value = value._uint64;
                    parameter.DbType = DbType.UInt64;
                    break;
                case StorageType.Single:
                    parameter.Value = value._single;
                    parameter.DbType = DbType.Single;
                    break;
                case StorageType.String:
                    parameter.Value = value._string;
                    parameter.DbType = DbType.AnsiString;
                    break;
                case StorageType.Object:
                    var converter = ConvertManager.GetConverter(value._object.GetType());
                    if (converter != null)
                    {
                        var dbType = value._dataType ?? DbType.String;
                        parameter.Value = converter.ConvertTo(value._object, dbType);
                        parameter.DbType = dbType;
                    }
                    break;
            }

            return parameter;
        }

        /// <summary>
        /// 尝试给常量分配动态值。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="formatter"></param>
        /// <returns></returns>
        public PropertyValue TryAllotValue(Type type, string formatter)
        {
            if (!_isConstant)
            {
                return this;
            }

            if (this == Now)
            {
                return type == typeof(string) ? DateTime.Now.ToString(formatter) : (PropertyValue)DateTime.Now;
            }
            else if (this == Today)
            {
                return type == typeof(string) ? DateTime.Today.ToString(formatter) : (PropertyValue)DateTime.Today;
            }
            else if (this == NewGuid)
            {
                return type == typeof(Guid) ? Guid.NewGuid() : (PropertyValue)Guid.NewGuid().ToString(formatter);
            }

            return this;
        }

        /// <summary>
        /// 安全地获取值。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object GetValueSafely(PropertyValue value)
        {
            if (value == Empty || value._storageType == StorageType.Empty)
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
            return value == Empty || value._storageType == StorageType.Empty || value.GetValue() == null;
        }

        /// <summary>
        /// 判断是否为空或为缺省值。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsEmptyOrDefault(PropertyValue value)
        {
            return value._storageType switch
            {
                StorageType.Byte => value._byte == 0 || value._byte == null,
                StorageType.SByte => value._sbyte == 0 || value._sbyte == null,
                StorageType.Int16 => value._int16 == 0 || value._int16 == null,
                StorageType.Int32 => value._int32 == 0 || value._int32 == null,
                StorageType.Int64 => value._int64 == 0 || value._int64 == null,
                StorageType.UInt16 => value._uint16 == 0 || value._uint16 == null,
                StorageType.UInt32 => value._uint32 == 0 || value._uint32 == null,
                StorageType.UInt64 => value._uint64 == 0 || value._uint64 == null,
                StorageType.Decimal => value._decimal == 0 || value._decimal == null,
                StorageType.Single => value._single == 0 || value._single == null,
                StorageType.Double => value._double == 0 || value._double == null,
                StorageType.Boolean => value._boolean == false || value._boolean == null,
                StorageType.String => string.IsNullOrEmpty(value._string),
                _ => value == Empty || value._storageType == StorageType.Empty || value.GetValue() == null,
            };
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
        /// System.SByte 类型的数据。
        /// </summary>
        SByte,
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
        /// System.UInt16 类型的数据。
        /// </summary>
        UInt16,
        /// <summary>
        /// System.UInt32 类型的数据。
        /// </summary>
        UInt32,
        /// <summary>
        /// System.UInt64 类型的数据。
        /// </summary>
        UInt64,
        /// <summary>
        /// System.Single 类型的数据。
        /// </summary>
        Single,
        /// <summary>
        /// System.String 类型的数据。
        /// </summary>
        String,
        /// <summary>
        /// System.Object 类型的数据。
        /// </summary>
        Object
    }
}
