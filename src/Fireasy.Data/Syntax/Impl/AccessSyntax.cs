// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Provider;
using System.Data;
using System.Reflection;
using System.Text;

namespace Fireasy.Data.Syntax
{
    public class AccessSyntax : ISyntaxProvider
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
            get { return strSyntax ?? (strSyntax = new AccessStringSyntax()); }
        }

        /// <summary>
        /// 获取日期函数相关的语法。
        /// </summary>
        public virtual DateTimeSyntax DateTime
        {
            get { return dtSyntax ?? (dtSyntax = new AccessDateTimeSyntax()); }
        }

        /// <summary>
        /// 获取数学函数相关的语法。
        /// </summary>
        public virtual MathSyntax Math
        {
            get { return mathSyntax ?? (mathSyntax = new AccessMathSyntax()); }
        }

        /// <summary>
        /// 获取最近创建的自动编号的查询文本。
        /// </summary>
        public virtual string IdentitySelect
        {
            get { return "SELECT @@IDENTITY"; }
        }

        /// <summary>
        /// 获取自增长列的关键词。
        /// </summary>
        public virtual string IdentityColumn
        {
            get { return "COUNTER (1, 1)"; }
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
            get { return ""; }
        }

        /// <summary>
        /// 获取列引号标识符。
        /// </summary>
        public virtual string[] Quote
        {
            get { return new[] { "[", "]" }; }
        }

        /// <summary>
        /// 获取换行符。
        /// </summary>
        public string Linefeed
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// 获取是否允许在聚合中使用 DISTINCT 关键字。
        /// </summary>
        public bool SupportDistinctInAggregates
        {
            get { return false; }
        }

        /// <summary>
        /// 获取是否允许在没有 FORM 的语句中使用子查询。
        /// </summary>
        public bool SupportSubqueryInSelectWithoutFrom
        {
            get { return false; }
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
            throw new SyntaxParseException(MethodInfo.GetCurrentMethod().Name);
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
                case DbType.String:
                case DbType.AnsiString:
                case DbType.Guid:
                case DbType.StringFixedLength:
                case DbType.AnsiStringFixedLength:
                    return string.Format("STR({0})", sourceExp);
                case DbType.Currency:
                case DbType.Decimal:
                case DbType.Double:
                case DbType.Single:
                case DbType.Boolean:
                case DbType.SByte:
                case DbType.Int16:
                case DbType.Int32:
                case DbType.Int64:
                case DbType.Byte:
                case DbType.UInt16:
                case DbType.UInt32:
                case DbType.UInt64:
                    return string.Format("VAL({0})", sourceExp);
                case DbType.Date:
                case DbType.DateTime:
                case DbType.Time:
                    return string.Format("CDATE({0})", sourceExp);
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
                case DbType.String:
                case DbType.AnsiString:
                    if (length == null || length <= 255)
                    {
                        return string.Format("VARCHAR({0})", length ?? 255);
                    }
                    if (length > 255 && length <= 65535)
                    {
                        return "TEXT";
                    }
                    //length > 65535 && length <= 16777215
                    return "MEDIUMTEXT";
                case DbType.StringFixedLength:
                case DbType.AnsiStringFixedLength:
                    if (length == null || length <= 255)
                    {
                        return string.Format("CHAR({0})", length ?? 255);
                    }
                    if (length > 255 && length <= 65535)
                    {
                        return "TEXT";
                    }
                    //length > 65535 && length <= 16777215
                    return "MEDIUMTEXT";
                case DbType.Guid:
                    return "VARCHAR(40)";
                case DbType.Binary:
                    if (length == null || length <= 127)
                    {
                        return "LONGBLOB";
                    }
                    if (length > 127 && length <= 65535)
                    {
                        return "BLOB";
                    }
                    //length > 65535 && length <= 16777215
                    return "MEDIUMBLOB";
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
                    return "DOUBLE";
                case DbType.Single:
                    return "FLOAT";
                case DbType.Boolean:
                    return "TINYINT(1)";
                case DbType.Byte:
                    return "TINY INT";
                case DbType.Currency:
                    return "MONEY";
                case DbType.Int16:
                    return "SMALLINT";
                case DbType.Int32:
                    return "INT";
                case DbType.Int64:
                    return "BIGINT";
                case DbType.SByte:
                    return "TINYINT";
                case DbType.UInt16:
                    return "SMALLINT";
                case DbType.UInt32:
                    return "MEDIUMINT";
                case DbType.UInt64:
                    return "BIT";
                case DbType.Date:
                    return "DATE";
                case DbType.DateTime:
                    return "DATETIME";
                case DbType.Time:
                    return "TIME";
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
            sb.AppendFormat("IIF({0} IS NOT NULL, ", sourceExp);
            sb.Append(sourceExp);

            for (var i = 0; i < argExps.Length - 1; i++)
            {
                sb.Append(", ");
                sb.AppendFormat("IIF({0} IS NOT NULL, ", argExps[i]);
                sb.Append(argExps[i]);
            }

            if (argExps.Length > 1)
            {
                sb.Append(", ");
                sb.Append(argExps[argExps.Length - 1]);
            }

            for (var i = 0; i < argExps.Length; i++)
            {
                sb.Append(")");
            }

            return sb.ToString();
        }

        /// <summary>
        /// 格式化参数名称。
        /// </summary>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public virtual string FormatParameter(string parameterName)
        {
            return ParameterPrefix + parameterName;
        }

        /// <summary>
        /// 获取判断表是否存在的语句。
        /// </summary>
        /// <param name="tableName">要判断的表的名称。</param>
        /// <returns></returns>
        public string ExistsTable(string tableName)
        {
            return string.Format("SELECT COUNT(1) FROM `INFORMATION_SCHEMA`.`TABLES` WHERE `TABLE_NAME`='{0}'", tableName);
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
    }
}
