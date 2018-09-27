// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Caching;
using Fireasy.Common.Configuration;
using Fireasy.Data.Entity.Linq.Translators;
using Fireasy.Data.Entity.Linq.Translators.Configuration;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq
{
    /// <summary>
    /// Expression 解析缓存。
    /// </summary>
    internal class TranslateCache
    {
        /// <summary>
        /// 尝试获取指定表达式的缓存。
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="func">当缓存不存在时，创建缓存数据的函数。</param>
        /// <returns></returns>
        internal static Delegate TryGetDelegate(Expression expression, Func<LambdaExpression> func)
        {
            if (!CachaebleChecker.Check(expression))
            {
                return func().Compile();
            }

            var section = ConfigurationUnity.GetSection<TranslatorConfigurationSection>();
            var option = section == null ? TranslateOptions.Default : section.Options;

            if (!option.ParseCacheEnabled)
            {
                return func().Compile();
            }

            var lazy = new Lazy<Delegate>(() =>
                {
                    //将表达式内的 Segment 替换成参数
                    var segParExp = Expression.Parameter(typeof(IDataSegment), "g");
                    var lambdaExp = func() as LambdaExpression;
                    var newExp = SegmentReplacer.Repalce(lambdaExp.Body, segParExp);
                    lambdaExp = Expression.Lambda(newExp, lambdaExp.Parameters[0], segParExp);

                    return lambdaExp.Compile();
                });

            var cacheKey = ExpressionKeyGenerator.GetKey(expression, "Trans");
            return MemoryCacheManager.Instance.TryGet(cacheKey, () => lazy.Value, () => new RelativeTime(TimeSpan.FromSeconds(option.ParseCacheExpired)));
        }

        /// <summary>
        /// 缓存检查器。
        /// </summary>
        private class CachaebleChecker : Common.Linq.Expressions.ExpressionVisitor
        {
            private bool cacheable = true;

            /// <summary>
            /// 检查表达式是否能够被缓存。
            /// </summary>
            /// <param name="expression"></param>
            /// <returns></returns>
            public static bool Check(Expression expression)
            {
                var checker = new CachaebleChecker();
                checker.Visit(expression);
                return checker.cacheable;
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                //增删改的操作，不能缓存
                switch (node.Method.Name)
                {
                    case nameof(Extensions.RemoveWhere):
                    case nameof(Extensions.UpdateWhere):
                    case nameof(Extensions.CreateEntity):
                    case nameof(Extensions.BatchOperate):
                    case nameof(IRepository.Insert):
                    case nameof(IRepository.Update):
                    case nameof(IRepository.Delete):
                        cacheable = false;
                        break;
                }

                return node;
            }
        }

        /// <summary>
        /// <see cref="IDataSegment"/> 替换器。
        /// </summary>
        private class SegmentReplacer : Common.Linq.Expressions.ExpressionVisitor
        {
            private ParameterExpression parExp;

            /// <summary>
            /// 使用 <paramref name="parExp"/> 替换表达式中的 <see cref="IDataSegment"/> 对象。
            /// </summary>
            /// <param name="expression"></param>
            /// <param name="parExp"></param>
            /// <returns></returns>
            public static Expression Repalce(Expression expression, ParameterExpression parExp)
            {
                var replaer = new SegmentReplacer { parExp = parExp };
                return replaer.Visit(expression);
            }

            protected override Expression VisitConstant(ConstantExpression constExp)
            {
                if (constExp.Value is IDataSegment)
                {
                    return parExp;
                }

                return constExp;
            }
        }
    }
}
