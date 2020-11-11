// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD
using Fireasy.Common.Options;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.QuartzNet
{
    public class QuartzScheduleOptions : IConfiguredOptions
    {
        private readonly List<QuartzJobOptions> _jobs = new List<QuartzJobOptions>();

        /// <summary>
        /// 获取执行的任务列表。
        /// </summary>
        public ReadOnlyCollection<QuartzJobOptions> Jobs
        {
            get
            {
                return new ReadOnlyCollection<QuartzJobOptions>(_jobs);
            }
        }

        /// <summary>
        /// 获取或设置配置中的实例名称。
        /// </summary>
        public string ConfigName { get; set; }

        bool IConfiguredOptions.IsConfigured { get; set; }

        /// <summary>
        /// 添加一个执行任务。
        /// </summary>
        /// <param name="setupAction"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public QuartzScheduleOptions Add(Action<QuartzJobOptions> setupAction, Action<IServiceProvider> action)
        {
            var options = new QuartzJobOptions(action);
            setupAction?.Invoke(options);
            _jobs.Add(options);
            return this;
        }

        /// <summary>
        /// 添加一个异步执行任务。
        /// </summary>
        /// <param name="setupAction"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public QuartzScheduleOptions AddAsync(Action<QuartzJobOptions> setupAction, Func<IServiceProvider, CancellationToken, Task> action)
        {
            var options = new QuartzJobOptions(action);
            setupAction?.Invoke(options);
            _jobs.Add(options);
            return this;
        }

        /// <summary>
        /// 定义执行任务。
        /// </summary>
        public sealed class QuartzJobOptions : IQuartzScheduleOptions
        {
            internal QuartzJobOptions(Action<IServiceProvider> executor)
            {
                Executor = executor;
            }

            internal QuartzJobOptions(Func<IServiceProvider, CancellationToken, Task> executor)
            {
                AsyncExecutor = executor;
            }

            /// <summary>
            /// 获取或设置 Cron 表达式。
            /// </summary>
            public string CronExpression { get; set; }

            /// <summary>
            /// 获取或设置开始时间。
            /// </summary>
            public DateTime? StartTime { get; set; }

            /// <summary>
            /// 获取或设置终止时间。
            /// </summary>
            public DateTime? EndTime { get; set; }

            /// <summary>
            /// 获取执行的委托。
            /// </summary>
            internal Action<IServiceProvider> Executor { get; }

            /// <summary>
            /// 获取异步执行的委托。
            /// </summary>
            internal Func<IServiceProvider, CancellationToken, Task> AsyncExecutor { get; }
        }
    }
}
#endif
