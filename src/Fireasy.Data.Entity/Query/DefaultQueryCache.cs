// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;
using Fireasy.Common.Caching;
using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
using Fireasy.Common.Ioc;
using Fireasy.Data.Entity.Linq;
using Fireasy.Data.Entity.Linq.Translators;
using Fireasy.Data.Entity.Linq.Translators.Configuration;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Query
{
    /// <summary>
    /// Expression 解析缓存处理器。
    /// </summary>
    internal class DefaultQueryCache : IQueryCache
    {
        internal static readonly IQueryCache Instance = new DefaultQueryCache();

        private readonly IServiceProvider _serviceProvider;

        public DefaultQueryCache()
            : this (ContainerUnity.GetContainer())
        {
        }

        public DefaultQueryCache(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 尝试获取指定表达式的缓存。
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="context"></param>
        /// <param name="creator">当缓存不存在时，创建缓存数据的函数。</param>
        /// <returns></returns>
        Delegate IQueryCache.TryGetDelegate(Expression expression, QueryCacheContext context, Func<LambdaExpression> creator)
        {
            var section = ConfigurationUnity.GetSection<TranslatorConfigurationSection>();
            var option = section == null ? TranslateOptions.Default : section.Options;

            var result = CacheableChecker.Check(expression);

            if (!result.Required ||
                (result.Enabled == null && (context.Enabled == false || (context.Enabled == null && !option.CacheParsing))) ||
                result.Enabled == false)
            {
                return creator().Compile();
            }

            var generator = _serviceProvider.TryGetService<IQueryCacheKeyGenerator>(() => ExpressionKeyGenerator.Instance);
            var cacheKey = _serviceProvider.GetCacheKey(generator.Generate(expression, "Trans"));

            Tracer.Debug($"QueryCache access to '{cacheKey}'");
            var cacheMgr = _serviceProvider.TryGetService<IMemoryCacheManager>(() => MemoryCacheManager.Instance);

            return cacheMgr.TryGet(cacheKey, () =>
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
            () => new RelativeTime(GetExpire(result, option, context)));
        }

        private TimeSpan GetExpire(CacheableCheckResult result, TranslateOptions option, QueryCacheContext cacheOpt)
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
                    case nameof(Linq.Extensions.RemoveWhere):
                    case nameof(Linq.Extensions.RemoveWhereAsync):
                    case nameof(Linq.Extensions.UpdateWhere):
                    case nameof(Linq.Extensions.UpdateWhereAsync):
                    case nameof(Linq.Extensions.CreateEntity):
                    case nameof(Linq.Extensions.CreateEntityAsync):
                    case nameof(Linq.Extensions.BatchOperate):
                    case nameof(Linq.Extensions.BatchOperateAsync):
                    case nameof(IRepository.Insert):
                    case nameof(IRepository.InsertAsync):
                    case nameof(IRepository.Update):
                    case nameof(IRepository.UpdateAsync):
                    case nameof(IRepository.Delete):
                    case nameof(IRepository.DeleteAsync):
                        result.Required = false;
                        break;
                    case nameof(Linq.Extensions.CacheParsing):
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
