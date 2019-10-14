// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Expressions
{
    /// <summary>
    /// 执行删除命令的表达式。
    /// </summary>
    public sealed class DeleteCommandExpression : CommandExpression
    {
        /// <summary>
        /// 初始化 <see cref="DeleteCommandExpression"/> 的新实例。
        /// </summary>
        /// <param name="table">操作的表的表达式。</param>
        /// <param name="where">条件表达式。</param>
        /// <param name="isAsync">是否异步执行。</param>
        public DeleteCommandExpression(Expression table, Expression where, bool isAsync)
            : base(DbExpressionType.Delete, isAsync, typeof(int))
        {
            Table = table;
            Where = where;
        }

        /// <summary>
        /// 获取操作的表的表达式。
        /// </summary>
        public Expression Table { get; private set; }

        /// <summary>
        /// 获取条件表达式。
        /// </summary>
        public Expression Where { get; private set; }

        /// <summary>
        /// 更新 <see cref="DeleteCommandExpression"/> 对象。
        /// </summary>
        /// <param name="table">操作的表的表达式。</param>
        /// <param name="where">条件表达式。</param>
        /// <returns></returns>
        public DeleteCommandExpression Update(Expression table, Expression where)
        {
            return table != Table || where != Where ? new DeleteCommandExpression(table, where, IsAsync) : this;
        }

    }
}
