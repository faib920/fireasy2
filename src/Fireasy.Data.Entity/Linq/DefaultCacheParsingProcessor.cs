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
    /// Expression 解析缓存处理器。
    /// </summary>
    internal class DefaultCacheParsingProcessor : ICacheParsingProcessor
    {
        internal static readonly ICacheParsingProcessor Instance = new DefaultCacheParsingProcessor();

        /// <summary>
        /// 尝试获取指定表达式的缓存。
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="cacheOpt"></param>
        /// <param name="creator">当缓存不存在时，创建缓存数据的函数。</param>
        /// <returns></returns>
        Delegate ICacheParsingProcessor.TryGetDelegate(Expression expression, CacheParsingOptions cacheOpt, Func<LambdaExpression> creator)
        {
            var section = ConfigurationUnity.GetSection<TranslatorConfigurationSection>();
            var option = section == null ? TranslateOptions.Default : section.Options;

            var result = CacheableChecker.Check(expression);

            if (!result.Required || 
                (result.Enabled == null && (cacheOpt.Enabled == false || (cacheOpt.Enabled == null && !option.CacheParsing))) || 
                result.Enabled == false)
            {
                return creator().Compile();
            }

            var cacheKey = ExpressionKeyGenerator.GetKey(expression, "Trans");
            if (cacheOpt != null && !string.IsNullOrEmpty(cacheOpt.CachePrefix))
            {
                cacheKey = string.Concat(cacheOpt.CachePrefix, cacheKey);
            }

            return MemoryCacheManager.Instance.TryGet(cacheKey, () =>
                {
                    var lambdaExp = creator() as LambdaExpression;
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
            () => new RelativeTime(GetExpire(result, option, cacheOpt)));
        }

        private TimeSpan GetExpire(CacheableCheckResult result, TranslateOptions option, CacheParsingOptions cacheOpt)
        {
            if (result.Expired != null)
            {
                return result.Expired.Value;
            }
            else if (cacheOpt.Times != null)
            {
                return cacheOpt.Times.Value;
            }

            return option.CacheExecutionTimes;
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
            private readonly CacheableCheckResult result = new CacheableCheckResult();

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
                    case nameof(Extensions.RemoveWhereAsync):
                    case nameof(Extensions.UpdateWhere):
                    case nameof(Extensions.UpdateWhereAsync):
                    case nameof(Extensions.CreateEntity):
                    case nameof(Extensions.CreateEntityAsync):
                    case nameof(Extensions.BatchOperate):
                    case nameof(Extensions.BatchOperateAsync):
                    case nameof(IRepository.Insert):
                    case nameof(IRepository.InsertAsync):
                    case nameof(IRepository.Update):
                    case nameof(IRepository.UpdateAsync):
                    case nameof(IRepository.Delete):
                    case nameof(IRepository.DeleteAsync):
                        result.Required = false;
                        break;
                    case nameof(Extensions.CacheParsing):
                        result.Enabled = (bool)((ConstantExpression)node.Arguments[1]).Value;
                        if (result.Enabled == true && node.Arguments.Count == 3 &&
                            node.Arguments[2] is ConstantExpression consExp && consExp.Value is TimeSpan expired
                            && expired != TimeSpan.Zero)
                        {
                            result.Expired = expired;
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
