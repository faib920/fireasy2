// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
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
        /// <param name="context">方法绑定的上下文对象。</param>
        /// <returns></returns>
        Expression Bind(MethodCallBindContext context);
    }
}
