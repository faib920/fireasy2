// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if !NET40

using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Common.Extensions
{
    /// <summary>
    /// Task 相关的扩展。
    /// </summary>
    public static class TaskExtension
    {
        private static readonly TaskFactory _taskFactory = new TaskFactory(CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);

        /// <summary>
        /// 将此任务作为同步调用。
        /// </summary>
        /// <param name="task"></param>
        public static void AsSync(this Task task)
        {
            Guard.ArgumentNull(task, nameof(task));

            task.GetAwaiter().GetResult();
        }

        /// <summary>
        /// 将此任务作为同步调用并返回值。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <returns></returns>
        public static T AsSync<T>(this Task<T> task)
        {
            Guard.ArgumentNull(task, nameof(task));

            return task.GetAwaiter().GetResult();
        }

        /// <summary>
        /// 运行同步方法。
        /// </summary>
        /// <param name="func">异步回调委托。</param>
        public static void RunSync(Func<Task> func)
        {
            var cultureUi = CultureInfo.CurrentUICulture;
            var culture = CultureInfo.CurrentCulture;

            _taskFactory.StartNew(delegate
                {
                    CultureInfo.DefaultThreadCurrentCulture = culture;
                    CultureInfo.DefaultThreadCurrentUICulture = cultureUi;
                    return func();
                }).Unwrap().AsSync();
        }

        /// <summary>
        /// 运行同步方法。
        /// </summary>
        /// <typeparam name="TResult">返回值类型。</typeparam>
        /// <param name="func">异步回调委托。</param>
        /// <returns>返回值</returns>
        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            var cultureUi = CultureInfo.CurrentUICulture;
            var culture = CultureInfo.CurrentCulture;

            return _taskFactory.StartNew(delegate
                {
                    CultureInfo.DefaultThreadCurrentCulture = culture;
                    CultureInfo.DefaultThreadCurrentUICulture = cultureUi;
                    return func();
                }).Unwrap().AsSync();
        }

        /// <summary>
        /// 如果 <paramref name="func"/> 返回的是一个 <see cref="Task"/>，则等待其结束。
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="func"></param>
        /// <param name="arg1"></param>
        /// <returns></returns>
        public static T2 Await<T1, T2>(this Func<T1, T2> func, T1 arg1)
        {
            Guard.ArgumentNull(func, nameof(func));

            var ret = func(arg1);
            if (ret is Task task)
            {
                task.AsSync();
            }

            return ret;
        }

        /// <summary>
        /// 如果 <paramref name="func"/> 返回的是一个 <see cref="Task"/>，则等待其结束。
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="func"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <returns></returns>
        public static T3 Await<T1, T2, T3>(this Func<T1, T2, T3> func, T1 arg1, T2 arg2)
        {
            Guard.ArgumentNull(func, nameof(func));

            var ret = func(arg1, arg2);
            if (ret is Task task)
            {
                task.AsSync();
            }

            return ret;
        }

        /// <summary>
        /// 如果 <paramref name="func"/> 返回的是一个 <see cref="Task"/>，则等待其结束。
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <param name="func"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <returns></returns>
        public static T4 Await<T1, T2, T3, T4>(this Func<T1, T2, T3, T4> func, T1 arg1, T2 arg2, T3 arg3)
        {
            Guard.ArgumentNull(func, nameof(func));

            var ret = func(arg1, arg2, arg3);
            if (ret is Task task)
            {
                task.AsSync();
            }

            return ret;
        }

        /// <summary>
        /// 如果 <paramref name="func"/> 返回的是一个 <see cref="Task"/>，则等待其结束。
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <param name="func"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <param name="arg4"></param>
        /// <returns></returns>
        public static T5 Await<T1, T2, T3, T4, T5>(this Func<T1, T2, T3, T4, T5> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            Guard.ArgumentNull(func, nameof(func));

            var ret = func(arg1, arg2, arg3, arg4);
            if (ret is Task task)
            {
                task.AsSync();
            }

            return ret;
        }

        /// <summary>
        /// 如果 <paramref name="func"/> 返回的是一个 <see cref="Task"/>，则等待其结束。
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <param name="func"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <param name="arg4"></param>
        /// <param name="arg5"></param>
        /// <returns></returns>
        public static T6 Await<T1, T2, T3, T4, T5, T6>(this Func<T1, T2, T3, T4, T5, T6> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            Guard.ArgumentNull(func, nameof(func));

            var ret = func(arg1, arg2, arg3, arg4, arg5);
            if (ret is Task task)
            {
                task.AsSync();
            }

            return ret;
        }

        /// <summary>
        /// 如果 <paramref name="func"/> 返回的是一个 <see cref="Task"/>，则等待其结束。
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <param name="func"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <param name="arg4"></param>
        /// <param name="arg5"></param>
        /// <param name="arg6"></param>
        /// <returns></returns>
        public static T7 Await<T1, T2, T3, T4, T5, T6, T7>(this Func<T1, T2, T3, T4, T5, T6, T7> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            Guard.ArgumentNull(func, nameof(func));

            var ret = func(arg1, arg2, arg3, arg4, arg5, arg6);
            if (ret is Task task)
            {
                task.AsSync();
            }

            return ret;
        }
    }
}
#endif