// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

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
        /// <summary>
        /// 添加一个读取实体持久化消息的方法。
        /// </summary>
        /// <param name="subscriber"></param>
        public static void AddSubscriber(Action<EntityPersistentSubject> subscriber)
        {
            SynchronizedSubscribeManager.Instance.AddSubscriber<EntityPersistentSubject>(subscriber);
        }

        /// <summary>
        /// 添加一个异步的读取实体持久化消息的方法。
        /// </summary>
        /// <param name="subscriber"></param>
        public static void AddAsyncSubscriber(Func<EntityPersistentSubject, Task> subscriber)
        {
            SynchronizedSubscribeManager.Instance.AddAsyncSubscriber<EntityPersistentSubject>(subscriber);
        }

        /// <summary>
        /// 添加一个读取实体持久化消息的方法。
        /// </summary>
        /// <param name="subscriber"></param>
        /// <param name="entityTypes">一个数组，指定需要接收订阅消息的实体类型。</param>
        public static void AddSubscriber(Action<EntityPersistentSubject> subscriber, params Type[] entityTypes)
        {
            if (entityTypes == null || entityTypes.Length == 0)
            {
                SynchronizedSubscribeManager.Instance.AddSubscriber(typeof(EntityPersistentSubject), subscriber);
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

            SynchronizedSubscribeManager.Instance.AddSubscriber(typeof(EntityPersistentSubject), action);
        }

        /// <summary>
        /// 添加一个读取实体持久化消息的方法。
        /// </summary>
        /// <param name="subscriber"></param>
        /// <param name="filter">一个函数，对接收的订阅消息进行过滤。</param>
        public static void AddSubscriber(Action<EntityPersistentSubject> subscriber, Func<EntityPersistentSubject, bool> filter)
        {
            if (filter == null)
            {
                SynchronizedSubscribeManager.Instance.AddSubscriber(typeof(EntityPersistentSubject), subscriber);
                return;
            }

            var action = new Action<EntityPersistentSubject>(t =>
            {
                if (filter(t))
                {
                    subscriber(t);
                }
            });

            SynchronizedSubscribeManager.Instance.AddSubscriber(typeof(EntityPersistentSubject), action);
        }

        public static TResult OnCreate<TEntity, TResult>(bool notification, IEntity entity, Func<TResult> func)
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

        public static async Task<TResult> OnCreateAsync<TEntity, TResult>(bool notification, IEntity entity, Func<Task<TResult>> func)
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

        public static TResult OnUpdate<TEntity, TResult>(bool notification, IEntity entity, Func<TResult> func)
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

        public static async Task<TResult> OnUpdateAsync<TEntity, TResult>(bool notification, IEntity entity, Func<Task<TResult>> func)
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

        public static TResult OnRemove<TEntity, TResult>(bool notification, IEntity entity, Func<TResult> func)
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

        public static async Task<TResult> OnRemoveAsync<TEntity, TResult>(bool notification, IEntity entity, Func<Task<TResult>> func)
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

        public static TResult OnBatch<TEntity, TResult>(bool notification, IEnumerable<IEntity> entities, EntityPersistentOperater operater, Func<TResult> func)
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

        public static async Task<TResult> OnBatchAsync<TEntity, TResult>(bool notification, IEnumerable<IEntity> entities, EntityPersistentOperater operater, Func<Task<TResult>> func)
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

        public static bool Publish<TEntity>(IEnumerable<IEntity> entities, EntityPersistentOperater operType, EntityPersistentEventType eventType)
        {
            var _event = new EntitiesArgs(entities, operType, eventType);
            var subject = new EntityPersistentSubject(typeof(TEntity), eventType, _event);

            SynchronizedSubscribeManager.Instance.Publish(subject);

            return !_event.Cancel;
        }

        public static bool Publish<TEntity>(EntityPersistentEventType eventType)
        {
            var _event = new EntityEventTypeArgs(eventType);
            var subject = new EntityPersistentSubject(typeof(TEntity), eventType, _event);

            SynchronizedSubscribeManager.Instance.Publish(subject);

            return !_event.Cancel;
        }

        public static bool Publish(IEntity entity, EntityPersistentEventType eventType)
        {
            var _event = new EntityEventArgs(entity, eventType);
            var subject = new EntityPersistentSubject(entity.EntityType, eventType, _event);

            SynchronizedSubscribeManager.Instance.Publish(subject);

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
