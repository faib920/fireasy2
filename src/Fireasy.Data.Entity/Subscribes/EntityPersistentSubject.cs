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
    public sealed class EntityPersistentSubject
    {
        /// <summary>
        /// 初始化 <see cref="EntityPersistentSubject"/> 类的新实例。
        /// </summary>
        /// <param name="entityType">实体类型。</param>
        /// <param name="eventType">持久化事件类型。</param>
        /// <param name="argument"></param>
        public EntityPersistentSubject(Type entityType, EntityPersistentEventType eventType, object argument)
        {
            EntityType = entityType;
            EventType = eventType;
            Argument = argument;
        }

        /// <summary>
        /// 获取实体类型。
        /// </summary>
        public Type EntityType { get; private set; }

        /// <summary>
        /// 获取当前的事件类型。
        /// </summary>
        public EntityPersistentEventType EventType { get; private set; }

        /// <summary>
        /// 获取持久化事件参数。
        /// </summary>
        public object Argument { get; private set; }
    }
}
