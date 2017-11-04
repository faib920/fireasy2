using Fireasy.Common.Subscribe;
using System.Collections.Generic;

namespace Fireasy.Data.Entity.Subscribes
{
    /// <summary>
    /// 实体持久化的事件订阅器抽象类。
    /// </summary>
    public abstract class EntityPersistentSubscriber : ISubscriber
    {
        /// <summary>
        /// 接收主题信息。
        /// </summary>
        /// <param name="subject"></param>
        void ISubscriber.Accept(ISubject subject)
        {
            var esub = subject as EntityPersistentSubject;
            if (esub == null)
            {
                return;
            }

            switch (esub.EventType)
            {
                case EntityPersistentEventType.BeforeCreate:
                    OnBeforeCreate(esub.Argument as IEntity);
                    break;
                case EntityPersistentEventType.AfterCreate:
                    OnAfterCreate(esub.Argument as IEntity);
                    break;
                case EntityPersistentEventType.BeforeUpdate:
                    OnBeforeUpdate(esub.Argument as IEntity);
                    break;
                case EntityPersistentEventType.AfterUpdate:
                    OnAfterUpdate(esub.Argument as IEntity);
                    break;
                case EntityPersistentEventType.BeforeRemove:
                    OnBeforeRemove(esub.Argument as IEntity);
                    break;
                case EntityPersistentEventType.AfterRemove:
                    OnAfterRemove(esub.Argument as IEntity);
                    break;
                case EntityPersistentEventType.BeforeBatch:
                    var objs1 = (object[])esub.Argument;
                    OnBeforeBatch(objs1[0] as IEnumerable<IEntity>, (EntityPersistentOperater)objs1[1]);
                    break;
                case EntityPersistentEventType.AfterBatch:
                    var objs2 = (object[])esub.Argument;
                    OnAfterBatch(objs2[0] as IEnumerable<IEntity>, (EntityPersistentOperater)objs2[1]);
                    break;
            }
        }

        /// <summary>
        /// 用于实体创建之前的通知。
        /// </summary>
        /// <param name="entity">创建的实体对象。</param>
        protected virtual void OnBeforeCreate(IEntity entity)
        {
        }

        /// <summary>
        /// 用于实体创建之后的通知。
        /// </summary>
        /// <param name="entity">创建的实体对象。</param>
        protected virtual void OnAfterCreate(IEntity entity)
        {
        }

        /// <summary>
        /// 用于实体更新之前的通知。
        /// </summary>
        /// <param name="entity">更新的实体对象。</param>
        protected virtual void OnBeforeUpdate(IEntity entity)
        {
        }

        /// <summary>
        /// 用于实体更新之后的通知。
        /// </summary>
        /// <param name="entity">更新的实体对象。</param>
        protected virtual void OnAfterUpdate(IEntity entity)
        {
        }

        /// <summary>
        /// 用于实体移除之前的通知。
        /// </summary>
        /// <param name="entity">移除的实体对象。</param>
        protected virtual void OnBeforeRemove(IEntity entity)
        {
        }

        /// <summary>
        /// 用于实体移除之后的通知。
        /// </summary>
        /// <param name="entity">移除的实体对象。</param>
        protected virtual void OnAfterRemove(IEntity entity)
        {
        }

        /// <summary>
        /// 用于实体批量处理之前的通知。
        /// </summary>
        /// <param name="entities">批量处理的实体对象。</param>
        /// <param name="operater"></param>
        protected virtual void OnBeforeBatch(IEnumerable<IEntity> entities, EntityPersistentOperater operater)
        {
        }

        /// <summary>
        /// 用于实体批量处理之后的通知。
        /// </summary>
        /// <param name="entities">批量处理的实体对象。</param>
        /// <param name="operater"></param>
        protected virtual void OnAfterBatch(IEnumerable<IEntity> entities, EntityPersistentOperater operater)
        {
        }
    }
}
