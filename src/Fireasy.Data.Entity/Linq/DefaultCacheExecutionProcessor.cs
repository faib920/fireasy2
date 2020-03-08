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
using Fireasy.Common.Tasks;
using Fireasy.Common.Threading;
using Fireasy.Data.Entity.Linq.Translators;
using Fireasy.Data.Entity.Linq.Translators.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Data.Entity.Linq
{
    /// <summary>
    /// 执行缓存处理器。
    /// </summary>
    internal class DefaultCacheExecutionProcessor : ICacheExecutionProcessor
    {
        internal static readonly ICacheExecutionProcessor Instance = new DefaultCacheExecutionProcessor();

        private const string CACHE_KEY = "fireasy.exec";
        private static readonly ReadWriteLocker locker = new ReadWriteLocker();
        private static readonly List<string> runners = new List<string>();

        /// <summary>
        /// 尝试通过表达式获取执行后的结果缓存。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <param name="cacheOpt"></param>
        /// <param name="creator"></param>
        /// <returns></returns>
        T ICacheExecutionProcessor.TryGet<T>(Expression expression, CacheExecutionOptions cacheOpt, Func<T> creator)
        {
            TryExpire(expression, cacheOpt?.CachePrefix);

            var section = ConfigurationUnity.GetSection<TranslatorConfigurationSection>();
            var option = section == null ? TranslateOptions.Default : section.Options;
            var result = CacheableChecker.Check(expression);

            ICacheManager cacheMgr;

            //没有开启数据缓存
            if ((result.Enabled == null &&
                    (cacheOpt.Enabled == false || (cacheOpt.Enabled == null && !option.CacheExecution))) ||
                result.Enabled == false ||
                (cacheMgr = CacheManagerFactory.CreateManager()) == null)
            {
                return creator();
            }

            var cacheKey = ExpressionKeyGenerator.GetKey(expression, CACHE_KEY);
            if (cacheOpt != null && !string.IsNullOrEmpty(cacheOpt.CachePrefix))
            {
                cacheKey = string.Concat(cacheOpt.CachePrefix, cacheKey);
            }

            var segment = SegmentFinder.Find(expression);
            var pager = segment as DataPager;

            var cacheItem = cacheMgr.TryGet(cacheKey,
                () => HandleCacheItem(cacheKey, cacheOpt?.CachePrefix, expression, creator(), pager),
                () => new RelativeTime(GetExpire(result, option, cacheOpt)));

            if (pager != null)
            {
                pager.RecordCount = cacheItem.Total;
            }

            TryStartRunner(cacheOpt?.CachePrefix);

            return cacheItem.Data;
        }

        /// <summary>
        /// 异步的，尝试通过表达式获取执行后的结果缓存。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <param name="cacheOpt"></param>
        /// <param name="creator"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task<T> ICacheExecutionProcessor.TryGetAsync<T>(Expression expression, CacheExecutionOptions cacheOpt, Func<CancellationToken, Task<T>> creator, CancellationToken cancellationToken)
        {
            TryExpire(expression, cacheOpt?.CachePrefix);

            var section = ConfigurationUnity.GetSection<TranslatorConfigurationSection>();
            var option = section == null ? TranslateOptions.Default : section.Options;
            var result = CacheableChecker.Check(expression);

            ICacheManager cacheMgr;

            //没有开启数据缓存
            if ((result.Enabled == null &&
                    (cacheOpt.Enabled == false || (cacheOpt.Enabled == null && !option.CacheExecution))) ||
                result.Enabled == false ||
                (cacheMgr = CacheManagerFactory.CreateManager()) == null)
            {
                return await creator(cancellationToken);
            }

            var cacheKey = ExpressionKeyGenerator.GetKey(expression, CACHE_KEY);
            if (cacheOpt != null && !string.IsNullOrEmpty(cacheOpt.CachePrefix))
            {
                cacheKey = string.Concat(cacheOpt.CachePrefix, cacheKey);
            }

            var segment = SegmentFinder.Find(expression);
            var pager = segment as DataPager;

            var cacheItem = cacheMgr.TryGet(cacheKey,
                () => HandleCacheItem(cacheKey, cacheOpt?.CachePrefix, expression, creator(cancellationToken).AsSync(), pager),
                () => new RelativeTime(GetExpire(result, option, cacheOpt)));

            if (pager != null)
            {
                pager.RecordCount = cacheItem.Total;
            }

            TryStartRunner(cacheOpt?.CachePrefix);

            return cacheItem.Data;
        }

        private static CacheItem<T> HandleCacheItem<T>(string cacheKey, string cachePrefix, Expression expression, T data, DataPager pager)
        {
            Task.Run(() => Reference(cacheKey, cachePrefix, expression));

            var total = 0;
            if (pager != null)
            {
                total = pager.RecordCount;
            }

            return new CacheItem<T> { Data = data, Total = total };
        }

        private void TryStartRunner(string cachePrefix)
        {
            locker.LockWrite(() =>
            {
                if (!runners.Contains(cachePrefix))
                {
                    var scheduler = TaskSchedulerFactory.CreateScheduler();
                    if (scheduler != null)
                    {
                        var random = new Random();
                        var delay = random.Next(1, 40);
                        var period = random.Next(50, 80);
                        var arguments = new Dictionary<string, object> { { "cachePrefix", cachePrefix } };
                        var startOption = new StartOptions<ClearCacheTaskExecutor>(TimeSpan.FromSeconds(delay), TimeSpan.FromSeconds(period)) { Arguments = arguments };
                        scheduler.StartAsync(startOption);
                        runners.Add(cachePrefix);
                    }
                }
            });
        }

        /// <summary>
        /// 使相关的缓存过期。
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="cachePrefix"></param>
        private static void TryExpire(Expression expression, string cachePrefix)
        {
            //查找是不是属于操作(新增，修改，删除)表达式，如果是，则需要清除关联缓存键
            var operateType = OperateFinder.Find(expression);
            if (operateType != null)
            {
                Task.Run(() => ClearKeys(cachePrefix, operateType));
            }
        }

        /// <summary>
        /// 数组缓存项。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class CacheItem<T>
        {
            /// <summary>
            /// 缓存的数据。
            /// </summary>
            public T Data { get; set; }

            /// <summary>
            /// 数据的总记录数。
            /// </summary>
            public int Total { get; set; }
        }

        /// <summary>
        /// 找出表达式中相关联的实体类型，进行关系维护
        /// </summary>
        /// <param name="key">缓存键。</param>
        /// <param name="cachePrefix"></param>
        /// <param name="expression"></param>
        private static void Reference(string key, string cachePrefix, Expression expression)
        {
            var types = RelationshipFinder.Find(expression);

            var cacheMgr = CacheManagerFactory.CreateManager();
            var rootKey = $"{cachePrefix}{CACHE_KEY}.keys";
            var hashSet = cacheMgr.GetHashSet<string, List<string>>(rootKey);

            void process(string key, string subKey)
            {
                var keys = hashSet.TryGet(subKey, () => new List<string>());

                if (!keys.Contains(key))
                {
                    keys.Add(key);
                }

                hashSet.Add(subKey, keys);
            }

            types.ForEachParallel(s =>
            {
                if (cacheMgr is IEnhancedCacheManager ehCache)
                {
                    ehCache.UseTransaction(string.Concat(rootKey, ":", s.FullName), () => process(key, s.FullName), TimeSpan.FromSeconds(10));
                }
                else
                {
                    locker.LockWrite(() => process(key, s.FullName));
                }
            });
        }

        /// <summary>
        /// 清理实体类型的全部缓存键。
        /// </summary>
        /// <param name="cachePrefix"></param>
        /// <param name="types"></param>
        private static void ClearKeys(string cachePrefix, params Type[] types)
        {
            if (types == null || types.Length == 0)
            {
                return;
            }

            var cacheMgr = CacheManagerFactory.CreateManager();
            var rootKey = $"{cachePrefix}{CACHE_KEY}.keys";
            var hashSet = cacheMgr.GetHashSet<string, List<string>>(rootKey);

            void process(string key)
            {
                try
                {
                    if (hashSet.TryGet(key, out List<string> keys))
                    {
                        keys.ForEach(s => cacheMgr.Remove(s));
                        hashSet.Remove(key);
                    }
                }
                catch (Exception exp)
                {
                    Tracer.Error($"CacheExecutionProcessor ClearKeys Error:{exp.Message}");
                }
            }

            types.ForEachParallel(type =>
                {
                    Tracer.Debug($"Clear ReferenceKeys for '{string.Concat(rootKey, ":", type.FullName)}'");

                    if (cacheMgr is IEnhancedCacheManager ehCache)
                    {
                        ehCache.UseTransaction(string.Concat(rootKey, ":", type.FullName), () => process(type.FullName), TimeSpan.FromSeconds(10));
                    }
                    else
                    {
                        locker.LockWrite(() => process(type.FullName));
                    }
                });
        }

        private TimeSpan GetExpire(CacheableCheckResult checkResult, TranslateOptions option, CacheExecutionOptions cacheOpt)
        {
            if (checkResult.Expired != null)
            {
                return checkResult.Expired.Value;
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
                        result.Enabled = false;
                        break;
                    case nameof(Extensions.CacheExecution):
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
        /// 操作(新增，删除，修改)查找器。
        /// </summary>
        private class OperateFinder : Common.Linq.Expressions.ExpressionVisitor
        {
            private Type operateType;

            /// <summary>
            /// 检查表达式，查找操作的实体类型。
            /// </summary>
            /// <param name="expression"></param>
            /// <returns></returns>
            internal static Type Find(Expression expression)
            {
                var checker = new OperateFinder();
                checker.Visit(expression);
                return checker.operateType;
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                //增删改的操作
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
                        VisitExpressionList(node.Arguments);
                        break;
                }

                return node;
            }

            protected override Expression VisitConstant(ConstantExpression constExp)
            {
                if (typeof(IQueryable).IsAssignableFrom(constExp.Type))
                {
                    var elementType = constExp.Type.GetGenericArguments()[0];
                    if (typeof(IEntity).IsAssignableFrom(elementType))
                    {
                        operateType = elementType;
                    }
                }

                return constExp;
            }
        }

        /// <summary>
        /// 关联关系查找器。
        /// </summary>
        private class RelationshipFinder : Common.Linq.Expressions.ExpressionVisitor
        {
            private readonly List<Type> types = new List<Type>();

            /// <summary>
            /// 在表达式中查找关联查询的实体类型。
            /// </summary>
            /// <param name="expression"></param>
            /// <returns></returns>
            internal static List<Type> Find(Expression expression)
            {
                var finder = new RelationshipFinder();
                finder.Visit(expression);
                return finder.types;
            }

            protected override Expression VisitMember(MemberExpression memberExp)
            {
                if (typeof(IEntity).IsAssignableFrom(memberExp.Member.DeclaringType))
                {
                    types.Add(memberExp.Member.DeclaringType);
                }

                return memberExp;
            }

            protected override Expression VisitConstant(ConstantExpression constExp)
            {
                if (typeof(IQueryable).IsAssignableFrom(constExp.Type))
                {
                    var elementType = constExp.Type.GetGenericArguments()[0];
                    if (typeof(IEntity).IsAssignableFrom(elementType))
                    {
                        types.Add(elementType);
                    }
                }

                return constExp;
            }
        }

        private class ClearCacheTaskExecutor : IAsyncTaskExecutor
        {
            public async Task ExecuteAsync(TaskExecuteContext context, CancellationToken cancellationToken)
            {
                var cachePrefix = context.Arguments["cachePrefix"];
                var cacheMgr = CacheManagerFactory.CreateManager();
                var rootKey = $"{cachePrefix}{CACHE_KEY}.keys";
                var hashSet = cacheMgr.GetHashSet<string, List<string>>(rootKey);

                async Task clearKeys(string key)
                {
                    try
                    {
                        if (hashSet.TryGet(key, out List<string> list))
                        {
                            for (var i = list.Count - 1; i >= 0; i--)
                            {
                                if (!await cacheMgr.ContainsAsync(list[i]))
                                {
                                    list.RemoveAt(i);
                                }
                            }
                        }
                        if (list.Count == 0)
                        {
                            await hashSet.RemoveAsync(key);
                        }
                        else
                        {
                            await hashSet.AddAsync(key, list);
                        }
                    }
                    catch (Exception exp)
                    {
                        Tracer.Error($"ClearCacheTaskExecutor Execute Error:\n{exp.Output()}");
                    }
                }

                hashSet.GetKeys().ForEachParallel(async s =>
                {
                    if (cacheMgr is IEnhancedCacheManager ehCache)
                    {
                        await ehCache.UseTransactionAsync(string.Concat(rootKey, ":", s), async () => await clearKeys(s), TimeSpan.FromSeconds(10));
                    }
                    else
                    {
                        locker.LockWrite(async () => await clearKeys(s));
                    }
                });
            }
        }
    }
}
