// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Fireasy.Data.Entity.Linq.Expressions
{
    /// <summary>
    /// 纯 SQL 命令行表达式。
    /// </summary>
    public class SqlExpression : DbExpression
    {
        /// <summary>
        /// 初始化 <see cref="SqlExpression"/> 类的新实例 。
        /// </summary>
        /// <param name="sqlCommand">SQL命令行。</param>
        public SqlExpression(string sqlCommand)
            : base (DbExpressionType.SqlText)
        {
            SqlCommand = sqlCommand;
        }

        /// <summary>
        /// 获取 SQL 命令行。
        /// </summary>
        public string SqlCommand { get; private set; }

        public SqlExpression Update(string sqlCommand)
        {
            if (sqlCommand != SqlCommand)
            {
                return new SqlExpression(sqlCommand);
            }

            return this;
        }
    }
}
