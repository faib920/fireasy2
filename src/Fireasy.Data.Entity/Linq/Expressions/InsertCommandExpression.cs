// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Expressions
{
    /// <summary>
    /// 执行插入命令的表达式。
    /// </summary>
    public class InsertCommandExpression : CommandExpression
    {
        /// <summary>
        /// 初始化 <see cref="InsertCommandExpression"/> 类的新实例。
        /// </summary>
        /// <param name="table">表的表达式。</param>
        /// <param name="arguments">插入的列表达式集合。</param>
        /// <param name="isAsync">是否异步执行。</param>
        public InsertCommandExpression(Expression table, IEnumerable<ColumnAssignment> arguments, bool isAsync)
            : base(DbExpressionType.Insert, isAsync, typeof(int))
        {
            Table = table;
            Assignments = arguments.ToReadOnly();
        }

        /// <summary>
        /// 获取操作的表的表达式。
        /// </summary>
        public Expression Table { get; private set; }

        /// <summary>
        /// 获取需要更新的列表达式集合。
        /// </summary>
        public ReadOnlyCollection<ColumnAssignment> Assignments { get; private set; }

        /// <summary>
        /// 获取或设置是否使用自增量插入。
        /// </summary>
        public bool WithAutoIncrement { get; set; }

        /// <summary>
        /// 获取或设置是否使用生成的值。
        /// </summary>
        public bool WithGenerateValue { get; set; }

        /// <summary>
        /// 更新 <see cref="InsertCommandExpression"/> 对象。
        /// </summary>
        /// <param name="table">表的表达式。</param>
        /// <param name="arguments">插入的列表达式集合。</param>
        /// <returns></returns>
        public InsertCommandExpression Update(Expression table, IEnumerable<ColumnAssignment> arguments)
        {
            return table != Table ||
                arguments != Assignments
                ? new InsertCommandExpression(table, arguments, IsAsync) { WithAutoIncrement = WithAutoIncrement, WithGenerateValue = WithGenerateValue } : this;
        }
    }
}
