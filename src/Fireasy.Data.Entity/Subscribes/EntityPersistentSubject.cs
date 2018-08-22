// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Subscribes;
using System;

namespace Fireasy.Data.Entity.Subscribes
{
    /// <summary>
    /// 实体持久化的消息主题。
    /// </summary>
    public sealed class EntityPersistentSubject : ISubject
    {
        public EntityPersistentSubject(Type entityType, EntityPersistentEventType eventType)
        {
            EntityType = entityType;
            EventType = eventType;
        }

        /// <summary>
        /// 获取实体类型。
        /// </summary>
        public Type EntityType { get; private set; }

        /// <summary>
        /// 获取当前的事件类型。
        /// </summary>
        public EntityPersistentEventType EventType { get; private set; }

        internal object Argument { get; set; }
    }
}
