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
            DefaultSubscribeManager.Instance.AddSubscriber(typeof(EntityPersistentSubject), subscriber);
        }

        public static T OnCreate<T>(IEntity entity, Func<T> func)
        {
            var ret = default(T);
            Publish(entity, EntityPersistentEventType.BeforeCreate);
            ret = func();
            Publish(entity, EntityPersistentEventType.AfterCreate);
            return ret;
        }

        public static T OnUpdate<T>(IEntity entity, Func<T> func)
        {
            var ret = default(T);
            Publish(entity, EntityPersistentEventType.BeforeUpdate);
            ret = func();
            Publish(entity, EntityPersistentEventType.AfterUpdate);
            return ret;
        }

        public static T OnRemove<T>(IEntity entity, Func<T> func)
        {
            var ret = default(T);
            Publish(entity, EntityPersistentEventType.BeforeRemove);
            ret = func();
            Publish(entity, EntityPersistentEventType.AfterRemove);
            return ret;
        }

        public static T OnBatch<T>(IEnumerable<IEntity> entities, EntityPersistentOperater operater, Func<T> func)
        {
            var ret = default(T);
            Publish<T>(entities, operater, EntityPersistentEventType.BeforeBatch);
            ret = func();
            Publish<T>(entities, operater, EntityPersistentEventType.AfterBatch);
            return ret;
        }

        public static void Publish<TEntity>(IEnumerable<IEntity> entities, EntityPersistentOperater operType, EntityPersistentEventType eventType)
        {
            var subject = new EntityPersistentSubject(typeof(TEntity), eventType, new EntitiesArgs(entities, operType, eventType));

            DefaultSubscribeManager.Instance.Publish(subject);
        }

        public static void Publish<TEntity>(EntityPersistentEventType eventType)
        {
            var subject = new EntityPersistentSubject(typeof(TEntity), eventType, new EntityEventTypeArgs(eventType));

            DefaultSubscribeManager.Instance.Publish(subject);
        }

        public static void Publish(IEntity entity, EntityPersistentEventType eventType)
        {
            var subject = new EntityPersistentSubject(entity.EntityType, eventType, new EntityEventArgs(entity, eventType));

            DefaultSubscribeManager.Instance.Publish(subject);
        }
    }

    internal class EntityEventTypeArgs
    {
        public EntityEventTypeArgs(EntityPersistentEventType eventType)
        {
            EventType = eventType;
        }

        public EntityPersistentEventType EventType { get; set; }
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
