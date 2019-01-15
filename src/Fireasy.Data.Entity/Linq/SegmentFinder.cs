// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq
{
    /// <summary>
    /// <see cref="IDataSegment"/> 查找器。
    /// </summary>
    internal class SegmentFinder : Common.Linq.Expressions.ExpressionVisitor
    {
        private IDataSegment dataSegment;

        /// <summary>
        /// <see cref="IDataSegment"/> 查找器。
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static IDataSegment Find(Expression expression)
        {
            var replaer = new SegmentFinder();
            replaer.Visit(expression);
            return replaer.dataSegment;
        }

        protected override Expression VisitConstant(ConstantExpression constExp)
        {
            if (constExp.Value is IDataSegment)
            {
                dataSegment = constExp.Value as IDataSegment;
            }

            return constExp;
        }
    }
}
