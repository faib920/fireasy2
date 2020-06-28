// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common;
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Subscribes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fireasy.Common.Extensions;
#if NETSTANDARD
using Microsoft.Extensions.DependencyInjection;
#endif

namespace Fireasy.Data.Entity.Subscribes
{
    /// <summary>
    /// 实体持久化订阅管理器。
    /// </summary>
    public class PersistentSubscribeManager
    {
        public static readonly InnerSubscribeManager Default = new InnerSubscribeManager();

        private readonly static SafetyDictionary<Type, InnerSubscribeManager> managers = new SafetyDictionary<Type, InnerSubscribeManager>();

        /// <summary>
        /// 获取上下文的持久化订阅管理器，如果没有注册过，则注册与之对应的管理器。
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <returns></returns>
        public static InnerSubscribeManager Get<TContext>() where TContext : EntityContext
        {
            return managers.GetOrAdd(typeof(TContext), k => new InnerSubscribeManager(k));
        }

        /// <summary>
        /// 获取上下文的持久化订阅管理器，如果没有注册过，则注册与之对应的管理器。
        /// </summary>
        /// <param name="contextType"></param>
        /// <returns></returns>
        public static InnerSubscribeManager GetManager(Type contextType)
        {
            if (contextType == null)
            {
                return Default;
            }

            return managers.GetOrAdd(contextType, k => new InnerSubscribeManager(k));
        }

        /// <summary>
        /// 获取上下文的持久化订阅管理器，如果没有注册与类型相对应的管理器，则返回默认的管理器。
        /// </summary>
        /// <param name="contextType"></param>
        /// <returns></returns>
        public static InnerSubscribeManager GetRequiredManager(Type contextType)
        {
            if (contextType == null)
            {
                return Default;
            }

            if (managers.TryGetValue(contextType, out InnerSubscribeManager mgr))
            {
                return mgr;
            }

            return Default;
        }

        /// <summary>
        /// 添加一个读取实体持久化消息的方法。
        /// </summary>
        /// <param name="subscriber"></param>
        public static void AddSubscriber(Action<PersistentSubject> subscriber)
        {
            GetManager(null).AddSubscriber(subscriber);
        }

        /// <summary>
        /// 添加一个异步的读取实体持久化消息的方法。
        /// </summary>
        /// <param name="subscriber"></param>
        public static void AddAsyncSubscriber(Func<PersistentSubject, Task> subscriber)
        {
            GetManager(null).AddAsyncSubscriber(subscriber);
        }

        /// <summary>
        /// 添加一个读取实体持久化消息的方法。
        /// </summary>
        /// <param name="subscriber"></param>
        /// <param name="entityTypes">一个数组，指定需要接收订阅消息的实体类型。</param>
        public static void AddSubscriber(Action<PersistentSubject> subscriber, params Type[] entityTypes)
        {
            GetManager(null).AddSubscriber(subscriber, entityTypes);
        }

        /// <summary>
        /// 添加一个读取实体持久化消息的方法。
        /// </summary>
        /// <param name="subscriber"></param>
        /// <param name="filter">一个函数，对接收的订阅消息进行过滤。</param>
        public static void AddSubscriber(Action<PersistentSubject> subscriber, Func<PersistentSubject, bool> filter)
        {
            GetManager(null).AddSubscriber(subscriber, filter);
        }
    }

    public class InnerSubscribeManager
    {
        private readonly ISubscribeManager subMgr;

        internal protected InnerSubscribeManager(Type contextType = null)
        {
            subMgr = contextType != null ? new SynchronizedSubscribeManager() : SynchronizedSubscribeManager.Instance;
        }

        /// <summary>
        /// 添加一个读取实体持久化消息的方法。
        /// </summary>
        /// <param name="subscriber"></param>
        public void AddSubscriber(Action<PersistentSubject> subscriber)
        {
            subMgr.AddSubscriber(subscriber);
        }

        /// <summary>
        /// 添加一个异步的读取实体持久化消息的方法。
        /// </summary>
        /// <param name="subscriber"></param>
        public void AddAsyncSubscriber(Func<PersistentSubject, Task> subscriber)
        {
            subMgr.AddAsyncSubscriber(subscriber);
        }

        /// <summary>
        /// 添加一个读取实体持久化消息的方法。
        /// </summary>
        /// <param name="subscriber"></param>
        /// <param name="entityTypes">一个数组，指定需要接收订阅消息的实体类型。</param>
        public void AddSubscriber(Action<PersistentSubject> subscriber, params Type[] entityTypes)
        {
            if (entityTypes == null || entityTypes.Length == 0)
            {
                subMgr.AddSubscriber(typeof(PersistentSubject), subscriber);
                return;
            }

            var action = new Action<PersistentSubject>(t =>
            {
                foreach (var type in entityTypes)
                {
                    if (t.EntityType == type || type.IsAssignableFrom(t.EntityType))
                    {
                        subscriber(t);
                    }
                }
            });

            subMgr.AddSubscriber(typeof(PersistentSubject), action);
        }

        /// <summary>
        /// 添加一个读取实体持久化消息的方法。
        /// </summary>
        /// <param name="subscriber"></param>
        /// <param name="filter">一个函数，对接收的订阅消息进行过滤。</param>
        public void AddSubscriber(Action<PersistentSubject> subscriber, Func<PersistentSubject, bool> filter)
        {
            if (filter == null)
            {
                subMgr.AddSubscriber(typeof(PersistentSubject), subscriber);
                return;
            }

            var action = new Action<PersistentSubject>(t =>
            {
                if (filter(t))
                {
                    subscriber(t);
                }
            });

            subMgr.AddSubscriber(typeof(PersistentSubject), action);
        }

        public TResult OnCreate<TEntity, TResult>(IServiceProvider serviceProvider, bool notification, IEntity entity, Func<TResult> func)
        {
            if (!notification)
            {
                return func();
            }

            var ret = default(TResult);
            if (Publish(serviceProvider, entity, PersistentEventType.BeforeCreate))
            {
                ret = func();
                Publish(serviceProvider, entity, PersistentEventType.AfterCreate);
            }

            return ret;
        }

        public async Task<TResult> OnCreateAsync<TEntity, TResult>(IServiceProvider serviceProvider, bool notification, IEntity entity, Func<Task<TResult>> func)
        {
            if (!notification)
            {
                return await func();
            }

            var ret = default(TResult);
            if (await PublishAsync(serviceProvider, entity, PersistentEventType.BeforeCreate))
            {
                ret = await func();
                await PublishAsync(serviceProvider, entity, PersistentEventType.AfterCreate);
            }

            return ret;
        }

        public TResult OnUpdate<TEntity, TResult>(IServiceProvider serviceProvider, bool notification, IEntity entity, Func<TResult> func)
        {
            if (!notification)
            {
                return func();
            }

            var ret = default(TResult);
            if (Publish(serviceProvider, entity, PersistentEventType.BeforeUpdate))
            {
                ret = func();
                Publish(serviceProvider, entity, PersistentEventType.AfterUpdate);
            }

            return ret;
        }

        public async Task<TResult> OnUpdateAsync<TEntity, TResult>(IServiceProvider serviceProvider, bool notification, IEntity entity, Func<Task<TResult>> func)
        {
            if (!notification)
            {
                return await func();
            }

            var ret = default(TResult);
            if (await PublishAsync(serviceProvider, entity, PersistentEventType.BeforeUpdate))
            {
                ret = await func();
                await PublishAsync(serviceProvider, entity, PersistentEventType.AfterUpdate);
            }

            return ret;
        }

        public TResult OnRemove<TEntity, TResult>(IServiceProvider serviceProvider, bool notification, IEntity entity, Func<TResult> func)
        {
            if (!notification)
            {
                return func();
            }

            var ret = default(TResult);
            if (Publish(serviceProvider, entity, PersistentEventType.BeforeRemove))
            {
                ret = func();
                Publish(serviceProvider, entity, PersistentEventType.AfterRemove);
            }

            return ret;
        }

        public async Task<TResult> OnRemoveAsync<TEntity, TResult>(IServiceProvider serviceProvider, bool notification, IEntity entity, Func<Task<TResult>> func)
        {
            if (!notification)
            {
                return await func();
            }

            var ret = default(TResult);
            if (await PublishAsync(serviceProvider, entity, PersistentEventType.BeforeRemove))
            {
                ret = await func();
                await PublishAsync(serviceProvider, entity, PersistentEventType.AfterRemove);
            }

            return ret;
        }

        public TResult OnBatch<TEntity, TResult>(IServiceProvider serviceProvider, bool notification, IEnumerable<IEntity> entities, PersistentOperator operater, Func<TResult> func)
        {
            if (!notification)
            {
                return func();
            }

            var ret = default(TResult);
            if (Publish<TEntity>(serviceProvider, entities, operater, PersistentEventType.BeforeBatch))
            {
                ret = func();
                Publish<TEntity>(serviceProvider, entities, operater, PersistentEventType.AfterBatch);
            }

            return ret;
        }

        public async Task<TResult> OnBatchAsync<TEntity, TResult>(IServiceProvider serviceProvider, bool notification, IEnumerable<IEntity> entities, PersistentOperator operater, Func<Task<TResult>> func)
        {
            if (!notification)
            {
                return await func();
            }

            var ret = default(TResult);
            if (await PublishAsync<TEntity>(serviceProvider, entities, operater, PersistentEventType.BeforeBatch))
            {
                ret = await func();
                await PublishAsync<TEntity>(serviceProvider, entities, operater, PersistentEventType.AfterBatch);
            }

            return ret;
        }

        public bool Publish<TEntity>(IServiceProvider serviceProvider, IEnumerable<IEntity> entities, PersistentOperator operType, PersistentEventType eventType)
        {
            if (!entities.Any())
            {
                return true;
            }

            var _event = new EntitiesArgs(entities, operType, eventType);
            var subject = new PersistentSubject(typeof(TEntity), eventType, _event);

            subMgr.Publish(subject);

            AcceptForServiceProvider(serviceProvider, subject);

            return !_event.Cancel;
        }

        public async Task<bool> PublishAsync<TEntity>(IServiceProvider serviceProvider, IEnumerable<IEntity> entities, PersistentOperator operType, PersistentEventType eventType)
        {
            if (!entities.Any())
            {
                return true;
            }

            var _event = new EntitiesArgs(entities, operType, eventType);
            var subject = new PersistentSubject(typeof(TEntity), eventType, _event);

            await subMgr.PublishAsync(subject);

            await AcceptForServiceProviderAsync(serviceProvider, subject);

            return !_event.Cancel;
        }

        public bool Publish(IServiceProvider serviceProvider, IEnumerable<IEntity> entities, PersistentOperator operType, PersistentEventType eventType)
        {
            IEntity first;
            if ((first = entities.FirstOrDefault()) == null)
            {
                return true;
            }

            var _event = new EntitiesArgs(entities, operType, eventType);
            var entityType = first.EntityType;
            var subject = new PersistentSubject(entityType, eventType, _event);

            subMgr.Publish(subject);

            AcceptForServiceProvider(serviceProvider, subject);

            return !_event.Cancel;
        }

        public async Task<bool> PublishAsync(IServiceProvider serviceProvider, IEnumerable<IEntity> entities, PersistentOperator operType, PersistentEventType eventType)
        {
            IEntity first;
            if ((first = entities.FirstOrDefault()) == null)
            {
                return true;
            }

            var _event = new EntitiesArgs(entities, operType, eventType);
            var entityType = first.EntityType;
            var subject = new PersistentSubject(entityType, eventType, _event);

            await subMgr.PublishAsync(subject);

            await AcceptForServiceProviderAsync(serviceProvider, subject);

            return !_event.Cancel;
        }

        public bool Publish<TEntity>(IServiceProvider serviceProvider, PersistentEventType eventType)
        {
            var _event = new EntityEventTypeArgs(eventType);
            var subject = new PersistentSubject(typeof(TEntity), eventType, _event);

            subMgr.Publish(subject);

            AcceptForServiceProvider(serviceProvider, subject);

            return !_event.Cancel;
        }

        public async Task<bool> PublishAsync<TEntity>(IServiceProvider serviceProvider, PersistentEventType eventType)
        {
            var _event = new EntityEventTypeArgs(eventType);
            var subject = new PersistentSubject(typeof(TEntity), eventType, _event);

            await subMgr.PublishAsync(subject);

            return !_event.Cancel;
        }

        public bool Publish(IServiceProvider serviceProvider, IEntity entity, PersistentEventType eventType)
        {
            Guard.ArgumentNull(entity, nameof(entity));

            var _event = new EntityEventArgs(entity, eventType);
            var subject = new PersistentSubject(entity.EntityType, eventType, _event);

            subMgr.Publish(subject);

            AcceptForServiceProvider(serviceProvider, subject);

            return !_event.Cancel;
        }

        public async Task<bool> PublishAsync(IServiceProvider serviceProvider, IEntity entity, PersistentEventType eventType)
        {
            Guard.ArgumentNull(entity, nameof(entity));

            var _event = new EntityEventArgs(entity, eventType);
            var subject = new PersistentSubject(entity.EntityType, eventType, _event);

            await subMgr.PublishAsync(subject);

            await AcceptForServiceProviderAsync(serviceProvider, subject);

            return !_event.Cancel;
        }

        private void AcceptForServiceProvider(IServiceProvider serviceProvider, PersistentSubject subject)
        {
#if NETSTANDARD
            if (serviceProvider != null)
            {
                foreach (var sub in serviceProvider.GetService<IEnumerable<PersistentSubscriber>>())
                {
                    sub.Accept(subject);
                }

                foreach (var sub in serviceProvider.GetService<IEnumerable<AsyncPersistentSubscriber>>())
                {
                    sub.AcceptAsync(subject).AsSync();
                }
            }
#endif
        }

        private async Task AcceptForServiceProviderAsync(IServiceProvider serviceProvider, PersistentSubject subject)
        {
#if NETSTANDARD
            if (serviceProvider != null)
            {
                foreach (var sub in serviceProvider.GetService<IEnumerable<PersistentSubscriber>>())
                {
                    sub.Accept(subject);
                }

                foreach (var sub in serviceProvider.GetService<IEnumerable<AsyncPersistentSubscriber>>())
                {
                    await sub.AcceptAsync(subject);
                }
            }
#endif
        }
    }

    internal class EntityEventTypeArgs
    {
        public EntityEventTypeArgs(PersistentEventType eventType)
        {
            EventType = eventType;
        }

        public PersistentEventType EventType { get; set; }

        public bool Cancel { get; set; }
    }

    internal class EntityEventArgs : EntityEventTypeArgs
    {
        public EntityEventArgs(IEntity entity, PersistentEventType eventType)
            : base(eventType)
        {
            Entity = entity;
        }

        public IEntity Entity { get; set; }
    }

    internal class EntitiesArgs : EntityEventTypeArgs
    {
        public EntitiesArgs(IEnumerable<IEntity> entities, PersistentOperator operType, PersistentEventType eventType)
            : base(eventType)
        {
            Entities = entities;
            OperType = operType;
        }

        public IEnumerable<IEntity> Entities { get; set; }

        public PersistentOperator OperType { get; set; }
    }
}
