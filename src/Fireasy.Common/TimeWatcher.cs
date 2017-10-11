// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Fireasy.Common
{
    /// <summary>
    /// 对方法执行的时间监视。
    /// </summary>
    public static class TimeWatcher
    {
        /// <summary>
        /// 监视方法执行所耗用的时间。
        /// </summary>
        /// <param name="method">要执行的方法。</param>
        /// <returns>所耗用的时间。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="method"/> 参数为 null。</exception>
        public static TimeSpan Watch(Action method)
        {
            Guard.ArgumentNull(method, nameof(method));

            var watch = new Stopwatch();
            watch.Start();
            method();
            watch.Stop();

            return watch.Elapsed;
        }

        /// <summary>
        /// 监视一组方法分别执行所耗用的时间。
        /// </summary>
        /// <param name="methods">要执行的一组方法。</param>
        /// <returns>每一个方法执行所耗用的时间。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="methods"/> 参数为 null。</exception>
        public static IEnumerable<TimeSpan> WatchApart(params Action[] methods)
        {
            Guard.ArgumentNull(methods, nameof(methods));

            var watch = new Stopwatch();
            watch.Start();
            foreach (var action in methods)
            {
#if NET35
                watch.Reset();
                watch.Start();
#else
                watch.Restart();
#endif
                action();
                watch.Stop();
                yield return watch.Elapsed;
            }
        }

        /// <summary>
        /// 监视一组方法执行的时刻。
        /// </summary>
        /// <param name="methods">要执行的方法。</param>
        /// <returns>每一个方法执行的时刻。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="methods"/> 参数为 null。</exception>
        public static IEnumerable<TimeSpan> WatchAround(params Action[] methods)
        {
            Guard.ArgumentNull(methods, nameof(methods));

            var watch = new Stopwatch();
            watch.Start();
            foreach (var action in methods)
            {
                action();
                yield return watch.Elapsed;
            }

            watch.Stop();
        }
    }
}
