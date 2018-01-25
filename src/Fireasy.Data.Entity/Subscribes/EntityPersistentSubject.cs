using Fireasy.Common.Subscribe;
using System;

namespace Fireasy.Data.Entity.Subscribes
{
    /// <summary>
    /// 实体持久化的消息主题。
    /// </summary>
    public sealed class EntityPersistentSubject : ISubject
    {
        /// <summary>
        /// 获取实体类型。
        /// </summary>
        public Type EntityType { get; private set; }

        /// <summary>
        /// 获取当前的事件类型。
        /// </summary>
        public EntityPersistentEventType EventType { get; private set; }

        internal object Argument { get; set; }

        /// <summary>
        /// 获取或设置过滤器。
        /// </summary>
        public Func<ISubscriber, bool> Filter { get; set; }

        void ISubject.Initialize(params object[] arguments)
        {
            if (arguments[0] is Type)
            {
                EntityType = arguments[0] as Type;
            }
            else
            {
                Argument = arguments[0];
                var entity = arguments[0] as IEntity;
                if (entity != null)
                {
                    EntityType = entity.EntityType;
                }
            }

            EventType = (EntityPersistentEventType)arguments[1];
        }
    }
}
