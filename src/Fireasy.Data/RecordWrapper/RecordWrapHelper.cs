// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Reflection;
using System;
using System.Data;
using System.Reflection;

namespace Fireasy.Data.RecordWrapper
{
    public static class RecordWrapHelper
    {
        /// <summary>
        /// 根据位置获取 <see cref="IRecordWrapper"/> 相对应的方法。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static MethodInfo GetMethodByOrdinal(DbType type)
        {
            var methodName = GetDbTypeName(type);
            if (string.IsNullOrEmpty(methodName))
            {
                throw new ArgumentNullException(SR.GetString(SRKind.NoMatchRecordWrapperMethod, type.ToString()));
            }

            return ReflectionCache.GetMember(methodName, typeof(int), methodName, (_, name) => typeof(IRecordWrapper).GetMethod(name, new[] { typeof(IDataReader), typeof(int) }));
        }

        /// <summary>
        /// 根据名称获取 <see cref="IRecordWrapper"/> 相对应的方法。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static MethodInfo GetMethodByName(DbType type)
        {
            var methodName = GetDbTypeName(type);
            if (string.IsNullOrEmpty(methodName))
            {
                throw new ArgumentNullException(SR.GetString(SRKind.NoMatchRecordWrapperMethod, type.ToString()));
            }

            return ReflectionCache.GetMember(methodName, typeof(string), methodName, (_, name) => typeof(IRecordWrapper).GetMethod(name, new[] { typeof(IDataReader), typeof(string) }));
        }

        private static string GetDbTypeName(DbType type)
        {
            switch (type)
            {
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.String:
                case DbType.StringFixedLength:
                    return nameof(IRecordWrapper.GetString);
                case DbType.Int16:
                case DbType.UInt16:
                    return nameof(IRecordWrapper.GetInt16);
                case DbType.Int32:
                case DbType.UInt32:
                    return nameof(IRecordWrapper.GetInt32);
                case DbType.Int64:
                case DbType.UInt64:
                    return nameof(IRecordWrapper.GetInt64);
                case DbType.Byte:
                case DbType.SByte:
                    return nameof(IRecordWrapper.GetByte);
                case DbType.Single:
                    return nameof(IRecordWrapper.GetFloat);
                case DbType.Decimal:
                    return nameof(IRecordWrapper.GetDecimal);
                case DbType.Double:
                    return nameof(IRecordWrapper.GetDouble);
                case DbType.Boolean:
                    return nameof(IRecordWrapper.GetBoolean);
                case DbType.Date:
                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.DateTimeOffset:
                    return nameof(IRecordWrapper.GetDateTime);
                case DbType.Binary:
                    return nameof(IRecordWrapper.GetBytes);
                default:
                    return string.Empty;
            }
        }
    }
}
