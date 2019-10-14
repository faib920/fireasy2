// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Fireasy.Common.Extensions;

namespace Fireasy.Data.Entity.Linq.Expressions
{
    /// <summary>
    /// 执行更新命令的表达式。
    /// </summary>
    public class UpdateCommandExpression : CommandExpression
    {
        /// <summary>
        /// 初始化 <see cref="UpdateCommandExpression"/> 类的新实例。
        /// </summary>
        /// <param name="table">表的表达式。</param>
        /// <param name="where">条件表达式。</param>
        /// <param name="arguments">更新的列表达式集合。</param>
        /// <param name="isAsync">是否异步执行。</param>
        public UpdateCommandExpression(Expression table, Expression where, IEnumerable<ColumnAssignment> arguments, bool isAsync)
            : base(DbExpressionType.Update, isAsync, typeof(int))
        {
            Table = table;
            Where = where;
            Assignments = arguments.ToReadOnly();
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
        /// 获取需要更新的列表达式集合。
        /// </summary>
        public ReadOnlyCollection<ColumnAssignment> Assignments { get; private set; }

        /// <summary>
        /// 更新 <see cref="UpdateCommandExpression"/> 对象。
        /// </summary>
        /// <param name="table">表的表达式。</param>
        /// <param name="where">条件表达式。</param>
        /// <param name="arguments">更新的列表达式集合。</param>
        /// <returns></returns>
        public UpdateCommandExpression Update(Expression table, Expression where, IEnumerable<ColumnAssignment> arguments)
        {
            return table != Table ||
                where != Where ||
                arguments != Assignments
                ? new UpdateCommandExpression(table, where, arguments, IsAsync) : this;
        }

    }
}
