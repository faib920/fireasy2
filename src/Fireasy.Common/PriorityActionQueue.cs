// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using System;
using System.Collections.Generic;

namespace Fireasy.Common
{
    /// <summary>
    /// 一个提供按优先级执行的委托队列。
    /// </summary>
    public sealed class PriorityActionQueue
    {
        private SortedDictionary<int, List<Action>> queue = new SortedDictionary<int, List<Action>>();

        /// <summary>
        /// 向队列里添加要执行的方法。
        /// </summary>
        /// <param name="priority">优先级，值越小优先级越高。</param>
        /// <param name="action">要执行的方法。</param>
        public void Add(int priority, Action action)
        {
            Guard.ArgumentNull(action, nameof(action));

            var list = queue.TryGetValue(priority, () => new List<Action>());
            list.Add(action);
        }

        /// <summary>
        /// 执行队列里面的所有方法，然后清空队列。
        /// </summary>
        public void Invoke()
        {
            foreach (var item in queue)
            {
                item.Value.ForEach(s => s.Invoke());
            }

            queue.Clear();
        }

        /// <summary>
        /// 枚举队列里的所有方法。
        /// </summary>
        /// <param name="action"></param>
        public void ForEach(Action<Action> action)
        {
            foreach (var item in queue)
            {
                item.Value.ForEach(s => action(s));
            }
        }
    }
}
