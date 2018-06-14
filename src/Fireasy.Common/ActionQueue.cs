// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if !NET35
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
        private static ConcurrentQueue<ActionEntry> queue = new ConcurrentQueue<ActionEntry>();
        private readonly static Thread thread;
        private static TimeSpan time = TimeSpan.FromSeconds(1);

        /// <summary>
        /// 用于处理异常的委托。
        /// </summary>
        public static Action<Action, Exception> ExceptionHandler { get; set; }

        static ActionQueue()
        {
            thread = new Thread(new ThreadStart(ProcessQueue));
            thread.IsBackground = true;
            thread.Priority = ThreadPriority.Highest;
            thread.Start();
        }

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
            queue.Enqueue(entry);
            return entry.Id;
        }

        /// <summary>
        /// 设置后台线程执行的间隔时间。默认为 1 秒。
        /// </summary>
        /// <param name="time">后台线程执行的间隔时间。</param>
        public static void SetPeriod(TimeSpan time)
        {
            ActionQueue.time = time;
        }

        /// <summary>
        /// 处理队列内的委托。
        /// </summary>
        private static void ProcessQueue()
        {
            while (true)
            {
                while (queue.TryDequeue(out ActionEntry entry))
                {
                    try
                    {
                        entry.Action();
                    }
                    catch (Exception exp)
                    {
                        if (entry.CanTry())
                        {
                            queue.Enqueue(entry);
                        }
                        else
                        {
                            ExceptionHandler?.Invoke(entry.Action, exp);
                        }
                    }
                }

                Thread.Sleep(time);
            }
        }

        private class ActionEntry
        {
            private int time = 0;
            private int tryTimes = 0;

            public ActionEntry(Action action, int tryTimes)
            {
                Id = Guid.NewGuid().ToString();
                this.tryTimes = tryTimes;
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
                return time++ < tryTimes;
            }
        }
    }
}
#endif