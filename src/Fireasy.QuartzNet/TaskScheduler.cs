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
using System.Threading;
using System.Threading.Tasks;
#endif
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Specialized;
using System.Collections.Generic;

namespace Fireasy.QuartzNet
{
    public class TaskScheduler : ITaskScheduler
    {
        private readonly IScheduler scheduler;
        private readonly IServiceProvider serviceProvider;

        public TaskScheduler()
        {
            var props = new NameValueCollection
                {
                    { "quartz.serializer.type", "binary" },
                    { "quartz.config", "~/quartz.config" }
                };

            var factory = new StdSchedulerFactory(props);
            scheduler = factory.GetScheduler().AsSync();
            scheduler.Start();
        }

#if NETSTANDARD
        public TaskScheduler(IServiceProvider serviceProvider, IEnumerable<TaskExecutorDefiniton> definitions)
            : this()
        {
            this.serviceProvider = serviceProvider;

            if (definitions == null)
            {
                return;
            }

            foreach (var def in definitions)
            {
                var executor = serviceProvider.GetService(def.ExecutorType);
                if (executor == null)
                {
                    continue;
                }

                var dataMap = new JobDataMap
                    {
                        { "executor", executor },
                        { "serviceProvider", serviceProvider }
                    };

                Func<JobBuilder> bfunc = null;
                if (executor is ITaskExecutor)
                {
                    bfunc = () => JobBuilder.Create<SyncJobWrapper>();

                }
                else if (executor is IAsyncTaskExecutor)
                {
                    bfunc = () => JobBuilder.Create<AsyncJobWrapper>();

                }

                if (bfunc != null)
                {
                    var job = bfunc().SetJobData(dataMap).Build();

                    scheduler.ScheduleJob(job, CreateTrigger(def.Delay, def.Period));
                }
            }
        }

        public TaskScheduler(IServiceProvider serviceProvider, QuartzScheduleOptions options)
            : this()
        {
            this.serviceProvider = serviceProvider;

            foreach (var task in options.Tasks)
            {
                var dataMap = new JobDataMap
                    {
                        { "executor", task.Executor },
                        { "serviceProvider", serviceProvider }
                    };

                var job = JobBuilder.Create<AnonymousJobWrapper>()
                    .SetJobData(dataMap)
                    .Build();

                scheduler.ScheduleJob(job, CreateTrigger(task.Delay, task.Period));
            }
        }
#endif

        public void Start<T>(StartOptions<T> options) where T : ITaskExecutor
        {
            var dataMap = new JobDataMap
            {
                { "arguments", options.Arguments },
                { "serviceProvider", serviceProvider },
                { "initializer", options.Initializer }
            };

            var job = JobBuilder.Create<SyncJobWrapper<T>>()
                .SetJobData(dataMap)
                .Build();

            scheduler.ScheduleJob(job, CreateTrigger(options.Delay, options.Period));
        }

        public void StartAsync<T>(StartOptions<T> options) where T : IAsyncTaskExecutor
        {
            var dataMap = new JobDataMap
            {
                { "arguments", options.Arguments },
                { "serviceProvider", serviceProvider },
                { "initializer", options.Initializer }
            };

            var job = JobBuilder.Create<AsyncJobWrapper<T>>()
                .SetJobData(dataMap)
                .Build();

            scheduler.ScheduleJob(job, CreateTrigger(options.Delay, options.Period));
        }

        public void Stop()
        {
            scheduler?.Shutdown();
            scheduler?.TryDispose();
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
