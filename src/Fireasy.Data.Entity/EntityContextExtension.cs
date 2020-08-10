// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;
using Fireasy.Common.Extensions;
using Fireasy.Data.Provider;
using System;
using System.Data;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Data.Entity
{
    public static class EntityContextExtension
    {
        /// <summary>
        /// 使用事务执行指定的操作。
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="context"></param>
        /// <param name="action">要执行的操作。</param>
        /// <param name="level">事务级别。</param>
        /// <param name="throwExp">发生异常时的处理。</param>
        public static void UseTransaction<TContext>(this TContext context, Action<TContext> action, IsolationLevel? level = null, Action<Exception> throwExp = null) where TContext : EntityContext
        {
            Guard.ArgumentNull(action, nameof(action));

            try
            {
                context.BeginTransaction(level);

                action(context);

                context.CommitTransaction();
            }
            catch (Exception exp)
            {
                context.RollbackTransaction();

                if (throwExp != null)
                {
                    throwExp(exp);
                }
                else
                {
                    throw exp;
                }
            }
        }

        /// <summary>
        /// 使用事务执行指定的操作，并返回数据。
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="context"></param>
        /// <param name="func">要执行的操作。</param>
        /// <param name="level">事务级别。</param>
        /// <param name="throwExp">发生异常时的处理。</param>
        /// <returns></returns>
        public static TResult UseTransaction<TContext, TResult>(this TContext context, Func<TContext, TResult> func, IsolationLevel? level = null, Func<Exception, TResult> throwExp = null) where TContext : EntityContext
        {
            Guard.ArgumentNull(func, nameof(func));

            try
            {
                context.BeginTransaction(level);

                var ret = func(context);

                context.CommitTransaction();

                return ret;
            }
            catch (Exception exp)
            {
                context.RollbackTransaction();

                if (throwExp != null)
                {
                    return throwExp(exp);
                }
                else
                {
                    throw exp;
                }
            }
        }

        /// <summary>
        /// 异步的，使用事务执行指定的操作。
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="context"></param>
        /// <param name="func">要执行的操作。</param>
        /// <param name="level">事务级别。</param>
        /// <param name="throwExp">发生异常时的处理。</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task UseTransactionAsync<TContext>(this TContext context, Func<TContext, CancellationToken, Task> func, IsolationLevel? level = null, Func<Exception, Task> throwExp = null, CancellationToken cancellationToken = default) where TContext : EntityContext
        {
            Guard.ArgumentNull(func, nameof(func));

            try
            {
                context.BeginTransaction(level);

                await func(context, cancellationToken);

                context.CommitTransaction();
            }
            catch (Exception exp)
            {
                context.RollbackTransaction();

                if (throwExp != null)
                {
                    await throwExp(exp);
                }
                else
                {
                    throw exp;
                }
            }
        }

        /// <summary>
        /// 异步的，使用事务执行指定的操作，并返回数据。
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="context"></param>
        /// <param name="func">要执行的操作。</param>
        /// <param name="level">事务级别。</param>
        /// <param name="throwExp">发生异常时的处理。</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<TResult> UseTransactionAsync<TContext, TResult>(this TContext context, Func<TContext, CancellationToken, Task<TResult>> func, IsolationLevel? level = null, Func<Exception, Task<TResult>> throwExp = null, CancellationToken cancellationToken = default) where TContext : EntityContext
        {
            Guard.ArgumentNull(func, nameof(func));

            try
            {
                context.BeginTransaction(level);

                var ret = await func(context, cancellationToken);

                context.CommitTransaction();

                return ret;
            }
            catch (Exception exp)
            {
                context.RollbackTransaction();

                if (throwExp != null)
                {
                    return await throwExp(exp);
                }
                else
                {
                    throw exp;
                }
            }
        }

        /// <summary>
        /// 构造一个实体代理对象。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="entityType">实体类型。</param>
        /// <returns></returns>
        public static IEntity New(this EntityContext context, Type entityType)
        {
            return context.New(entityType, false);
        }

        /// <summary>
        /// 构造一个实体代理对象。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="entityType">实体类型。</param>
        /// <param name="applyDefaultValue">是否应用默认值。</param>
        /// <returns></returns>
        public static IEntity New<TContext>(this TContext context, Type entityType, bool applyDefaultValue) where TContext : EntityContext
        {
            var provider = (IProvider)context.GetService(typeof(IProvider));
            var proxyType = EntityProxyManager.GetType(typeof(TContext), entityType);
            var entity = proxyType.New<IEntity>();

            if (applyDefaultValue)
            {
                return entity.ApplyDefaultValue();
            }

            return entity;
        }

        /// <summary>
        /// 构造一个实体代理对象。
        /// </summary>
        /// <typeparam name="TEntity">实体类型。</typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public static TEntity New<TEntity>(this EntityContext context) where TEntity : IEntity
        {
            return (TEntity)context.New(typeof(TEntity), false);
        }

        /// <summary>
        /// 构造一个实体代理对象。
        /// </summary>
        /// <typeparam name="TEntity">实体类型。</typeparam>
        /// <param name="context"></param>
        /// <param name="applyDefaultValue">是否应用默认值。</param>
        /// <returns></returns>
        public static TEntity New<TEntity>(this EntityContext context, bool applyDefaultValue) where TEntity : IEntity
        {
            return (TEntity)context.New(typeof(TEntity), applyDefaultValue);
        }

        /// <summary>
        /// 通过 <see cref="MemberInitExpression"/> 表达式来构造一个实体代理对象。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="entityType">实体类型。</param>
        /// <param name="initExp">一个成员绑定的表达式。</param>
        /// <returns></returns>
        public static IEntity Wrap(this EntityContext context, Type entityType, LambdaExpression initExp)
        {
            return context.Wrap(entityType, initExp, false);
        }

        /// <summary>
        /// 通过 <see cref="MemberInitExpression"/> 表达式来构造一个实体代理对象。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="entityType">实体类型。</param>
        /// <param name="initExp">一个成员绑定的表达式。</param>
        /// <param name="applyDefaultValue">是否应用默认值。</param>
        /// <returns></returns>
        public static IEntity Wrap(this EntityContext context, Type entityType, LambdaExpression initExp, bool applyDefaultValue)
        {
            var entity = context.New(entityType, applyDefaultValue);

            entity.InitByExpression(initExp);
            return entity;
        }

        /// <summary>
        /// 通过 <see cref="MemberInitExpression"/> 表达式来构造一个实体代理对象。
        /// </summary>
        /// <typeparam name="TEntity">实体类型。</typeparam>
        /// <param name="context"></param>
        /// <param name="initExp">一个成员绑定的表达式。</param>
        /// <returns></returns>
        public static TEntity Wrap<TEntity>(this EntityContext context, Expression<Func<TEntity>> initExp) where TEntity : IEntity
        {
            return (TEntity)context.Wrap(typeof(TEntity), initExp, false);
        }

        /// <summary>
        /// 通过 <see cref="MemberInitExpression"/> 表达式来构造一个实体代理对象。
        /// </summary>
        /// <typeparam name="TEntity">实体类型。</typeparam>
        /// <param name="context"></param>
        /// <param name="initExp">一个成员绑定的表达式。</param>
        /// <param name="applyDefaultValue">是否应用默认值。</param>
        /// <returns></returns>
        public static TEntity Wrap<TEntity>(this EntityContext context, Expression<Func<TEntity>> initExp, bool applyDefaultValue) where TEntity : IEntity
        {
            return (TEntity)context.Wrap(typeof(TEntity), initExp, applyDefaultValue);
        }
    }
}
