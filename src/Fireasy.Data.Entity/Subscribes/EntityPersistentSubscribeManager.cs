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
using System.Threading.Tasks;

namespace Fireasy.Data.Entity.Subscribes
{
    /// <summary>
    /// 实体持久化订阅管理器。
    /// </summary>
    public class EntityPersistentSubscribeManager
    {
        public static readonly InnerPersistentSubscribeManager Default = new InnerPersistentSubscribeManager();

        private readonly static SafetyDictionary<Type, InnerPersistentSubscribeManager> managers = new SafetyDictionary<Type, InnerPersistentSubscribeManager>();

        /// <summary>
        /// 获取上下文的持久化订阅管理器，如果没有注册过，则注册与之对应的管理器。
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <returns></returns>
        public static InnerPersistentSubscribeManager Get<TContext>() where TContext : EntityContext
        {
            return managers.GetOrAdd(typeof(TContext), k => new InnerPersistentSubscribeManager(k));
        }

        /// <summary>
        /// 获取上下文的持久化订阅管理器，如果没有注册过，则注册与之对应的管理器。
        /// </summary>
        /// <param name="contextType"></param>
        /// <returns></returns>
        public static InnerPersistentSubscribeManager GetManager(Type contextType)
        {
            if (contextType == null)
            {
                return Default;
            }

            return managers.GetOrAdd(contextType, k => new InnerPersistentSubscribeManager(k));
        }

        /// <summary>
        /// 获取上下文的持久化订阅管理器，如果没有注册与类型相对应的管理器，则返回默认的管理器。
        /// </summary>
        /// <param name="contextType"></param>
        /// <returns></returns>
        public static InnerPersistentSubscribeManager GetRequiredManager(Type contextType)
        {
            if (contextType == null)
            {
                return Default;
            }

            if (managers.TryGetValue(contextType, out InnerPersistentSubscribeManager mgr))
            {
                return mgr;
            }

            return Default;
        }

        /// <summary>
        /// 添加一个读取实体持久化消息的方法。
        /// </summary>
        /// <param name="subscriber"></param>
        public static void AddSubscriber(Action<EntityPersistentSubject> subscriber)
        {
            GetManager(null).AddSubscriber(subscriber);
        }

        /// <summary>
        /// 添加一个异步的读取实体持久化消息的方法。
        /// </summary>
        /// <param name="subscriber"></param>
        public static void AddAsyncSubscriber(Func<EntityPersistentSubject, Task> subscriber)
        {
            GetManager(null).AddAsyncSubscriber(subscriber);
        }

        /// <summary>
        /// 添加一个读取实体持久化消息的方法。
        /// </summary>
        /// <param name="subscriber"></param>
        /// <param name="entityTypes">一个数组，指定需要接收订阅消息的实体类型。</param>
        public static void AddSubscriber(Action<EntityPersistentSubject> subscriber, params Type[] entityTypes)
        {
            GetManager(null).AddSubscriber(subscriber, entityTypes);
        }

        /// <summary>
        /// 添加一个读取实体持久化消息的方法。
        /// </summary>
        /// <param name="subscriber"></param>
        /// <param name="filter">一个函数，对接收的订阅消息进行过滤。</param>
        public static void AddSubscriber(Action<EntityPersistentSubject> subscriber, Func<EntityPersistentSubject, bool> filter)
        {
            GetManager(null).AddSubscriber(subscriber, filter);
        }
    }

    public class InnerPersistentSubscribeManager
    {
        private readonly ISubscribeManager subMgr;

        internal protected InnerPersistentSubscribeManager(Type contextType = null)
        {
            subMgr = contextType != null ? new SynchronizedSubscribeManager() : SynchronizedSubscribeManager.Instance;
        }

        /// <summary>
        /// 添加一个读取实体持久化消息的方法。
        /// </summary>
        /// <param name="subscriber"></param>
        public void AddSubscriber(Action<EntityPersistentSubject> subscriber)
        {
            subMgr.AddSubscriber(subscriber);
        }

        /// <summary>
        /// 添加一个异步的读取实体持久化消息的方法。
        /// </summary>
        /// <param name="subscriber"></param>
        public void AddAsyncSubscriber(Func<EntityPersistentSubject, Task> subscriber)
        {
            subMgr.AddAsyncSubscriber(subscriber);
        }

        /// <summary>
        /// 添加一个读取实体持久化消息的方法。
        /// </summary>
        /// <param name="subscriber"></param>
        /// <param name="entityTypes">一个数组，指定需要接收订阅消息的实体类型。</param>
        public void AddSubscriber(Action<EntityPersistentSubject> subscriber, params Type[] entityTypes)
        {
            if (entityTypes == null || entityTypes.Length == 0)
            {
                subMgr.AddSubscriber(typeof(EntityPersistentSubject), subscriber);
                return;
            }

            var action = new Action<EntityPersistentSubject>(t =>
            {
                foreach (var type in entityTypes)
                {
                    if (t.EntityType == type || type.IsAssignableFrom(t.EntityType))
                    {
                        subscriber(t);
                    }
                }
            });

            subMgr.AddSubscriber(typeof(EntityPersistentSubject), action);
        }

        /// <summary>
        /// 添加一个读取实体持久化消息的方法。
        /// </summary>
        /// <param name="subscriber"></param>
        /// <param name="filter">一个函数，对接收的订阅消息进行过滤。</param>
        public void AddSubscriber(Action<EntityPersistentSubject> subscriber, Func<EntityPersistentSubject, bool> filter)
        {
            if (filter == null)
            {
                subMgr.AddSubscriber(typeof(EntityPersistentSubject), subscriber);
                return;
            }

            var action = new Action<EntityPersistentSubject>(t =>
            {
                if (filter(t))
                {
                    subscriber(t);
                }
            });

            subMgr.AddSubscriber(typeof(EntityPersistentSubject), action);
        }

        public TResult OnCreate<TEntity, TResult>(bool notification, IEntity entity, Func<TResult> func)
        {
            if (!notification)
            {
                return func();
            }

            var ret = default(TResult);
            if (Publish(entity, EntityPersistentEventType.BeforeCreate))
            {
                ret = func();
                Publish(entity, EntityPersistentEventType.AfterCreate);
            }

            return ret;
        }

        public async Task<TResult> OnCreateAsync<TEntity, TResult>(bool notification, IEntity entity, Func<Task<TResult>> func)
        {
            if (!notification)
            {
                return await func();
            }

            var ret = default(TResult);
            if (Publish(entity, EntityPersistentEventType.BeforeCreate))
            {
                ret = await func();
                Publish(entity, EntityPersistentEventType.AfterCreate);
            }

            return ret;
        }

        public TResult OnUpdate<TEntity, TResult>(bool notification, IEntity entity, Func<TResult> func)
        {
            if (!notification)
            {
                return func();
            }

            var ret = default(TResult);
            if (Publish(entity, EntityPersistentEventType.BeforeUpdate))
            {
                ret = func();
                Publish(entity, EntityPersistentEventType.AfterUpdate);
            }

            return ret;
        }

        public async Task<TResult> OnUpdateAsync<TEntity, TResult>(bool notification, IEntity entity, Func<Task<TResult>> func)
        {
            if (!notification)
            {
                return await func();
            }

            var ret = default(TResult);
            if (Publish(entity, EntityPersistentEventType.BeforeUpdate))
            {
                ret = await func();
                Publish(entity, EntityPersistentEventType.AfterUpdate);
            }

            return ret;
        }

        public TResult OnRemove<TEntity, TResult>(bool notification, IEntity entity, Func<TResult> func)
        {
            if (!notification)
            {
                return func();
            }

            var ret = default(TResult);
            if (Publish(entity, EntityPersistentEventType.BeforeRemove))
            {
                ret = func();
                Publish(entity, EntityPersistentEventType.AfterRemove);
            }

            return ret;
        }

        public async Task<TResult> OnRemoveAsync<TEntity, TResult>(bool notification, IEntity entity, Func<Task<TResult>> func)
        {
            if (!notification)
            {
                return await func();
            }

            var ret = default(TResult);
            if (Publish(entity, EntityPersistentEventType.BeforeRemove))
            {
                ret = await func();
                Publish(entity, EntityPersistentEventType.AfterRemove);
            }

            return ret;
        }

        public TResult OnBatch<TEntity, TResult>(bool notification, IEnumerable<IEntity> entities, EntityPersistentOperater operater, Func<TResult> func)
        {
            if (!notification)
            {
                return func();
            }

            var ret = default(TResult);
            if (Publish<TEntity>(entities, operater, EntityPersistentEventType.BeforeBatch))
            {
                ret = func();
                Publish<TEntity>(entities, operater, EntityPersistentEventType.AfterBatch);
            }

            return ret;
        }

        public async Task<TResult> OnBatchAsync<TEntity, TResult>(bool notification, IEnumerable<IEntity> entities, EntityPersistentOperater operater, Func<Task<TResult>> func)
        {
            if (!notification)
            {
                return await func();
            }

            var ret = default(TResult);
            if (Publish<TEntity>(entities, operater, EntityPersistentEventType.BeforeBatch))
            {
                ret = await func();
                Publish<TEntity>(entities, operater, EntityPersistentEventType.AfterBatch);
            }

            return ret;
        }

        public bool Publish<TEntity>(IEnumerable<IEntity> entities, EntityPersistentOperater operType, EntityPersistentEventType eventType)
        {
            var _event = new EntitiesArgs(entities, operType, eventType);
            var subject = new EntityPersistentSubject(typeof(TEntity), eventType, _event);

            subMgr.Publish(subject);

            return !_event.Cancel;
        }

        public bool Publish<TEntity>(EntityPersistentEventType eventType)
        {
            var _event = new EntityEventTypeArgs(eventType);
            var subject = new EntityPersistentSubject(typeof(TEntity), eventType, _event);

            subMgr.Publish(subject);

            return !_event.Cancel;
        }

        public bool Publish(IEntity entity, EntityPersistentEventType eventType)
        {
            Guard.ArgumentNull(entity, nameof(entity));

            var _event = new EntityEventArgs(entity, eventType);
            var subject = new EntityPersistentSubject(entity.EntityType, eventType, _event);

            subMgr.Publish(subject);

            return !_event.Cancel;
        }
    }

    internal class EntityEventTypeArgs
    {
        public EntityEventTypeArgs(EntityPersistentEventType eventType)
        {
            EventType = eventType;
        }

        public EntityPersistentEventType EventType { get; set; }

        public bool Cancel { get; set; }
    }

    internal class EntityEventArgs : EntityEventTypeArgs
    {
        public EntityEventArgs(IEntity entity, EntityPersistentEventType eventType)
            : base(eventType)
        {
            Entity = entity;
        }

        public IEntity Entity { get; set; }
    }

    internal class EntitiesArgs : EntityEventTypeArgs
    {
        public EntitiesArgs(IEnumerable<IEntity> entities, EntityPersistentOperater operType, EntityPersistentEventType eventType)
            : base(eventType)
        {
            Entities = entities;
            OperType = operType;
        }

        public IEnumerable<IEntity> Entities { get; set; }

        public EntityPersistentOperater OperType { get; set; }
    }
}
