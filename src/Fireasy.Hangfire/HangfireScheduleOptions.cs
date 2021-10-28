// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD
using Fireasy.Common.Options;
using Hangfire;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Hangfire
{
    public class HangfireScheduleOptions : IConfiguredOptions
    {
        private readonly List<HangfireJobOptions> _jobs = new List<HangfireJobOptions>();

        /// <summary>
        /// 获取执行的任务列表。
        /// </summary>
        public ReadOnlyCollection<HangfireJobOptions> Jobs
        {
            get
            {
                return new ReadOnlyCollection<HangfireJobOptions>(_jobs);
            }
        }

        /// <summary>
        /// 获取或设置配置中的实例名称。
        /// </summary>
        public string ConfigName { get; set; }

        bool IConfiguredOptions.IsConfigured { get; set; }

        /// <summary>
        /// 使用存储。
        /// </summary>
        /// <typeparam name="TStorage"></typeparam>
        /// <param name="storage"></param>
        /// <returns></returns>
        public HangfireScheduleOptions UseStorage<TStorage>(TStorage storage) where TStorage : JobStorage
        {
            JobStorage.Current = storage;
            return this;
        }

        /// <summary>
        /// 添加一个执行任务。
        /// </summary>
        /// <param name="setupAction"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public HangfireScheduleOptions Add(Action<HangfireJobOptions> setupAction, Action<IServiceProvider> action)
        {
            var options = new HangfireJobOptions(action);
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
        public HangfireScheduleOptions AddAsync(Action<HangfireJobOptions> setupAction, Func<IServiceProvider, CancellationToken, Task> action)
        {
            var options = new HangfireJobOptions(action);
            setupAction?.Invoke(options);
            _jobs.Add(options);
            return this;
        }

        /// <summary>
        /// 定义执行任务。
        /// </summary>
        public sealed class HangfireJobOptions
        {
            internal HangfireJobOptions(Action<IServiceProvider> executor)
            {
                Executor = executor;
            }

            internal HangfireJobOptions(Func<IServiceProvider, CancellationToken, Task> executor)
            {
                AsyncExecutor = executor;
            }

            /// <summary>
            /// 获取或设置 Cron 表达式。
            /// </summary>
            public string CronExpression { get; set; }

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
