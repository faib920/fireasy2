// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.Linq.Expressions;
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
        /// <summary>
        /// 获取一个 ELinq 翻译器。
        /// </summary>
        /// <returns></returns>
        public abstract TranslatorBase CreateTranslator();

        /// <summary>
        /// 对 ELinq 表达式进行翻译，并返回翻译的结果。
        /// </summary>
        /// <param name="expression">一个 ELinq 表达式。</param>
        /// <param name="options">翻译的选项。</param>
        /// <returns></returns>
        public virtual Expression Translate(Expression expression, TranslateOptions options = null)
        {
            expression = PartialEvaluator.Eval(expression, CanBeEvaluatedLocally);
            return TranslateInternal(expression, options);
        }

        public virtual Expression BuildExecutionPlan(Expression expression, TranslateOptions option = null)
        {
            var translator = CreateTranslator();
            translator.Options = option;
            translator.Syntax = TranslateScope.Current.Context.Database.Provider.GetService<ISyntaxProvider>();
            translator.Environment = (TranslateScope.Current.Context as IEntityPersistentEnvironment).Environment;

            return ExecutionBuilder.Build(expression, e => translator.Translate(e));
        }

        private Expression TranslateInternal(Expression expression, TranslateOptions options)
        {
            var syntax = TranslateScope.Current.Context.Database.Provider.GetService<ISyntaxProvider>();
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

            var rewritten = RelationshipIncluder.Include(TranslateScope.Current.Context, translation);
            if (rewritten != translation)
            {
                translation = rewritten;
                translation = UnusedColumnRemover.Remove(translation);
                translation = RedundantColumnRemover.Remove(translation);
                translation = RedundantSubqueryRemover.Remove(translation);
                translation = RedundantJoinRemover.Remove(translation);
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
            var cex = expression as ConstantExpression;
            if (cex != null)
            {
                var query = cex.Value as IQueryable;
                if (query != null && query.Provider is QueryProvider)
                {
                    return false;
                }
            }

            var mc = expression as MethodCallExpression;
            if (mc != null &&
                (mc.Method.DeclaringType == typeof(Enumerable) ||
                 mc.Method.DeclaringType == typeof(Queryable) ||
                 mc.Method.DeclaringType == typeof(Extensions)))
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