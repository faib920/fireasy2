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
using Fireasy.Common.MultiTenancy;
using Fireasy.Common.Tasks;
using Fireasy.Common.Threading;
using Fireasy.Data.Entity.Linq;
using Fireasy.Data.Entity.Linq.Translators;
using Fireasy.Data.Entity.Linq.Translators.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Data.Entity.Query
{
    /// <summary>
    /// 执行缓存处理器。
    /// </summary>
    internal class DefaultExecuteCache : IExecuteCache
    {
        internal static readonly IExecuteCache Instance = new DefaultExecuteCache();

        private const string CACHE_KEY = "fireasy.exec";
        private static readonly ReadWriteLocker _locker = new ReadWriteLocker();
        private static readonly List<string> _barriers = new List<string>();
        private readonly IServiceProvider _serviceProvider;
        private readonly ICacheManager _cacheMgr;

        public DefaultExecuteCache()
            : this(ContainerUnity.GetContainer())
        {
        }

        public DefaultExecuteCache(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _cacheMgr = serviceProvider.TryGetService<ICacheManager>(() => CacheManagerFactory.CreateManager());
        }

        /// <summary>
        /// 尝试通过表达式获取执行后的结果缓存。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <param name="context"></param>
        /// <param name="creator"></param>
        /// <returns></returns>
        T IExecuteCache.TryGet<T>(Expression expression, ExecuteCacheContext context, Func<T> creator)
        {
            var section = ConfigurationUnity.GetSection<TranslatorConfigurationSection>();
            var option = section == null ? TranslateOptions.Default : section.Options;

            if (context.Enabled == true || (context.Enabled == null && option.CacheExecution))
            {
                TryExpire(expression);
            }

            var result = CacheableChecker.Check(expression);

            //没有开启数据缓存
            if ((result.Enabled == null && (context.Enabled == false || (context.Enabled == null && !option.CacheExecution))) ||
                result.Enabled == false || _cacheMgr == null)
            {
                return creator();
            }

            var generator = _serviceProvider.TryGetService<IExecuteCacheKeyGenerator>(() => ExpressionKeyGenerator.Instance);
            var cacheKey = _serviceProvider.GetCacheKey(generator.Generate(expression, CACHE_KEY));

            Tracer.Debug($"ExecuteCache access to '{cacheKey}'");

            var segment = SegmentFinder.Find(expression);
            var pager = segment as DataPager;

            using var edps = new EntityDeserializeProcessorScope();
            var cacheItem = _cacheMgr.TryGet(cacheKey,
                () => HandleCacheItem(cacheKey, expression, creator(), pager),
                () => new RelativeTime(GetExpire(result, option, context)));

            if (pager != null)
            {
                pager.RecordCount = cacheItem.Total;
            }

            TryStartRunner();

            return cacheItem.Data;
        }

        /// <summary>
        /// 异步的，尝试通过表达式获取执行后的结果缓存。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <param name="context"></param>
        /// <param name="creator"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task<T> IExecuteCache.TryGetAsync<T>(Expression expression, ExecuteCacheContext context, Func<CancellationToken, Task<T>> creator, CancellationToken cancellationToken)
        {
            var section = ConfigurationUnity.GetSection<TranslatorConfigurationSection>();
            var option = section == null ? TranslateOptions.Default : section.Options;

            if (context.Enabled == true || (context.Enabled == null && option.CacheExecution))
            {
                TryExpire(expression);
            }

            var result = CacheableChecker.Check(expression);

            //没有开启数据缓存
            if ((result.Enabled == null && (context.Enabled == false || (context.Enabled == null && !option.CacheExecution))) ||
                result.Enabled == false || _cacheMgr == null)
            {
                return await creator(cancellationToken);
            }

            var generator = _serviceProvider.TryGetService<IExecuteCacheKeyGenerator>(() => ExpressionKeyGenerator.Instance);
            var cacheKey = _serviceProvider.GetCacheKey(generator.Generate(expression, CACHE_KEY));

            Tracer.Debug($"DefaultExecuteCache access to '{cacheKey}'");

            var segment = SegmentFinder.Find(expression);
            var pager = segment as DataPager;

            using var edps = new EntityDeserializeProcessorScope();
            var cacheItem = _cacheMgr.TryGet(cacheKey,
                () => HandleCacheItem(cacheKey, expression, creator(cancellationToken).AsSync(), pager),
                () => new RelativeTime(GetExpire(result, option, context)));

            if (pager != null)
            {
                pager.RecordCount = cacheItem.Total;
            }

            TryStartRunner();

            return cacheItem.Data;
        }

        private CacheItem<T> HandleCacheItem<T>(string cacheKey, Expression expression, T data, DataPager pager)
        {
            Task.Run(() => Reference(cacheKey, expression));

            var total = 0;
            if (pager != null)
            {
                data = EnumerateData(data);

                total = pager.RecordCount;
            }

            return new CacheItem<T> { Data = data, Total = total };
        }

        /// <summary>
        /// 如果有分页，则要进行枚举遍列。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        private static T EnumerateData<T>(T data)
        {
            if (data is IEnumerable && !(data is ICollection))
            {
                var elementType = typeof(T).GetEnumerableElementType();
                var result = typeof(List<>).MakeGenericType(elementType).New<IList>();
                var en = ((IEnumerable)data).GetEnumerator();
                while (en.MoveNext())
                {
                    result.Add(en.Current);
                }

                en.TryDispose();

                return (T)result;
            }

            return data;
        }

        private void TryStartRunner()
        {
            var tenancyProvider = _serviceProvider.TryGetService<ITenancyProvider<CacheTenancyInfo>>();
            var barrier = tenancyProvider == null ? "default" : tenancyProvider.Resolve(null).Key;
            if (_barriers.Contains(barrier))
            {
                return;
            }

            lock (this)
            {
                if (_barriers.Contains(barrier))
                {
                    return;
                }

                var scheduler = _serviceProvider.TryGetService<ITaskScheduler>();
                if (scheduler != null)
                {
                    var random = new Random();
                    var delay = random.Next(1, 40);
                    var period = random.Next(50, 80);
                    var arguments = new Dictionary<string, object> { { "barrier", barrier } };

                    var startOption = new StartOptions<CacheClearTaskExecutor>(TimeSpan.FromSeconds(delay), TimeSpan.FromSeconds(period)) { Arguments = arguments };
                    scheduler.StartExecutor(startOption);
                }

                _barriers.Add(barrier);
            }
        }

        /// <summary>
        /// 使相关的缓存过期。
        /// </summary>
        /// <param name="expression"></param>
        private void TryExpire(Expression expression)
        {
            //查找是不是属于操作(新增，修改，删除)表达式，如果是，则需要清除关联缓存键
            var operateType = OperateFinder.Find(expression);
            if (operateType != null)
            {
                Task.Run(() => ClearKeys(operateType));
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
        private void Reference(string key, Expression expression)
        {
            var types = RelationshipFinder.Find(expression);
            var rootKey = $"{CACHE_KEY}.keys";
            var hashSet = _cacheMgr.GetHashSet<string, List<string>>(rootKey, checkExpiration: false);

            void process(string key, string subKey)
            {
                var keys = hashSet.TryGet(subKey, () => new List<string>()) ?? new List<string>();

                if (!keys.Contains(key))
                {
                    keys.Add(key);
                    hashSet.Add(subKey, keys);
                }
            }

            types.ForEach(s =>
            {
                if (_cacheMgr is IEnhancedCacheManager ehCache)
                {
                    ehCache.UseTransaction(string.Concat(rootKey, ":", s.FullName, ":Execute"), () => process(key, s.FullName), TimeSpan.FromSeconds(10));
                }
                else
                {
                    _locker.LockWrite(() => process(key, s.FullName));
                }
            });
        }

        /// <summary>
        /// 清理实体类型的全部缓存键。
        /// </summary>
        /// <param name="types"></param>
        private void ClearKeys(params Type[] types)
        {
            if (_cacheMgr == null || types == null || types.Length == 0)
            {
                return;
            }

            var rootKey = $"{CACHE_KEY}.keys";
            var hashSet = _cacheMgr.GetHashSet<string, List<string>>(rootKey, checkExpiration: false);

            void process(string key)
            {
                try
                {
                    if (hashSet.TryGet(key, out List<string> keys) && keys != null)
                    {
                        keys.ForEach(s => _cacheMgr.Remove(s));
                        hashSet.Remove(key);
                    }
                }
                catch (Exception exp)
                {
                    Tracer.Error($"The task throw exception when the reference-keys of '{key}' is cleared:{exp.Message}");
                }
            }

            types.ForEach(type =>
                {
                    Tracer.Debug($"Now clear the reference-keys of '{string.Concat(rootKey, ":", type.FullName)}'");

                    if (_cacheMgr is IEnhancedCacheManager ehCache)
                    {
                        ehCache.UseTransaction(string.Concat(rootKey, ":", type.FullName, ":Execute"), () => process(type.FullName), TimeSpan.FromSeconds(10));
                    }
                    else
                    {
                        _locker.LockWrite(() => process(type.FullName));
                    }
                });
        }

        private TimeSpan GetExpire(CacheableCheckResult checkResult, TranslateOptions option, ExecuteCacheContext cacheOpt)
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
                        result.Enabled = false;
                        break;
                    case nameof(Linq.Extensions.CacheExecution):
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
                var elementType = memberExp.Member.DeclaringType;
                TryAddType(elementType);

                return memberExp;
            }

            protected override Expression VisitConstant(ConstantExpression constExp)
            {
                if (constExp.Value is IQueryable querable)
                {
                    var elementType = constExp.Type.GetGenericArguments()[0];
                    TryAddType(elementType);

                    Visit(querable.Expression);
                }

                return constExp;
            }

            private void TryAddType(Type elementType)
            {
                if (typeof(IEntity).IsAssignableFrom(elementType) && !types.Contains(elementType))
                {
                    types.Add(elementType);
                }
            }
        }

        private class CacheClearTaskExecutor : ITaskExecutor
        {
            public void Execute(TaskExecuteContext context)
            {
                var cacheMgr = context.ServiceProvider.TryGetService<ICacheManager>();
                if (cacheMgr == null)
                {
                    return;
                }

                var barrier = context.Arguments["barrier"];
                Tracer.Debug($"CacheClearTaskExecutor is executing for '{barrier}'.");

                var rootKey = barrier == null ? $"{CACHE_KEY}.keys" : CacheHelper.GetCacheKey(context.ServiceProvider, $"{CACHE_KEY}.keys", barrier);
                var hashSet = cacheMgr.GetHashSet<string, List<string>>(rootKey, checkExpiration: false);

                void clearKeys(string key)
                {
                    try
                    {
                        if (hashSet.TryGet(key, out List<string> subKeys) && subKeys != null)
                        {
                            for (var i = subKeys.Count - 1; i >= 0; i--)
                            {
                                if (!cacheMgr.Contains(subKeys[i]))
                                {
                                    subKeys.RemoveAt(i);
                                }
                            }

                            hashSet.Add(key, subKeys);
                        }
                    }
                    catch (Exception exp)
                    {
                        Tracer.Error($"CacheClearTaskExecutor throw exception when the key of '{barrier}-{key}' is cleared:\n{exp.Output()}");
                    }
                }

                hashSet.GetKeys().ForEach(s =>
                {
                    if (cacheMgr is IEnhancedCacheManager ehCache)
                    {
                        ehCache.UseTransaction(string.Concat(rootKey, ":", s, ":Execute"), () => clearKeys(s), TimeSpan.FromSeconds(10));
                    }
                    else
                    {
                        _locker.LockWrite(() => clearKeys(s));
                    }
                });
            }
        }
    }
}
