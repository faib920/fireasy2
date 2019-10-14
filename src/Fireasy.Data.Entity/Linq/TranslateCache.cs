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
using System.Collections.Generic;
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
            var section = ConfigurationUnity.GetSection<TranslatorConfigurationSection>();
            var option = section == null ? TranslateOptions.Default : section.Options;

            var result = CacheableChecker.Check(expression);
            if (!result.Required || (result.Enabled == null && !option.CacheParsing) || result.Enabled == false)
            {
                return func().Compile();
            }

            var cacheKey = ExpressionKeyGenerator.GetKey(expression, "Trans");
            cacheKey = NativeCacheKeyContext.GetKey(cacheKey);

            return MemoryCacheManager.Instance.TryGet(cacheKey, () =>
                {
                    var lambdaExp = func() as LambdaExpression;
                    var segment = SegmentFinder.Find(expression);
                    if (segment != null)
                    {
                        //将表达式内的 Segment 替换成参数
                        var segParExp = Expression.Parameter(typeof(IDataSegment), "g");
                        var newExp = SegmentReplacer.Repalce(lambdaExp.Body, segParExp);
                        var parameters = new List<ParameterExpression>(lambdaExp.Parameters);
                        parameters.Insert(1, segParExp);
                        lambdaExp = Expression.Lambda(newExp, parameters.ToArray());
                    }

                    return lambdaExp.Compile();
                }, 
            () => new RelativeTime(result.Expired ?? TimeSpan.FromSeconds(option.CacheParsingTimes)));
        }

        /// <summary>
        /// 缓存检查的返回结果。
        /// </summary>
        private class CacheableCheckResult
        {
            /// <summary>
            /// 是否开启缓存。
            /// </summary>
            public bool? Enabled { get; set; }

            /// <summary>
            /// 是否必须的。
            /// </summary>
            public bool Required { get; set; } = true;

            /// <summary>
            /// 过期时间。
            /// </summary>
            public TimeSpan? Expired { get; set; }
        }

        /// <summary>
        /// 缓存检查器。
        /// </summary>
        private class CacheableChecker : Common.Linq.Expressions.ExpressionVisitor
        {
            private CacheableCheckResult result = new CacheableCheckResult();

            /// <summary>
            /// 检查表达式是否能够被缓存。
            /// </summary>
            /// <param name="expression"></param>
            /// <returns></returns>
            public static CacheableCheckResult Check(Expression expression)
            {
                var checker = new CacheableChecker();
                checker.Visit(expression);
                return checker.result;
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
                        result.Required = false;
                        break;
                    case nameof(Extensions.CacheParsing):
                        result.Enabled = (bool)((ConstantExpression)node.Arguments[1]).Value;
                        if (result.Enabled == true && node.Arguments.Count == 3)
                        {
                            result.Expired = (TimeSpan?)((ConstantExpression)node.Arguments[2]).Value;
                        }
                        break;
                }

                return base.VisitMethodCall(node);
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
