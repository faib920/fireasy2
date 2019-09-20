// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace Fireasy.Data.Entity.Subscribes
{
    /// <summary>
    /// 实体持久化的事件订阅器抽象类。
    /// </summary>
    public abstract class EntityPersistentSubscriber
    {
        /// <summary>
        /// 接收主题信息并进行处理。
        /// </summary>
        /// <param name="subject"></param>
        public void Accept(EntityPersistentSubject subject)
        {
            switch (subject.EventType)
            {
                case EntityPersistentEventType.BeforeCreate:
                    {
                        if (subject.Argument is EntityEventArgs arg)
                        {
                            arg.Cancel = !OnBeforeCreate(arg.Entity);
                        }
                    }
                    break;
                case EntityPersistentEventType.AfterCreate:
                    {
                        if (subject.Argument is EntityEventArgs arg)
                        {
                            OnAfterCreate(arg.Entity);
                        }
                    }

                    OnCreate(subject.EntityType);
                    break;
                case EntityPersistentEventType.BeforeUpdate:
                    {
                        if (subject.Argument is EntityEventArgs arg)
                        {
                            arg.Cancel = !OnBeforeUpdate(arg.Entity);
                        }
                    }
                    break;
                case EntityPersistentEventType.AfterUpdate:
                    {
                        if (subject.Argument is EntityEventArgs arg)
                        {
                            OnAfterUpdate(arg.Entity);
                        }
                    }

                    OnUpdate(subject.EntityType);
                    break;
                case EntityPersistentEventType.BeforeRemove:
                    {
                        if (subject.Argument is EntityEventArgs arg)
                        {
                            arg.Cancel = !OnBeforeRemove(arg.Entity);
                        }
                    }
                    break;
                case EntityPersistentEventType.AfterRemove:
                    {
                        if (subject.Argument is EntityEventArgs arg)
                        {
                            OnAfterRemove(arg.Entity);
                        }
                    }

                    OnRemove(subject.EntityType);
                    break;
                case EntityPersistentEventType.BeforeBatch:
                    {
                        var arg = subject.Argument as EntitiesArgs;
                        arg.Cancel = !OnBeforeBatch(arg.Entities, arg.OperType);
                    }
                    break;
                case EntityPersistentEventType.AfterBatch:
                    {
                        var arg = subject.Argument as EntitiesArgs;
                        OnAfterBatch(arg.Entities, arg.OperType);

                        switch (arg.OperType)
                        {
                            case EntityPersistentOperater.Create:
                                OnCreate(subject.EntityType);
                                break;
                            case EntityPersistentOperater.Update:
                                OnUpdate(subject.EntityType);
                                break;
                            case EntityPersistentOperater.Remove:
                                OnRemove(subject.EntityType);
                                break;
                        }
                    }

                    break;
            }
        }

        /// <summary>
        /// 用于实体创建之前的通知。
        /// </summary>
        /// <param name="entity">创建的实体对象。</param>
        /// <returns>取消则返回 false。</returns>
        protected virtual bool OnBeforeCreate(IEntity entity)
        {
            return true;
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
        /// <returns>取消则返回 false。</returns>
        protected virtual bool OnBeforeUpdate(IEntity entity)
        {
            return true;
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
        /// <returns>取消则返回 false。</returns>
        protected virtual bool OnBeforeRemove(IEntity entity)
        {
            return true;
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
        /// <returns>取消则返回 false。</returns>
        protected virtual bool OnBeforeBatch(IEnumerable<IEntity> entities, EntityPersistentOperater operater)
        {
            return true;
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
