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
            return type switch
            {
                DbType.AnsiString or DbType.AnsiStringFixedLength or DbType.String or DbType.StringFixedLength => nameof(IRecordWrapper.GetString),
                DbType.Int16 or DbType.UInt16 => nameof(IRecordWrapper.GetInt16),
                DbType.Int32 or DbType.UInt32 => nameof(IRecordWrapper.GetInt32),
                DbType.Int64 or DbType.UInt64 => nameof(IRecordWrapper.GetInt64),
                DbType.Byte or DbType.SByte => nameof(IRecordWrapper.GetByte),
                DbType.Single => nameof(IRecordWrapper.GetFloat),
                DbType.Decimal => nameof(IRecordWrapper.GetDecimal),
                DbType.Double => nameof(IRecordWrapper.GetDouble),
                DbType.Boolean => nameof(IRecordWrapper.GetBoolean),
                DbType.Date or DbType.DateTime or DbType.DateTime2 or DbType.DateTimeOffset => nameof(IRecordWrapper.GetDateTime),
                DbType.Binary => nameof(IRecordWrapper.GetBytes),
                _ => string.Empty,
            };
        }
    }
}
