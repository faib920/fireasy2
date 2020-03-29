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
    /// SQLite函数语法解析。
    /// </summary>
    public class SQLiteSyntax : ISyntaxProvider
    {
        IProvider IProviderService.Provider { get; set; }

        /// <summary>
        /// 获取字符串函数相关的语法。
        /// </summary>
        public virtual StringSyntax String => new SQLiteStringSyntax();

        /// <summary>
        /// 获取日期函数相关的语法。
        /// </summary>
        public virtual DateTimeSyntax DateTime => new SQLiteDateTimeSyntax();

        /// <summary>
        /// 获取数学函数相关的语法。
        /// </summary>
        public virtual MathSyntax Math => new SQLiteMathSyntax();

        /// <summary>
        /// 获取最近创建的自动编号的查询文本。
        /// </summary>
        public virtual string IdentitySelect => "SELECT LAST_INSERT_ROWID()";

        /// <summary>
        /// 获取自增长列的关键词。
        /// </summary>
        public string IdentityColumn => "AUTOINCREMENT";

        /// <summary>
        /// 获取受影响的行数的查询文本。
        /// </summary>
        public string RowsAffected => "CHANGES()";

        /// <summary>
        /// 获取伪查询的表名称。
        /// </summary>
        public string FakeSelect => string.Empty;

        /// <summary>
        /// 获取存储参数的前缀。
        /// </summary>
        public virtual string ParameterPrefix => "@";

        /// <summary>
        /// 获取列引号标识符。
        /// </summary>
        public string[] Quote => new[] { "[", "]" };

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
            return  @$"{commandText}
LIMIT {(segment.Length != 0 ? segment.Length : 1000)}{(segment.Start != null ? $" OFFSET {segment.Start - 1}" : string.Empty)}";
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
                    return $"CAST({sourceExp} AS TEXT)";
                case DbType.Binary:
                    return $"CAST({sourceExp} AS BLOB)";
                case DbType.Boolean:
                case DbType.Decimal:
                case DbType.Double:
                case DbType.Single:
                    return $"CAST({sourceExp} AS NUMERIC)";
                case DbType.Int16:
                case DbType.Int32:
                case DbType.Int64:
                case DbType.Byte:
                    return $"CAST({sourceExp} AS INTEGER)";
                case DbType.Date:
                    return $"DATE({sourceExp})";
                case DbType.DateTime:
                    return $"DATETIME({sourceExp})";
                case DbType.Time:
                    return $"TIME({sourceExp})";
                case DbType.Guid:
                    return $"CAST({sourceExp} AS UNIQUEIDENTIFIER)";
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
                case DbType.AnsiStringFixedLength:
                case DbType.String:
                case DbType.StringFixedLength:
                    return length != null ? $"TEXT({length})" : "TEXT";
                case DbType.Binary:
                    return "BLOB";
                case DbType.Boolean:
                case DbType.Decimal:
                case DbType.Double:
                case DbType.Single:
                case DbType.VarNumeric:
                    return "NUMERIC";
                case DbType.Int16:
                case DbType.Int32:
                case DbType.Int64:
                case DbType.Byte:
                case DbType.UInt16:
                case DbType.UInt32:
                case DbType.UInt64:
                case DbType.SByte:
                    return "INTEGER";
                case DbType.Date:
                    return "DATE";
                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.DateTimeOffset:
                    return "DATETIME";
                case DbType.Time:
                    return "TIME";
                case DbType.Guid:
                    return "UNIQUEIDENTIFIER";
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
        public virtual string ExistsTable(string tableName)
        {
            return $"SELECT COUNT(1) FROM SQLITE_MASTER WHERE TYPE='table' AND TBL_NAME='{tableName}'";
        }

        /// <summary>
        /// 获取判断多个表是否存在的语句。
        /// </summary>
        /// <param name="tableName">要判断的表的名称数组。</param>
        /// <returns></returns>
        public virtual string ExistsTables(string[] tableNames)
        {
            var names = string.Join(",", tableNames.Select(s => $"'{s}'"));
            return $"SELECT TBL_NAME FROM SQLITE_MASTER WHERE TYPE='table' AND TBL_NAME IN ({names})";
        }

        /// <summary>
        /// 修正 <see cref="DbType"/> 值。
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public virtual DbType CorrectDbType(DbType dbType)
        {
            return dbType;
        }

    }
}
