// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Data;
using Fireasy.Data.Provider;
using Fireasy.Data.Schema;

namespace Fireasy.Data.Syntax
{
    /// <summary>
    /// 数据库查询语言的语法解析接口。
    /// </summary>
    public interface ISyntaxProvider : IProviderService
    {
        /// <summary>
        /// 获取字符串函数相关的语法。
        /// </summary>
        StringSyntax String { get; }

        /// <summary>
        /// 获取日期函数相关的语法。
        /// </summary>
        DateTimeSyntax DateTime { get; }

        /// <summary>
        /// 获取数学函数相关的语法。
        /// </summary>
        MathSyntax Math { get; }

        /// <summary>
        /// 获取最近创建的自动编号的查询文本。
        /// </summary>
        string IdentitySelect { get; }

        /// <summary>
        /// 获取自增长列的关键词。
        /// </summary>
        string IdentityColumn { get; }

        /// <summary>
        /// 获取受影响的行数的查询文本。
        /// </summary>
        string RowsAffected { get; }

        /// <summary>
        /// 获取伪查询的表名称。
        /// </summary>
        string FakeSelect { get; }

        /// <summary>
        /// 获取存储参数的前缀。
        /// </summary>
        string ParameterPrefix { get; }

        /// <summary>
        /// 获取列引号标识符。
        /// </summary>
        string[] Quote { get; }

        /// <summary>
        /// 获取换行符。
        /// </summary>
        string Linefeed { get; }

        /// <summary>
        /// 获取是否允许在聚合中使用 DISTINCT 关键字。
        /// </summary>
        bool SupportDistinctInAggregates { get; }

        /// <summary>
        /// 获取是否允许在没有 FORM 的语句中使用子查询。
        /// </summary>
        bool SupportSubqueryInSelectWithoutFrom { get; }

        /// <summary>
        /// 对命令文本进行分段处理，使之能够返回小范围内的数据。
        /// </summary>
        /// <param name="commandText">命令文本。</param>
        /// <param name="segment">数据分段对象。</param>
        /// <returns>处理后的分段命令文本。</returns>
        /// <exception cref="SegmentNotSupportedException">当前数据库或版本不支持分段时，引发该异常。</exception>
        string Segment(string commandText, IDataSegment segment);

        /// <summary>
        /// 对命令文本进行分段处理，使之能够返回小范围内的数据。
        /// </summary>
        /// <param name="context">命令上下文对象。</param>
        /// <exception cref="SegmentNotSupportedException">当前数据库或版本不支持分段时，引发该异常。</exception>
        bool Segment(CommandContext context);

        /// <summary>
        /// 转换源表达式的数据类型。
        /// </summary>
        /// <param name="sourceExp">要转换的源表达式。</param>
        /// <param name="dbType">要转换的类型。</param>
        /// <returns></returns>
        string Convert(object sourceExp, DbType dbType);

        /// <summary>
        /// 根据数据类型生成相应的列。
        /// </summary>
        /// <param name="dbType">数据类型。</param>
        /// <param name="length">数据长度。</param>
        /// <param name="precision">数值的精度。</param>
        /// <param name="scale">数值的小数位。</param>
        /// <returns></returns>
        string Column(DbType dbType, int? length = null, int? precision = null, int? scale = null);

        /// <summary>
        /// 如果源表达式为 null，则依次判断给定的一组参数，直至某参数非 null 时返回。
        /// </summary>
        /// <param name="sourceExp">要转换的源表达式。</param>
        /// <param name="argExps">参与判断的一组参数。</param>
        /// <returns></returns>
        string Coalesce(object sourceExp, params object[] argExps);

        /// <summary>
        /// 格式化参数名称。
        /// </summary>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        string FormatParameter(string parameterName);

        /// <summary>
        /// 获取判断表是否存在的语句。
        /// </summary>
        /// <param name="tableName">要判断的表的名称。</param>
        /// <returns></returns>
        string ExistsTable(string tableName);

        /// <summary>
        /// 获取判断多个表是否存在的语句。
        /// </summary>
        /// <param name="tableName">要判断的表的名称数组。</param>
        /// <returns></returns>
        string ExistsTables(string[] tableNames);

        /// <summary>
        /// 修正 <see cref="DbType"/> 值。
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        DbType CorrectDbType(DbType dbType);
    }
}
