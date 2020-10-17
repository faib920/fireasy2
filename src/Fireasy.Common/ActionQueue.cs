﻿// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Fireasy.Common
{
    /// <summary>
    /// 一个提供委托执行的队列。
    /// </summary>
    public static class ActionQueue
    {
        private static readonly ConcurrentQueue<ActionEntry> _queue = new ConcurrentQueue<ActionEntry>();
        private static readonly Thread _thread = new Thread(new ThreadStart(ProcessQueue)) { IsBackground = true };

        /// <summary>
        /// 获取或设置用于处理异常的委托。
        /// </summary>
        public static Action<Action, Exception> ExceptionHandler { get; set; }

        /// <summary>
        /// 将一个委托添加到队列中。
        /// </summary>
        /// <param name="action">要执行的委托。</param>
        /// <param name="tryTimes">重试次数。</param>
        /// <returns>执行的标识。</returns>
        public static string Push(Action action, int tryTimes = 0)
        {
            Guard.ArgumentNull(action, nameof(action));

            var entry = new ActionEntry(action, tryTimes);
            _queue.Enqueue(entry);

            if (_thread.ThreadState != ThreadState.Background)
            {
                _thread.Start();
            }

            return entry.Id;
        }

        /// <summary>
        /// 处理队列内的委托。
        /// </summary>
        private static void ProcessQueue()
        {
            while (true)
            {
                if (_queue.Count == 0)
                {
                    Thread.Sleep(1000);
                }

                if (_queue.TryDequeue(out ActionEntry entry) && entry != null && entry.Action != null)
                {
                    try
                    {
                        entry.Action.Invoke();
                    }
                    catch (Exception exp)
                    {
                        if (entry.CanTry())
                        {
                            _queue.Enqueue(entry);
                        }
                        else if (ExceptionHandler != null)
                        {
                            ExceptionHandler.Invoke(entry.Action, exp);
                        }
                    }
                }
            }
        }

        private class ActionEntry
        {
            private int _time = 0;
            private readonly int _tryTimes = 0;

            public ActionEntry(Action action, int tryTimes)
            {
                Id = Guid.NewGuid().ToString();
                _tryTimes = tryTimes;
                Action = action;
            }

            /// <summary>
            /// 获取或设置委托的标识。
            /// </summary>
            public string Id { get; private set; }

            /// <summary>
            /// 获取或设置要执行的委托。
            /// </summary>
            public Action Action { get; private set; }

            /// <summary>
            /// 判断是否可以重试。
            /// </summary>
            /// <returns></returns>
            public bool CanTry()
            {
                return _time++ < _tryTimes;
            }
        }
    }
}