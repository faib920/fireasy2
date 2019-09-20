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

            Expression condition;
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
        /// <typeparam name="T"></typeparam>
        /// <param name="metadata"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static Expression BuildOrderByExpression<T>(EntityTreeMetadata metadata, Expression source)
        {
            var parExp = Expression.Parameter(typeof(T), "s");
            var memberExp = Expression.MakeMemberAccess(parExp, metadata.InnerSign.Info.ReflectionInfo);

            var lambdaExp = Expression.Lambda(memberExp, parExp);

            return Expression.Call(typeof(Queryable), "OrderBy", new[] { typeof(T), typeof(string) }, new[] { source, lambdaExp });
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
        /// 构造方法 GetChildren 的表达式。 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="metadata"></param>
        /// <param name="no">编码。</param>
        /// <returns></returns>
        internal static Expression BuildGetChildrenExpression<T>(EntityTreeMetadata metadata, string no) where T : class, IEntity
        {
            var parExp = Expression.Parameter(typeof(T), "s");
            var memberExp = Expression.MakeMemberAccess(parExp, metadata.InnerSign.Info.ReflectionInfo);

            var condition1 = Expression.Call(null, MthLike, memberExp, Expression.Constant(string.Concat(no, "_%")));
            var condition2 = Expression.NotEqual(memberExp, Expression.Constant(no));
            var condition = Expression.And(condition1, condition2);

            return Expression.Lambda<Func<T, bool>>(condition, parExp);
        }

        /// <summary>
        /// 构造获取兄弟节点及孩子的表达式。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="metadata"></param>
        /// <param name="argument"></param>
        /// <param name="includeCurrent"></param>
        /// <param name="excludeArg"></param>
        /// <param name="isTop"></param>
        /// <returns></returns>
        internal static Expression BuildGetBrothersAndChildrenExpression<T>(EntityTreeMetadata metadata, EntityTreeUpfydatingArgument argument, bool includeCurrent, EntityTreeUpfydatingArgument excludeArg, bool isTop = false)
        {
            var parExp = Expression.Parameter(typeof(T), "s");
            var memberExp = Expression.MakeMemberAccess(parExp, metadata.InnerSign.Info.ReflectionInfo);

            return null;
        }

        /// <summary>
        /// 构造获取新排序值的表达式。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="metadata"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static Expression BuildGetNewOrderNumberExpression<T>(EntityTreeMetadata metadata, Expression source)
        {
            var parExp = Expression.Parameter(typeof(T), "s");
            var orderExp = GetOrderExpression(metadata, parExp);

            var expression = Expression.Call(typeof(Queryable), "Max", new[] { typeof(T) }, orderExp);
            return Expression.Lambda<Func<T, int>>(expression, parExp);
        }

        /// <summary>
        /// 获取可以组织到查询里的属性。
        /// </summary>
        /// <param name="metadata"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static Expression AddUseableSelectExpression<T>(EntityTreeMetadata metadata, Expression source)
        {
            var parExp = Expression.Parameter(typeof(T), "s");
            var members = new List<MemberBinding>();

            foreach (var pkProperty in PropertyUnity.GetPrimaryProperties(typeof(T)))
            {
                if (pkProperty != metadata.InnerSign)
                {
                    members.Add(Expression.Bind(pkProperty.Info.ReflectionInfo, Expression.MakeMemberAccess(parExp, pkProperty.Info.ReflectionInfo)));
                }
            }

            members.Add(Expression.Bind(metadata.InnerSign.Info.ReflectionInfo, Expression.MakeMemberAccess(parExp, metadata.InnerSign.Info.ReflectionInfo)));

            if (metadata.Name != null)
            {
                members.Add(Expression.Bind(metadata.Name.Info.ReflectionInfo, Expression.MakeMemberAccess(parExp, metadata.Name.Info.ReflectionInfo)));
            }

            if (metadata.FullName != null)
            {
                members.Add(Expression.Bind(metadata.FullName.Info.ReflectionInfo, Expression.MakeMemberAccess(parExp, metadata.FullName.Info.ReflectionInfo)));
            }

            if (metadata.Order != null)
            {
                members.Add(Expression.Bind(metadata.Order.Info.ReflectionInfo, Expression.MakeMemberAccess(parExp, metadata.Order.Info.ReflectionInfo)));
            }

            if (metadata.Level != null)
            {
                members.Add(Expression.Bind(metadata.Level.Info.ReflectionInfo, Expression.MakeMemberAccess(parExp, metadata.Level.Info.ReflectionInfo)));
            }

            var mbrInit = Expression.MemberInit(Expression.New(typeof(T)), members);
            var lambdaExp = Expression.Lambda(mbrInit, parExp);

            return Expression.Call(typeof(Queryable), "Select", new[] { typeof(T), typeof(T) }, new[] { source, lambdaExp });
        }

        /// <summary>
        /// 将数据隔离表达式添加当前表达式中。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="isolation"></param>
        /// <returns></returns>
        internal static Expression AddIsolationExpression<T>(Expression source, Expression isolation)
        {
            if (isolation == null)
            {
                return source;
            }

            var parExp = Expression.Parameter(typeof(T), "s");
            var expressions = new List<Expression>();

            var mbrInit = GetLambda(isolation).Body as MemberInitExpression;
            if (mbrInit == null)
            {
                return source;
            }

            foreach (MemberAssignment bind in mbrInit.Bindings)
            {
                expressions.Add(Expression.Equal(Expression.MakeMemberAccess(parExp, bind.Member), bind.Expression));
            }

            var whereExp = expressions.Aggregate((e1, e2) => Expression.And(e1, e2));

            var lambdaExp = Expression.Lambda(whereExp, parExp);

            return Expression.Call(typeof(Queryable), "Where", new[] { typeof(T) }, new[] { source, lambdaExp });
        }

        /// <summary>
        /// 获取Order的表达式。
        /// </summary>
        /// <param name="metadata"></param>
        /// <param name="parExp"></param>
        /// <returns></returns>
        private static Expression GetOrderExpression(EntityTreeMetadata metadata, Expression parExp)
        {
            //如果Order没有指定，则取InnerId的后N位转成数字
            if (metadata.Order == null)
            {
                var mbrExp = Expression.MakeMemberAccess(parExp, metadata.InnerSign.Info.ReflectionInfo);
                var lenExp = Expression.MakeMemberAccess(mbrExp, typeof(string).GetProperty("Length"));
                var sigExp = Expression.Constant(metadata.SignLength, typeof(int));
                var calExp = Expression.Subtract(Expression.Add(lenExp, Expression.Constant(1, typeof(int))), sigExp);

                return Expression.Call(typeof(string), "Substring", null, mbrExp, calExp, sigExp);
            }

            return Expression.MakeMemberAccess(parExp, metadata.Order.Info.ReflectionInfo);
        }

        /// <summary>
        /// 获取Level的表达式。
        /// </summary>
        /// <param name="metadata"></param>
        /// <param name="parExp"></param>
        /// <returns></returns>
        private static Expression GetLevelExpression(EntityTreeMetadata metadata, Expression parExp)
        {
            //如果Level没有指定，则取InnerId的长度除以N
            if (metadata.Level == null)
            {
                var mbrExp = Expression.MakeMemberAccess(parExp, metadata.InnerSign.Info.ReflectionInfo);
                var lenExp = Expression.MakeMemberAccess(mbrExp, typeof(string).GetProperty("Length"));
                var sigExp = Expression.Constant(metadata.SignLength, typeof(int));

                return Expression.Divide(lenExp, sigExp);
            }

            return Expression.MakeMemberAccess(parExp, metadata.Level.Info.ReflectionInfo);
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
