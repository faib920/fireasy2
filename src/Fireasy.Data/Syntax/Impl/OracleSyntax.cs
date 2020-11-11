// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Data.Provider;
using System.Data;
using System.Linq;
using System.Text;

namespace Fireasy.Data.Syntax
{
    /// <summary>
    /// Oracle函数语法解析。
    /// </summary>
    public class OracleSyntax : ISyntaxProvider
    {
        IProvider IProviderService.Provider { get; set; }

        /// <summary>
        /// 获取字符串函数相关的语法。
        /// </summary>
        public virtual StringSyntax String => new OracleStringSyntax();

        /// <summary>
        /// 获取日期函数相关的语法。
        /// </summary>
        public virtual DateTimeSyntax DateTime => new OracleDateTimeSyntax();

        /// <summary>
        /// 获取数学函数相关的语法。
        /// </summary>
        public virtual MathSyntax Math => new OracleMathSyntax();

        /// <summary>
        /// 获取最近创建的自动编号的查询文本。
        /// </summary>
        public virtual string IdentitySelect => string.Empty;

        /// <summary>
        /// 获取自增长列的关键词。
        /// </summary>
        public string IdentityColumn => string.Empty;

        /// <summary>
        /// 获取受影响的行数的查询文本。
        /// </summary>
        public string RowsAffected => string.Empty;

        /// <summary>
        /// 获取伪查询的表名称。
        /// </summary>
        public string FakeSelect => " FROM DUAL";

        /// <summary>
        /// 获取存储参数的前缀。
        /// </summary>
        public virtual string ParameterPrefix => ":";

        /// <summary>
        /// 获取定界符。
        /// </summary>
        public string[] Delimiter => new[] { "\"", "\"" };

        /// <summary>
        /// 获取换行符。
        /// </summary>
        public string Linefeed => "\n;\n";

        /// <summary>
        /// 获取是否允许在聚合中使用 DISTINCT 关键字。
        /// </summary>
        public bool SupportDistinctInAggregates => true;

        /// <summary>
        /// 获取是否允许在没有 FORM 的语句中使用子查询。
        /// </summary>
        public bool SupportSubqueryInSelectWithoutFrom => true;

        /// <summary>
        /// 获取是否支持同时返回自增值。
        /// </summary>
        public bool SupportReturnIdentityValue => false;

        /// <summary>
        /// 对命令文本进行分段处理，使之能够返回小范围内的数据。
        /// </summary>
        /// <param name="context">命令上下文对象。</param>
        /// <exception cref="SegmentNotSupportedException">当前数据库或版本不支持分段时，引发该异常。</exception>
        public virtual bool Segment(CommandContext context)
        {
            context.Command.CommandText = Segment(context.Command.CommandText, context.Segment);
            return true;
        }

        /// <summary>
        /// 对命令文本进行分段处理，使之能够返回小范围内的数据。
        /// </summary>
        /// <param name="commandText">命令文本。</param>
        /// <param name="segment">数据分段对象。</param>
        /// <returns>处理后的分段命令文本。</returns>
        /// <exception cref="SegmentNotSupportedException">当前数据库或版本不支持分段时，引发该异常。</exception>
        public virtual string Segment(string commandText, IDataSegment segment)
        {
            //** rownnum <= n 放在内层能够提高10倍的速度!
            return @$"
                SELECT T.* FROM
                (
                    SELECT T.*, ROWNUM ROW_NUM
                    FROM ({commandText}) T {(segment.End != null ? $"WHERE ROWNUM <= {segment.End}" : string.Empty)}
                ) T {(segment.Start != null ? $"WHERE ROW_NUM >= {segment.Start}" : string.Empty)}";
        }

        /// <summary>
        /// 转换源表达式的数据类型。
        /// </summary>
        /// <param name="sourceExp">要转换的源表达式。</param>
        /// <param name="dbType">要转换的类型。</param>
        /// <returns></returns>
        public virtual string Convert(object sourceExp, DbType dbType)
        {
            switch (dbType)
            {
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.String:
                case DbType.StringFixedLength:
                    return $"TO_CHAR({sourceExp})";
                case DbType.Binary:
                    return $"CAST({sourceExp} AS BLOB)";
                case DbType.Boolean:
                    return $"CAST({sourceExp} AS NUMBER)";
                case DbType.Byte:
                    return $"CAST({sourceExp} AS NUMBER)";
                case DbType.Currency:
                    return $"CAST({sourceExp} AS NUMBER)";
                case DbType.Date:
                    return $"TO_DATE({sourceExp}, 'YYYY-MM-DD')";
                case DbType.Time:
                    return $"TO_DATE({sourceExp}, 'HH24:MI:SS')";
                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.DateTimeOffset:
                    return $"TO_DATE({sourceExp}, 'YYYY-MM-DD HH24:MI:SS')";
                case DbType.Decimal:
                    return $"CAST({sourceExp} AS NUMBER)";
                case DbType.Double:
                    return $"CAST({sourceExp} AS NUMBER)";
                case DbType.Guid:
                    break;
                case DbType.Int16:
                case DbType.Int32:
                case DbType.Int64:
                case DbType.UInt16:
                case DbType.UInt32:
                case DbType.UInt64:
                    return $"CAST({sourceExp} AS NUMBER)";
                case DbType.SByte:
                    return $"CAST({sourceExp} AS NUMBER)";
                case DbType.Single:
                    return $"CAST({sourceExp} AS FLOAT)";
                case DbType.VarNumeric:
                    break;
                case DbType.Xml:
                    break;
            }
            return ExceptionHelper.ThrowSyntaxConvertException(dbType);
        }

        /// <summary>
        /// 根据数据类型生成相应的列。
        /// </summary>
        /// <param name="dbType">数据类型。</param>
        /// <param name="length">数据长度。</param>
        /// <param name="precision">数值的精度。</param>
        /// <param name="scale">数值的小数位。</param>
        /// <returns></returns>
        public string Column(DbType dbType, int? length, int? precision, int? scale = new int?())
        {
            switch (dbType)
            {
                case DbType.AnsiString:
                    if (length == null)
                    {
                        return "VARCHAR2(255)";
                    }
                    if (length > 8000)
                    {
                        return "CLOB";
                    }
                    return $"VARCHAR2({length})";
                case DbType.AnsiStringFixedLength:
                    return length == null ? "NCHAR(255)" : $"NCHAR({length})";
                case DbType.Binary:
                    return "BLOB";
                case DbType.Boolean:
                    return "NUMBER(1, 0)";
                case DbType.Byte:
                    return "NUMBER(3, 0)";
                case DbType.Currency:
                    return "NUMBER(20, 0)";
                case DbType.Date:
                case DbType.DateTime:
                case DbType.DateTime2:
                    return "DATE";
                case DbType.DateTimeOffset:
                    return "TIMESTAMP(4)";
                case DbType.Decimal:
                    if (precision == null && scale == null)
                    {
                        return "NUMBER";
                    }
                    else if (precision == null)
                    {
                        return $"NUMBER(19, {scale})";
                    }
                    else if (scale == null)
                    {
                        return $"NUMBER({precision})";
                    }
                    return $"NUMBER({precision}, {scale})";
                case DbType.Double:
                    return "DOUBLE PRECISION";
                case DbType.Guid:
                    return "CHAR(38)";
                case DbType.Int16:
                    return "NUMBER(5, 0)";
                case DbType.Int32:
                    return "NUMBER(10, 0)";
                case DbType.Int64:
                    return "NUMBER(20, 0)";
                case DbType.SByte:
                    return "NUMBER(5, 0)";
                case DbType.Single:
                    return "FLOAT(24)";
                case DbType.String:
                    if (length == null)
                    {
                        return "NVARCHAR2(255)";
                    }
                    if (length > 4000)
                    {
                        return "NCLOB";
                    }
                    return $"NVARCHAR2({length})";
                case DbType.StringFixedLength:
                    if (length == null)
                    {
                        return "NCHAR(255)";
                    }
                    return $"NCHAR({length})";
                case DbType.Time:
                    return "TIMESTAMP(4)";
                case DbType.UInt16:
                    return "NUMBER(5, 0)";
                case DbType.UInt32:
                    return "NUMBER(10, 0)";
                case DbType.UInt64:
                    return "NUMBER(20, 0)";
                case DbType.VarNumeric:
                    break;
                case DbType.Xml:
                    break;
            }
            return ExceptionHelper.ThrowSyntaxCreteException(dbType);
        }

        /// <summary>
        /// 如果源表达式为 null，则依次判断给定的一组参数，直至某参数非 null 时返回。
        /// </summary>
        /// <param name="sourceExp">要转换的源表达式。</param>
        /// <param name="argExps">参与判断的一组参数。</param>
        /// <returns></returns>
        public virtual string Coalesce(object sourceExp, params object[] argExps)
        {
            if (argExps == null || argExps.Length == 0)
            {
                return sourceExp.ToString();
            }

            var sb = new StringBuilder();

            sb.AppendFormat("NVL({0}", sourceExp);

            for (int i = 0, n = argExps.Length - 1; i < n; i++)
            {
                sb.AppendFormat(", NVL({0}", argExps[i]);
            }

            sb.AppendFormat(", {0}", argExps[argExps.Length - 1]);

            for (int i = 0, n = argExps.Length - 1; i < n; i++)
            {
                sb.Append(")");
            }

            sb.Append(")");

            return sb.ToString();
        }

        /// <summary>
        /// 格式化参数名称。
        /// </summary>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public virtual string FormatParameter(string parameterName)
        {
            return string.Concat(ParameterPrefix, parameterName);
        }

        /// <summary>
        /// 获取判断表是否存在的语句。
        /// </summary>
        /// <param name="tableName">要判断的表的名称。</param>
        /// <returns></returns>
        public string ExistsTable(string tableName)
        {
            return $"SELECT COUNT(1) FROM USER_TABLES WHERE TABLE_NAME = '{tableName}'";
        }

        /// <summary>
        /// 获取判断多个表是否存在的语句。
        /// </summary>
        /// <param name="tableName">要判断的表的名称数组。</param>
        /// <returns></returns>
        public virtual string ExistsTables(string[] tableNames)
        {
            var names = string.Join(",", tableNames.Select(s => $"'{s}'"));
            return $"SELECT TABLE_NAME FROM USER_TABLES WHERE TABLE_NAME IN ({names})";
        }

        /// <summary>
        /// 给表名添加界定符。
        /// </summary>
        /// <param name="tableName"></param>
        public virtual string DelimitTable(string tableName)
        {
            return DbUtility.FormatByDelimiter(this, tableName);
        }

        /// <summary>
        /// 给列名添加界定符
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public virtual string DelimitColumn(string columnName)
        {
            return DbUtility.FormatByDelimiter(this, columnName);
        }

        /// <summary>
        /// 修正 <see cref="DbType"/> 值。
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public DbType CorrectDbType(DbType dbType)
        {
            return dbType == DbType.Boolean ? DbType.Decimal : dbType;
        }
    }
}