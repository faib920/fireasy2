// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Common.Tasks;
#if NETSTANDARD
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
#endif
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Threading;
using Fireasy.Common;
using Fireasy.Common.ComponentModel;

namespace Fireasy.QuartzNet
{
    public class TaskScheduler : DisposableBase, ITaskScheduler, IServiceProviderAccessor
    {
        private readonly IScheduler scheduler;
        private readonly CancellationTokenSource stopToken = new CancellationTokenSource();

        public TaskScheduler()
        {
            var props = new NameValueCollection
                {
                    { "quartz.serializer.type", "binary" }
                };

            var factory = new StdSchedulerFactory(props);
            scheduler = factory.GetScheduler().AsSync();
            scheduler.Start();
        }

#if NETSTANDARD
        public TaskScheduler(IServiceProvider serviceProvider, IOptions<QuartzScheduleOptions> options)
            : this()
        {
            ServiceProvider = serviceProvider;
            var _options = options.Value;

            foreach (var task in _options.Tasks)
            {
                var dataMap = new JobDataMap
                    {
                        { "executor", task.Executor },
                        { "serviceProvider", serviceProvider },
                        { "cancellationToken", stopToken.Token }
                    };

                var job = JobBuilder.Create<AnonymousJobWrapper>()
                    .SetJobData(dataMap)
                    .Build();

                scheduler.ScheduleJob(job, CreateTrigger(task.Delay, task.Period));
            }
        }
#endif

        /// <summary>
        /// 获取预执行器列表。
        /// </summary>
        public Queue<TaskExecutorDefiniton> PreTasks { get; } = new Queue<TaskExecutorDefiniton>();

        /// <summary>
        /// 获取或设置应用程序服务提供者实例。
        /// </summary>
        public IServiceProvider ServiceProvider { get; set; }

        public void StartExecutor<T>(StartOptions<T> options) where T : ITaskExecutor
        {
            var dataMap = new JobDataMap
            {
                { "arguments", options.Arguments },
                { "serviceProvider", ServiceProvider },
                { "cancellationToken", stopToken.Token },
                { "initializer", options.Initializer }
            };

            var job = JobBuilder.Create<SyncJobWrapper<T>>()
                .SetJobData(dataMap)
                .Build();

            scheduler.ScheduleJob(job, CreateTrigger(options.Delay, options.Period));
        }

        public void StartExecutorAsync<T>(StartOptions<T> options) where T : IAsyncTaskExecutor
        {
            var dataMap = new JobDataMap
            {
                { "arguments", options.Arguments },
                { "serviceProvider", ServiceProvider },
                { "cancellationToken", stopToken.Token },
                { "initializer", options.Initializer }
            };

            var job = JobBuilder.Create<AsyncJobWrapper<T>>()
                .SetJobData(dataMap)
                .Build();

            scheduler.ScheduleJob(job, CreateTrigger(options.Delay, options.Period));
        }

        public void Start()
        {
            while (PreTasks.Count > 0)
            {
                TaskRunHelper.Run(this, PreTasks.Dequeue());
            }
        }

        public void Stop()
        {
            stopToken.Cancel();
            scheduler?.Shutdown();
            scheduler?.TryDispose();
        }

        protected override bool Dispose(bool disposing)
        {
            stopToken.Dispose();

            return base.Dispose(disposing);
        }

        private ITrigger CreateTrigger(TimeSpan delay, TimeSpan period)
        {
            return TriggerBuilder.Create()
                .StartAt(DateTimeOffset.Now.Add(delay))
                .WithSimpleSchedule(x => x
                    .WithInterval(period)
                    .RepeatForever())
                .Build();
        }

#if NETSTANDARD
        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            Start();
            return Task.CompletedTask;
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            Stop();
            return Task.CompletedTask;
        }
#endif
    }
}
