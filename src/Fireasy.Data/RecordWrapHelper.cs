// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Data.RecordWrapper;
using System;
using System.Data;
using System.Reflection;

namespace Fireasy.Data
{
    internal class RecordWrapHelper
    {
        private class MethodCache
        {
            internal protected static readonly MethodInfo GetChar = typeof(IRecordWrapper).GetMethod(nameof(IRecordWrapper.GetChar), new[] { typeof(IDataRecord), typeof(int) });
            internal protected static readonly MethodInfo GetByte = typeof(IRecordWrapper).GetMethod(nameof(IRecordWrapper.GetByte), new[] { typeof(IDataRecord), typeof(int) });
            internal protected static readonly MethodInfo GetBoolean = typeof(IRecordWrapper).GetMethod(nameof(IRecordWrapper.GetBoolean), new[] { typeof(IDataRecord), typeof(int) });
            internal protected static readonly MethodInfo GetInt16 = typeof(IRecordWrapper).GetMethod(nameof(IRecordWrapper.GetInt16), new[] { typeof(IDataRecord), typeof(int) });
            internal protected static readonly MethodInfo GetInt32 = typeof(IRecordWrapper).GetMethod(nameof(IRecordWrapper.GetInt32), new[] { typeof(IDataRecord), typeof(int) });
            internal protected static readonly MethodInfo GetInt64 = typeof(IRecordWrapper).GetMethod(nameof(IRecordWrapper.GetInt64), new[] { typeof(IDataRecord), typeof(int) });
            internal protected static readonly MethodInfo GetString = typeof(IRecordWrapper).GetMethod(nameof(IRecordWrapper.GetString), new[] { typeof(IDataRecord), typeof(int) });
            internal protected static readonly MethodInfo GetDateTime = typeof(IRecordWrapper).GetMethod(nameof(IRecordWrapper.GetDateTime), new[] { typeof(IDataRecord), typeof(int) });
            internal protected static readonly MethodInfo GetDecimal = typeof(IRecordWrapper).GetMethod(nameof(IRecordWrapper.GetDecimal), new[] { typeof(IDataRecord), typeof(int) });
            internal protected static readonly MethodInfo GetDouble = typeof(IRecordWrapper).GetMethod(nameof(IRecordWrapper.GetDouble), new[] { typeof(IDataRecord), typeof(int) });
            internal protected static readonly MethodInfo GetSingle = typeof(IRecordWrapper).GetMethod(nameof(IRecordWrapper.GetFloat), new[] { typeof(IDataRecord), typeof(int) });
            internal protected static readonly MethodInfo GetGuid = typeof(IRecordWrapper).GetMethod(nameof(IRecordWrapper.GetGuid), new[] { typeof(IDataRecord), typeof(int) });
            internal protected static readonly MethodInfo GetBytes = typeof(IRecordWrapper).GetMethod(nameof(IRecordWrapper.GetBytes), new[] { typeof(IDataRecord), typeof(int) });

        }

        /// <summary>
        /// 根据值的类型返回获取值的方法。
        /// </summary>
        /// <param name="dataType">值的数据类型。</param>
        /// <returns>对应的 <see cref="IRecordWrapper"/> 所提供的方法。</returns>
        internal static MethodInfo GetGetValueMethod(Type dataType)
        {
            if (dataType == typeof(Guid))
            {
                return MethodCache.GetGuid;
            }

            switch (Type.GetTypeCode(dataType))
            {
                case TypeCode.SByte:
                case TypeCode.Byte: return MethodCache.GetByte;
                case TypeCode.Char: return MethodCache.GetChar;
                case TypeCode.Boolean: return MethodCache.GetBoolean;
                case TypeCode.Int16:
                case TypeCode.UInt16:
                    return MethodCache.GetInt16;
                case TypeCode.Int32:
                case TypeCode.UInt32:
                    return MethodCache.GetInt32;
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    return MethodCache.GetInt64;
                case TypeCode.String: return MethodCache.GetString;
                case TypeCode.DateTime: return MethodCache.GetDateTime;
                case TypeCode.Decimal: return MethodCache.GetDecimal;
                case TypeCode.Double: return MethodCache.GetDouble;
                case TypeCode.Single: return MethodCache.GetSingle;
                case TypeCode.Object: return MethodCache.GetBytes;
                default:
                    return null;
            }
        }

        internal static object GetValue(IRecordWrapper wrapper, IDataRecord reader, int index)
        {
            var method = GetGetValueMethod(reader.GetFieldType(index));
            if (method != null)
            {
                return method.FastInvoke(wrapper, new object[] { reader, index });
            }

            return null;
        }
    }
}
