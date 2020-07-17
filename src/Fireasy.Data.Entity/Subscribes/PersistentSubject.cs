// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Data.Entity.Subscribes
{
    /// <summary>
    /// 实体持久化的消息主题。
    /// </summary>
    public sealed class PersistentSubject
    {
        /// <summary>
        /// 初始化 <see cref="PersistentSubject"/> 类的新实例。
        /// </summary>
        /// <param name="entityType">实体类型。</param>
        /// <param name="eventType">持久化事件类型。</param>
        /// <param name="argument"></param>
        public PersistentSubject(Type entityType, PersistentEventType eventType, object argument)
        {
            EntityType = entityType;
            EventType = eventType;
            Argument = argument;
        }

        /// <summary>
        /// 获取实体类型。
        /// </summary>
        public Type EntityType { get; }

        /// <summary>
        /// 获取当前的事件类型。
        /// </summary>
        public PersistentEventType EventType { get; }

        /// <summary>
        /// 获取持久化事件参数。
        /// </summary>
        public object Argument { get; }
    }
}
