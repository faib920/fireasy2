// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Extensions;
using Fireasy.Data.Converter;
using Fireasy.Data.Provider;
using Fireasy.Data.Syntax;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Data.Extensions
{
    /// <summary>
    /// 数据相关的扩展方法。
    /// </summary>
    public static class DataExtension
    {
        /// <summary>
        /// 将数组或 <see cref="IEnumerable"/> 中的成员转换到 <see cref="DataTable"/> 对象。
        /// </summary>
        /// <param name="data"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static DataTable ToDataTable(this object data, string tableName = null)
        {
            if (data == null)
            {
                return null;
            }

            if (!(data is DataTable table))
            {
                if (data is IEnumerable)
                {
                    table = ParseFromEnumerable(data as IEnumerable);
                }
                else
                {
                    table = ParseFromEnumerable(new List<object> { data });
                }
            }

            if (table != null)
            {
                table.TableName = tableName;
            }

            return table;
        }

        /// <summary>
        /// 将 <see cref="DataTable"/> 对象输出为 Insert 语句表示。
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static IEnumerable<string> ToInsertSql(this DataTable table)
        {
            foreach (DataRow row in table.Rows)
            {
                var sb = new StringBuilder();
                sb.AppendFormat("INSERT INTO {0} VALUES(", table.TableName);
                var flag = new AssertFlag();
                foreach (DataColumn column in table.Columns)
                {
                    if (!flag.AssertTrue())
                    {
                        sb.Append(",");
                    }

                    if (row[column] == DBNull.Value)
                    {
                        sb.Append("NULL");
                    }
                    else if (column.DataType == typeof(string) || column.DataType == typeof(DateTime))
                    {
                        sb.AppendFormat("'{0}'", row[column]);
                    }
                    else if (column.DataType == typeof(bool))
                    {
                        sb.Append(row[column].To<int>());
                    }
                    else
                    {
                        sb.Append(row[column]);
                    }
                }

                sb.Append(")");
                yield return sb.ToString();
            }
        }

        /// <summary>
        /// 将 <see cref="DataTable"/> 中的数据转换为数组。
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static object[][] ToArray(this DataTable table)
        {
            var rows = table.Rows.Count;
            var cols = table.Columns.Count;
            var data = new object[cols][];

            for (var i = 0; i < cols; i++)
            {
                data[i] = new object[rows];
            }

            for (var i = 0; i < rows; i++)
            {
                for (var j = 0; j < cols; j++)
                {
                    data[j][i] = table.Rows[i][j];
                }
            }

            return data;
        }

        /// <summary>
        /// 判断 <see cref="DataSet"/> 是否为 null 或没有任何的 <see cref="DataTable"/> 成员。
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this DataSet ds)
        {
            return ds == null || ds.Tables.Count == 0;
        }

        /// <summary>
        /// 判断 <see cref="DataTable"/> 是否为 null 或没有任何的 <see cref="DataRow"/> 成员。
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this DataTable table)
        {
            return table == null || table.Rows.Count == 0;
        }

        /// <summary>
        /// 循环取 <see cref="DataTable"/> 中的 <see cref="DataColumn"/> 集合。
        /// </summary>
        /// <param name="table"></param>
        /// <param name="action"></param>
        /// <param name="predicate"></param>
        public static void EachColumns(this DataTable table, Action<DataColumn, int> action, Func<DataColumn, bool> predicate = null)
        {
            if (table.IsNullOrEmpty() || action == null)
            {
                return;
            }

            var count = table.Columns.Count;
            for (var i = 0; i < count; i++)
            {
                var column = table.Columns[i];
                if (predicate != null && !predicate(column))
                {
                    continue;
                }

                action(column, i);
            }
        }

        /// <summary>
        /// 循环取 <see cref="DataTable"/> 中的 <see cref="DataColumn"/> 集合。
        /// </summary>
        /// <param name="table"></param>
        /// <param name="action"></param>
        /// <param name="predicate"></param>
        public static void EachColumns(this DataTable table, Action<DataColumn> action, Func<DataColumn, bool> predicate = null)
        {
            if (table.IsNullOrEmpty() || action == null)
            {
                return;
            }

            var count = table.Columns.Count;
            for (var i = 0; i < count; i++)
            {
                var column = table.Columns[i];
                if (predicate != null && !predicate(column))
                {
                    continue;
                }

                action(column);
            }
        }

        /// <summary>
        /// 循环取 <see cref="DataTable"/> 中的 <see cref="DataRow"/> 集合。
        /// </summary>
        /// <param name="table"></param>
        /// <param name="action"></param>
        /// <param name="predicate"></param>
        public static void EachRows(this DataTable table, Action<DataRow, int> action, Func<DataRow, bool> predicate = null)
        {
            if (table.IsNullOrEmpty() || action == null)
            {
                return;
            }

            var count = table.Rows.Count;
            for (var i = 0; i < count; i++)
            {
                var row = table.Rows[i];
                if (predicate != null && !predicate(row))
                {
                    continue;
                }

                action(row, i);
            }
        }

        /// <summary>
        /// 循环取 <see cref="DataTable"/> 中的 <see cref="DataRow"/> 集合。
        /// </summary>
        /// <param name="table"></param>
        /// <param name="action"></param>
        /// <param name="predicate"></param>
        public static void EachRows(this DataTable table, Action<DataRow> action, Func<DataRow, bool> predicate = null)
        {
            if (table.IsNullOrEmpty() || action == null)
            {
                return;
            }

            var count = table.Rows.Count;
            for (var i = 0; i < count; i++)
            {
                var row = table.Rows[i];
                if (predicate != null && !predicate(row))
                {
                    continue;
                }

                action(row);
            }
        }
        /// <summary>
        /// 判断参数是否为输入型参数。
        /// </summary>
        /// <param name="parameter">存储参数。</param>
        /// <returns></returns>
        public static bool IsInput(this Parameter parameter)
        {
            return parameter.Direction == ParameterDirection.Input ||
                parameter.Direction == ParameterDirection.InputOutput;
        }

        /// <summary>
        /// 判断参数是否为输出型参数。
        /// </summary>
        /// <param name="parameter">存储参数。</param>
        /// <returns></returns>
        public static bool IsOutput(this Parameter parameter)
        {
            return parameter.Direction != ParameterDirection.Input;
        }

        /// <summary>
        /// 获取类型所对应的DbType类型。
        /// </summary>
        /// <param name="type">源类型。</param>
        /// <returns></returns>
        public static DbType GetDbType(this Type type)
        {
            Guard.ArgumentNull(type, nameof(type));
            if (type.IsNullableType())
            {
                var baseType = type.GetGenericArguments()[0];
                return GetDbType(baseType);
            }

            if (type.IsArray)
            {
                return DbType.Binary;
            }

            return type.IsEnum ? DbType.Int32 : GetGenericDbType(type);
        }

        /// <summary>
        /// 从 <see cref="DbType"/> 转换到类型。
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public static Type FromDbType(this DbType dbType)
        {
            switch (dbType)
            {
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.String:
                case DbType.StringFixedLength:
                    return typeof(string);
                case DbType.Byte:
                    return typeof(byte);
                case DbType.SByte:
                    return typeof(sbyte);
                case DbType.Boolean:
                    return typeof(bool);
                case DbType.Date:
                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.DateTimeOffset:
                case DbType.Time:
                    return typeof(DateTime);
                case DbType.Decimal:
                    return typeof(decimal);
                case DbType.Single:
                    return typeof(float);
                case DbType.Double:
                    return typeof(double);
                case DbType.Int16:
                    return typeof(short);
                case DbType.Int32:
                    return typeof(int);
                case DbType.Int64:
                    return typeof(long);
                case DbType.UInt16:
                    return typeof(ushort);
                case DbType.UInt32:
                    return typeof(uint);
                case DbType.UInt64:
                    return typeof(ulong);
                case DbType.Guid:
                    return typeof(Guid);
                case DbType.Binary:
                    return typeof(byte[]);
                default:
                    return typeof(object);
            }
        }

        /// <summary>
        /// 获取数据类型的大小。
        /// </summary>
        /// <param name="type">源类型。</param>
        /// <param name="defaultSize">默认大小。</param>
        /// <returns></returns>
        public static int GetDbTypeSize(this Type type, int? defaultSize)
        {
            Guard.ArgumentNull(type, nameof(type));
            if (type == typeof(string))
            {
                if (defaultSize != null)
                {
                    return (int)defaultSize;
                }

                return 0;
            }

            if (type == typeof(DateTime))
            {
                return 8;
            }

            return System.Runtime.InteropServices.Marshal.SizeOf(type);
        }

        /// <summary>
        /// 输出 <see cref="DbCommand"/> 的命令文本及参数。
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static string Output(this IDbCommand command)
        {
            var sb = new StringBuilder();
            if (command.Parameters.Count > 0)
            {
                sb.AppendFormat("'{0}'", command.CommandText);
                foreach (DbParameter par in command.Parameters)
                {
                    //字符或日期型，加'
                    if (par.Value is string || par.Value is DateTime || par.Value is char)
                    {
                        sb.AppendFormat(",{0}='{1}'", par.ParameterName, TrimStringValue(par.Value));
                    }

                    //字节数组，转换为字符串
                    else if (par.Value is byte[])
                    {
                        sb.AppendFormat(",{0}='{1}'", par.ParameterName, TrimByteValue(par.Value as byte[]));
                    }
                    else
                    {
                        sb.AppendFormat(",{0}={1}", par.ParameterName, par.Value);
                    }
                }
            }
            else
            {
                sb.Append(command.CommandText);
            }

            return sb.ToString();
        }

        /// <summary>
        /// 清理IDbCommand中的参数。
        /// </summary>
        /// <param name="command"></param>
        public static void ClearParameters(this IDbCommand command)
        {
            if (command.Parameters.Count != 0)
            {
                command.Parameters.Clear();
            }
        }

        /// <summary>
        /// 同步IDbCommand中的输出型参数。
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameters"></param>
        public static void SyncParameters(this IDbCommand command, IEnumerable<Parameter> parameters)
        {
            if (command.Parameters.Count == 0 ||
                parameters == null)
            {
                return;
            }

            foreach (var paramter in parameters)
            {
                if (!paramter.IsOutput())
                {
                    continue;
                }

                var par = command.Parameters[paramter.ParameterName];
                if (par != null)
                {
                    paramter.Value = ((DbParameter)par).Value;
                }
            }
        }

        /// <summary>
        /// 将参数添加到DbCommand。
        /// </summary>
        /// <param name="command"></param>
        /// <param name="provider"></param>
        /// <param name="parameters"></param>
        public static void PrepareParameters(this DbCommand command, IProvider provider, IEnumerable<Parameter> parameters)
        {
            var syntax = provider.GetService<ISyntaxProvider>();

            foreach (var parameter in parameters)
            {
                if (command.CommandType == CommandType.Text && parameter.Value is IEnumerable && !(parameter.Value is string) && !(parameter.Value is byte[]))
                {
                    var parmeterName = parameter.ParameterName[0] == syntax.ParameterPrefix[0] ? parameter.ParameterName : syntax.ParameterPrefix + parameter.ParameterName;

                    var sb = new StringBuilder();
                    var index = 0;
                    foreach (var value in parameter.Value as IEnumerable)
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append(", ");
                        }

                        var name = string.Concat(parmeterName, "_auto_", index++);
                        var newPar = new Parameter(name, value);
                        sb.Append(name);
                        command.Parameters.Add(provider.PrepareParameter(newPar.ToDbParameter(provider)));
                    }

                    command.CommandText = command.CommandText.Replace(parmeterName, sb.ToString());
                }
                else
                {
                    command.Parameters.Add(provider.PrepareParameter(parameter.ToDbParameter(provider)));
                }
            }
        }

        /// <summary>
        /// 将自定的Parameter转换到IDataParameter接口
        /// </summary>
        /// <param name="sourcePar"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static DbParameter ToDbParameter(this Parameter sourcePar, IProvider provider)
        {
            Guard.ArgumentNull(sourcePar, nameof(sourcePar));

            var parameter = provider.DbProviderFactory.CreateParameter();
            Guard.NullReference(parameter);

            parameter.ParameterName = sourcePar.ParameterName;
            parameter.SourceColumn = sourcePar.SourceColumn;
            parameter.Direction = sourcePar.Direction;
            parameter.DbType = sourcePar.DbType;

            //处理空值
            if (sourcePar.IsInput() &&
                sourcePar.Value == null)
            {
                parameter.Value = DBNull.Value;
            }
            else
            {
                parameter.Value = sourcePar.Value;
            }

            var dbParameter = parameter as IDbDataParameter;
            if (sourcePar.Size != 0)
            {
                dbParameter.Size = sourcePar.Size;
            }

            if (sourcePar.Precision != 0)
            {
                dbParameter.Precision = sourcePar.Precision;
            }

            if (sourcePar.Scale != 0)
            {
                dbParameter.Scale = sourcePar.Scale;
            }

            if (!string.IsNullOrEmpty(sourcePar.SourceColumn))
            {
                dbParameter.SourceColumn = sourcePar.SourceColumn;
            }

            dbParameter.SourceVersion = sourcePar.SourceVersion;

            return parameter;
        }

        /// <summary>
        /// 添加一个作为查询的参数。
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Parameter AddQuerable(this ParameterCollection parameters, string parameterName, object value)
        {
            var parameter = new QueryableParameter(parameterName) { Value = value };
            parameters.Add(parameter);
            return parameter;
        }

        /// <summary>
        /// 格式化插入语句。
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public static string FormatInsert(this ParameterCollection parameters, string commandText)
        {
            var arr1 = new List<string>();
            var arr2 = new List<string>();
            foreach (var par in parameters)
            {
                if (par is QueryableParameter)
                {
                    continue;
                }

                arr1.Add(par.SourceColumn ?? par.ParameterName);
                arr2.Add("@" + par.ParameterName);
            }

            var propertyNames = arr1.ToArray();
            var values = arr2.ToArray();
            return string.Format(commandText, string.Join(",", propertyNames), string.Join(",", values));
        }

        /// <summary>
        /// 格式化更新语句。
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public static string FormatUpdate(this ParameterCollection parameters, string commandText)
        {
            var arr2 = new List<string>();
            foreach (var par in parameters)
            {
                if (par is QueryableParameter)
                {
                    continue;
                }

                arr2.Add((par.SourceColumn ?? par.ParameterName) + " = @" + par.ParameterName);
            }

            var values = arr2.ToArray();
            return string.Format(commandText, string.Join(",", values));
        }

        /// <summary>
        /// 获取是否支持数据类型。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsDbTypeSupported(this Type type)
        {
            Guard.ArgumentNull(type, nameof(type));
            type = type.GetNonNullableType();
            var typeCode = Type.GetTypeCode(type);
            if (ConvertManager.CanConvert(type))
            {
                return true;
            }

            return typeCode != TypeCode.Object &&
                typeCode != TypeCode.Empty &&
                typeCode != TypeCode.DBNull;
        }

        /// <summary>
        /// 判断 <paramref name="dbType"/> 是否为字符串类型。
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public static bool IsStringDbType(this DbType dbType)
        {
            return dbType == DbType.String ||
                dbType == DbType.StringFixedLength ||
                dbType == DbType.AnsiString ||
                dbType == DbType.AnsiStringFixedLength;
        }

        /// <summary>
        /// 获取 SQL 中的主要表名。
        /// </summary>
        /// <param name="queryCommand"></param>
        /// <param name="syntax"></param>
        /// <param name="operateMarks">操作符。</param>
        /// <returns></returns>
        public static string GetMainTableName(this IQueryCommand queryCommand, ISyntaxProvider syntax, string[] operateMarks = null)
        {
            if (queryCommand is SpecialCommand || queryCommand is ProcedureCommand)
            {
                return null;
            }

            var commandText = queryCommand.ToString();

            string[] tableMarkers = { "FROM", "INTO", "UPDATE" };

            var sqlItems = commandText.Split(new[] { " ", "\r\n", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            var sqlItemsLength = sqlItems.Length;
            if (operateMarks != null)
            {
                var checkOperate = false;
                for (var i = 0; i < sqlItemsLength; i++)
                {
                    foreach (var marker in operateMarks)
                    {
                        if (sqlItems[i].Equals(marker, StringComparison.OrdinalIgnoreCase))
                        {
                            checkOperate = true;
                            continue;
                        }
                    }
                }

                if (!checkOperate)
                {
                    return string.Empty;
                }
            }

            for (var i = 0; i < sqlItemsLength; i++)
            {
                foreach (var marker in tableMarkers)
                {
                    if (!sqlItems[i].Equals(marker, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    ++i;
                    if (i >= sqlItemsLength)
                    {
                        continue;
                    }

                    var tableName = string.Empty;

                    var tableNameParts = sqlItems[i].Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                    if (tableNameParts.Length == 1)
                    {
                        tableName = tableNameParts[0].Trim();
                    }
                    else if (tableNameParts.Length >= 2)
                    {
                        tableName = tableNameParts[1].Trim();
                    }

                    if (string.IsNullOrWhiteSpace(tableName) || tableName.Trim().StartsWith("("))
                    {
                        continue;
                    }

                    tableName = tableName.Replace(syntax.Delimiter[0], string.Empty)
                                        .Replace(syntax.Delimiter[1], string.Empty);

                    return tableName;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 获取 SQL 中的所有表名。
        /// </summary>
        /// <param name="queryCommand"></param>
        /// <param name="syntax"></param>
        /// <returns></returns>
        public static List<string> GetTableNames(this IQueryCommand queryCommand, ISyntaxProvider syntax)
        {
            if (queryCommand is SpecialCommand || queryCommand is ProcedureCommand)
            {
                return null;
            }

            var commandText = queryCommand.ToString();

            string[] tableMarkers = { "FROM", "INTO", "UPDATE", "JOIN" };

            var tables = new List<string>();

            var sqlItems = commandText.Split(new[] { " ", "\r\n", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            var sqlItemsLength = sqlItems.Length;
            for (var i = 0; i < sqlItemsLength; i++)
            {
                foreach (var marker in tableMarkers)
                {
                    if (!sqlItems[i].Equals(marker, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    ++i;
                    if (i >= sqlItemsLength)
                    {
                        continue;
                    }

                    var tableName = string.Empty;

                    var tableNameParts = sqlItems[i].Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                    if (tableNameParts.Length == 1)
                    {
                        tableName = tableNameParts[0].Trim();
                    }
                    else if (tableNameParts.Length >= 2)
                    {
                        tableName = tableNameParts[1].Trim();
                    }

                    if (string.IsNullOrWhiteSpace(tableName) || tableName.Trim().StartsWith("("))
                    {
                        continue;
                    }

                    tableName = tableName.Replace(syntax.Delimiter[0], string.Empty)
                                        .Replace(syntax.Delimiter[1], string.Empty);

                    tables.Add(tableName);
                }
            }

            return tables;
        }

        internal static TC ToTypeEx<TS, TC>(this TS value)
        {
            var converter = ConvertManager.GetConverter(typeof(TC));
            if (converter != null)
            {
                return (TC)converter.ConvertFrom(value, typeof(TC).GetDbType());
            }

            return value.To<TS, TC>();
        }

        /// <summary>
        /// 构造 <see cref="IDataSegment"/> 的分页条件。
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="fieldName">分页列名称。</param>
        /// <returns></returns>
        internal static string Condition(this IDataSegment segment, string fieldName)
        {
            if (segment.Start != null && segment.End != null)
            {
                return string.Format("{0} BETWEEN {1} AND {2}", fieldName, segment.Start, segment.End);
            }

            if (segment.Start != null && segment.End == null)
            {
                return string.Format("{0} >= {1}", fieldName, segment.Start);
            }

            if (segment.Start == null && segment.End != null)
            {
                return string.Format("{0} <= {1}", fieldName, segment.End);
            }

            return "1 = 1";
        }

        internal static DbConnection TryOpen(this DbConnection connection)
        {
            if (connection.State == ConnectionState.Closed || connection.State == ConnectionState.Broken)
            {
                try
                {
                    if (connection.State == ConnectionState.Broken)
                    {
                        connection.Close();
                    }

                    connection.Open();
                }
                catch (DbException exp)
                {
                    Tracer.Error($"Opening Connection of '{connection.ConnectionString}' throw exception:{exp.Output()}");
                    throw ConnectionException.Throw(connection, ConnectionState.Open, exp);
                }
            }

            return connection;
        }

        internal static async Task<DbConnection> TryOpenAsync(this DbConnection connection, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (connection.State == ConnectionState.Closed || connection.State == ConnectionState.Broken)
            {
                try
                {
                    if (connection.State == ConnectionState.Broken)
                    {
                        connection.Close();
                    }

                    await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (DbException exp)
                {
                    Tracer.Error($"Opening Connection of '{connection.ConnectionString}' throw exception:{exp.Output()}");
                    throw ConnectionException.Throw(connection, ConnectionState.Open, exp);
                }
            }

            return connection;
        }

        internal static DbConnection TryClose(this DbConnection connection, bool allowClose = true)
        {
            if (!allowClose)
            {
                return connection;
            }

            if (connection.State == ConnectionState.Open || connection.State == ConnectionState.Broken)
            {
                try
                {
                    connection.Close();
                }
                catch (DbException exp)
                {
                    Tracer.Error($"Closing Connection of '{connection.ConnectionString}' throw exception:{exp.Output()}");
                    throw ConnectionException.Throw(connection, ConnectionState.Open, exp);
                }
            }

            return connection;
        }

        private static string TrimStringValue(object value)
        {
            if (value == null)
            {
                return null;
            }

            var str = value.ToString();

            if (str.Length > 128)
            {
                return str.Left(128) + $"...<<{str.Length}>>";
            }

            return str;
        }

        private static string TrimByteValue(byte[] bytes)
        {
            if (bytes == null)
            {
                return null;
            }

            if (bytes.Length > 128)
            {
                return Encoding.UTF8.GetString(bytes.Take(128).ToArray()) + $"...<<{bytes.Length}>>";
            }

            return Encoding.UTF8.GetString(bytes);
        }

        private static DbType GetGenericDbType(Type type)
        {
            var typeCode = Type.GetTypeCode(type);

            switch (typeCode)
            {
                case TypeCode.String:
                    return DbType.String;
                case TypeCode.Boolean:
                    return DbType.Boolean;
                case TypeCode.Int16:
                    return DbType.Int16;
                case TypeCode.Int32:
                    return DbType.Int32;
                case TypeCode.Int64:
                    return DbType.Int64;
                case TypeCode.UInt16:
                    return DbType.UInt16;
                case TypeCode.UInt32:
                    return DbType.UInt32;
                case TypeCode.UInt64:
                    return DbType.UInt64;
                case TypeCode.DateTime:
                    return DbType.DateTime;
                case TypeCode.Decimal:
                    return DbType.Decimal;
                case TypeCode.Double:
                    return DbType.Double;
                case TypeCode.Byte:
                    return DbType.Byte;
                case TypeCode.SByte:
                    return DbType.SByte;
                case TypeCode.Char:
                    return DbType.Byte;
                case TypeCode.Single:
                    return DbType.Single;
                default:
                    switch (type.FullName)
                    {
                        case "System.Drawing.Color":
                        case "System.Drawing.Point":
                        case "System.Drawing.Size":
                        case "System.Drawing.Rectangle":
                        case "System.Drawing.Font":
                            return DbType.String;
                        case "System.Drawing.Image":
                        case "System.Drawing.Bitmap":
                            return DbType.Binary;
                        case "System.Guid":
                            return DbType.Guid;
                    }

                    if (type == typeof(byte[]))
                    {
                        return DbType.Binary;
                    }

                    return DbType.Object;
            }
        }

        private class QueryableParameter : Parameter
        {
            public QueryableParameter(string parameterName)
                : base(parameterName)
            {
            }
        }

        private static DataTable ParseFromEnumerable(IEnumerable enumerable)
        {
            TypeDescriptorUtility.AddDefaultDynamicProvider();

            var enumerator = enumerable.GetEnumerator();
            var table = new DataTable();
            var flag = new AssertFlag();
            PropertyDescriptorCollection properties = null;
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                if (flag.AssertTrue())
                {
                    properties = TypeDescriptor.GetProperties(current);
                    foreach (PropertyDescriptor pro in properties)
                    {
                        table.Columns.Add(pro.Name, pro.PropertyType.GetNonNullableType());
                    }
                }

                var data = new object[properties.Count];
                for (int i = 0, n = data.Length; i < n; i++)
                {
                    data[i] = properties[i].GetValue(current) ?? DBNull.Value;
                }

                table.Rows.Add(data);
            }

            return table;
        }
    }
}
