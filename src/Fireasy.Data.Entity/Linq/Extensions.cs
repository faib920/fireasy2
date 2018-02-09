// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common;
using Fireasy.Common.Extensions;
using Fireasy.Common.Linq.Expressions;
using Fireasy.Data.Entity.Linq.Translators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Fireasy.Data.Entity.Linq
{
    /// <summary>
    /// LINQ 扩展方法。
    /// </summary>
    public static class Extensions
    {
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

            var expression = Expression.Call(typeof(Queryable), "Where", new[] { typeof(T) }, source.Expression, predicate);

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

            var expression = Expression.Call(typeof(Queryable), "Where", new[] { typeof(T) }, source.Expression, condition ? isTruePredicate : isFalsePredicate);

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

            var method = typeof(Extensions).GetMethod("ExtendAs").MakeGenericMethod(typeof(TResult));

            var newExp = ExpressionReplacer.Replace(selector.Body, parExp);
            var newSelector = Expression.Lambda<Func<object>>(newExp);

            var callExp = Expression.Call(null, method, parExp, newSelector);
            var lambda = Expression.Lambda(callExp, parExp);

            var expression = Expression.Call(typeof(Queryable), "Select", new[] { typeof(TSource), typeof(TResult) }, source.Expression, lambda);

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

            var parExp = Expression.Parameter(source.ElementType, "s");

            Expression joinExp = null;
            foreach (var item in collection)
            {
                var exp = ParameterRewriter.Rewrite(predicate, parExp, item);
                joinExp = joinExp == null ? exp : Expression.Or(joinExp, exp);
            }

            if (joinExp == null)
            {
                return source;
            }

            var lambda = Expression.Lambda<Func<TSource, bool>>(joinExp, parExp);
            var expression = Expression.Call(typeof(Queryable), "Where", new[] { typeof(TSource) }, source.Expression, lambda);

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

            var parExp = Expression.Parameter(source.ElementType, "s");
            Expression joinExp = null;
            foreach (var item in collection)
            {
                var exp = ParameterRewriter.Rewrite(predicate, parExp, item);
                joinExp = joinExp == null ? exp : Expression.And(joinExp, exp);
            }

            if (joinExp == null)
            {
                return source;
            }

            var lambda = Expression.Lambda<Func<TSource, bool>>(joinExp, parExp);
            var expression = Expression.Call(typeof(Queryable), "Where", new[] { typeof(TSource) }, source.Expression, lambda);

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

            var methodName = sortOrder == SortOrder.Ascending ? "OrderBy" : "OrderByDescending";

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

            var methodName = sortOrder == SortOrder.Ascending ? "ThenBy" : "ThenByDescending";

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

#if !NET35
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
            expression = Expression.Call(typeof(Queryable), methodName, new [] { sourceType, propertyType }, source.Expression, lambda);

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
        internal static TEntity GetByPrimary<TEntity, TKey>(this IQueryable queryable, TKey[] primaryValues)
        {
            Guard.ArgumentNull(primaryValues, nameof(primaryValues));

            var predicate = BindPrimaryExpression(queryable.ElementType, primaryValues);
            if (predicate == null)
            {
                return default(TEntity);
            }

            var expression = Expression.Call(typeof(Queryable), "FirstOrDefault", new[] { queryable.ElementType }, Expression.Constant(queryable), (Expression)predicate);

            return queryable.Provider.Execute<TEntity>(expression);
        }

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
            var result = queryable.Provider.Execute(expression);
            if (primary != null && !entity.IsModified(primary.Name) &&
                !result.IsNullOrEmpty())
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
                return 0;
            }

            return queryable.UpdateWhere(entity, expression);
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
        internal static int RemoveByPrimary<TKey>(this IQueryable queryable, TKey[] primaryKeys, bool logicalDelete)
        {
            var expression = BindPrimaryExpression(queryable.ElementType, primaryKeys);
            if (expression == null)
            {
                return 0;
            }

            return queryable.RemoveWhere(expression, logicalDelete);
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
        /// <param name="calculator"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        internal static int UpdateWhere(this IQueryable queryable, LambdaExpression calculator, LambdaExpression predicate)
        {
            predicate = predicate ?? Expression.Lambda(Expression.Equal(Expression.Constant(1), Expression.Constant(1)), Expression.Parameter(queryable.ElementType, "s"));
            var method = (MethodInfo)MethodBase.GetCurrentMethod();
            var expression = Expression.Call(null, method,
                new[] { Expression.Constant(queryable), (Expression)calculator, predicate });

            return queryable.Provider.Execute<int>(expression);
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

        /// <summary>
        /// 创建 (u, s) => u.Insert(s) 这样的 lambda 表达式。
        /// </summary>
        /// <param name="queryable"></param>
        /// <returns></returns>
        internal static LambdaExpression CreateInsertExpression(this IQueryable queryable)
        {
            var rpType = typeof(IRepository<>).MakeGenericType(queryable.ElementType);
            var method = rpType.GetMethod("Insert");
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
            var method = rpType.GetMethod("Update", new[] { queryable.ElementType });
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
            var method = rpType.GetMethod("Delete", new[] { queryable.ElementType, typeof(bool) });
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

            var parExp = Expression.Parameter(entity.EntityType, "s");
            Expression expression = null;
            foreach (var p in primaryProperties)
            {
                var kv = entity.GetValue(p);
                var condition = Expression.MakeMemberAccess(parExp, p.Info.ReflectionInfo).Equal(Expression.Constant(kv));
                if (expression == null)
                {
                    expression = condition;
                }
                else
                {
                    expression = Expression.And(expression, condition);
                }
            }

            return Expression.Lambda(expression, parExp);
        }

        /// <summary>
        /// 构建主键查询表达式。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="primaryValues"></param>
        /// <returns></returns>
        private static LambdaExpression BindPrimaryExpression<TKey>(Type type, TKey[] primaryValues)
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

            var parExp = Expression.Parameter(type, "s");
            Expression predicate = null;
            for (var i = 0; i < primaryValues.Length; i++)
            {
                var pkValue = primaryValues[i];
                if (pkValue == null)
                {
                    return null;
                }

                var expression = Expression.MakeMemberAccess(parExp, pkProperties[i].Info.ReflectionInfo)
                    .Equal(Expression.Constant(pkValue));

                if (predicate == null)
                {
                    predicate = expression;
                }
                else
                {
                    predicate = Expression.And(predicate, expression);
                }
            }

            return Expression.Lambda(predicate, parExp);
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
                    (methodCallExp.Method.Name == "OrderBy" || methodCallExp.Method.Name == "OrderByDescending" ||
                    methodCallExp.Method.Name == "ThenBy" || methodCallExp.Method.Name == "ThenByDescending"))
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