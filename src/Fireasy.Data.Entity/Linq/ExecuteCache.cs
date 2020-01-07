// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Caching;
using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
using Fireasy.Common.Threading;
using Fireasy.Data.Entity.Linq.Translators;
using Fireasy.Data.Entity.Linq.Translators.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Fireasy.Data.Entity.Linq
{
    /// <summary>
    /// 执行缓存管理器。
    /// </summary>
    internal class ExecuteCache
    {
        private const string CACHE_KEY = "fireasy.exec";
        private static ReadWriteLocker locker = new ReadWriteLocker();

        /// <summary>
        /// 判断是否被缓存。
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        internal static bool CanCache(Expression expression)
        {
            TryExpire(expression);

            var section = ConfigurationUnity.GetSection<TranslatorConfigurationSection>();
            var option = section == null ? TranslateOptions.Default : section.Options;
            var result = CacheableChecker.Check(expression);

            return result.Enabled == true || (result.Enabled == null && option.CacheExecution);
        }

        /// <summary>
        /// 尝试通过表达式获取执行后的结果缓存。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        internal static T TryGet<T>(Expression expression, Func<T> func)
        {
            var section = ConfigurationUnity.GetSection<TranslatorConfigurationSection>();
            var option = section == null ? TranslateOptions.Default : section.Options;
            var result = CacheableChecker.Check(expression);

            ICacheManager cacheMgr;

            //没有开启数据缓存
            if ((result.Enabled == null && !option.CacheExecution) || result.Enabled == false ||
                (cacheMgr = CacheManagerFactory.CreateManager()) == null)
            {
                return func();
            }

            var cacheKey = ExpressionKeyGenerator.GetKey(expression, CACHE_KEY);

            var segment = SegmentFinder.Find(expression);
            var pager = segment as DataPager;

            var cacheItem = cacheMgr.TryGet(cacheKey,
                () => HandleCacheItem(cacheKey, expression, func(), pager),
                () => new RelativeTime(result.Expired ?? TimeSpan.FromSeconds(option.CacheExecutionTimes)));

            if (pager != null)
            {
                pager.RecordCount = cacheItem.Total;
            }

            return cacheItem.Data;
        }

        /// <summary>
        /// 异步的，尝试通过表达式获取执行后的结果缓存。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        internal static async Task<T> TryGetAsync<T>(Expression expression, Func<Task<T>> func)
        {
            var section = ConfigurationUnity.GetSection<TranslatorConfigurationSection>();
            var option = section == null ? TranslateOptions.Default : section.Options;
            var result = CacheableChecker.Check(expression);

            ICacheManager cacheMgr;

            //没有开启数据缓存
            if ((result.Enabled == null && !option.CacheExecution) || result.Enabled == false ||
                (cacheMgr = CacheManagerFactory.CreateManager()) == null)
            {
                return await func();
            }

            var cacheKey = ExpressionKeyGenerator.GetKey(expression, CACHE_KEY);

            var segment = SegmentFinder.Find(expression);
            var pager = segment as DataPager;

            var cacheItem = cacheMgr.TryGet(cacheKey,
                () => HandleCacheItem(cacheKey, expression, func().AsSync(), pager),
                () => new RelativeTime(result.Expired ?? TimeSpan.FromSeconds(option.CacheExecutionTimes)));

            if (pager != null)
            {
                pager.RecordCount = cacheItem.Total;
            }

            return cacheItem.Data;
        }

        private static CacheItem<T> HandleCacheItem<T>(string cacheKey, Expression expression, T data, DataPager pager)
        {
            Task.Run(() => Reference(cacheKey, expression));

            var total = 0;
            if (pager != null)
            {
                total = pager.RecordCount;
            }

            return new CacheItem<T> { Data = data, Total = total };
        }

        /// <summary>
        /// 使相关的缓存过期。
        /// </summary>
        /// <param name="expression"></param>
        internal static void TryExpire(Expression expression)
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
        /// <param name="expression"></param>
        private static void Reference(string key, Expression expression)
        {
            var types = RelationshipFinder.Find(expression);

            var cacheMgr = CacheManagerFactory.CreateManager();
            var rootKey = $"{CACHE_KEY}.keys";
            var hashSet = cacheMgr.GetHashSet<string, List<string>>(rootKey);

            foreach (var type in types)
            {
                locker.LockWrite(() =>
                    {
                        var keys = hashSet.TryGet(type.FullName, () => new List<string>());

                        if (!keys.Contains(key))
                        {
                            keys.Add(key);
                        }

                        hashSet.Add(type.FullName, keys);
                    });
            }
        }

        /// <summary>
        /// 清理实体类型的全部缓存键。
        /// </summary>
        /// <param name="types"></param>
        internal static void ClearKeys(params Type[] types)
        {
            if (types == null || types.Length == 0)
            {
                return;
            }

            var cacheMgr = CacheManagerFactory.CreateManager();
            var rootKey = $"{CACHE_KEY}.keys";
            var hashSet = cacheMgr.GetHashSet<string, List<string>>(rootKey);

            types.ForEach(type =>
                {
                    locker.LockWrite(() =>
                        {
                            if (hashSet.TryGet(type.FullName, out List<string> keys))
                            {
                                keys.ForEach(s => cacheMgr.Remove(s));
                                hashSet.Remove(type.FullName);
                            }
                        });
                });

            foreach (var key in hashSet.GetKeys())
            {
                if (!hashSet.TryGet(key, out List<string> keys1))
                {
                    continue;
                }

                locker.LockWrite(() =>
                    {
                        var isChanged = false;
                        for (var i = keys1.Count - 1; i >= 0; i--)
                        {
                            if (!cacheMgr.Contains(keys1[i]))
                            {
                                isChanged = true;
                                keys1.RemoveAt(i);
                            }
                        }

                        if (isChanged)
                        {
                            hashSet.Add(key, keys1);
                        }
                    });
            }
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
            private List<Type> types = new List<Type>();

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
    }
}
