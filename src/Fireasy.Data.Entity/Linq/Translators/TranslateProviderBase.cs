// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.Linq.Expressions;
using Fireasy.Data.Provider;
using Fireasy.Data.Syntax;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// 一个抽象类，提供对 ELinq 表达式的翻译。
    /// </summary>
    public abstract class TranslateProviderBase : ITranslateProvider
    {
        IProvider IProviderService.Provider { get; set; }

        /// <summary>
        /// 获取一个 ELinq 翻译器。
        /// </summary>
        /// <returns></returns>
        public abstract TranslatorBase CreateTranslator();

        /// <summary>
        /// 对 ELinq 表达式进行翻译，并返回翻译的结果。
        /// </summary>
        /// <param name="expression">一个 ELinq 表达式。</param>
        /// <returns></returns>
        public virtual Expression Translate(Expression expression)
        {
            expression = PartialEvaluator.Eval(expression, CanBeEvaluatedLocally);
            return TranslateInternal(expression);
        }

        private Expression TranslateInternal(Expression expression)
        {
            var syntax = TranslateScope.Current.SyntaxProvider;
            var translation = QueryBinder.Bind(expression, syntax);

            translation = LogicalDeleteFlagRewriter.Rewrite(translation);
            translation = GlobalQueryPolicyRewriter.Rewrite(translation);
            translation = AggregateRewriter.Rewrite(translation);
            translation = UnusedColumnRemover.Remove(translation);
            translation = RedundantColumnRemover.Remove(translation);
            translation = RedundantSubqueryRemover.Remove(translation);
            translation = RedundantJoinRemover.Remove(translation);

            var bound = RelationshipBinder.Bind(translation);
            if (bound != translation)
            {
                translation = bound;
                translation = RedundantColumnRemover.Remove(translation);
                translation = RedundantJoinRemover.Remove(translation);
            }

            translation = ComparisonRewriter.Rewrite(translation);

            Expression rewritten;
            if (TranslateScope.Current != null && TranslateScope.Current.QueryPolicy != null)
            {
                rewritten = RelationshipIncluder.Include(TranslateScope.Current.QueryPolicy, translation);
                if (rewritten != translation)
                {
                    translation = rewritten;
                    translation = UnusedColumnRemover.Remove(translation);
                    translation = RedundantColumnRemover.Remove(translation);
                    translation = RedundantSubqueryRemover.Remove(translation);
                    translation = RedundantJoinRemover.Remove(translation);
                }
            }

            rewritten = SingletonProjectionRewriter.Rewrite(translation);
            if (rewritten != translation)
            {
                translation = rewritten;
                translation = UnusedColumnRemover.Remove(translation);
                translation = RedundantColumnRemover.Remove(translation);
                translation = RedundantSubqueryRemover.Remove(translation);
                translation = RedundantJoinRemover.Remove(translation);
            }

            rewritten = ClientJoinedProjectionRewriter.Rewrite(translation);
            if (rewritten != translation)
            {
                translation = rewritten;
                translation = UnusedColumnRemover.Remove(translation);
                translation = RedundantColumnRemover.Remove(translation);
                translation = RedundantSubqueryRemover.Remove(translation);
                translation = RedundantJoinRemover.Remove(translation);
            }

            translation = BuildExpression(translation);

            return translation;
        }

        protected virtual Expression BuildExpression(Expression expression)
        {
            expression = UnusedColumnRemover.Remove(expression);
            expression = RedundantColumnRemover.Remove(expression);
            expression = RedundantSubqueryRemover.Remove(expression);

            var rewritten = CrossApplyRewriter.Rewrite(expression);

            rewritten = CrossJoinRewriter.Rewrite(rewritten);

            if (rewritten != expression)
            {
                expression = rewritten;
                expression = UnusedColumnRemover.Remove(expression);
                expression = RedundantSubqueryRemover.Remove(expression);
                expression = RedundantJoinRemover.Remove(expression);
                expression = RedundantColumnRemover.Remove(expression);
            }

            return expression;
        }

        /// <summary>
        /// 判断表达式中的常量是否可以被计算。
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public virtual bool CanBeEvaluatedLocally(Expression expression)
        {
            return EvaluatedLocallyFunc(expression);
        }

        /// <summary>
        /// 判断表达式中的常量是否可以被计算。
        /// </summary>
        /// <returns></returns>
        public static Func<Expression, bool> EvaluatedLocallyFunc = new Func<Expression, bool>(expression =>
        {
            if (expression is ConstantExpression cex)
            {
                if (cex.Value is IQueryable query)
                {
                    return false;
                }
            }

            if (expression is MethodCallExpression mc &&
                (mc.Method.DeclaringType == typeof(Enumerable) ||
                 mc.Method.DeclaringType == typeof(Queryable) ||
                 mc.Method.DeclaringType == typeof(Extensions) ||
                 mc.Method.IsDefined(typeof(MethodCallBindAttribute), false)))
            {
                return false;
            }

            if (expression.NodeType == ExpressionType.Convert &&
                expression.Type == typeof(object))
            {
                return true;
            }

            return expression.NodeType != ExpressionType.Parameter &&
                   expression.NodeType != ExpressionType.Lambda;
        });
    }
}