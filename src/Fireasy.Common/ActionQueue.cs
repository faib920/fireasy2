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
        private static Timer timer = new Timer(ProcessQueue, null, TimeSpan.FromMilliseconds(0), TimeSpan.FromSeconds(10));
        private static bool isProcessing = false;
        private static int tryTime = 0;

        /// <summary>
        /// 用于处理异常的委托。
        /// </summary>
        public static Action<Exception> ExceptionHandler { get; set; }

        /// <summary>
        /// 将一个委托添加到队列中。
        /// </summary>
        /// <param name="action">要执行的委托。</param>
        /// <param name="tryTimes">当生产异常时，可重试的次数。</param>
        /// <returns>执行的标识。</returns>
        public static string Push(Action action, int tryTimes = 0)
        {
            Guard.ArgumentNull(action, nameof(action));

            var entry = new ActionEntry(action, tryTimes);
            queue.Enqueue(entry);
            return entry.Id;
        }

        /// <summary>
        /// 设置后台线程执行的间隔时间。默认为 10 秒。
        /// </summary>
        /// <param name="time">后台线程执行的间隔时间。</param>
        public static void SetPeriod(TimeSpan time)
        {
            timer.Change(TimeSpan.FromSeconds(1), time);
        }

        /// <summary>
        /// 设置发生异常后允许重试的次数。默认为 0。
        /// </summary>
        /// <param name="times">重试次数。</param>
        public static void SetTryTimes(int times)
        {
            tryTime = times;
        }

        /// <summary>
        /// 处理队列内的委托。
        /// </summary>
        /// <param name="state"></param>
        private static void ProcessQueue(object state)
        {
            if (isProcessing)
            {
                return;
            }

            ActionEntry entry;

            isProcessing = true;
            while (queue.TryDequeue(out entry))
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
                        ExceptionHandler?.Invoke(exp);
                    }
                }
            }

            isProcessing = false;
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