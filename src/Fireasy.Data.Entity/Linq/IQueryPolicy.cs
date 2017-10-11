// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Linq.Expressions;
using System.Reflection;

namespace Fireasy.Data.Entity.Linq
{
    /// <summary>
    /// 查询的辅助策略。
    /// </summary>
    public interface IQueryPolicy
    {
        /// <summary>
        /// 判断是否包含指定的成员。
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        bool IsIncluded(MemberInfo member);

        /// <summary>
        /// 对表达式应用策略。
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        Expression ApplyPolicy(Expression expression, MemberInfo member);
    }
}
