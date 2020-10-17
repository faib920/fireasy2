// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace Fireasy.QuartzNet
{
    public class QuartzScheduleOptions
    {
        private readonly List<TaskDefinition> tasks = new List<TaskDefinition>();

        /// <summary>
        /// 获取执行的任务列表。
        /// </summary>
        public ReadOnlyCollection<TaskDefinition> Tasks
        {
            get
            {
                return new ReadOnlyCollection<TaskDefinition>(tasks);
            }
        }

        /// <summary>
        /// 添加一个执行任务。
        /// </summary>
        /// <param name="delay">延迟时间。</param>
        /// <param name="period">执行触发间隔时间。</param>
        /// <param name="action">执行的方法。</param>
        /// <returns></returns>
        public QuartzScheduleOptions Add(TimeSpan delay, TimeSpan period, Action<IServiceProvider, CancellationToken> action)
        {
            var task = new TaskDefinition { Delay = delay, Period = period, Executor = action };
            tasks.Add(task);
            return this;
        }

        /// <summary>
        /// 定义执行任务。
        /// </summary>
        public class TaskDefinition
        {
            /// <summary>
            /// 获取或设置延迟时间。
            /// </summary>
            public TimeSpan Delay { get; set; }

            /// <summary>
            /// 获取或设置执行触发间隔时间。
            /// </summary>
            public TimeSpan Period { get; set; }

            /// <summary>
            /// 获取或设置执行的委托。
            /// </summary>
            public Action<IServiceProvider, CancellationToken> Executor { get; set; }
        }
    }
}
#endif
