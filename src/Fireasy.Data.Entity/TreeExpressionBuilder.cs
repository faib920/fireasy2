// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Data.Entity.Linq.Translators;
using Fireasy.Data.Entity.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 实体树操作相关的表达式构造器。
    /// </summary>
    internal class TreeExpressionBuilder
    {
        private static MethodInfo MthLike = typeof(StringExtension).GetMethod("Like", BindingFlags.Public | BindingFlags.Static);

        /// <summary>
        /// 构造方法 QueryChildren 的表达式。 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="metadata"></param>
        /// <param name="parent"></param>
        /// <param name="predicate"></param>
        /// <param name="recurrence"></param>
        /// <returns></returns>
        internal static Expression BuildQueryChildrenExpression<T>(EntityTreeMetadata metadata, T parent, Expression<Func<T, bool>> predicate, bool recurrence = false) where T : class, IEntity
        {
            var parExp = Expression.Parameter(typeof(T), "s");
            var memberExp = Expression.MakeMemberAccess(parExp, metadata.InnerSign.Info.ReflectionInfo);
            var no = parent == null ? string.Empty : (string)parent.GetValue(metadata.InnerSign);

            Expression condition = null;
            if (recurrence)
            {
                condition = Expression.Call(null, MthLike, memberExp, Expression.Constant(string.Concat(no, "%")));
            }
            else
            {
                condition = Expression.Call(null, MthLike, memberExp, Expression.Constant(string.Concat(no, new string('_', metadata.SignLength))));
            }

            if (predicate != null)
            {
                var lambda = GetLambda(predicate);
                if (lambda != null)
                {
                    condition = condition.And(DbExpressionReplacer.Replace(lambda.Body, lambda.Parameters[0], parExp));
                }
            }

            return Expression.Lambda<Func<T, bool>>(condition, parExp);
        }

        /// <summary>
        /// 构造方法 HasChildren 的表达式。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="metadata"></param>
        /// <param name="parent"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        internal static Expression BuildHasChildrenExpression<T>(EntityTreeMetadata metadata, T parent, Expression<Func<T, bool>> predicate) where T : class, IEntity
        {
            var parExp = Expression.Parameter(typeof(T), "s");

            var memberExp = Expression.MakeMemberAccess(parExp, metadata.InnerSign.Info.ReflectionInfo);
            var no = parent == null ? string.Empty : (string)parent.GetValue(metadata.InnerSign);
            var condition = (Expression)Expression.Call(null, MthLike, memberExp, Expression.Constant(string.Concat(no, new string('_', metadata.SignLength))));

            if (predicate != null)
            {
                var lambda = GetLambda(predicate);
                if (lambda != null)
                {
                    condition = condition.And(DbExpressionReplacer.Replace(lambda.Body, lambda.Parameters[0], parExp));
                }
            }

            return Expression.Lambda<Func<T, bool>>(condition, parExp);
        }

        /// <summary>
        /// 构造匹配多个内部编码的表达式。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="metadata"></param>
        /// <param name="predicate"></param>
        /// <param name="innerIds"></param>
        /// <returns></returns>
        internal static Expression BuildGetByInnerIdExpression<T>(EntityTreeMetadata metadata, Expression<Func<T, bool>> predicate, List<string> innerIds)
        {
            var parExp = Expression.Parameter(typeof(T), "s");
            var memberExp = Expression.MakeMemberAccess(parExp, metadata.InnerSign.Info.ReflectionInfo);

            var expres = new List<Expression>();

            foreach (var innerId in innerIds)
            {
                expres.Add(Expression.Equal(memberExp, Expression.Constant(innerId)));
            }

            Expression condition = null;

            //如果没有父编码，返回不成立的 1=0 表达式
            if (expres.Count == 0)
            {
                condition = Expression.Equal(Expression.Constant(1, typeof(int)), Expression.Constant(0, typeof(int)));
            }
            else
            {
                condition = expres.Aggregate(Expression.Or);
            }

            if (predicate != null)
            {
                var lambda = GetLambda(predicate);
                if (lambda != null)
                {
                    condition = condition.And(DbExpressionReplacer.Replace(lambda.Body, lambda.Parameters[0], parExp));
                }
            }

            return Expression.Lambda<Func<T, bool>>(condition, parExp);
        }
        /// <summary>
        /// 构造采用编码排序的表达式。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="metadata"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static Expression BuildOrderByExpression<TEntity>(EntityTreeMetadata metadata, Expression source)
        {
            var parExp = Expression.Parameter(typeof(TEntity), "s");
            var memberExp = Expression.MakeMemberAccess(parExp, metadata.InnerSign.Info.ReflectionInfo);

            var lambdaExp =  Expression.Lambda(memberExp, parExp);

            return Expression.Call(typeof(Queryable), "OrderBy", new[] { typeof(TEntity), typeof(string) }, new[] { source, lambdaExp });
        }

        /// <summary>
        /// 构造采用编码长度倒序排序的表达式。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="metadata"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static Expression BuildOrderByLengthDescExpression<TEntity>(EntityTreeMetadata metadata, Expression source)
        {
            var parExp = Expression.Parameter(typeof(TEntity), "s");
            var memberExp = Expression.MakeMemberAccess(parExp, metadata.InnerSign.Info.ReflectionInfo);
            memberExp = Expression.MakeMemberAccess(memberExp, typeof(string).GetProperty("Length"));

            var lambdaExp = Expression.Lambda(memberExp, parExp);

            return Expression.Call(typeof(Queryable), "OrderByDescending", new[] { typeof(TEntity), typeof(int) }, new[] { source, lambdaExp });
        }

        /// <summary>
        /// 在表达式中查找 <see cref="LambdaExpression"/>。
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
