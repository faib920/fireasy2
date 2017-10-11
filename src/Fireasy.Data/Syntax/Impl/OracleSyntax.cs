// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Data;
using System.Text;

namespace Fireasy.Data.Syntax
{
    /// <summary>
    /// Oracle函数语法解析。
    /// </summary>
    public class OracleSyntax : ISyntaxProvider
    {
        private StringSyntax strSyntax;
        private DateTimeSyntax dtSyntax;
        private MathSyntax mathSyntax;

        /// <summary>
        /// 获取字符串函数相关的语法。
        /// </summary>
        public virtual StringSyntax String
        {
            get { return strSyntax ?? (strSyntax = new OracleStringSyntax()); }
        }

        /// <summary>
        /// 获取日期函数相关的语法。
        /// </summary>
        public virtual DateTimeSyntax DateTime
        {
            get { return dtSyntax ?? (dtSyntax = new OracleDateTimeSyntax()); }
        }

        /// <summary>
        /// 获取数学函数相关的语法。
        /// </summary>
        public virtual MathSyntax Math
        {
            get { return mathSyntax ?? (mathSyntax = new OracleMathSyntax()); }
        }

        /// <summary>
        /// 获取最近创建的自动编号的查询文本。
        /// </summary>
        public virtual string IdentitySelect
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// 获取自增长列的关键词。
        /// </summary>
        public string IdentityColumn
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// 获取受影响的行数的查询文本。
        /// </summary>
        public string RowsAffected
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// 获取伪查询的表名称。
        /// </summary>
        public string FakeSelect
        {
            get { return " FROM DUAL"; }
        }

        /// <summary>
        /// 获取存储参数的前缀。
        /// </summary>
        public virtual string ParameterPrefix
        {
            get { return ":"; }
        }

        /// <summary>
        /// 获取列引号标识符。
        /// </summary>
        public string[] Quote
        {
            get { return new[] { "\"", "\"" }; }
        }

        /// <summary>
        /// 获取换行符。
        /// </summary>
        public string Linefeed
        {
            get { return "\n;\n"; }
        }

        /// <summary>
        /// 获取是否允许在聚合中使用 DISTINCT 关键字。
        /// </summary>
        public bool SupportDistinctInAggregates
        {
            get { return true; }
        }

        /// <summary>
        /// 获取是否允许在没有 FORM 的语句中使用子查询。
        /// </summary>
        public bool SupportSubqueryInSelectWithoutFrom
        {
            get { return true; }
        }

        /// <summary>
        /// 对命令文本进行分段处理，使之能够返回小范围内的数据。
        /// </summary>
        /// <param name="context">命令上下文对象。</param>
        /// <returns>处理后的分段命令文本。</returns>
        /// <exception cref="SegmentNotSupportedException">当前数据库或版本不支持分段时，引发该异常。</exception>
        public virtual string Segment(CommandContext context)
        {
            return Segment(context.Command.CommandText, context.Segment);
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
            commandText = string.Format(@"
                SELECT T.* FROM
                (
                    SELECT T.*, ROWNUM ROW_NUM
                    FROM ({0}) T {1}
                ) T {2}", 
                    commandText, segment.End != null ? "WHERE ROWNUM <= " + segment.End : string.Empty,
                    segment.Start != null ? "WHERE ROW_NUM >= " + segment.Start: string.Empty);
            return commandText;
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
                    return string.Format("TO_CHAR({0})", sourceExp);
                case DbType.AnsiStringFixedLength:
                    return string.Format("TO_CHAR({0})", sourceExp);
                case DbType.Binary:
                    return string.Format("CAST({0} AS BLOB)", sourceExp);
                case DbType.Boolean:
                    return string.Format("CAST({0} AS NUMBER)", sourceExp);
                case DbType.Byte:
                    return string.Format("CAST({0} AS NUMBER)", sourceExp);
                case DbType.Currency:
                    return string.Format("CAST({0} AS NUMBER)", sourceExp);
                case DbType.Date:
                    return string.Format("TO_DATE({0}, 'YYYY-MM-DD')", sourceExp);
                case DbType.DateTime:
                    return string.Format("TO_DATE({0}, 'YYYY-MM-DD HH24:MI:SS')", sourceExp);
                case DbType.DateTime2:
                    return string.Format("TO_DATE({0}, 'YYYY-MM-DD HH24:MI:SS')", sourceExp);
                case DbType.DateTimeOffset:
                    return string.Format("TO_DATE({0}, 'YYYY-MM-DD HH24:MI:SS')", sourceExp);
                case DbType.Decimal:
                    return string.Format("CAST({0} AS NUMBER)", sourceExp);
                case DbType.Double:
                    return string.Format("CAST({0} AS NUMBER)", sourceExp);
                case DbType.Guid:
                    break;
                case DbType.Int16:
                    return string.Format("CAST({0} AS NUMBER)", sourceExp);
                case DbType.Int32:
                    return string.Format("CAST({0} AS NUMBER)", sourceExp);
                case DbType.Int64:
                    return string.Format("CAST({0} AS NUMBER)", sourceExp);
                case DbType.SByte:
                    return string.Format("CAST({0} AS NUMBER)", sourceExp);
                case DbType.Single:
                    return string.Format("CAST({0} AS FLOAT)", sourceExp);
                case DbType.String:
                    return string.Format("TO_CHAR({0})", sourceExp);
                case DbType.StringFixedLength:
                    return string.Format("TO_CHAR({0})", sourceExp);
                case DbType.Time:
                    return string.Format("TO_DATE({0}, 'HH24:MI:SS')", sourceExp);
                case DbType.UInt16:
                    return string.Format("CAST({0} AS NUMBER)", sourceExp);
                case DbType.UInt32:
                    return string.Format("CAST({0} AS NUMBER)", sourceExp);
                case DbType.UInt64:
                    return string.Format("CAST({0} AS NUMBER)", sourceExp);
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
                    return string.Format("VARCHAR2({0})", length);
                case DbType.AnsiStringFixedLength:
                    return length == null ? "NCHAR(255)" : string.Format("NCHAR({0})", length);
                case DbType.Binary:
                    return "BLOB";
                case DbType.Boolean:
                    return "NUMBER(1,0)";
                case DbType.Byte:
                    return "NUMBER(3,0)";
                case DbType.Currency:
                    return "NUMBER(20,0)";
                case DbType.Date:
                case DbType.DateTime:
                case DbType.DateTime2:
                    return "DATE";
                case DbType.DateTimeOffset:
                    return "TIMESTAMP(4)";
                case DbType.Decimal:
                    if (precision == null && scale == null)
                    {
                        return "NUMBER(19, 5)";
                    }
                    if (precision == null)
                    {
                        return string.Format("NUMBER(19, {0})", scale);
                    }
                    if (scale == null)
                    {
                        return string.Format("NUMBER({0}, 5)", precision);
                    }
                    return string.Format("NUMBER({0}, {1})", precision, scale);
                case DbType.Double:
                    return "DOUBLE PRECISION";
                case DbType.Guid:
                    return "CHAR(38)";
                case DbType.Int16:
                    return "NUMBER(5,0)";
                case DbType.Int32:
                    return "NUMBER(10,0)";
                case DbType.Int64:
                    return "NUMBER(20,0)";
                case DbType.SByte:
                    return "NUMBER(5,0)";
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
                    return string.Format("NVARCHAR2({0})", length);
                case DbType.StringFixedLength:
                    if (length == null)
                    {
                        return "NCHAR(255)";
                    }
                    return string.Format("NCHAR({0})", length);
                case DbType.Time:
                    return "TIMESTAMP(4)";
                case DbType.UInt16:
                    return "NUMBER(5,0)";
                case DbType.UInt32:
                    return "NUMBER(10,0)";
                case DbType.UInt64:
                    return "NUMBER(20,0)";
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
            for (var i = 0; i < argExps.Length - 1; i++)
            {
                sb.AppendFormat(", NVL({0}", argExps[i]);
            }
            sb.AppendFormat(", {0}", argExps[argExps.Length - 1]);
            for (var i = 0; i < argExps.Length - 1; i++)
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
            return string.Format("SELECT COUNT(1) FROM USER_TABLES WHERE TABLE_NAME = '{0}'", tableName);
        }

        /// <summary>
        /// 修正 <see cref="DbType"/> 值。
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public DbType CorrectDbType(DbType dbType)
        {
            if (dbType == DbType.Boolean)
            {
                return DbType.Decimal;
            }

            return dbType;
        }
    }
}