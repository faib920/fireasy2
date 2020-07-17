// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Entity.Linq.Expressions;
using Fireasy.Data.Syntax;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// 方法绑定的上下文对象。
    /// </summary>
    public class MethodCallBindContext
    {
        internal MethodCallBindContext(DbExpressionVisitor visitor, MethodCallExpression callExp, ISyntaxProvider syntax)
        {
            Visitor = visitor;
            Expression = callExp;
            Syntax = syntax;
        }

        /// <summary>
        /// 获取 ELinq 访问器。
        /// </summary>
        public DbExpressionVisitor Visitor { get; }

        /// <summary>
        /// 获取要绑定的 <see cref="MethodCallExpression"/> 表达式。
        /// </summary>
        public MethodCallExpression Expression { get; }

        /// <summary>
        /// 获取当前数据库的语法服务。
        /// </summary>
        public ISyntaxProvider Syntax { get; }
    }
}
