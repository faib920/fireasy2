// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using Fireasy.Data.Extensions;
using Fireasy.Data.Provider;

namespace Fireasy.Data.Syntax
{
    /// <summary>
    /// MsSql函数语法解析。
    /// </summary>
    public class MsSqlSyntax : ISyntaxProvider
    {
        private StringSyntax strSyntax;
        private DateTimeSyntax dtSyntax;
        private MathSyntax mathSyntax;

        IProvider IProviderService.Provider { get; set; }

        /// <summary>
        /// 获取字符串函数相关的语法。
        /// </summary>
        public virtual StringSyntax String
        {
            get { return strSyntax ?? (strSyntax = new MsSqlStringSyntax()); }
        }

        /// <summary>
        /// 获取日期函数相关的语法。
        /// </summary>
        public virtual DateTimeSyntax DateTime
        {
            get { return dtSyntax ?? (dtSyntax = new MsSqlDateTimeSyntax()); }
        }

        /// <summary>
        /// 获取数学函数相关的语法。
        /// </summary>
        public virtual MathSyntax Math
        {
            get { return mathSyntax ?? (mathSyntax = new MsSqlMathSyntax()); }
        }

        /// <summary>
        /// 获取最近创建的自动编号的查询文本。
        /// </summary>
        public virtual string IdentitySelect
        {
            get { return "SELECT SCOPE_IDENTITY()"; }
        }

        /// <summary>
        /// 获取自增长列的关键词。
        /// </summary>
        public string IdentityColumn
        {
            get { return "IDENTITY(1, 1)"; }
        }

        /// <summary>
        /// 获取受影响的行数的查询文本。
        /// </summary>
        public string RowsAffected
        {
            get { return "@@ROWCOUNT"; }
        }

        /// <summary>
        /// 获取伪查询的表名称。
        /// </summary>
        public string FakeSelect
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// 获取存储参数的前缀。
        /// </summary>
        public virtual string ParameterPrefix
        {
            get { return "@"; }
        }

        /// <summary>
        /// 获取列引号标识符。
        /// </summary>
        public string[] Quote
        {
            get { return new [] { "[", "]" }; }
        }

        /// <summary>
        /// 获取换行符。
        /// </summary>
        public string Linefeed
        {
            get { return "\nGO\n"; }
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
            //取版本号，sql server 2005(9.0)以下不支持分页
            var version = GetServerVersion(context.Database);
            if (version < 9)
            {
                throw new SegmentNotSupportedException();
            }
            else if (version >= 11)
            {
                return SegmentWith2012(context.Command.CommandText, context.Segment);
            }

            return Segment(context.Command.CommandText, context.Segment);
        }

        /// <summary>
        /// 2012以上版本支持的新分页语法。
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="segment"></param>
        /// <returns></returns>
        private string SegmentWith2012(string commandText, IDataSegment segment)
        {
            var orderBy = DbUtility.FindOrderBy(commandText);

            commandText = string.Format(@"{0}
{1}{2} FETCH NEXT {3} ROWS ONLY",
                commandText,
                string.IsNullOrEmpty(orderBy) ? "ORDER BY 1" : string.Empty,
                segment.Start != null ? string.Format(" OFFSET {0} ROW", (segment.Start - 1)) : string.Empty,
                segment.Length != 0 ? segment.Length : 1000);
            return commandText;
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
            var orderBy = DbUtility.FindOrderBy(commandText);
            var regAlias = new Regex(@"(\w+)?\.");
            //如果有排序
            if (!string.IsNullOrEmpty(orderBy))
            {
                //去除子句中的Order并移到OVER后
                commandText = string.Format(@"
                    SELECT T.* FROM 
                    (
                        SELECT T.*, ROW_NUMBER() OVER ({2}) AS ROW_NUM 
                        FROM ({0}) T
                    ) T 
                    WHERE {1}",
                    commandText.Replace(orderBy, string.Empty).Trim(),
                    segment.Condition("ROW_NUM"),
                    regAlias.Replace(orderBy, string.Empty));
            }
            else
            {
                commandText = string.Format(@"
                    SELECT T.* FROM 
                    (
                        SELECT T.*, ROW_NUMBER() OVER (ORDER BY (SELECT 1)) AS ROW_NUM 
                        FROM ({0}) T
                    ) T 
                    WHERE {1}",
                    commandText, segment.Condition("ROW_NUM"));
            }
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
                    return string.Format("CAST({0} AS VARCHAR)", sourceExp);
                case DbType.AnsiStringFixedLength:
                    return string.Format("CAST({0} AS CHAR)", sourceExp);
                case DbType.Binary:
                    return string.Format("CAST({0} AS VARBINARY)", sourceExp);
                case DbType.Boolean:
                    return string.Format("CAST({0} AS BIT)", sourceExp);
                case DbType.Byte:
                    return string.Format("CAST({0} AS TINYINT)", sourceExp);
                case DbType.Currency:
                    return string.Format("CAST({0} AS MONEY)", sourceExp);
                case DbType.Date:
                    return string.Format("CAST({0} AS DATETIME)", sourceExp);
                case DbType.DateTime:
                    return string.Format("CAST({0} AS DATETIME)", sourceExp);
                case DbType.DateTime2:
                    return string.Format("CAST({0} AS DATETIME2)", sourceExp);
                case DbType.DateTimeOffset:
                    return string.Format("CAST({0} AS DATETIMEOFFSET)", sourceExp);
                case DbType.Decimal:
                    return string.Format("CAST({0} AS DECIMAL)", sourceExp);
                case DbType.Double:
                    return string.Format("CAST({0} AS DOUBLE PRECISION)", sourceExp);
                case DbType.Guid:
                    return string.Format("CAST({0} AS UNIQUEIDENTIFIER)", sourceExp);
                case DbType.Int16:
                    return string.Format("CAST({0} AS SMALLINT)", sourceExp);
                case DbType.Int32:
                    return string.Format("CAST({0} AS INT)", sourceExp);
                case DbType.Int64:
                    return string.Format("CAST({0} AS BIGINT)", sourceExp);
                case DbType.SByte:
                    break;
                case DbType.Single:
                    return string.Format("CAST({0} AS REAL)", sourceExp);
                case DbType.String:
                    return string.Format("CAST({0} AS NVARCHAR)", sourceExp);
                case DbType.StringFixedLength:
                    return string.Format("CAST({0} AS NCHAR)", sourceExp);
                case DbType.Time:
                    break;
                case DbType.UInt16:
                    break;
                case DbType.UInt32:
                    break;
                case DbType.UInt64:
                    break;
                case DbType.VarNumeric:
                    break;
                case DbType.Xml:
                    return string.Format("CAST({0} AS XML)", sourceExp);
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
                        return "VARCHAR(255)";
                    }
                    if (length > 8000)
                    {
                        return "NTEXT";
                    }
                    return string.Format("VARCHAR({0})", length);
                case DbType.AnsiStringFixedLength:
                    return length == null ? "CHAR(255)" : string.Format("CHAR({0})", length);
                case DbType.Binary:
                    if (length == null)
                    {
                        return "VARBINARY(8000)";
                    }
                    if (length > 8000)
                    {
                        return "IMAGE";
                    }
                    return string.Format("VARBINARY({0})", length);
                case DbType.Boolean:
                    return "BIT";
                case DbType.Byte:
                    return "TINYINT";
                case DbType.Currency:
                    return "MONEY";
                case DbType.Date:
                    return "DATETIME";
                case DbType.DateTime:
                    return "DATETIME";
                case DbType.DateTime2:
                    return "DATETIME2";
                case DbType.DateTimeOffset:
                    return "DATETIMEOFFSET";
                case DbType.Decimal:
                    if (precision == null && scale == null)
                    {
                        return "DECIMAL(19, 5)";
                    }
                    if (precision == null)
                    {
                        return string.Format("DECIMAL(19, {0})", scale);
                    }
                    if (scale == null)
                    {
                        return string.Format("DECIMAL({0}, 5)", precision);
                    }
                    return string.Format("DECIMAL({0}, {1})", precision, scale);
                case DbType.Double:
                    return "DOUBLE PRECISION";
                case DbType.Guid:
                    return "UNIQUEIDENTIFIER";
                case DbType.Int16:
                    return "SMALLINT";
                case DbType.Int32:
                    return "INT";
                case DbType.Int64:
                    return "BIGINT";
                case DbType.SByte:
                    return "TINYINT";
                case DbType.Single:
                    return "REAL";
                case DbType.String:
                    if (length == null)
                    {
                        return "VARCHAR(255)";
                    }
                    if (length > 8000)
                    {
                        return "NTEXT";
                    }
                    return string.Format("VARCHAR({0})", length);
                case DbType.StringFixedLength:
                    if (length == null)
                    {
                        return "NCHAR(255)";
                    }
                    return string.Format("NCHAR({0})", length);
                case DbType.Time:
                    return "DATETIME";
                case DbType.UInt16:
                    break;
                case DbType.UInt32:
                    break;
                case DbType.UInt64:
                    break;
                case DbType.VarNumeric:
                    break;
                case DbType.Xml:
                    return "XML";
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
            sb.AppendFormat("COALESCE({0}", sourceExp);
            foreach (var par in argExps)
            {
                sb.AppendFormat(", {0}", par);
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
            return string.Format("SELECT COUNT(1) FROM SYS.OBJECTS WHERE NAME = '{0}' AND TYPE = 'U'", tableName);
        }

        /// <summary>
        /// 修正 <see cref="DbType"/> 值。
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public DbType CorrectDbType(DbType dbType)
        {
            return dbType;
        }

        /// <summary>
        /// 获取数据库版本。
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        private int GetServerVersion(IDatabase database)
        {
            if (database.Connection == null)
            {
                return 0;
            }
            database.Connection.TryOpen();
            if (string.IsNullOrEmpty(database.Connection.ServerVersion))
            {
                return 0;
            }
            return int.Parse(database.Connection.ServerVersion.Split('.')[0]);
        }
    }
}
