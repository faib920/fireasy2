// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;

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
            : base(DbExpressionType.SqlText)
        {
            SqlCommand = sqlCommand;
        }

        /// <summary>
        /// 初始化 <see cref="SqlExpression"/> 类的新实例 。
        /// </summary>
        /// <param name="sqlCommand">SQL命令行。</param>
        /// <param name="type">返回类型。</param>
        public SqlExpression(string sqlCommand, Type type)
            : base(DbExpressionType.SqlText, type)
        {
            SqlCommand = sqlCommand;
        }

        /// <summary>
        /// 初始化 <see cref="SqlExpression"/> 类的新实例 。
        /// </summary>
        /// <param name="sqlCommand">SQL命令行。</param>
        /// <param name="parameters"></param>
        public SqlExpression(string sqlCommand, List<NamedValueExpression> parameters)
            : base(DbExpressionType.SqlText, typeof(bool))
        {
            SqlCommand = sqlCommand;
            Parameters = parameters;
        }

        /// <summary>
        /// 获取 SQL 命令行。
        /// </summary>
        public string SqlCommand { get; }

        /// <summary>
        /// 获取参数集合。
        /// </summary>
        public List<NamedValueExpression> Parameters { get; }

        public SqlExpression Update(string sqlCommand)
        {
            if (sqlCommand != SqlCommand)
            {
                return new SqlExpression(sqlCommand);
            }

            return this;
        }

        public SqlExpression Update(string sqlCommand, List<NamedValueExpression> parameters = null)
        {
            if (sqlCommand != SqlCommand || parameters != Parameters)
            {
                return new SqlExpression(sqlCommand, parameters);
            }

            return this;
        }

        public override string ToString()
        {
            return $"Sql({SqlCommand})";
        }
    }
}
