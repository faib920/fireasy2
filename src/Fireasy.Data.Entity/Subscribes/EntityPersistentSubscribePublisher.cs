
using Fireasy.Common;
using Fireasy.Common.Subscribe;
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

        public static void RaiseEvent(IEntity entity, EntityPersistentEventType eventType)
        {
            SubscribeManager.Publish<EntityPersistentSubject>(entity, eventType);
        }
    }
}
