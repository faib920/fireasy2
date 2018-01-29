
using Fireasy.Common;
using Fireasy.Common.Subscribe;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fireasy.Data.Entity.Subscribes
{
    public class EntityPersistentSubscribePublisher
    {
        public static void OnBeforeCreate(IEntity entity)
        {
            Guard.ArgumentNull(entity, "entity");
            RaiseEvent(entity, EntityPersistentEventType.BeforeCreate);
        }

        public static void OnAfterCreate(IEntity entity)
        {
            Guard.ArgumentNull(entity, "entity");
            RaiseEvent(entity, EntityPersistentEventType.AfterCreate);
        }

        public static T OnCreate<T>(IEntity entity, Func<T> func)
        {
            var ret = default(T);
            RaiseEvent(entity, EntityPersistentEventType.BeforeCreate);
            ret = func();
            RaiseEvent(entity, EntityPersistentEventType.AfterCreate);
            return ret;
        }

        public static void OnBeforeUpdate(IEntity entity)
        {
            Guard.ArgumentNull(entity, "entity");
            RaiseEvent(entity, EntityPersistentEventType.BeforeUpdate);
        }

        public static void OnAfterUpdate(IEntity entity)
        {
            Guard.ArgumentNull(entity, "entity");
            RaiseEvent(entity, EntityPersistentEventType.AfterUpdate);
        }

        public static T OnUpdate<T>(IEntity entity, Func<T> func)
        {
            var ret = default(T);
            RaiseEvent(entity, EntityPersistentEventType.BeforeUpdate);
            ret = func();
            RaiseEvent(entity, EntityPersistentEventType.AfterUpdate);
            return ret;
        }

        public static void OnBeforeRemove(IEntity entity)
        {
            Guard.ArgumentNull(entity, "entity");
            RaiseEvent(entity, EntityPersistentEventType.BeforeRemove);
        }

        public static void OnAfterRemove(IEntity entity)
        {
            Guard.ArgumentNull(entity, "entity");
            RaiseEvent(entity, EntityPersistentEventType.AfterRemove);
        }

        public static T OnRemove<T>(IEntity entity, Func<T> func)
        {
            var ret = default(T);
            RaiseEvent(entity, EntityPersistentEventType.BeforeRemove);
            ret = func();
            RaiseEvent(entity, EntityPersistentEventType.AfterRemove);
            return ret;
        }

        public static T OnBatch<T>(IEnumerable<IEntity> entities, EntityPersistentOperater operater, Func<T> func)
        {
            var ret = default(T);
            RaiseEvent(entities, operater, EntityPersistentEventType.BeforeBatch);
            ret = func();
            RaiseEvent(entities, operater, EntityPersistentEventType.AfterBatch);
            return ret;
        }

        public static void RaiseEvent(IEnumerable<IEntity> entities, EntityPersistentOperater operType, EntityPersistentEventType eventType)
        {
            SubscribeManager.Publish<EntityPersistentSubject>(new EntitiesArgs(entities, operType, eventType));
        }

        public static void RaiseEvent<TEntity>(EntityPersistentEventType eventType)
        {
            SubscribeManager.Publish<EntityPersistentSubject>(new EntityEventTypeArgs(typeof(TEntity), eventType));
        }

        public static void RaiseEvent(IEntity entity, EntityPersistentEventType eventType)
        {
            SubscribeManager.Publish<EntityPersistentSubject>(new EntityEventArgs(entity, eventType));
        }
    }

    internal class EntityEventTypeArgs
    {
        public EntityEventTypeArgs(Type entityType, EntityPersistentEventType eventType)
        {
            EntityType = entityType;
            EventType = eventType;
        }

        public Type EntityType { get; set; }

        public EntityPersistentEventType EventType { get; set; }
    }

    internal class EntityEventArgs : EntityEventTypeArgs
    {
        public EntityEventArgs(IEntity entity, EntityPersistentEventType eventType)
            : base(entity.EntityType, eventType)
        {
            Entity = entity;
        }

        public IEntity Entity { get; set; }
    }

    internal class EntitiesArgs : EntityEventTypeArgs
    {
        public EntitiesArgs(IEnumerable<IEntity> entities, EntityPersistentOperater operType, EntityPersistentEventType eventType)
            : base(entities.FirstOrDefault().EntityType, eventType)
        {
            Entities = entities;
            OperType = operType;
        }

        public IEnumerable<IEntity> Entities { get; set; }

        public EntityPersistentOperater OperType { get; set; }
    }
}
