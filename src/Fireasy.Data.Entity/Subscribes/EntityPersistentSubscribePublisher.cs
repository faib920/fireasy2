
using Fireasy.Common;
using Fireasy.Common.Subscribe;
using System;
using System.Collections.Generic;

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

        public static void OnBeforeBatch(IEnumerable<IEntity> entities, EntityPersistentEventType eventType)
        {
            Guard.ArgumentNull(entities, "entity");
            RaiseEvent(new object[] { entities, eventType }, EntityPersistentEventType.BeforeBatch);
        }

        public static void OnAfterBatch(IEnumerable<IEntity> entities, EntityPersistentEventType eventType)
        {
            Guard.ArgumentNull(entities, "entity");
            RaiseEvent(new object[] { entities, eventType }, EntityPersistentEventType.AfterBatch);
        }

        public static T OnBatch<T>(IEnumerable<IEntity> entities, EntityPersistentOperater operater, Func<T> func)
        {
            var ret = default(T);
            RaiseEvent(new object[] { entities, operater }, EntityPersistentEventType.BeforeBatch);
            ret = func();
            RaiseEvent(new object[] { entities, operater }, EntityPersistentEventType.AfterBatch);
            return ret;
        }

        public static void RaiseEvent(object[] arguments, EntityPersistentEventType eventType)
        {
            SubscribeManager.Publish<EntityPersistentSubject>(arguments, eventType);
        }

        public static void RaiseEvent(IEntity entity, EntityPersistentEventType eventType)
        {
            SubscribeManager.Publish<EntityPersistentSubject>(entity, eventType);
        }
    }
}
