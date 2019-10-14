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
using Fireasy.Data.Entity.Linq.Translators;
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
        private static MethodInfo MthCreateEntityAsync = typeof(Extensions).GetMethod(nameof(Extensions.CreateEntityAsync), BindingFlags.NonPublic | BindingFlags.Static);
        private static MethodInfo MthRemoveWhereAsync = typeof(Extensions).GetMethod(nameof(Extensions.RemoveWhereAsync), BindingFlags.NonPublic | BindingFlags.Static);
        private static MethodInfo MthUpdateWhereAsync = typeof(Extensions).GetMethod(nameof(Extensions.UpdateWhereAsync), BindingFlags.NonPublic | BindingFlags.Static);
        private static MethodInfo MthUpdateWhereByCalculatorAsync = typeof(Extensions).GetMethod(nameof(Extensions.UpdateWhereByCalculatorAsync), BindingFlags.NonPublic | BindingFlags.Static);
        private static MethodInfo MthBatchOperateAsync = typeof(Extensions).GetMethod(nameof(Extensions.BatchOperateAsync), BindingFlags.NonPublic | BindingFlags.Static);
        private static MethodInfo MthFirstOrDefaultAsync2 = typeof(Extensions).GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(s => s.Name == nameof(Extensions.FirstOrDefaultAsync) && s.GetParameters().Length == 2);
        private static MethodInfo MthFirstOrDefaultAsync3 = typeof(Extensions).GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(s => s.Name == nameof(Extensions.FirstOrDefaultAsync) && s.GetParameters().Length == 3);
        private static MethodInfo MthLastOrDefaultAsync2 = typeof(Extensions).GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(s => s.Name == nameof(Extensions.LastOrDefaultAsync) && s.GetParameters().Length == 2);
        private static MethodInfo MthLastOrDefaultAsync3 = typeof(Extensions).GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(s => s.Name == nameof(Extensions.LastOrDefaultAsync) && s.GetParameters().Length == 3);
        private static MethodInfo MthAnyAsync2 = typeof(Extensions).GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(s => s.Name == nameof(Extensions.AnyAsync) && s.GetParameters().Length == 2);
        private static MethodInfo MthAnyAsync3 = typeof(Extensions).GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(s => s.Name == nameof(Extensions.AnyAsync) && s.GetParameters().Length == 3);
        private static MethodInfo MthAllAsync2 = typeof(Extensions).GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(s => s.Name == nameof(Extensions.AllAsync) && s.GetParameters().Length == 2);
        private static MethodInfo MthAllAsync3 = typeof(Extensions).GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(s => s.Name == nameof(Extensions.AllAsync) && s.GetParameters().Length == 3);
        private static MethodInfo MthToListAsync = typeof(Extensions).GetMethod(nameof(Extensions.ToListAsync), BindingFlags.Public | BindingFlags.Static);

        /// <summary>
        /// 使用 <see cref="IDataSegment"/> 对象对序列进行分段筛选，如果使用 <see cref="DataPager"/>，则可返回详细的分页信息(数据行数和页码总数)。
        /// </summary>
        /// <typeparam name="T">数据类型。</typeparam>
        /// <param name="source">要进行分段的序列。</param>
        /// <param name="segment">分段对象，可使用 <see cref="DataPager"/>。</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">参数 source 为空时抛出此异常。</exception>
        public static IQueryable<T> Segment<T>(this IQueryable<T> source, IDataSegment segment)
        {
            if (source == null || segment == null)
            {
                return source;
            }

            var expression = Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(
                new[] { typeof(T) }),
                new[] { source.Expression, Expression.Constant(segment) });

            return source.Provider.CreateQuery<T>(expression);
        }

        /// <summary>
        /// 标记此查询不使用状态跟踪。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IQueryable<T> AsNoTracking<T>(this IQueryable<T> source)
        {
            var method = (MethodInfo)MethodBase.GetCurrentMethod();
            method = method.MakeGenericMethod(typeof(T));
            var expression = Expression.Call(null, method,
                new[] { source.Expression });

            return source.Provider.CreateQuery<T>(expression);
        }

        /// <summary>
        /// 设置是否允许 LINQ 解析放入到缓存中。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="enabled">为 true 时开启缓存。</param>
        /// <param name="expired">过期时间。</param>
        /// <returns></returns>
        public static IQueryable<T> CacheParsing<T>(this IQueryable<T> source, bool enabled, TimeSpan? expired)
        {
            var method = (MethodInfo)MethodBase.GetCurrentMethod();
            method = method.MakeGenericMethod(typeof(T));
            var expression = Expression.Call(null, method,
                new[] { source.Expression, Expression.Constant(enabled), Expression.Constant(expired, typeof(TimeSpan?)) });

            return source.Provider.CreateQuery<T>(expression);
        }

        /// <summary>
        /// 设置是否允许 LINQ 解析放入到缓存中。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="enabled">为 true 时开启缓存。</param>
        /// <returns></returns>
        public static IQueryable<T> CacheParsing<T>(this IQueryable<T> source, bool enabled)
        {
            var method = (MethodInfo)MethodBase.GetCurrentMethod();
            method = method.MakeGenericMethod(typeof(T));
            var expression = Expression.Call(null, method,
                new[] { source.Expression, Expression.Constant(enabled) });

            return source.Provider.CreateQuery<T>(expression);
        }

        /// <summary>
        /// 设置是否允许将执行结果放入缓存中。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="enabled">为 true 时开启缓存。</param>
        /// <param name="expired">过期时间(秒)。</param>
        /// <returns></returns>
        public static IQueryable<T> CacheExecution<T>(this IQueryable<T> source, bool enabled, TimeSpan? expired)
        {
            var method = (MethodInfo)MethodBase.GetCurrentMethod();
            method = method.MakeGenericMethod(typeof(T));
            var expression = Expression.Call(null, method,
                new[] { source.Expression, Expression.Constant(enabled), Expression.Constant(expired, typeof(TimeSpan?)) });

            return source.Provider.CreateQuery<T>(expression);
        }

        /// <summary>
        /// 设置是否允许将执行结果放入缓存中。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="enabled">为 true 时开启缓存。</param>
        /// <returns></returns>
        public static IQueryable<T> CacheExecution<T>(this IQueryable<T> source, bool enabled)
        {
            var method = (MethodInfo)MethodBase.GetCurrentMethod();
            method = method.MakeGenericMethod(typeof(T));
            var expression = Expression.Call(null, method,
                new[] { source.Expression, Expression.Constant(enabled) });

            return source.Provider.CreateQuery<T>(expression);
        }

        /// <summary>
        /// 将查询转换为带分页信息的结构输出。查询中需要使用 Segment 扩展方法带入 <see cref="IPager"/> 分页对象。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static PaginalResult<T> ToPaginalResult<T>(this IQueryable<T> source)
        {
            var segment = SegmentFinder.Find(source.Expression);
            var list = source.ToList();

            return new PaginalResult<T>(list, segment as IPager);
        }

        /// <summary>
        /// 根据断言进行序列的筛选。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="condition">要计算的条件表达式。如果条件为 true，则进行筛选，否则不筛选。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <returns></returns>
        public static IQueryable<T> AssertWhere<T>(this IQueryable<T> source, bool condition, Expression<Func<T, bool>> predicate)
        {
            if (source == null || !condition)
            {
                return source;
            }

            var expression = Expression.Call(typeof(Queryable), nameof(Queryable.Where), new[] { typeof(T) }, source.Expression, predicate);

            return source.Provider.CreateQuery<T>(expression);
        }

        /// <summary>
        /// 根据断言进行序列的筛选。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="condition">要计算的条件表达式。如果条件为 true，则进行筛选，否则不筛选。</param>
        /// <param name="isTruePredicate">用于条件为 true 时测试每个元素是否满足条件的函数。</param>
        /// <param name="isFalsePredicate">用于条件为 false 时测试每个元素是否满足条件的函数。</param>
        /// <returns></returns>
        public static IQueryable<T> AssertWhere<T>(this IQueryable<T> source, bool condition, Expression<Func<T, bool>> isTruePredicate, Expression<Func<T, bool>> isFalsePredicate)
        {
            if (source == null)
            {
                return source;
            }

            var expression = Expression.Call(typeof(Queryable), nameof(Queryable.Where), new[] { typeof(T) }, source.Expression, condition ? isTruePredicate : isFalsePredicate);

            return source.Provider.CreateQuery<T>(expression);
        }

        /// <summary>
        /// 根据断言进行序列的筛选。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="condition">要计算的条件表达式。如果条件为 true，则进行筛选，否则不筛选。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="otherwise">如果条件不成立，则采用其他的查询。</param>
        /// <returns></returns>
        public static IQueryable<T> AssertWhere<T>(this IQueryable<T> source, bool condition, Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IQueryable<T>> otherwise)
        {
            if (source == null || !condition)
            {
                if (otherwise != null)
                {
                    return otherwise(source);
                }

                return source;
            }

            var expression = Expression.Call(typeof(Queryable), nameof(Queryable.Where), new[] { typeof(T) }, source.Expression, predicate);

            return source.Provider.CreateQuery<T>(expression);
        }

        /// <summary>
        /// 在 Select 中应用 <see cref="ExtendAs{T}(IEntity, Expression{Func{object}})"/> 来扩展返回的结果。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static IQueryable<TResult> ExtendSelect<TSource, TResult>(this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selector)
        {
            var parExp = Expression.Parameter(typeof(TSource), "t");

            var method = typeof(Extensions).GetMethod(nameof(Extensions.ExtendAs)).MakeGenericMethod(typeof(TResult));

            var newExp = ExpressionReplacer.Replace(selector.Body, parExp);
            var newSelector = Expression.Lambda<Func<object>>(newExp);

            var callExp = Expression.Call(null, method, parExp, newSelector);
            var lambda = Expression.Lambda(callExp, parExp);

            var expression = Expression.Call(typeof(Queryable), nameof(Queryable.Select), new[] { typeof(TSource), typeof(TResult) }, source.Expression, lambda);

            return source.Provider.CreateQuery<TResult>(expression);
        }

        /*
        /// <summary>
        /// 配置缓存的过期时间。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="expired"></param>
        /// <returns></returns>
        public static IQueryable<T> Cache<T>(this IQueryable<T> source, TimeSpan expired)
        {
            if (source == null)
            {
                return source;
            }

            var expression = Expression.Call(typeof(Extensions), "Cache", new[] { typeof(T) }, source.Expression, Expression.Constant(expired));

            return source.Provider.CreateQuery<T>(expression);
        }
        */

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
            if (source == null || collection == null || predicate == null)
            {
                return source;
            }

            var parExp = Expression.Parameter(typeof(TSource), "s");

            Expression joinExp = null;
            foreach (var item in collection)
            {
                var exp = ParameterRewriter.Rewrite(predicate.Body, parExp, item);
                joinExp = joinExp == null ? exp : Expression.Or(joinExp, exp);
            }

            if (joinExp == null)
            {
                return source;
            }

            var lambda = Expression.Lambda<Func<TSource, bool>>(joinExp, parExp);
            var expression = Expression.Call(typeof(Queryable), nameof(Queryable.Where), new[] { typeof(TSource) }, source.Expression, lambda);

            return source.Provider.CreateQuery<TSource>(expression);
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
            if (source == null || collection == null || predicate == null)
            {
                return source;
            }

            var parExp = Expression.Parameter(typeof(TSource), "s");
            Expression joinExp = null;
            foreach (var item in collection)
            {
                var exp = ParameterRewriter.Rewrite(predicate.Body, parExp, item);
                joinExp = joinExp == null ? exp : Expression.And(joinExp, exp);
            }

            if (joinExp == null)
            {
                return source;
            }

            var lambda = Expression.Lambda<Func<TSource, bool>>(joinExp, parExp);
            var expression = Expression.Call(typeof(Queryable), nameof(Queryable.Where), new[] { typeof(TSource) }, source.Expression, lambda);

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
        public static IQueryable<TSource> OrderBy<TSource>(this IQueryable<TSource> source, SortDefinition sort, Expression<Func<IQueryable<TSource>, IQueryable<TSource>>> otherwise = null)
        {
            if (sort == null)
            {
                if (otherwise != null)
                {
                    return UseDefinitionQuery<TSource>(source, otherwise);
                }

                return source;
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
        public static IQueryable<TSource> ThenBy<TSource>(this IQueryable<TSource> source, SortDefinition sort, Expression<Func<IQueryable<TSource>, IQueryable<TSource>>> otherwise = null)
        {
            if (sort == null)
            {
                if (otherwise != null)
                {
                    return UseDefinitionQuery<TSource>(source, otherwise);
                }

                return source;
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
        public static IQueryable<TSource> OrderBy<TSource>(this IQueryable<TSource> source, string memberName, SortOrder sortOrder = SortOrder.None, Expression<Func<IQueryable<TSource>, IQueryable<TSource>>> otherwise = null)
        {
            if (string.IsNullOrEmpty(memberName) && sortOrder == SortOrder.None)
            {
                if (otherwise != null)
                {
                    return UseDefinitionQuery<TSource>(source, otherwise);
                }

                return source;
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
        public static IQueryable<TSource> ThenBy<TSource>(this IQueryable<TSource> source, string memberName, SortOrder sortOrder = SortOrder.Ascending, Expression<Func<IQueryable<TSource>, IQueryable<TSource>>> otherwise = null)
        {
            if (string.IsNullOrEmpty(memberName) && sortOrder == SortOrder.None)
            {
                if (otherwise != null)
                {
                    return UseDefinitionQuery<TSource>(source, otherwise);
                }

                return source;
            }

            var methodName = sortOrder == SortOrder.Ascending ? nameof(Queryable.ThenBy) : nameof(Queryable.ThenByDescending);

            return CreateOrderExpression(source, methodName, memberName);
        }

        /// <summary>
        /// 将实体进行扩展，附加 <paramref name="selector"/> 表达式中的字段，返回 <typeparamref name="T"/> 类型的对象。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static T ExtendAs<T>(this IEntity entity, Expression<Func<object>> selector)
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

        public static async Task<T[]> ToArray<T>(this IQueryable<T> queryable, CancellationToken cancellationToken = default)
        {
            return (await queryable.ToListAsync()).ToArray();
        }

        public static async Task<List<T>> ToListAsync<T>(this IQueryable<T> queryable, CancellationToken cancellationToken = default)
        {
            var method = MthToListAsync.MakeGenericMethod(typeof(T));
            var expression = Expression.Call(null, method,
                new[] { (Expression)Expression.Constant(queryable), Expression.Constant(cancellationToken, typeof(CancellationToken)) });

#if NETSTANDARD && !NETSTANDARD2_0
            var enumerable = await ((IAsyncQueryProvider)queryable.Provider).ExecuteAsync<IAsyncEnumerable<T>>(expression, cancellationToken);
            if (enumerable == null)
            {
                return null;
            }

            var result = new List<T>();
            await foreach (var item in enumerable)
            {
                result.Add(item);
            }

            return result;
#else
            var task = await ((IAsyncQueryProvider)queryable.Provider).ExecuteAsync<Task<IEnumerable<T>>>(expression, cancellationToken);
            return (await task).ToList();
#endif
        }

        /// <summary>
        /// 获取第一个对象。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<T> FirstOrDefaultAsync<T>(this IQueryable<T> queryable, CancellationToken cancellationToken = default)
        {
            var method = MthFirstOrDefaultAsync2.MakeGenericMethod(typeof(T));
            var expression = Expression.Call(null, method,
                new[] { (Expression)Expression.Constant(queryable), Expression.Constant(cancellationToken, typeof(CancellationToken)) });

            return await (await ((IAsyncQueryProvider)queryable.Provider).ExecuteAsync<Task<T>>(expression, cancellationToken));
        }

        /// <summary>
        /// 获取第一个对象。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable"></param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<T> FirstOrDefaultAsync<T>(this IQueryable<T> queryable, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var method = MthFirstOrDefaultAsync3.MakeGenericMethod(typeof(T));
            var expression = Expression.Call(null, method,
                new[] { (Expression)Expression.Constant(queryable), predicate, Expression.Constant(cancellationToken, typeof(CancellationToken)) });

            return await (await ((IAsyncQueryProvider)queryable.Provider).ExecuteAsync<Task<T>>(expression, cancellationToken));
        }

        /// <summary>
        /// 获取最后一个对象。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<T> LastOrDefaultAsync<T>(this IQueryable<T> queryable, CancellationToken cancellationToken = default)
        {
            var method = MthLastOrDefaultAsync2.MakeGenericMethod(typeof(T));
            var expression = Expression.Call(null, method,
                new[] { (Expression)Expression.Constant(queryable), Expression.Constant(cancellationToken, typeof(CancellationToken)) });

            return await (await ((IAsyncQueryProvider)queryable.Provider).ExecuteAsync<Task<T>>(expression, cancellationToken));
        }

        /// <summary>
        /// 获取最后一个对象。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable"></param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<T> LastOrDefaultAsync<T>(this IQueryable<T> queryable, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var method = MthLastOrDefaultAsync3.MakeGenericMethod(typeof(T));
            var expression = Expression.Call(null, method,
                new[] { (Expression)Expression.Constant(queryable), predicate, Expression.Constant(cancellationToken, typeof(CancellationToken)) });

            return await (await ((IAsyncQueryProvider)queryable.Provider).ExecuteAsync<Task<T>>(expression, cancellationToken));
        }

        /// <summary>
        /// 判断任何序列只要满足指定的条件。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<bool> AnyAsync<T>(this IQueryable<T> queryable, CancellationToken cancellationToken = default)
        {
            var method = MthAnyAsync2.MakeGenericMethod(typeof(T));
            var expression = Expression.Call(null, method,
                new[] { (Expression)Expression.Constant(queryable), Expression.Constant(cancellationToken, typeof(CancellationToken)) });

            return await (await ((IAsyncQueryProvider)queryable.Provider).ExecuteAsync<Task<bool>>(expression, cancellationToken));
        }

        /// <summary>
        /// 判断任何序列只要满足指定的条件。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable"></param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<bool> AnyAsync<T>(this IQueryable<T> queryable, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var method = MthAnyAsync3.MakeGenericMethod(typeof(T));
            var expression = Expression.Call(null, method,
                new[] { (Expression)Expression.Constant(queryable), predicate, Expression.Constant(cancellationToken, typeof(CancellationToken)) });

            return await (await ((IAsyncQueryProvider)queryable.Provider).ExecuteAsync<Task<bool>>(expression, cancellationToken));
        }

        /// <summary>
        /// 判断所有序列必须满足指定的条件。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable"></param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<bool> AllAsync<T>(this IQueryable<T> queryable, CancellationToken cancellationToken = default)
        {
            var method = MthAllAsync2.MakeGenericMethod(typeof(T));
            var expression = Expression.Call(null, method,
                new[] { (Expression)Expression.Constant(queryable), Expression.Constant(cancellationToken, typeof(CancellationToken)) });

            return await (await ((IAsyncQueryProvider)queryable.Provider).ExecuteAsync<Task<bool>>(expression, cancellationToken));
        }

        /// <summary>
        /// 判断所有序列必须满足指定的条件。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable"></param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<bool> AllAsync<T>(this IQueryable<T> queryable, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var method = MthAllAsync3.MakeGenericMethod(typeof(T));
            var expression = Expression.Call(null, method,
                new[] { (Expression)Expression.Constant(queryable), predicate, Expression.Constant(cancellationToken, typeof(CancellationToken)) });

            return await (await ((IAsyncQueryProvider)queryable.Provider).ExecuteAsync<Task<bool>>(expression, cancellationToken));
        }

#if NETSTANDARD && !NETSTANDARD2_0
        /// <summary>
        /// 转换成异步枚举。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable"></param>
        /// <returns></returns>
        public static IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IQueryable<T> queryable)
        {
            return (IAsyncEnumerable<T>)queryable;
        }
#endif

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
                var property = propertyType.GetProperty(member);
                if (property == null)
                {
                    throw new PropertyNotFoundException(member);
                }

                expression = Expression.MakeMemberAccess(expression, property);
                propertyType = property.PropertyType;
            }

            var delegateType = typeof(Func<,>).MakeGenericType(sourceType, propertyType);
            var lambda = Expression.Lambda(delegateType, expression, parExp);
            expression = Expression.Call(typeof(Queryable), methodName, new[] { sourceType, propertyType }, source.Expression, lambda);

            return source.Provider.CreateQuery<T>(expression);
        }

        /// <summary>
        /// 当客户端排序定义为空时，使用预定义的排序表达式。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="orderPredicate"></param>
        /// <returns></returns>
        private static IQueryable<T> UseDefinitionQuery<T>(IQueryable<T> source, Expression<Func<IQueryable<T>, IQueryable<T>>> orderPredicate)
        {
            var sourceType = typeof(T);
            var parExp = Expression.Parameter(sourceType, "s");
            var orderBys = OrderGatherer.Gather(orderPredicate.Body);
            var expression = source.Expression;
            foreach (var kvp in orderBys)
            {
                expression = Expression.Call(null, kvp.Key, new[] { expression, kvp.Value });
            }

            return source.Provider.CreateQuery<T>(expression);
        }

        /// <summary>
        /// 使用主键返回一个实体。
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="primaryValues"></param>
        /// <returns></returns>
        internal static T GetByPrimary<T>(this IQueryable queryable, PropertyValue[] primaryValues)
        {
            Guard.ArgumentNull(primaryValues, nameof(primaryValues));

            var predicate = BindPrimaryExpression(queryable.ElementType, primaryValues);
            if (predicate == null)
            {
                return default(T);
            }

            var expression = Expression.Call(typeof(Queryable), nameof(Queryable.FirstOrDefault), new[] { queryable.ElementType }, Expression.Constant(queryable), (Expression)predicate);

            return queryable.Provider.Execute<T>(expression);
        }

        /// <summary>
        /// 使用主键返回一个实体。
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="primaryValues"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal static async Task<T> GetByPrimaryAsync<T>(this IQueryable queryable, PropertyValue[] primaryValues, CancellationToken cancellationToken = default)
        {
            Guard.ArgumentNull(primaryValues, nameof(primaryValues));

            var predicate = BindPrimaryExpression(queryable.ElementType, primaryValues);
            if (predicate == null)
            {
                return default(T);
            }

            var method = MthFirstOrDefaultAsync3.MakeGenericMethod(typeof(T));
            var expression = Expression.Call(null, method,
                new[] { (Expression)Expression.Constant(queryable), predicate, Expression.Constant(cancellationToken, typeof(CancellationToken)) });

            return await (await ((IAsyncQueryProvider)queryable.Provider).ExecuteAsync<Task<T>>(expression, cancellationToken));
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
        /// <param name="queryable"></param>
        /// <param name="entity"></param>
        internal static int CreateEntity(this IQueryable queryable, IEntity entity)
        {
            var method = (MethodInfo)MethodBase.GetCurrentMethod();
            var expression = Expression.Call(null, method,
                new[] { Expression.Constant(queryable), (Expression)Expression.Constant(entity) });

            var primary = PropertyUnity.GetPrimaryProperties(entity.EntityType).FirstOrDefault(s => s.Info.GenerateType == IdentityGenerateType.AutoIncrement);
            var result = queryable.Provider.Execute<int>(expression);
            if (primary != null && !result.IsNullOrEmpty())
            {
                entity.SetValue(primary, PropertyValue.NewValue(result, primary.Type));
            }

            return result.To<int>();
        }

        /// <summary>
        /// 创建一个实体。
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="entity"></param>
        internal static async Task<int> CreateEntityAsync(this IQueryable queryable, IEntity entity, CancellationToken cancellationToken = default)
        {
            var expression = Expression.Call(null, MthCreateEntityAsync,
                new[] { Expression.Constant(queryable), (Expression)Expression.Constant(entity), Expression.Constant(cancellationToken, typeof(CancellationToken)) });

            var primary = PropertyUnity.GetPrimaryProperties(entity.EntityType).FirstOrDefault(s => s.Info.GenerateType == IdentityGenerateType.AutoIncrement);
            var result = await (await ((IAsyncQueryProvider)queryable.Provider).ExecuteAsync<Task<int>>(expression, cancellationToken));
            if (primary != null && result > 0)
            {
                entity.SetValue(primary, PropertyValue.NewValue(result, primary.Type));
            }

            return result.To<int>();
        }

        /// <summary>
        /// 更新一个实体。
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="entity"></param>
        internal static int UpdateEntity(this IQueryable queryable, IEntity entity)
        {
            var expression = BindPrimaryExpression(entity);
            if (expression == null)
            {
                expression = BindAllFieldExpression(entity);
            }

            return queryable.UpdateWhere(entity, expression);
        }

        /// <summary>
        /// 更新一个实体。
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="entity"></param>
        internal static async Task<int> UpdateEntityAsync(this IQueryable queryable, IEntity entity, CancellationToken cancellationToken = default)
        {
            var expression = BindPrimaryExpression(entity);
            if (expression == null)
            {
                expression = BindAllFieldExpression(entity);
            }

            return await queryable.UpdateWhereAsync(entity, expression, cancellationToken);
        }

        /// <summary>
        /// 移除一个实体。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable"></param>
        /// <param name="entity"></param>
        /// <param name="logicalDelete"></param>
        internal static int RemoveEntity(this IQueryable queryable, IEntity entity, bool logicalDelete)
        {
            var expression = BindPrimaryExpression(entity);
            if (expression == null)
            {
                expression = BindAllFieldExpression(entity);
            }

            return queryable.RemoveWhere(expression, logicalDelete);
        }

        /// <summary>
        /// 移除一个实体。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable"></param>
        /// <param name="entity"></param>
        /// <param name="logicalDelete"></param>
        internal static async Task<int> RemoveEntityAsync(this IQueryable queryable, IEntity entity, bool logicalDelete, CancellationToken cancellationToken = default)
        {
            var expression = BindPrimaryExpression(entity);
            if (expression == null)
            {
                expression = BindAllFieldExpression(entity);
            }

            return await queryable.RemoveWhereAsync(expression, logicalDelete, cancellationToken);
        }

        /// <summary>
        /// 通过主键删除一个实体。
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="primaryKeys"></param>
        /// <param name="logicalDelete"></param>
        internal static int RemoveByPrimary(this IQueryable queryable, PropertyValue[] primaryKeys, bool logicalDelete)
        {
            var expression = BindPrimaryExpression(queryable.ElementType, primaryKeys);
            if (expression == null)
            {
                return 0;
            }

            return queryable.RemoveWhere(expression, logicalDelete);
        }

        /// <summary>
        /// 通过主键删除一个实体。
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="primaryKeys"></param>
        /// <param name="logicalDelete"></param>
        internal static async Task<int> RemoveByPrimaryAsync(this IQueryable queryable, PropertyValue[] primaryKeys, bool logicalDelete, CancellationToken cancellationToken = default)
        {
            var expression = BindPrimaryExpression(queryable.ElementType, primaryKeys);
            if (expression == null)
            {
                return 0;
            }

            return await queryable.RemoveWhereAsync(expression, logicalDelete, cancellationToken);
        }

        /// <summary>
        /// 根据LINQ删除实体。
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="predicate"></param>
        /// <param name="logicalDelete"></param>
        /// <returns></returns>
        internal static int RemoveWhere(this IQueryable queryable, LambdaExpression predicate = null, bool logicalDelete = true)
        {
            predicate = predicate ?? Expression.Lambda(Expression.Equal(Expression.Constant(1), Expression.Constant(1)), Expression.Parameter(queryable.ElementType, "s"));
            var method = (MethodInfo)MethodBase.GetCurrentMethod();
            var expression = Expression.Call(null, method,
                new[] { Expression.Constant(queryable), predicate, (Expression)Expression.Constant(logicalDelete) });

            return queryable.Provider.Execute<int>(expression);
        }

        /// <summary>
        /// 根据LINQ删除实体。
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="predicate"></param>
        /// <param name="logicalDelete"></param>
        /// <returns></returns>
        internal static async Task<int> RemoveWhereAsync(this IQueryable queryable, LambdaExpression predicate = null, bool logicalDelete = true, CancellationToken cancellationToken = default)
        {
            predicate = predicate ?? Expression.Lambda(Expression.Equal(Expression.Constant(1), Expression.Constant(1)), Expression.Parameter(queryable.ElementType, "s"));
            var expression = Expression.Call(null, MthRemoveWhereAsync,
                new[] { Expression.Constant(queryable), predicate, (Expression)Expression.Constant(logicalDelete), Expression.Constant(cancellationToken, typeof(CancellationToken)) });

            return await (await ((IAsyncQueryProvider)queryable.Provider).ExecuteAsync<Task<int>>(expression, cancellationToken));
        }

        /// <summary>
        /// 根据LINQ更新实体。
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="entity"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        internal static int UpdateWhere(this IQueryable queryable, IEntity entity, LambdaExpression predicate)
        {
            predicate = predicate ?? Expression.Lambda(Expression.Equal(Expression.Constant(1), Expression.Constant(1)), Expression.Parameter(entity.EntityType, "s"));
            var method = (MethodInfo)MethodBase.GetCurrentMethod();
            var expression = Expression.Call(null, method,
                new[] { Expression.Constant(queryable), (Expression)Expression.Constant(entity), predicate });

            return queryable.Provider.Execute<int>(expression);
        }

        /// <summary>
        /// 根据LINQ更新实体。
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="entity"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        internal static async Task<int> UpdateWhereAsync(this IQueryable queryable, IEntity entity, LambdaExpression predicate, CancellationToken cancellationToken = default)
        {
            predicate = predicate ?? Expression.Lambda(Expression.Equal(Expression.Constant(1), Expression.Constant(1)), Expression.Parameter(entity.EntityType, "s"));
            var expression = Expression.Call(null, MthUpdateWhereAsync,
                new[] { Expression.Constant(queryable), (Expression)Expression.Constant(entity), predicate, Expression.Constant(cancellationToken, typeof(CancellationToken)) });

            return await (await ((IAsyncQueryProvider)queryable.Provider).ExecuteAsync<Task<int>>(expression, cancellationToken));
        }

        /// <summary>
        /// 根据LINQ更新实体。
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="calculator"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        internal static int UpdateWhereByCalculator(this IQueryable queryable, LambdaExpression calculator, LambdaExpression predicate)
        {
            predicate = predicate ?? Expression.Lambda(Expression.Equal(Expression.Constant(1), Expression.Constant(1)), Expression.Parameter(queryable.ElementType, "s"));
            var method = (MethodInfo)MethodBase.GetCurrentMethod();
            var expression = Expression.Call(null, method,
                new[] { Expression.Constant(queryable), (Expression)calculator, predicate });

            return queryable.Provider.Execute<int>(expression);
        }

        /// <summary>
        /// 根据LINQ更新实体。
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="calculator"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        internal static async Task<int> UpdateWhereByCalculatorAsync(this IQueryable queryable, LambdaExpression calculator, LambdaExpression predicate, CancellationToken cancellationToken = default)
        {
            predicate = predicate ?? Expression.Lambda(Expression.Equal(Expression.Constant(1), Expression.Constant(1)), Expression.Parameter(queryable.ElementType, "s"));
            var expression = Expression.Call(null, MthUpdateWhereByCalculatorAsync,
                new[] { Expression.Constant(queryable), (Expression)calculator, predicate, Expression.Constant(cancellationToken, typeof(CancellationToken)) });

            return await (await ((IAsyncQueryProvider)queryable.Provider).ExecuteAsync<Task<int>>(expression, cancellationToken));
        }

        internal static int BatchOperate(this IQueryable queryable, IEnumerable<IEntity> instances, LambdaExpression fnOperation)
        {
            if (instances.IsNullOrEmpty())
            {
                return 0;
            }

            var method = (MethodInfo)MethodBase.GetCurrentMethod();
            var expression = Expression.Call(null, method,
                new[] { Expression.Constant(queryable), (Expression)Expression.Constant(instances), fnOperation });

            return queryable.Provider.Execute<int>(expression);
        }

        internal static async Task<int> BatchOperateAsync(this IQueryable queryable, IEnumerable<IEntity> instances, LambdaExpression fnOperation, CancellationToken cancellationToken = default)
        {
            if (instances.IsNullOrEmpty())
            {
                return 0;
            }

            var expression = Expression.Call(null, MthBatchOperateAsync,
                new[] { Expression.Constant(queryable), (Expression)Expression.Constant(instances), fnOperation, Expression.Constant(cancellationToken, typeof(CancellationToken)) });

            return await (await ((IAsyncQueryProvider)queryable.Provider).ExecuteAsync<Task<int>>(expression, cancellationToken));
        }

        /// <summary>
        /// 创建 (u, s) => u.Insert(s) 这样的 lambda 表达式。
        /// </summary>
        /// <param name="queryable"></param>
        /// <returns></returns>
        internal static LambdaExpression CreateInsertExpression(this IQueryable queryable)
        {
            var rpType = typeof(IRepository<>).MakeGenericType(queryable.ElementType);
            var method = rpType.GetMethod(nameof(IRepository.Insert), new[] { queryable.ElementType });
            var parSet = Expression.Parameter(rpType, "u");
            var parEle = Expression.Parameter(queryable.ElementType, "s");
            return Expression.Lambda(Expression.Call(parSet, method, parEle), parSet, parEle);
        }

        /// <summary>
        /// 创建 (u, s) => u.Update(s) 这样的 lambda 表达式。
        /// </summary>
        /// <param name="queryable"></param>
        /// <returns></returns>
        internal static LambdaExpression CreateUpdateExpression(this IQueryable queryable)
        {
            var rpType = typeof(IRepository<>).MakeGenericType(queryable.ElementType);
            var method = rpType.GetMethod(nameof(IRepository.Update), new[] { queryable.ElementType });
            var parSet = Expression.Parameter(rpType, "u");
            var parEle = Expression.Parameter(queryable.ElementType, "s");
            return Expression.Lambda(Expression.Call(parSet, method, parEle), parSet, parEle);
        }

        /// <summary>
        /// 创建 (u, s) => u.Delete(s, true) 这样的 lambda 表达式。
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="logicalDelete"></param>
        /// <returns></returns>
        internal static LambdaExpression CreateDeleteExpression(this IQueryable queryable, bool logicalDelete)
        {
            var rpType = typeof(IRepository<>).MakeGenericType(queryable.ElementType);
            var method = rpType.GetMethod(nameof(IRepository.Delete), new[] { queryable.ElementType, typeof(bool) });
            var parSet = Expression.Parameter(rpType, "u");
            var parEle = Expression.Parameter(queryable.ElementType, "s");
            return Expression.Lambda(Expression.Call(parSet, method, parEle, Expression.Constant(logicalDelete)), parSet, parEle);
        }

        private static LambdaExpression BindPrimaryExpression(IEntity entity)
        {
            var primaryProperties = PropertyUnity.GetPrimaryProperties(entity.EntityType).ToList();
            if (primaryProperties.IsNullOrEmpty())
            {
                return null;
            }

            var expressions = new List<Expression>();
            var parExp = Expression.Parameter(entity.EntityType, "s");

            foreach (var property in primaryProperties)
            {
                var value = entity.GetValue(property);
                if (PropertyValue.IsEmpty(value))
                {
                    continue;
                }

                var getValExp = BindGetValueExpression(property, value);
                if (getValExp == null)
                {
                    continue;
                }

                var equalExp = Expression.MakeMemberAccess(parExp, property.Info.ReflectionInfo).Equal(getValExp);

                expressions.Add(equalExp);
            }

            var predicate = expressions.Aggregate(Expression.And);

            return Expression.Lambda(predicate, parExp);
        }

        /// <summary>
        /// 构建主键查询表达式。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="primaryValues"></param>
        /// <returns></returns>
        private static LambdaExpression BindPrimaryExpression(Type type, PropertyValue[] primaryValues)
        {
            Guard.ArgumentNull(primaryValues, nameof(primaryValues));

            var pkProperties = PropertyUnity.GetPrimaryProperties(type).ToList();
            if (pkProperties.IsNullOrEmpty())
            {
                return null;
            }

            //主键个数不一致
            if (primaryValues.Length != pkProperties.Count)
            {
                throw new EntityPersistentException(SR.GetString(SRKind.DisaccordArgument, pkProperties.Count, primaryValues.Length), null);
            }

            var expressions = new List<Expression>();
            var parExp = Expression.Parameter(type, "s");

            for (var i = 0; i < primaryValues.Length; i++)
            {
                var pkValue = primaryValues[i];
                if (pkValue == null)
                {
                    return null;
                }

                var equalExp = Expression.MakeMemberAccess(parExp, pkProperties[i].Info.ReflectionInfo)
                    .Equal(Expression.Constant(pkValue));

                expressions.Add(equalExp);
            }

            var predicate = expressions.Aggregate(Expression.And);

            return Expression.Lambda(predicate, parExp);
        }

        /// <summary>
        /// 构造所有字段的查询表达式。
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private static LambdaExpression BindAllFieldExpression(IEntity entity)
        {
            var expressions = new List<Expression>();
            var parExp = Expression.Parameter(entity.EntityType, "s");

            foreach (var property in PropertyUnity.GetPersistentProperties(entity.EntityType))
            {
                var oldValue = entity.GetOldValue(property);
                if (PropertyValue.IsEmpty(oldValue))
                {
                    continue;
                }

                var getValExp = BindGetValueExpression(property, oldValue);
                if (getValExp == null)
                {
                    continue;
                }

                var equalExp = Expression.MakeMemberAccess(parExp, property.Info.ReflectionInfo)
                    .Equal(getValExp);

                expressions.Add(equalExp);
            }

            var predicate = expressions.Aggregate(Expression.And);

            return Expression.Lambda(predicate, parExp);
        }

        private static Expression BindGetValueExpression(IProperty property, PropertyValue value)
        {
            var op_Explicit = typeof(PropertyValue).GetMethods().FirstOrDefault(s => s.Name == "op_Explicit" && s.ReturnType == property.Type);
            if (op_Explicit == null)
            {
                return null;
            }

            var constExp = Expression.Constant(value);
            return Expression.Call(op_Explicit, constExp);
        }

        /// <summary>
        /// 用于收集表达式中使用的排序表达式。
        /// </summary>
        private class OrderGatherer : Fireasy.Common.Linq.Expressions.ExpressionVisitor
        {
            private Dictionary<MethodInfo, Expression> orderBys = new Dictionary<MethodInfo, Expression>();

            internal static Dictionary<MethodInfo, Expression> Gather(Expression expression)
            {
                var gatherer = new OrderGatherer();
                gatherer.Visit(expression);
                return gatherer.orderBys;
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCallExp)
            {
                if (methodCallExp.Method.DeclaringType == typeof(Queryable) &&
                    (methodCallExp.Method.Name == nameof(Queryable.OrderBy) || methodCallExp.Method.Name == nameof(Queryable.OrderByDescending) ||
                    methodCallExp.Method.Name == nameof(Queryable.ThenBy) || methodCallExp.Method.Name == nameof(Queryable.ThenByDescending)))
                {
                    Visit(methodCallExp.Arguments[0]);
                    orderBys.Add(methodCallExp.Method, methodCallExp.Arguments[1]);
                }
                else
                {
                    throw new ArgumentException(SR.GetString(SRKind.InvalidOrderExpression));
                }

                return methodCallExp;
            }
        }
    }
}