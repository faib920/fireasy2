// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common;
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Extensions;
using Fireasy.Common.Linq.Expressions;
using Fireasy.Common.Reflection;
using Fireasy.Data.Entity.Linq.Translators;
using Fireasy.Data.Entity.Metadata;
using Fireasy.Data.Entity.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Data.Entity.Linq
{
    /// <summary>
    /// LINQ 扩展方法。
    /// </summary>
    public static class Extensions
    {
        private class MethodCache
        {
            internal protected static readonly MethodInfo CreateEntityAsync = typeof(Extensions).GetMethod(nameof(Extensions.CreateEntityAsync), BindingFlags.NonPublic | BindingFlags.Static);
            internal protected static readonly MethodInfo RemoveWhereAsync = typeof(Extensions).GetMethod(nameof(Extensions.RemoveWhereAsync), BindingFlags.NonPublic | BindingFlags.Static);
            internal protected static readonly MethodInfo UpdateWhereAsync = typeof(Extensions).GetMethod(nameof(Extensions.UpdateWhereAsync), BindingFlags.NonPublic | BindingFlags.Static);
            internal protected static readonly MethodInfo UpdateWhereByCalculatorAsync = typeof(Extensions).GetMethod(nameof(Extensions.UpdateWhereByCalculatorAsync), BindingFlags.NonPublic | BindingFlags.Static);
            internal protected static readonly MethodInfo BatchOperateAsync = typeof(Extensions).GetMethod(nameof(Extensions.BatchOperateAsync), BindingFlags.NonPublic | BindingFlags.Static);
            internal protected static readonly MethodInfo FirstOrDefaultAsync2 = typeof(Extensions).GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(s => s.Name == nameof(Extensions.FirstOrDefaultAsync) && s.GetParameters().Length == 2);
            internal protected static readonly MethodInfo FirstOrDefaultAsync3 = typeof(Extensions).GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(s => s.Name == nameof(Extensions.FirstOrDefaultAsync) && s.GetParameters().Length == 3);
            internal protected static readonly MethodInfo LastOrDefaultAsync2 = typeof(Extensions).GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(s => s.Name == nameof(Extensions.LastOrDefaultAsync) && s.GetParameters().Length == 2);
            internal protected static readonly MethodInfo LastOrDefaultAsync3 = typeof(Extensions).GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(s => s.Name == nameof(Extensions.LastOrDefaultAsync) && s.GetParameters().Length == 3);
            internal protected static readonly MethodInfo SingleOrDefaultAsync2 = typeof(Extensions).GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(s => s.Name == nameof(Extensions.SingleOrDefaultAsync) && s.GetParameters().Length == 2);
            internal protected static readonly MethodInfo SingleOrDefaultAsync3 = typeof(Extensions).GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(s => s.Name == nameof(Extensions.SingleOrDefaultAsync) && s.GetParameters().Length == 3);
            internal protected static readonly MethodInfo AnyAsync2 = typeof(Extensions).GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(s => s.Name == nameof(Extensions.AnyAsync) && s.GetParameters().Length == 2);
            internal protected static readonly MethodInfo AnyAsync3 = typeof(Extensions).GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(s => s.Name == nameof(Extensions.AnyAsync) && s.GetParameters().Length == 3);
            internal protected static readonly MethodInfo AllAsync2 = typeof(Extensions).GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(s => s.Name == nameof(Extensions.AllAsync) && s.GetParameters().Length == 2);
            internal protected static readonly MethodInfo AllAsync3 = typeof(Extensions).GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(s => s.Name == nameof(Extensions.AllAsync) && s.GetParameters().Length == 3);
            internal protected static readonly MethodInfo CountAsync2 = typeof(Extensions).GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(s => s.Name == nameof(Extensions.CountAsync) && s.GetParameters().Length == 2);
            internal protected static readonly MethodInfo CountAsync3 = typeof(Extensions).GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(s => s.Name == nameof(Extensions.CountAsync) && s.GetParameters().Length == 3);
            internal protected static readonly MethodInfo AverageAsync2 = typeof(Extensions).GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(s => s.Name == nameof(Extensions.AverageAsync) && s.GetParameters().Length == 2);
            internal protected static readonly MethodInfo AverageAsync3 = typeof(Extensions).GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(s => s.Name == nameof(Extensions.AverageAsync) && s.GetParameters().Length == 3);
            internal protected static readonly MethodInfo SumAsync2 = typeof(Extensions).GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(s => s.Name == nameof(Extensions.SumAsync) && s.GetParameters().Length == 2);
            internal protected static readonly MethodInfo SumAsync3 = typeof(Extensions).GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(s => s.Name == nameof(Extensions.SumAsync) && s.GetParameters().Length == 3);
            internal protected static readonly MethodInfo MinAsync2 = typeof(Extensions).GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(s => s.Name == nameof(Extensions.MinAsync) && s.GetParameters().Length == 2);
            internal protected static readonly MethodInfo MinAsync3 = typeof(Extensions).GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(s => s.Name == nameof(Extensions.MinAsync) && s.GetParameters().Length == 3);
            internal protected static readonly MethodInfo MaxAsync2 = typeof(Extensions).GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(s => s.Name == nameof(Extensions.MaxAsync) && s.GetParameters().Length == 2);
            internal protected static readonly MethodInfo MaxAsync3 = typeof(Extensions).GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(s => s.Name == nameof(Extensions.MaxAsync) && s.GetParameters().Length == 3);
            internal protected static readonly MethodInfo ToListAsync = typeof(Extensions).GetMethod(nameof(Extensions.ToListAsync), BindingFlags.Public | BindingFlags.Static);
            internal protected static readonly MethodInfo ExtendGenericAs = typeof(Extensions).GetMethod(nameof(Extensions.ExtendGenericAs), BindingFlags.NonPublic | BindingFlags.Static);
        }

        /// <summary>
        /// 使用 <see cref="IDataSegment"/> 对象对序列进行分段筛选，如果使用 <see cref="DataPager"/>，则可返回详细的分页信息(数据行数和页码总数)。
        /// </summary>
        /// <typeparam name="TSource">数据类型。</typeparam>
        /// <param name="source">要进行分段的序列。</param>
        /// <param name="segment">分段对象，可使用 <see cref="DataPager"/>。</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">参数 source 为空时抛出此异常。</exception>
        public static IQueryable<TSource> Segment<TSource>(this IQueryable<TSource> source, IDataSegment segment)
        {
            if (source == null || segment == null)
            {
                return source;
            }

            var method = (MethodInfo)MethodBase.GetCurrentMethod();
            method = method.MakeGenericMethod(new[] { typeof(TSource) });
            var expression = Expression.Call(null, method,
                new[] { source.Expression, Expression.Constant(segment) });

            return source.Provider.CreateQuery<TSource>(expression);
        }

        /// <summary>
        /// 标记此查询不使用状态跟踪。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IQueryable<TSource> AsNoTracking<TSource>(this IQueryable<TSource> source)
        {
            var method = (MethodInfo)MethodBase.GetCurrentMethod();
            method = method.MakeGenericMethod(typeof(TSource));
            var expression = Expression.Call(null, method,
                new[] { source.Expression });

            return source.Provider.CreateQuery<TSource>(expression);
        }

        /// <summary>
        /// 设置是否允许 LINQ 解析放入到缓存中。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="enabled">为 true 时开启缓存。</param>
        /// <param name="expired">过期时间。</param>
        /// <returns></returns>
        public static IQueryable<TSource> CacheParsing<TSource>(this IQueryable<TSource> source, bool enabled, TimeSpan expired)
        {
            var method = (MethodInfo)MethodBase.GetCurrentMethod();
            method = method.MakeGenericMethod(typeof(TSource));
            var expression = Expression.Call(null, method,
                new[] { source.Expression, Expression.Constant(enabled), Expression.Constant(expired) });

            return source.Provider.CreateQuery<TSource>(expression);
        }

        /// <summary>
        /// 设置是否允许 LINQ 解析放入到缓存中。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="enabled">为 true 时开启缓存。</param>
        /// <returns></returns>
        public static IQueryable<TSource> CacheParsing<TSource>(this IQueryable<TSource> source, bool enabled)
        {
            var method = (MethodInfo)MethodBase.GetCurrentMethod();
            method = method.MakeGenericMethod(typeof(TSource));
            var expression = Expression.Call(null, method,
                new[] { source.Expression, Expression.Constant(enabled) });

            return source.Provider.CreateQuery<TSource>(expression);
        }

        /// <summary>
        /// 设置是否允许将执行结果放入缓存中。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="enabled">为 true 时开启缓存。</param>
        /// <param name="expired">过期时间(秒)。</param>
        /// <returns></returns>
        public static IQueryable<TSource> CacheExecution<TSource>(this IQueryable<TSource> source, bool enabled, TimeSpan expired)
        {
            var method = (MethodInfo)MethodBase.GetCurrentMethod();
            method = method.MakeGenericMethod(typeof(TSource));
            var expression = Expression.Call(null, method,
                new[] { source.Expression, Expression.Constant(enabled), Expression.Constant(expired) });

            return source.Provider.CreateQuery<TSource>(expression);
        }

        /// <summary>
        /// 设置是否允许将执行结果放入缓存中。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="enabled">为 true 时开启缓存。</param>
        /// <returns></returns>
        public static IQueryable<TSource> CacheExecution<TSource>(this IQueryable<TSource> source, bool enabled)
        {
            var method = (MethodInfo)MethodBase.GetCurrentMethod();
            method = method.MakeGenericMethod(typeof(TSource));
            var expression = Expression.Call(null, method,
                new[] { source.Expression, Expression.Constant(enabled) });

            return source.Provider.CreateQuery<TSource>(expression);
        }

        /// <summary>
        /// 将查询转换为带分页信息的结构输出。查询中需要使用 Segment 扩展方法带入 <see cref="IPager"/> 分页对象。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static PaginalResult<TSource> ToPaginalResult<TSource>(this IQueryable<TSource> source)
        {
            var segment = SegmentFinder.Find(source.Expression);
            var list = source.ToList();

            return new PaginalResult<TSource>(list, segment as IPager);
        }

        /// <summary>
        /// 根据断言进行序列的筛选。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="isTrue">要计算的条件表达式。如果条件为 true，则进行筛选，否则不筛选。</param>
        /// <param name="isTruePredicate">用于条件为 true 时测试每个元素是否满足条件的函数。</param>
        /// <returns></returns>
        public static IQueryable<TSource> AssertWhere<TSource>(this IQueryable<TSource> source, bool isTrue, Expression<Func<TSource, bool>> isTruePredicate)
        {
            if (source == null || !isTrue)
            {
                return source;
            }

            return source.Where(isTruePredicate);
        }

        /// <summary>
        /// 根据断言进行序列的筛选。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="isTrue">要计算的条件表达式。如果条件为 true，则进行筛选，否则不筛选。</param>
        /// <param name="isTruePredicate">用于条件为 true 时测试每个元素是否满足条件的函数。</param>
        /// <param name="isFalsePredicate">用于条件为 false 时测试每个元素是否满足条件的函数。</param>
        /// <returns></returns>
        public static IQueryable<TSource> AssertWhere<TSource>(this IQueryable<TSource> source, bool isTrue, Expression<Func<TSource, bool>> isTruePredicate, Expression<Func<TSource, bool>> isFalsePredicate)
        {
            if (source == null)
            {
                return source;
            }

            return isTrue ? source.Where(isTruePredicate) : source.Where(isFalsePredicate);
        }

        /// <summary>
        /// 根据断言进行序列的筛选。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="isTrue">要计算的条件表达式。如果条件为 true，则进行筛选，否则不筛选。</param>
        /// <param name="isTruePredicate">用于条件为 true 时测试每个元素是否满足条件的函数。</param>
        /// <param name="otherwise">如果条件不成立，则采用其他的查询。</param>
        /// <returns></returns>
        public static IQueryable<TSource> AssertWhere<TSource>(this IQueryable<TSource> source, bool isTrue, Expression<Func<TSource, bool>> isTruePredicate, Func<IQueryable<TSource>, IQueryable<TSource>> otherwise)
        {
            if (source == null || !isTrue)
            {
                if (otherwise != null)
                {
                    return otherwise(source);
                }

                return source;
            }

            return source.Where(isTruePredicate);
        }

        /// <summary>
        /// 使用 Switch 判断不同的值时所应用的筛选。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="source"></param>
        /// <param name="value">变量的值。</param>
        /// <param name="buildAction">构造表达式的方法。</param>
        /// <returns></returns>
        public static IQueryable<TSource> SwitchWhere<TSource, TValue>(this IQueryable<TSource> source, TValue value, Action<SwitchBuilder<TSource, TValue>> buildAction) where TValue : IComparable
        {
            if (buildAction == null)
            {
                return source;
            }

            var builder = new SwitchBuilder<TSource, TValue>(value);
            buildAction(builder);

            return builder.Expression == null ? source : source.Where(builder.Expression);
        }

        /// <summary>
        /// 在 Select 中应用 <see cref="ExtendAs{TSource, TResult}(TSource, Expression{Func{TResult}})"/> 来扩展返回的结果。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static IQueryable<TResult> ExtendSelect<TSource, TResult>(this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selector)
        {
            var parExp = Expression.Parameter(typeof(TSource), "t");

            var method = MethodCache.ExtendGenericAs.MakeGenericMethod(typeof(TSource), typeof(TResult));

            var newExp = ExpressionReplacer.Replace(selector.Body, parExp);
            var newSelector = Expression.Lambda<Func<TResult>>(newExp);

            var callExp = Expression.Call(null, method, parExp, newSelector);
            var lambda = Expression.Lambda(callExp, parExp);

            var expression = Expression.Call(typeof(Queryable), nameof(Queryable.Select), new[] { typeof(TSource), typeof(TResult) }, source.Expression, lambda);

            return source.Provider.CreateQuery<TResult>(expression);
        }

        /// <summary>
        /// 使用集合中的元素根据测试条件进行(或者)连接。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TVar"></typeparam>
        /// <param name="source"></param>
        /// <param name="collection">包含测试条件中第二个参数的对象集合。</param>
        /// <param name="predicate">用于连接的测试函数。</param>
        /// <returns></returns>
        public static IQueryable<TSource> BatchOr<TSource, TVar>(this IQueryable<TSource> source, IEnumerable<TVar> collection, Expression<Func<TSource, TVar, bool>> predicate)
        {
            if (source == null || collection == null || !collection.Any() || predicate == null)
            {
                return source;
            }

            var parExp = Expression.Parameter(typeof(TSource), "s");
            var expression = collection.Select(v => ParameterRewriter.Rewrite(predicate.Body, parExp, v)).Aggregate(Expression.Or);

            if (expression == null)
            {
                return source;
            }

            var lambda = Expression.Lambda<Func<TSource, bool>>(expression, parExp);
            return source.Where(lambda);
        }

        /// <summary>
        /// 使用集合中的元素根据测试条件进行(并且)连接。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TVar"></typeparam>
        /// <param name="source"></param>
        /// <param name="collection">包含测试条件中第二个参数的对象集合。</param>
        /// <param name="predicate">用于连接的测试函数。</param>
        /// <returns></returns>
        public static IQueryable<TSource> BatchAnd<TSource, TVar>(this IQueryable<TSource> source, IEnumerable<TVar> collection, Expression<Func<TSource, TVar, bool>> predicate)
        {
            if (source == null || collection == null || !collection.Any() || predicate == null)
            {
                return source;
            }

            var parExp = Expression.Parameter(typeof(TSource), "s");
            var expression = collection.Select(v => ParameterRewriter.Rewrite(predicate.Body, parExp, v)).Aggregate(Expression.And);

            if (expression == null)
            {
                return source;
            }

            var lambda = Expression.Lambda<Func<TSource, bool>>(expression, parExp);
            return source.Where(lambda);
        }

        /// <summary>
        /// 使用一个 SQL 条件表达式。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="condition">一个条件表达式。</param>
        /// <param name="parameterAct">用于初始化参数的方法。</param>
        /// <returns></returns>
        public static IQueryable<TSource> Where<TSource>(this IQueryable<TSource> source, string condition, Action<ParameterCollection> parameterAct = null)
        {
            if (source == null || string.IsNullOrEmpty(condition))
            {
                return source;
            }

            var parameters = new ParameterCollection();
            parameterAct?.Invoke(parameters);

            return Where(source, condition, parameters);
        }

        /// <summary>
        /// 使用一个 SQL 条件表达式。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="condition">一个条件表达式。</param>
        /// <param name="parameters">一个参数集合。</param>
        /// <returns></returns>
        public static IQueryable<TSource> Where<TSource>(this IQueryable<TSource> source, string condition, ParameterCollection parameters)
        {
            if (source == null || string.IsNullOrEmpty(condition))
            {
                return source;
            }

            var method = (MethodInfo)MethodBase.GetCurrentMethod();

            var expression = Expression.Call(method.MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(condition), Expression.Constant(parameters));

            return source.Provider.CreateQuery<TSource>(expression);
        }

        /// <summary>
        /// 使用一个排序定义对象对集合中的元素进行排序。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="sort">排序定义。</param>
        /// <param name="otherwise">当 <paramref name="sort"/> 为 Empty 时，使用此表达式进行排序。</param>
        /// <returns></returns>
        public static IQueryable<TSource> OrderBy<TSource>(this IQueryable<TSource> source, SortDefinition sort, Func<IQueryable<TSource>, IQueryable<TSource>> otherwise = null)
        {
            if (sort == null)
            {
                return otherwise == null ? source : otherwise(source);
            }

            return source.OrderBy(sort.Member, sort.Order, otherwise);
        }

        /// <summary>
        /// 使用一个排序定义对象对集合中的元素执行后续排序。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="sort">排序定义。</param>
        /// <param name="otherwise">当 <paramref name="sort"/> 为 Empty 时，使用此表达式进行排序。</param>
        /// <returns></returns>
        public static IQueryable<TSource> ThenBy<TSource>(this IQueryable<TSource> source, SortDefinition sort, Func<IQueryable<TSource>, IQueryable<TSource>> otherwise = null)
        {
            if (sort == null)
            {
                return otherwise == null ? source : otherwise(source);
            }

            return source.ThenBy(sort.Member, sort.Order, otherwise);
        }

        /// <summary>
        /// 使用一个属性名对集合中的元素进行排序。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="memberName">作为排序的键名称。</param>
        /// <param name="sortOrder">排序类型。</param>
        /// <param name="otherwise">当 <paramref name="memberName"/> 为空时，使用此表达式进行排序。</param>
        /// <returns></returns>
        public static IQueryable<TSource> OrderBy<TSource>(this IQueryable<TSource> source, string memberName, SortOrder sortOrder = SortOrder.None, Func<IQueryable<TSource>, IQueryable<TSource>> otherwise = null)
        {
            if (string.IsNullOrEmpty(memberName) && sortOrder == SortOrder.None)
            {
                return otherwise == null ? source : otherwise(source);
            }

            var methodName = sortOrder == SortOrder.Ascending ? nameof(Queryable.OrderBy) : nameof(Queryable.OrderByDescending);

            return CreateOrderExpression(source, methodName, memberName);
        }

        /// <summary>
        /// 使用一个属性名对集合中的元素执行后续排序。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="memberName">作为排序的键名称。</param>
        /// <param name="sortOrder">排序类型。</param>
        /// <param name="otherwise">当 <paramref name="memberName"/> 为空时，使用此表达式进行排序。</param>
        /// <returns></returns>
        public static IQueryable<TSource> ThenBy<TSource>(this IQueryable<TSource> source, string memberName, SortOrder sortOrder = SortOrder.Ascending, Func<IQueryable<TSource>, IQueryable<TSource>> otherwise = null)
        {
            if (string.IsNullOrEmpty(memberName) && sortOrder == SortOrder.None)
            {
                return otherwise == null ? source : otherwise(source);
            }

            var methodName = sortOrder == SortOrder.Ascending ? nameof(Queryable.ThenBy) : nameof(Queryable.ThenByDescending);

            return CreateOrderExpression(source, methodName, memberName);
        }

        /// <summary>
        /// 删除前导查询中的数据。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="source"></param>
        /// <param name="logicalDelete">是否逻辑删除。</param>
        /// <returns></returns>
        public static int Delete<TEntity>(this IQueryable<TEntity> source, bool logicalDelete = true) where TEntity : IEntity
        {
            var predicate = PredicateGatherer<TEntity>.Gather(source.Expression);
            if (predicate == null)
            {
                return -1;
            }

            return RemoveWhere(source, predicate, logicalDelete);
        }

        /// <summary>
        /// 异步的，删除前导查询中的数据。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="source"></param>
        /// <param name="logicalDelete">是否逻辑删除。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        public static async Task<int> DeleteAsync<TEntity>(this IQueryable<TEntity> source, bool logicalDelete = true, CancellationToken cancellationToken = default) where TEntity : IEntity
        {
            var predicate = PredicateGatherer<TEntity>.Gather(source.Expression);
            if (predicate == null)
            {
                return -1;
            }

            return await RemoveWhereAsync(source, predicate, logicalDelete, cancellationToken);
        }

        /// <summary>
        /// 更新前导查询中的数据。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="source"></param>
        /// <param name="entity">更新的参考对象。</param>
        /// <returns></returns>
        public static int Update<TEntity>(this IQueryable<TEntity> source, TEntity entity) where TEntity : IEntity
        {
            var predicate = PredicateGatherer<TEntity>.Gather(source.Expression);
            if (predicate == null)
            {
                return -1;
            }

            return UpdateWhere(source, entity, predicate);
        }

        /// <summary>
        /// 异步的，更新前导查询中的数据。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="source"></param>
        /// <param name="entity">更新的参考对象。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        public static async Task<int> UpdateAsync<TEntity>(this IQueryable<TEntity> source, TEntity entity, CancellationToken cancellationToken = default) where TEntity : IEntity
        {
            var predicate = PredicateGatherer<TEntity>.Gather(source.Expression);
            if (predicate == null)
            {
                return -1;
            }

            return await UpdateWhereAsync(source, entity, predicate, cancellationToken);
        }

        /// <summary>
        /// 更新前导查询中的数据。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="source"></param>
        /// <param name="valueCreator">一个构造实例并成员绑定的表达式。</param>
        /// <returns></returns>
        public static int Update<TEntity>(this IQueryable<TEntity> source, Expression<Func<TEntity>> valueCreator) where TEntity : IEntity
        {
            var predicate = PredicateGatherer<TEntity>.Gather(source.Expression);
            if (predicate == null)
            {
                return -1;
            }

            var entity = EntityProxyManager.GetType(source.Provider as IContextTypeAware, typeof(TEntity)).New<TEntity>();
            entity.InitByExpression(valueCreator);

            return UpdateWhere(source, entity, predicate);
        }

        /// <summary>
        /// 异步的，更新前导查询中的数据。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="source"></param>
        /// <param name="valueCreator">一个构造实例并成员绑定的表达式。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        public static async Task<int> UpdateAsync<TEntity>(this IQueryable<TEntity> source, Expression<Func<TEntity>> valueCreator, CancellationToken cancellationToken = default) where TEntity : IEntity
        {
            var predicate = PredicateGatherer<TEntity>.Gather(source.Expression);
            if (predicate == null)
            {
                return -1;
            }

            var entity = EntityProxyManager.GetType(source.Provider as IContextTypeAware, typeof(TEntity)).New<TEntity>();
            entity.InitByExpression(valueCreator);

            return await UpdateWhereAsync(source, entity, predicate, cancellationToken);
        }

        /// <summary>
        /// 更新前导查询中的数据。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="source"></param>
        /// <param name="initializer">一个初始化成员的函数。</param>
        /// <returns></returns>
        public static int Update<TEntity>(this IQueryable<TEntity> source, Action<TEntity> initializer) where TEntity : IEntity
        {
            Guard.ArgumentNull(initializer, nameof(initializer));

            var predicate = PredicateGatherer<TEntity>.Gather(source.Expression);
            if (predicate == null)
            {
                return -1;
            }

            var entity = EntityProxyManager.GetType(source.Provider as IContextTypeAware, typeof(TEntity)).New<TEntity>();
            initializer(entity);

            return UpdateWhere(source, entity, predicate);
        }

        /// <summary>
        /// 异步的，更新前导查询中的数据。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="source"></param>
        /// <param name="initializer">一个初始化成员的函数。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        public static async Task<int> UpdateAsync<TEntity>(this IQueryable<TEntity> source, Action<TEntity> initializer, CancellationToken cancellationToken = default) where TEntity : IEntity
        {
            Guard.ArgumentNull(initializer, nameof(initializer));

            var predicate = PredicateGatherer<TEntity>.Gather(source.Expression);
            if (predicate == null)
            {
                return -1;
            }

            var entity = EntityProxyManager.GetType(source.Provider as IContextTypeAware, typeof(TEntity)).New<TEntity>();
            initializer(entity);

            return await UpdateWhereAsync(source, entity, predicate, cancellationToken);
        }

        /// <summary>
        /// 将实体进行扩展，附加 <paramref name="selector"/> 表达式中的字段，返回 <typeparamref name="TEntity"/> 类型的对象。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static TEntity ExtendAs<TEntity>(this IEntity entity, Expression<Func<object>> selector)
        {
            throw new NotSupportedException(SR.GetString(SRKind.MethodMustInExpression));
        }

        /// <summary>
        /// 将实体进行动态扩展，附加 <paramref name="selector"/> 表达式中的字段。
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static dynamic Extend(this IEntity entity, Expression<Func<object>> selector)
        {
            throw new NotSupportedException(SR.GetString(SRKind.MethodMustInExpression));
        }

        /// <summary>
        /// 将实体进行扩展，附加 <paramref name="selector"/> 表达式中的字段，返回 <typeparamref name="TResult"/> 类型的对象。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        internal static TResult ExtendGenericAs<TSource, TResult>(this TSource source, Expression<Func<TResult>> selector)
        {
            throw new NotSupportedException(SR.GetString(SRKind.MethodMustInExpression));
        }

        /// <summary>
        /// 异步的，将序列转换为数组。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<TSource[]> ToArrayAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
        {
            return (await source.ToListAsync(cancellationToken)).ToArray();
        }

        /// <summary>
        /// 异步的，将序列转换为 List。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<List<TSource>> ToListAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
        {
            CheckAsyncImplementd(source.Provider);

            var method = MethodCache.ToListAsync.MakeGenericMethod(typeof(TSource));
            var expression = Expression.Call(null, method,
                new[] { (Expression)Expression.Constant(source), Expression.Constant(cancellationToken, typeof(CancellationToken)) });

#if NETSTANDARD && !NETSTANDARD2_0
            var enumerable = ((IAsyncQueryProvider)source.Provider).ExecuteEnumerableAsync<TSource>(expression, cancellationToken);
            if (enumerable == null)
            {
                return null;
            }

            var result = new List<TSource>();
            await foreach (var item in enumerable)
            {
                result.Add(item);
            }

            return result;
#else
            var enumerable = await ((IAsyncQueryProvider)source.Provider).ExecuteAsync<IEnumerable<TSource>>(expression, cancellationToken);
            return enumerable.ToList();
#endif
        }

        /// <summary>
        /// 异步的，返回序列中的第一个元素。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
        {
            CheckAsyncImplementd(source.Provider);

            var method = MethodCache.FirstOrDefaultAsync2.MakeGenericMethod(typeof(TSource));
            var expression = Expression.Call(null, method,
                new[] { (Expression)Expression.Constant(source), Expression.Constant(cancellationToken, typeof(CancellationToken)) });

            return await ((IAsyncQueryProvider)source.Provider).ExecuteAsync<TSource>(expression, cancellationToken);
        }

        /// <summary>
        /// 异步的，返回序列中的第一个元素。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
        {
            CheckAsyncImplementd(source.Provider);

            var method = MethodCache.FirstOrDefaultAsync3.MakeGenericMethod(typeof(TSource));
            var expression = Expression.Call(null, method,
                new[] { (Expression)Expression.Constant(source), predicate, Expression.Constant(cancellationToken, typeof(CancellationToken)) });

            return await ((IAsyncQueryProvider)source.Provider).ExecuteAsync<TSource>(expression, cancellationToken);
        }

        /// <summary>
        /// 异步的，返回序列中的最后一个元素。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<TSource> LastOrDefaultAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
        {
            CheckAsyncImplementd(source.Provider);

            var method = MethodCache.LastOrDefaultAsync2.MakeGenericMethod(typeof(TSource));
            var expression = Expression.Call(null, method,
                new[] { (Expression)Expression.Constant(source), Expression.Constant(cancellationToken, typeof(CancellationToken)) });

            return await ((IAsyncQueryProvider)source.Provider).ExecuteAsync<TSource>(expression, cancellationToken);
        }

        /// <summary>
        /// 异步的，返回序列中的最后一个元素。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<TSource> LastOrDefaultAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
        {
            CheckAsyncImplementd(source.Provider);

            var method = MethodCache.LastOrDefaultAsync3.MakeGenericMethod(typeof(TSource));
            var expression = Expression.Call(null, method,
                new[] { (Expression)Expression.Constant(source), predicate, Expression.Constant(cancellationToken, typeof(CancellationToken)) });

            return await ((IAsyncQueryProvider)source.Provider).ExecuteAsync<TSource>(expression, cancellationToken);
        }

        /// <summary>
        /// 异步的，返回序列中的唯一一个元素。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<TSource> SingleOrDefaultAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
        {
            CheckAsyncImplementd(source.Provider);

            var method = MethodCache.SingleOrDefaultAsync2.MakeGenericMethod(typeof(TSource));
            var expression = Expression.Call(null, method,
                new[] { (Expression)Expression.Constant(source), Expression.Constant(cancellationToken, typeof(CancellationToken)) });

            return await ((IAsyncQueryProvider)source.Provider).ExecuteAsync<TSource>(expression, cancellationToken);
        }

        /// <summary>
        /// 异步的，返回序列中的唯一一个元素。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<TSource> SingleOrDefaultAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
        {
            CheckAsyncImplementd(source.Provider);

            var method = MethodCache.SingleOrDefaultAsync3.MakeGenericMethod(typeof(TSource));
            var expression = Expression.Call(null, method,
                new[] { (Expression)Expression.Constant(source), predicate, Expression.Constant(cancellationToken, typeof(CancellationToken)) });

            return await ((IAsyncQueryProvider)source.Provider).ExecuteAsync<TSource>(expression, cancellationToken);
        }

        /// <summary>
        /// 异步的，返回序列中是否有任意一个满足条件的元素。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<bool> AnyAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
        {
            CheckAsyncImplementd(source.Provider);

            var method = MethodCache.AnyAsync2.MakeGenericMethod(typeof(TSource));
            var expression = Expression.Call(null, method,
                new[] { (Expression)Expression.Constant(source), Expression.Constant(cancellationToken, typeof(CancellationToken)) });

            return await ((IAsyncQueryProvider)source.Provider).ExecuteAsync<bool>(expression, cancellationToken);
        }

        /// <summary>
        /// 异步的，返回序列中是否有任意一个满足条件的元素。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<bool> AnyAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
        {
            CheckAsyncImplementd(source.Provider);

            var method = MethodCache.AnyAsync3.MakeGenericMethod(typeof(TSource));
            var expression = Expression.Call(null, method,
                new[] { (Expression)Expression.Constant(source), predicate, Expression.Constant(cancellationToken, typeof(CancellationToken)) });

            return await ((IAsyncQueryProvider)source.Provider).ExecuteAsync<bool>(expression, cancellationToken);
        }

        /// <summary>
        /// 异步的，返回序列中所有元素是否均满足条件。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<bool> AllAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
        {
            CheckAsyncImplementd(source.Provider);

            var method = MethodCache.AllAsync2.MakeGenericMethod(typeof(TSource));
            var expression = Expression.Call(null, method,
                new[] { (Expression)Expression.Constant(source), Expression.Constant(cancellationToken, typeof(CancellationToken)) });

            return await ((IAsyncQueryProvider)source.Provider).ExecuteAsync<bool>(expression, cancellationToken);
        }

        /// <summary>
        /// 异步的，返回序列中所有元素是否均满足条件。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<bool> AllAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
        {
            CheckAsyncImplementd(source.Provider);

            var method = MethodCache.AllAsync3.MakeGenericMethod(typeof(TSource));
            var expression = Expression.Call(null, method,
                new[] { (Expression)Expression.Constant(source), predicate, Expression.Constant(cancellationToken, typeof(CancellationToken)) });

            return await ((IAsyncQueryProvider)source.Provider).ExecuteAsync<bool>(expression, cancellationToken);
        }

        /// <summary>
        /// 异步的，返回序列中元素的个数。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<int> CountAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
        {
            CheckAsyncImplementd(source.Provider);

            var method = MethodCache.CountAsync2.MakeGenericMethod(typeof(TSource));
            var expression = Expression.Call(null, method,
                new[] { (Expression)Expression.Constant(source), Expression.Constant(cancellationToken, typeof(CancellationToken)) });

            return await ((IAsyncQueryProvider)source.Provider).ExecuteAsync<int>(expression, cancellationToken);
        }

        /// <summary>
        /// 异步的，返回序列中元素的个数。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<int> CountAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
        {
            CheckAsyncImplementd(source.Provider);

            var method = MethodCache.CountAsync3.MakeGenericMethod(typeof(TSource));
            var expression = Expression.Call(null, method,
                new[] { (Expression)Expression.Constant(source), predicate, Expression.Constant(cancellationToken, typeof(CancellationToken)) });

            return await ((IAsyncQueryProvider)source.Provider).ExecuteAsync<int>(expression, cancellationToken);
        }

        /// <summary>
        /// 异步的，计算序列的平均值。
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<TResult> AverageAsync<TResult>(this IQueryable<TResult> source, CancellationToken cancellationToken = default)
        {
            CheckAsyncImplementd(source.Provider);

            var method = MethodCache.AverageAsync2.MakeGenericMethod(typeof(TResult));
            var expression = Expression.Call(null, method,
                new[] { (Expression)Expression.Constant(source), Expression.Constant(cancellationToken, typeof(CancellationToken)) });

            return await ((IAsyncQueryProvider)source.Provider).ExecuteAsync<TResult>(expression, cancellationToken);
        }

        /// <summary>
        /// 异步的，计算序列的平均值。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector">每一个元素的投影函数。</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<TResult> AverageAsync<TSource, TResult>(this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selector, CancellationToken cancellationToken = default)
        {
            CheckAsyncImplementd(source.Provider);

            var method = MethodCache.AverageAsync3.MakeGenericMethod(typeof(TSource), typeof(TResult));
            var expression = Expression.Call(null, method,
                new[] { (Expression)Expression.Constant(source), selector, Expression.Constant(cancellationToken, typeof(CancellationToken)) });

            return await ((IAsyncQueryProvider)source.Provider).ExecuteAsync<TResult>(expression, cancellationToken);
        }

        /// <summary>
        /// 异步的，计算序列总合。
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<TResult> SumAsync<TResult>(this IQueryable<TResult> source, CancellationToken cancellationToken = default)
        {
            CheckAsyncImplementd(source.Provider);

            var method = MethodCache.SumAsync2.MakeGenericMethod(typeof(TResult));
            var expression = Expression.Call(null, method,
                new[] { (Expression)Expression.Constant(source), Expression.Constant(cancellationToken, typeof(CancellationToken)) });

            return await ((IAsyncQueryProvider)source.Provider).ExecuteAsync<TResult>(expression, cancellationToken);
        }

        /// <summary>
        /// 异步的，计算序列总合。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector">每一个元素的投影函数。</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<TResult> SumAsync<TSource, TResult>(this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selector, CancellationToken cancellationToken = default)
        {
            CheckAsyncImplementd(source.Provider);

            var method = MethodCache.SumAsync3.MakeGenericMethod(typeof(TSource), typeof(TResult));
            var expression = Expression.Call(null, method,
                new[] { (Expression)Expression.Constant(source), selector, Expression.Constant(cancellationToken, typeof(CancellationToken)) });

            return await ((IAsyncQueryProvider)source.Provider).ExecuteAsync<TResult>(expression, cancellationToken);
        }

        /// <summary>
        /// 异步的，计算序列中的最大值。
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<TResult> MaxAsync<TResult>(this IQueryable<TResult> source, CancellationToken cancellationToken = default)
        {
            CheckAsyncImplementd(source.Provider);

            var method = MethodCache.MaxAsync2.MakeGenericMethod(typeof(TResult));
            var expression = Expression.Call(null, method,
                new[] { (Expression)Expression.Constant(source), Expression.Constant(cancellationToken, typeof(CancellationToken)) });

            return await ((IAsyncQueryProvider)source.Provider).ExecuteAsync<TResult>(expression, cancellationToken);
        }

        /// <summary>
        /// 异步的，计算序列中的最大值。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector">每一个元素的投影函数。</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<TResult> MaxAsync<TSource, TResult>(this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selector, CancellationToken cancellationToken = default)
        {
            CheckAsyncImplementd(source.Provider);

            var method = MethodCache.MaxAsync3.MakeGenericMethod(typeof(TSource), typeof(TResult));
            var expression = Expression.Call(null, method,
                new[] { (Expression)Expression.Constant(source), selector, Expression.Constant(cancellationToken, typeof(CancellationToken)) });

            return await ((IAsyncQueryProvider)source.Provider).ExecuteAsync<TResult>(expression, cancellationToken);
        }

        /// <summary>
        /// 异步的，计算序列中的最小值。
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<TResult> MinAsync<TResult>(this IQueryable<TResult> source, CancellationToken cancellationToken = default)
        {
            CheckAsyncImplementd(source.Provider);

            var method = MethodCache.MinAsync2.MakeGenericMethod(typeof(TResult));
            var expression = Expression.Call(null, method,
                new[] { (Expression)Expression.Constant(source), Expression.Constant(cancellationToken, typeof(CancellationToken)) });

            return await ((IAsyncQueryProvider)source.Provider).ExecuteAsync<TResult>(expression, cancellationToken);
        }

        /// <summary>
        /// 异步的，计算序列中的最小值。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector">每一个元素的投影函数。</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<TResult> MinAsync<TSource, TResult>(this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selector, CancellationToken cancellationToken = default)
        {
            CheckAsyncImplementd(source.Provider);

            var method = MethodCache.MinAsync3.MakeGenericMethod(typeof(TSource), typeof(TResult));
            var expression = Expression.Call(null, method,
                new[] { (Expression)Expression.Constant(source), selector, Expression.Constant(cancellationToken, typeof(CancellationToken)) });

            return await ((IAsyncQueryProvider)source.Provider).ExecuteAsync<TResult>(expression, cancellationToken);
        }

#if NETSTANDARD && !NETSTANDARD2_0
        /// <summary>
        /// 异步的，将序列转换成异步枚举。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IQueryable<T> source)
        {
            return (IAsyncEnumerable<T>)source;
        }
#endif

        /// <summary>
        /// 使用主键返回一个实体。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="primaryValues"></param>
        /// <returns></returns>
        internal static T GetByPrimary<T>(this IQueryable source, PropertyValue[] primaryValues)
        {
            Guard.ArgumentNull(primaryValues, nameof(primaryValues));

            var predicate = BindPrimaryExpression(source.ElementType, primaryValues);
            if (predicate == null)
            {
                return default;
            }

            var expression = Expression.Call(typeof(Queryable), nameof(Queryable.FirstOrDefault), new[] { source.ElementType }, Expression.Constant(source), (Expression)predicate);

            return source.Provider.Execute<T>(expression);
        }

        /// <summary>
        /// 异步的，使用主键返回一个实体。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="primaryValues"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal static async Task<T> GetByPrimaryAsync<T>(this IQueryable source, PropertyValue[] primaryValues, CancellationToken cancellationToken = default)
        {
            Guard.ArgumentNull(primaryValues, nameof(primaryValues));
            CheckAsyncImplementd(source.Provider);

            var predicate = BindPrimaryExpression(source.ElementType, primaryValues);
            if (predicate == null)
            {
                return default;
            }

            var method = MethodCache.FirstOrDefaultAsync3.MakeGenericMethod(typeof(T));
            var expression = Expression.Call(null, method,
                new[] { (Expression)Expression.Constant(source), predicate, Expression.Constant(cancellationToken, typeof(CancellationToken)) });

            return await ((IAsyncQueryProvider)source.Provider).ExecuteAsync<T>(expression, cancellationToken);
        }

#if NETSTANDARD && !NETSTANDARD2_0
        internal static async Task<T> FirstOrDefaultCoreAsnyc<T>(this IAsyncEnumerable<T> enumerable)
        {
            await foreach (var item in enumerable)
            {
                return item;
            }

            return default;
        }

        internal static async Task<T> SingleOrDefaultCoreAsnyc<T>(this IAsyncEnumerable<T> enumerable)
        {
            await foreach (var item in enumerable)
            {
                return item;
            }

            return default;
        }
#else
        internal static async Task<T> FirstOrDefaultCoreAsnyc<T>(this Task<IEnumerable<T>> task)
        {
            return (await task).FirstOrDefault();
        }

        internal static async Task<T> SingleOrDefaultCoreAsnyc<T>(this Task<IEnumerable<T>> task)
        {
            return (await task).SingleOrDefault();
        }
#endif

        /// <summary>
        /// 创建一个实体。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="entity"></param>
        internal static int CreateEntity(this IQueryable source, IEntity entity)
        {
            CheckRepositoryIsReadonly(source.ElementType);

            var method = (MethodInfo)MethodBase.GetCurrentMethod();
            var expression = Expression.Call(null, method,
                new[] { Expression.Constant(source), (Expression)Expression.Constant(entity) });

            var primary = PropertyUnity.GetPrimaryProperties(entity.EntityType).FirstOrDefault(s => s.Info.GenerateType != IdentityGenerateType.None);

            var result = source.Provider.Execute<int>(expression);

            if (primary != null && result > 0 &&
                primary.Type.IsNumericType() &&
                !entity.IsModified(primary.Name) &&
                result != (int)entity.GetValue(primary))
            {
                entity.SetValue(primary, PropertyValue.NewValue(result, primary.Type));
            }

            return result.To<int>();
        }

        /// <summary>
        /// 异步的，创建一个实体。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="entity"></param>
        internal static async Task<int> CreateEntityAsync(this IQueryable source, IEntity entity, CancellationToken cancellationToken = default)
        {
            CheckRepositoryIsReadonly(source.ElementType);
            CheckAsyncImplementd(source.Provider);

            var expression = Expression.Call(null, MethodCache.CreateEntityAsync,
                new[] { Expression.Constant(source), (Expression)Expression.Constant(entity), Expression.Constant(cancellationToken, typeof(CancellationToken)) });

            var primary = PropertyUnity.GetPrimaryProperties(entity.EntityType).FirstOrDefault(s => s.Info.GenerateType != IdentityGenerateType.None);

            var result = await ((IAsyncQueryProvider)source.Provider).ExecuteAsync<int>(expression, cancellationToken);

            if (primary != null && result > 0 && !entity.IsModified(primary.Name) && result != (int)entity.GetValue(primary))
            {
                entity.SetValue(primary, PropertyValue.NewValue(result, primary.Type));
            }

            return result;
        }

        /// <summary>
        /// 更新一个实体。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="entity"></param>
        internal static int UpdateEntity(this IQueryable source, IEntity entity)
        {
            CheckRepositoryIsReadonly(source.ElementType);

            var expression = BindPrimaryExpression(entity);
            if (expression == null)
            {
                expression = BindAllFieldExpression(entity);
            }

            return source.UpdateWhere(entity, expression);
        }

        /// <summary>
        /// 异步的，更新一个实体。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="entity"></param>
        internal static async Task<int> UpdateEntityAsync(this IQueryable source, IEntity entity, CancellationToken cancellationToken = default)
        {
            CheckRepositoryIsReadonly(source.ElementType);
            CheckAsyncImplementd(source.Provider);

            var expression = BindPrimaryExpression(entity);
            if (expression == null)
            {
                expression = BindAllFieldExpression(entity);
            }

            return await source.UpdateWhereAsync(entity, expression, cancellationToken);
        }

        /// <summary>
        /// 移除一个实体。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="entity"></param>
        /// <param name="logicalDelete"></param>
        internal static int RemoveEntity(this IQueryable source, IEntity entity, PropertyValue logicalDelete)
        {
            CheckRepositoryIsReadonly(source.ElementType);

            var expression = BindPrimaryExpression(entity);
            if (expression == null)
            {
                expression = BindAllFieldExpression(entity);
            }

            return source.RemoveWhere(expression, logicalDelete);
        }

        /// <summary>
        /// 异步的，移除一个实体。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="entity"></param>
        /// <param name="logicalDelete"></param>
        internal static async Task<int> RemoveEntityAsync(this IQueryable source, IEntity entity, PropertyValue logicalDelete, CancellationToken cancellationToken = default)
        {
            CheckRepositoryIsReadonly(source.ElementType);
            CheckAsyncImplementd(source.Provider);

            var expression = BindPrimaryExpression(entity);
            if (expression == null)
            {
                expression = BindAllFieldExpression(entity);
            }

            return await source.RemoveWhereAsync(expression, logicalDelete, cancellationToken);
        }

        /// <summary>
        /// 通过主键删除一个实体。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="primaryKeys"></param>
        /// <param name="logicalDelete"></param>
        internal static int RemoveByPrimary(this IQueryable source, PropertyValue[] primaryKeys, PropertyValue logicalDelete)
        {
            CheckRepositoryIsReadonly(source.ElementType);

            var expression = BindPrimaryExpression(source.ElementType, primaryKeys);
            if (expression == null)
            {
                return 0;
            }

            return source.RemoveWhere(expression, logicalDelete);
        }

        /// <summary>
        /// 异步的，通过主键删除一个实体。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="primaryKeys"></param>
        /// <param name="logicalDelete"></param>
        internal static async Task<int> RemoveByPrimaryAsync(this IQueryable source, PropertyValue[] primaryKeys, PropertyValue logicalDelete, CancellationToken cancellationToken = default)
        {
            CheckRepositoryIsReadonly(source.ElementType);
            CheckAsyncImplementd(source.Provider);

            var expression = BindPrimaryExpression(source.ElementType, primaryKeys);
            if (expression == null)
            {
                return 0;
            }

            return await source.RemoveWhereAsync(expression, logicalDelete, cancellationToken);
        }

        /// <summary>
        /// 根据 lambda 表达式删除实体。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <param name="logicalDelete"></param>
        /// <returns></returns>
        internal static int RemoveWhere(this IQueryable source, LambdaExpression predicate, PropertyValue logicalDelete)
        {
            CheckRepositoryIsReadonly(source.ElementType);

            predicate ??= Expression.Lambda(Expression.Equal(Expression.Constant(1), Expression.Constant(1)), Expression.Parameter(source.ElementType, "s"));
            var method = (MethodInfo)MethodBase.GetCurrentMethod();
            var expression = Expression.Call(null, method,
                new[] { Expression.Constant(source), predicate, (Expression)Expression.Constant(logicalDelete) });

            return source.Provider.Execute<int>(expression);
        }

        /// <summary>
        /// 异步的，根据 lambda 表达式删除实体。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <param name="logicalDelete"></param>
        /// <returns></returns>
        internal static async Task<int> RemoveWhereAsync(this IQueryable source, LambdaExpression predicate, PropertyValue logicalDelete, CancellationToken cancellationToken = default)
        {
            CheckRepositoryIsReadonly(source.ElementType);
            CheckAsyncImplementd(source.Provider);

            predicate ??= Expression.Lambda(Expression.Equal(Expression.Constant(1), Expression.Constant(1)), Expression.Parameter(source.ElementType, "s"));
            var expression = Expression.Call(null, MethodCache.RemoveWhereAsync,
                new[] { Expression.Constant(source), predicate, (Expression)Expression.Constant(logicalDelete), Expression.Constant(cancellationToken, typeof(CancellationToken)) });

            return await ((IAsyncQueryProvider)source.Provider).ExecuteAsync<int>(expression, cancellationToken);
        }

        /// <summary>
        /// 根据 lambda 表达式更新实体。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="entity"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        internal static int UpdateWhere(this IQueryable source, IEntity entity, LambdaExpression predicate)
        {
            CheckRepositoryIsReadonly(source.ElementType);

            predicate ??= Expression.Lambda(Expression.Equal(Expression.Constant(1), Expression.Constant(1)), Expression.Parameter(entity.EntityType, "s"));
            var method = (MethodInfo)MethodBase.GetCurrentMethod();
            var expression = Expression.Call(null, method,
                new[] { Expression.Constant(source), (Expression)Expression.Constant(entity), predicate });

            return source.Provider.Execute<int>(expression);
        }

        /// <summary>
        /// 异步的，根据 lambda 表达式更新实体。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="entity"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        internal static async Task<int> UpdateWhereAsync(this IQueryable source, IEntity entity, LambdaExpression predicate, CancellationToken cancellationToken = default)
        {
            CheckRepositoryIsReadonly(source.ElementType);
            CheckAsyncImplementd(source.Provider);

            predicate ??= Expression.Lambda(Expression.Equal(Expression.Constant(1), Expression.Constant(1)), Expression.Parameter(entity.EntityType, "s"));
            var expression = Expression.Call(null, MethodCache.UpdateWhereAsync,
                new[] { Expression.Constant(source), (Expression)Expression.Constant(entity), predicate, Expression.Constant(cancellationToken, typeof(CancellationToken)) });

            return await ((IAsyncQueryProvider)source.Provider).ExecuteAsync<int>(expression, cancellationToken);
        }

        /// <summary>
        /// 根据 lambda 表达式更新实体。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="calculator"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        internal static int UpdateWhereByCalculator(this IQueryable source, LambdaExpression calculator, LambdaExpression predicate)
        {
            CheckRepositoryIsReadonly(source.ElementType);

            predicate ??= Expression.Lambda(Expression.Equal(Expression.Constant(1), Expression.Constant(1)), Expression.Parameter(source.ElementType, "s"));
            var method = (MethodInfo)MethodBase.GetCurrentMethod();
            var expression = Expression.Call(null, method,
                new[] { Expression.Constant(source), (Expression)calculator, predicate });

            return source.Provider.Execute<int>(expression);
        }

        /// <summary>
        /// 异步的，根据 lambda 表达式更新实体。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="calculator"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        internal static async Task<int> UpdateWhereByCalculatorAsync(this IQueryable source, LambdaExpression calculator, LambdaExpression predicate, CancellationToken cancellationToken = default)
        {
            CheckRepositoryIsReadonly(source.ElementType);
            CheckAsyncImplementd(source.Provider);

            predicate ??= Expression.Lambda(Expression.Equal(Expression.Constant(1), Expression.Constant(1)), Expression.Parameter(source.ElementType, "s"));
            var expression = Expression.Call(null, MethodCache.UpdateWhereByCalculatorAsync,
                new[] { Expression.Constant(source), (Expression)calculator, predicate, Expression.Constant(cancellationToken, typeof(CancellationToken)) });

            return await ((IAsyncQueryProvider)source.Provider).ExecuteAsync<int>(expression, cancellationToken);
        }

        internal static int BatchOperate(this IQueryable source, IEnumerable<IEntity> entitites, LambdaExpression fnOperation, BatchOperateOptions batchOpt = null)
        {
            CheckRepositoryIsReadonly(source.ElementType);

            if (entitites.IsNullOrEmpty())
            {
                return 0;
            }

            var method = (MethodInfo)MethodBase.GetCurrentMethod();
            var expression = Expression.Call(null, method,
                new[] { Expression.Constant(source), (Expression)Expression.Constant(entitites), fnOperation, Expression.Constant(batchOpt, typeof(BatchOperateOptions)) });

            return source.Provider.Execute<int>(expression);
        }

        internal static async Task<int> BatchOperateAsync(this IQueryable source, IEnumerable<IEntity> entitites, LambdaExpression fnOperation, BatchOperateOptions batchOpt = null, CancellationToken cancellationToken = default)
        {
            CheckRepositoryIsReadonly(source.ElementType);
            CheckAsyncImplementd(source.Provider);

            if (entitites.IsNullOrEmpty())
            {
                return 0;
            }

            var expression = Expression.Call(null, MethodCache.BatchOperateAsync,
                new[] { Expression.Constant(source), (Expression)Expression.Constant(entitites), fnOperation, Expression.Constant(batchOpt, typeof(BatchOperateOptions)), Expression.Constant(cancellationToken, typeof(CancellationToken)) });

            return await ((IAsyncQueryProvider)source.Provider).ExecuteAsync<int>(expression, cancellationToken);
        }

        /// <summary>
        /// 创建 (u, s) => u.Insert(s) 这样的 lambda 表达式。
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static LambdaExpression CreateInsertExpression(this IQueryable source)
        {
            var rpType = ReflectionCache.GetMember("RepositoryImpl", source.ElementType, k => typeof(IRepository<>).MakeGenericType(k));
            var method = ReflectionCache.GetMember("RepositoryInsert", source.ElementType, rpType, (k, rt) => rt.GetMethod(nameof(IRepository.Insert), new[] { k }));
            var parSet = Expression.Parameter(rpType, "u");
            var parEle = Expression.Parameter(source.ElementType, "s");
            return Expression.Lambda(Expression.Call(parSet, method, parEle), parSet, parEle);
        }

        /// <summary>
        /// 创建 (u, s) => u.Update(s) 这样的 lambda 表达式。
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static LambdaExpression CreateUpdateExpression(this IQueryable source)
        {
            var rpType = ReflectionCache.GetMember("RepositoryImpl", source.ElementType, k => typeof(IRepository<>).MakeGenericType(k));
            var method = ReflectionCache.GetMember("RepositoryUpdate", source.ElementType, rpType, (k, rt) => rt.GetMethod(nameof(IRepository.Update), new[] { k }));
            var parSet = Expression.Parameter(rpType, "u");
            var parEle = Expression.Parameter(source.ElementType, "s");
            return Expression.Lambda(Expression.Call(parSet, method, parEle), parSet, parEle);
        }

        /// <summary>
        /// 创建 (u, s) => u.Delete(s, true) 这样的 lambda 表达式。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="logicalDelete"></param>
        /// <returns></returns>
        internal static LambdaExpression CreateDeleteExpression(this IQueryable source, PropertyValue logicalDelete)
        {
            var rpType = ReflectionCache.GetMember("RepositoryImpl", source.ElementType, k => typeof(IRepository<>).MakeGenericType(k));
            var method = ReflectionCache.GetMember("RepositoryDelete", source.ElementType, rpType, (k, rt) => rt.GetMethod(nameof(IRepository.Delete), new[] { k, typeof(bool) }));
            var parSet = Expression.Parameter(rpType, "u");
            var parEle = Expression.Parameter(source.ElementType, "s");
            return Expression.Lambda(Expression.Call(parSet, method, parEle, Expression.Constant(logicalDelete)), parSet, parEle);
        }

        /// <summary>
        /// 通过实体构造主键查询表达式。
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private static LambdaExpression BindPrimaryExpression(IEntity entity)
        {
            var pkProperties = PropertyUnity.GetPrimaryProperties(entity.EntityType).ToList();
            if (!pkProperties.Any())
            {
                return null;
            }

            var parExp = Expression.Parameter(entity.EntityType, "s");

            var exps = from p in pkProperties
                       let value = entity.GetValue(p)
                       where !PropertyValue.IsEmpty(value)
                       let getValExp = BindGetValueExpression(p, value)
                       select Expression.MakeMemberAccess(parExp, p.Info.ReflectionInfo)
                        .Equal(getValExp);

            var predicate = exps.Aggregate(Expression.And);

            return Expression.Lambda(predicate, parExp);
        }

        /// <summary>
        /// 通过主键值数组构建主键查询表达式。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="pkValues"></param>
        /// <returns></returns>
        private static LambdaExpression BindPrimaryExpression(Type type, PropertyValue[] pkValues)
        {
            Guard.ArgumentNull(pkValues, nameof(pkValues));

            var pkProperties = PropertyUnity.GetPrimaryProperties(type).ToList();
            if (!pkProperties.Any())
            {
                return null;
            }

            //主键个数不一致
            if (pkValues.Length != pkProperties.Count)
            {
                throw new EntityPersistentException(SR.GetString(SRKind.DisaccordArgument, pkProperties.Count, pkValues.Length), null);
            }

            var parExp = Expression.Parameter(type, "s");

            var exps = new List<Expression>();
            pkValues.ForEach((p, i) =>
                {
                    exps.Add(Expression.MakeMemberAccess(parExp, pkProperties[i].Info.ReflectionInfo).Equal(Expression.Constant(p)));
                });

            var predicate = exps.Aggregate(Expression.And);

            return Expression.Lambda(predicate, parExp);
        }

        /// <summary>
        /// 根据实体对象构造所有字段的查询表达式。
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private static LambdaExpression BindAllFieldExpression(IEntity entity)
        {
            var parExp = Expression.Parameter(entity.EntityType, "s");

            var exps = from p in PropertyUnity.GetPersistentProperties(entity.EntityType)
                       let oldValue = entity.GetOldValue(p)
                       where !PropertyValue.IsEmpty(oldValue)
                       let getValExp = BindGetValueExpression(p, oldValue)
                       select Expression.MakeMemberAccess(parExp, p.Info.ReflectionInfo)
                        .Equal(getValExp);

            var predicate = exps.Aggregate(Expression.And);

            return Expression.Lambda(predicate, parExp);
        }

        /// <summary>
        /// 获取取值的表达式。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static Expression BindGetValueExpression(IProperty property, PropertyValue value)
        {
            //找强制转换的方法
            var op_Explicit = ReflectionCache.GetMember("PropertyValue_op_Explicit", property.Type, k => typeof(PropertyValue).GetMethods().FirstOrDefault(s => s.Name == "op_Explicit" && s.ReturnType == k));
            if (op_Explicit == null)
            {
                return Expression.Constant(value.GetValue(), property.Type);
            }

            var constExp = Expression.Constant(value);
            return Expression.Call(op_Explicit, constExp);
        }

        /// <summary>
        /// 创建排序表达式。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="methodName"></param>
        /// <param name="memberName"></param>
        /// <returns></returns>
        private static IQueryable<T> CreateOrderExpression<T>(IQueryable<T> source, string methodName, string memberName)
        {
            var sourceType = typeof(T);
            var parExp = Expression.Parameter(sourceType, "s");

            var propertyType = sourceType;
            Expression expression = parExp;
            foreach (var member in memberName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (string.IsNullOrWhiteSpace(member))
                {
                    continue;
                }

                var property = propertyType.GetProperty(member);
                if (property == null)
                {
                    throw new PropertyNotFoundException(member);
                }

                expression = Expression.MakeMemberAccess(expression, property);
                propertyType = property.PropertyType;
            }

            var delegateType = ReflectionCache.GetMember("FuncType", new[] { sourceType, propertyType }, pars => typeof(Func<,>).MakeGenericType(pars));
            var lambda = Expression.Lambda(delegateType, expression, parExp);
            expression = Expression.Call(typeof(Queryable), methodName, new[] { sourceType, propertyType }, source.Expression, lambda);

            return source.Provider.CreateQuery<T>(expression);
        }

        /// <summary>
        /// 检查是否实现了 IAsyncQueryProvider 接口。
        /// </summary>
        /// <param name="provider"></param>
        private static void CheckAsyncImplementd(IQueryProvider provider)
        {
            if (!(provider is IAsyncQueryProvider))
            {
                throw new NotImplementedException(SR.GetString(SRKind.NotImplementAsyncQueryProvider));
            }
        }

        /// <summary>
        /// 检查实体是否为只读的。
        /// </summary>
        /// <param name="entityType"></param>
        private static void CheckRepositoryIsReadonly(Type entityType)
        {
            var metadata = EntityMetadataUnity.GetEntityMetadata(entityType);
            if (metadata != null && metadata.IsReadonly)
            {
                throw new InvalidOperationException(SR.GetString(SRKind.InvalidOperationWhenRepositoryIsReadonly));
            }
        }

        /// <summary>
        /// 用于收集表达式中的所有查询条件。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private class PredicateGatherer<T> : Fireasy.Common.Linq.Expressions.ExpressionVisitor
        {
            private readonly List<Expression> _exps = new List<Expression>();

            internal static LambdaExpression Gather(Expression expression)
            {
                var gatherer = new PredicateGatherer<T>();
                gatherer.Visit(expression);
                return gatherer.Build();
            }

            private LambdaExpression Build()
            {
                if (_exps.Count == 0)
                {
                    return null;
                }

                var parExp = Expression.Parameter(typeof(T), "s");
                return Expression.Lambda(_exps.Aggregate(Expression.And), parExp);
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.DeclaringType == typeof(Queryable) && node.Method.Name == nameof(Queryable.Where))
                {
                    Visit(node.Arguments[0]);
                    _exps.Add(GetLambda(node.Arguments[1]).Body);
                }
                else
                {
                    Visit(node.Arguments[0]);
                }

                return node;
            }

            /// <summary>
            /// 获取 Lambda 表达式。
            /// </summary>
            /// <param name="e"></param>
            /// <returns></returns>
            private static LambdaExpression GetLambda(Expression e)
            {
                while (e.NodeType == ExpressionType.Quote)
                {
                    e = ((UnaryExpression)e).Operand;
                }
                if (e.NodeType == ExpressionType.Constant)
                {
                    return ((ConstantExpression)e).Value as LambdaExpression;
                }

                return e as LambdaExpression;
            }
        }
    }
}