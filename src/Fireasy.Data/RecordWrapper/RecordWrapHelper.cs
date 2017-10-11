// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using System;
using System.Data;
using System.Reflection;

namespace Fireasy.Data.RecordWrapper
{
    public static class RecordWrapHelper
    {
        public static MethodInfo GetMethodByOrdinal(DbType type)
        {
            var methodName = GetDbTypeName(type);
            var method = typeof(IRecordWrapper).GetMethod(methodName, new[] { typeof(IDataReader), typeof(int) });
            if (method == null)
            {
                throw new ArgumentNullException(type.ToString());
            }

            return method;
        }

        public static MethodInfo GetMethodByName(DbType type)
        {
            var methodName = GetDbTypeName(type);
            var method = typeof(IRecordWrapper).GetMethod(methodName, new[] { typeof(IDataReader), typeof(string) });
            if (method == null)
            {
                throw new ArgumentNullException(type.ToString());
            }

            return method;
        }

        private static string GetDbTypeName(DbType type)
        {
            switch (type)
            {
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.String:
                case DbType.StringFixedLength:
                    return "GetString";
                case DbType.Int16:
                case DbType.UInt16:
                    return "GetInt16";
                case DbType.Int32:
                case DbType.UInt32:
                    return "GetInt32";
                case DbType.Int64:
                case DbType.UInt64:
                    return "GetInt64";
                case DbType.Byte:
                case DbType.SByte:
                    return "GetByte";
                case DbType.Single:
                    return "GetFloat";
                case DbType.Decimal:
                    return "GetDecimal";
                case DbType.Double:
                    return "GetDouble";
                case DbType.Boolean:
                    return "GetBoolean";
                case DbType.Date:
                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.DateTimeOffset:
                    return "GetDateTime";
                case DbType.Binary:
                    return "GetBytes";
                default:
                    return string.Empty;
            }
        }
    }
}
