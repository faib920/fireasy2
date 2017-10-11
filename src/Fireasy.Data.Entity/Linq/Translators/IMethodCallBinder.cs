// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Entity.Linq.Expressions;
using System;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// 提供对方法调用的绑定。
    /// </summary>
    public interface IMethodCallBinder
    {
        /// <summary>
        /// 使用转换来绑定方法调用。
        /// </summary>
        /// <param name="visitor">上下文中的访问器。</param>
        /// <param name="callExp">当前的方法调用表达式。</param>
        /// <returns></returns>
        Expression Bind(DbExpressionVisitor visitor, MethodCallExpression callExp);
    }
}
