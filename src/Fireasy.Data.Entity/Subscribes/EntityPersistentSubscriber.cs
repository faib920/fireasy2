using Fireasy.Common.Subscribe;
using System;
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
                    if (esub.Argument != null)
                    {
                        OnBeforeCreate(esub.Argument as IEntity);
                    }
                    break;
                case EntityPersistentEventType.AfterCreate:
                    if (esub.Argument != null)
                    {
                        OnAfterCreate(esub.Argument as IEntity);
                    }

                    OnCreate(esub.EntityType);
                    break;
                case EntityPersistentEventType.BeforeUpdate:
                    if (esub.Argument != null)
                    {
                        OnBeforeUpdate(esub.Argument as IEntity);
                    }
                    break;
                case EntityPersistentEventType.AfterUpdate:
                    if (esub.Argument != null)
                    {
                        OnAfterUpdate(esub.Argument as IEntity);
                    }

                    OnUpdate(esub.EntityType);
                    break;
                case EntityPersistentEventType.BeforeRemove:
                    if (esub.Argument != null)
                    {
                        OnBeforeRemove(esub.Argument as IEntity);
                    }
                    break;
                case EntityPersistentEventType.AfterRemove:
                    if (esub.Argument != null)
                    {
                        OnAfterRemove(esub.Argument as IEntity);
                    }

                    OnRemove(esub.EntityType);
                    break;
                case EntityPersistentEventType.BeforeBatch:
                    var objs1 = (object[])esub.Argument;
                    OnBeforeBatch(objs1[0] as IEnumerable<IEntity>, (EntityPersistentOperater)objs1[1]);
                    break;
                case EntityPersistentEventType.AfterBatch:
                    var objs2 = (object[])esub.Argument;
                    var operater = (EntityPersistentOperater)objs2[1];
                    OnAfterBatch(objs2[0] as IEnumerable<IEntity>, operater);

                    switch (operater)
                    {
                        case EntityPersistentOperater.Create:
                            OnCreate(esub.EntityType);
                            break;
                        case EntityPersistentOperater.Update:
                            OnCreate(esub.EntityType);
                            break;
                        case EntityPersistentOperater.Remove:
                            OnCreate(esub.EntityType);
                            break;
                    }

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
        /// 用于实体创建之前的通知。
        /// </summary>
        /// <param name="entityType"></param>
        protected virtual void OnCreate(Type entityType)
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
        /// 用于实体更新之后的通知。
        /// </summary>
        /// <param name="entityType"></param>
        protected virtual void OnUpdate(Type entityType)
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
        /// 用于实体移除之后的通知。
        /// </summary>
        /// <param name="entityType"></param>
        protected virtual void OnRemove(Type entityType)
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
