using Fireasy.Common.Extensions;
using Fireasy.Data.RecordWrapper;
using System;
using System.Data;
using System.Reflection;

namespace Fireasy.Data
{
    internal class RecordWrapHelper
    {
        #region IRecordWrapper的方法

        private static readonly MethodInfo GetCharMethod = typeof(IRecordWrapper).GetMethod(nameof(IRecordWrapper.GetChar), new[] { typeof(IDataRecord), typeof(int) });
        private static readonly MethodInfo GetByteMethod = typeof(IRecordWrapper).GetMethod(nameof(IRecordWrapper.GetByte), new[] { typeof(IDataRecord), typeof(int) });
        private static readonly MethodInfo GetBooleanMethod = typeof(IRecordWrapper).GetMethod(nameof(IRecordWrapper.GetBoolean), new[] { typeof(IDataRecord), typeof(int) });
        private static readonly MethodInfo GetInt16Method = typeof(IRecordWrapper).GetMethod(nameof(IRecordWrapper.GetInt16), new[] { typeof(IDataRecord), typeof(int) });
        private static readonly MethodInfo GetInt32Method = typeof(IRecordWrapper).GetMethod(nameof(IRecordWrapper.GetInt32), new[] { typeof(IDataRecord), typeof(int) });
        private static readonly MethodInfo GetInt64Method = typeof(IRecordWrapper).GetMethod(nameof(IRecordWrapper.GetInt64), new[] { typeof(IDataRecord), typeof(int) });
        private static readonly MethodInfo GetStringMethod = typeof(IRecordWrapper).GetMethod(nameof(IRecordWrapper.GetString), new[] { typeof(IDataRecord), typeof(int) });
        private static readonly MethodInfo GetDateTimeMethod = typeof(IRecordWrapper).GetMethod(nameof(IRecordWrapper.GetDateTime), new[] { typeof(IDataRecord), typeof(int) });
        private static readonly MethodInfo GetDecimalMethod = typeof(IRecordWrapper).GetMethod(nameof(IRecordWrapper.GetDecimal), new[] { typeof(IDataRecord), typeof(int) });
        private static readonly MethodInfo GetDoubleMethod = typeof(IRecordWrapper).GetMethod(nameof(IRecordWrapper.GetDouble), new[] { typeof(IDataRecord), typeof(int) });
        private static readonly MethodInfo GetSingleMethod = typeof(IRecordWrapper).GetMethod(nameof(IRecordWrapper.GetFloat), new[] { typeof(IDataRecord), typeof(int) });
        private static readonly MethodInfo GetGuidMethod = typeof(IRecordWrapper).GetMethod(nameof(IRecordWrapper.GetGuid), new[] { typeof(IDataRecord), typeof(int) });
        private static readonly MethodInfo GetBytesMethod = typeof(IRecordWrapper).GetMethod(nameof(IRecordWrapper.GetBytes), new[] { typeof(IDataRecord), typeof(int) });

        #endregion IRecordWrapper的方法

        /// <summary>
        /// 根据值的类型返回获取值的方法。
        /// </summary>
        /// <param name="dataType">值的数据类型。</param>
        /// <returns>对应的 <see cref="IRecordWrapper"/> 所提供的方法。</returns>
        internal static MethodInfo GetGetValueMethod(Type dataType)
        {
            if (dataType == typeof(Guid))
            {
                return GetGuidMethod;
            }

            switch (Type.GetTypeCode(dataType))
            {
                case TypeCode.SByte:
                case TypeCode.Byte: return GetByteMethod;
                case TypeCode.Char: return GetCharMethod;
                case TypeCode.Boolean: return GetBooleanMethod;
                case TypeCode.Int16:
                case TypeCode.UInt16:
                    return GetInt16Method;
                case TypeCode.Int32:
                case TypeCode.UInt32:
                    return GetInt32Method;
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    return GetInt64Method;
                case TypeCode.String: return GetStringMethod;
                case TypeCode.DateTime: return GetDateTimeMethod;
                case TypeCode.Decimal: return GetDecimalMethod;
                case TypeCode.Double: return GetDoubleMethod;
                case TypeCode.Single: return GetSingleMethod;
                case TypeCode.Object: return GetBytesMethod;
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
