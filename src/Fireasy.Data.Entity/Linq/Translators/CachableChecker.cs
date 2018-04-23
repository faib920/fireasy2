// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using Fireasy.Common.Extensions;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// 用于检查 ELinq 表达式树能否被缓存。无法继承此类。
    /// </summary>
    public sealed class CachableChecker : Common.Linq.Expressions.ExpressionVisitor
    {
        private bool isCachable = true;

        /// <summary>
        /// 检查表达式是否可以被缓存。
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static bool Check(Expression expression)
        {
            var checker = new CachableChecker();
            checker.Visit(expression);
            return checker.isCachable;
        }

        protected override Expression VisitConstant(ConstantExpression constExp)
        {
            if (constExp.Value != null && 
                !constExp.Value.Is<IQueryable>() &&
                !constExp.Value.Is<IEnumerable>() && 
                constExp.Type.ToString() == constExp.Value.ToString())
            {
                isCachable = false;
                return constExp;
            }

            return base.VisitConstant(constExp);
        }
    }
}
