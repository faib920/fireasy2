// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;
using System.Collections.Generic;

namespace Fireasy.Data.Entity.Subscribes
{
    /// <summary>
    /// 声明一个范围，在此范围内禁用事件订阅。
    /// </summary>
    public sealed class DisableSubscribeScope : Scope<DisableSubscribeScope>
    {
        /// <summary>
        /// 初始化 <see cref="DisableSubscribeScope"/> 类的新实例。
        /// </summary>
        public DisableSubscribeScope()
        {
        }

        /// <summary>
        /// 初始化 <see cref="DisableSubscribeScope"/> 类的新实例。
        /// </summary>
        /// <param name="disabledEventTypes">指定禁用的事件类型。</param>
        public DisableSubscribeScope(params PersistentEventType[] disabledEventTypes)
        {
            if (disabledEventTypes != null)
            {
                DisabledEventTypes = new List<PersistentEventType>(disabledEventTypes);
            }
        }

        /// <summary>
        /// 获取禁用的事件类型。如果没有指定，默认禁用所有事件。
        /// </summary>
        public List<PersistentEventType> DisabledEventTypes { get; private set; }
    }
}
