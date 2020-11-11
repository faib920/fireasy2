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
using Fireasy.Common.Options;
using Fireasy.Common.Tasks.Configuration;
using System.Linq;
#endif
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Specialized;
using System.Threading;
using Fireasy.Common;
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Configuration;
using System.Threading.Tasks;
using Fireasy.Common.Ioc;

namespace Fireasy.QuartzNet
{
    [ConfigurationSetting(typeof(QuartzConfigurationSetting))]
    public class TaskScheduler : DisposableBase, ITaskScheduler, IConfigurationSettingHostService, IServiceProviderAccessor
    {
        private readonly IScheduler _scheduler;
        private bool _isRunning;
        private readonly CancellationTokenSource _stopToken = new CancellationTokenSource();
        private QuartzConfigurationSetting _setting;

        /// <summary>
        /// 初始化 <see cref="TaskScheduler"/> 类的新实例。
        /// </summary>
        public TaskScheduler()
            : this(ContainerUnity.GetContainer())
        {
        }

        /// <summary>
        /// 初始化 <see cref="TaskScheduler"/> 类的新实例。
        /// </summary>
        /// <param name="serviceProvider"></param>
        public TaskScheduler(IServiceProvider serviceProvider)
        {
            var props = new NameValueCollection
                {
                    { "quartz.serializer.type", "binary" }
                };

            ServiceProvider = serviceProvider;
            var factory = new StdSchedulerFactory(props);
            _scheduler = factory.GetScheduler().AsSync();
        }

#if NETSTANDARD
        /// <summary>
        /// 初始化 <see cref="TaskScheduler"/> 类的新实例。
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="options"></param>
        public TaskScheduler(IServiceProvider serviceProvider, IOptionsMonitor<QuartzScheduleOptions> options)
            : this(serviceProvider)
        {
            ServiceProvider = serviceProvider;
            var optValue = options.CurrentValue;
            var setting = GetQuartzSetting(optValue);

            setting?.Jobs.ForEach(s => StartJob(s));

            foreach (var jobOpt in optValue.Jobs)
            {
                if (jobOpt.Executor != null)
                {
                    var dataMap = new JobDataMap
                    {
                        { "executor", jobOpt.Executor },
                        { "serviceProvider", serviceProvider },
                        { "cancellationToken", _stopToken.Token }
                    };

                    var job = JobBuilder.Create<AnonymousJobWrapper>()
                        .SetJobData(dataMap)
                        .Build();

                    _scheduler.ScheduleJob(job, CreateTrigger(jobOpt));
                }
                else if (jobOpt.AsyncExecutor != null)
                {
                    var dataMap = new JobDataMap
                    {
                        { "executor", jobOpt.AsyncExecutor },
                        { "serviceProvider", serviceProvider },
                        { "cancellationToken", _stopToken.Token }
                    };

                    var job = JobBuilder.Create<AnonymousAsyncJobWrapper>()
                        .SetJobData(dataMap)
                        .Build();

                    _scheduler.ScheduleJob(job, CreateTrigger(jobOpt));
                }
            }
        }
#endif

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
                { "initializer", options.Initializer }
            };

            var job = JobBuilder.Create<SyncJobWrapper<T>>()
                .SetJobData(dataMap)
                .Build();

            _scheduler.ScheduleJob(job, CreateTrigger(options));
        }

        public void StartExecutorAsync<T>(StartOptions<T> options) where T : IAsyncTaskExecutor
        {
            var dataMap = new JobDataMap
            {
                { "arguments", options.Arguments },
                { "serviceProvider", ServiceProvider },
                { "cancellationToken", _stopToken.Token },
                { "initializer", options.Initializer }
            };

            var job = JobBuilder.Create<AsyncJobWrapper<T>>()
                .SetJobData(dataMap)
                .Build();

            _scheduler.ScheduleJob(job, CreateTrigger(options));
        }

        public void Start(StartOptions options, Action<IServiceProvider> executor)
        {
            var dataMap = new JobDataMap
                {
                    { "executor", executor },
                    { "serviceProvider", ServiceProvider }
                };

            var job = JobBuilder.Create<AnonymousJobWrapper>()
                .SetJobData(dataMap)
                .Build();

            _scheduler.ScheduleJob(job, CreateTrigger(options));
        }

        public void StartAsync(StartOptions options, Func<IServiceProvider, CancellationToken, Task> executor)
        {
            var dataMap = new JobDataMap
                {
                    { "executor", executor },
                    { "serviceProvider", ServiceProvider },
                    { "cancellationToken", _stopToken.Token }
                };

            var job = JobBuilder.Create<AnonymousAsyncJobWrapper>()
                .SetJobData(dataMap)
                .Build();

            _scheduler.ScheduleJob(job, CreateTrigger(options));
        }

        public void Start()
        {
            if (_isRunning)
            {
                return;
            }

            _isRunning = true;
            _scheduler.Start();
        }

        public void Stop()
        {
            _stopToken.Cancel();
            _scheduler?.Shutdown();
            _scheduler?.TryDispose();
            _isRunning = false;
        }

        protected override bool Dispose(bool disposing)
        {
            _stopToken.Dispose();

            return base.Dispose(disposing);
        }
        private ITrigger CreateTrigger(StartOptions options)
        {
            return TriggerBuilder.Create()
                .StartAt(DateTimeOffset.Now.Add(options.Delay))
                .WithSimpleSchedule(x => x
                    .WithInterval(options.Period)
                    .RepeatForever())
                .Build();
        }

        private ITrigger CreateTrigger(IQuartzScheduleOptions options)
        {
            var builder = TriggerBuilder.Create();

            if (!string.IsNullOrEmpty(options.CronExpression))
            {
                builder = builder.WithCronSchedule(options.CronExpression);
            }

            if (options.StartTime != null)
            {
                builder = builder.StartAt(options.StartTime.Value);
            }
            else
            {
                builder = builder.StartAt(DateTimeOffset.Now);
            }

            return builder.EndAt(options.EndTime).Build();
        }

        private void StartJob(QuartzJobSetting setting)
        {
            Type jobType = null;

            if (typeof(ITaskExecutor).IsAssignableFrom(setting.ExecutorType))
            {
                jobType = typeof(SyncJobWrapper<>).MakeGenericType(setting.ExecutorType);
            }
            else if (typeof(IAsyncTaskExecutor).IsAssignableFrom(setting.ExecutorType))
            {
                jobType = typeof(AsyncJobWrapper<>).MakeGenericType(setting.ExecutorType);
            }

            if (jobType == null)
            {
                return;
            }

            var dataMap = new JobDataMap
                {
                    { "arguments", setting.Arguments },
                    { "serviceProvider", ServiceProvider },
                    { "cancellationToken", _stopToken.Token },
                    { "initializer", null }
                };

            var job = JobBuilder.Create(jobType)
                .SetJobData(dataMap)
                .Build();

            _scheduler.ScheduleJob(job, CreateTrigger(setting));
        }

        void IConfigurationSettingHostService.Attach(IConfigurationSettingItem setting)
        {
            if (_setting == null)
            {
                _setting = (QuartzConfigurationSetting)setting;

                _setting.Jobs.ForEach(s => StartJob(s));
            }
        }

        IConfigurationSettingItem IConfigurationSettingHostService.GetSetting()
        {
            return _setting;
        }

#if NETSTANDARD
        private QuartzConfigurationSetting GetQuartzSetting(QuartzScheduleOptions options)
        {
            var section = ConfigurationUnity.GetSection<TaskScheduleConfigurationSection>();

            if (section != null)
            {
                if (!options.IsConfigured() || string.IsNullOrEmpty(options.ConfigName))
                {
                    var matchSetting = section.Settings.FirstOrDefault(s => s.Value.SchedulerType == typeof(TaskScheduler)).Value;
                    if (matchSetting != null && section.GetSetting(matchSetting.Name) is ExtendConfigurationSetting extSetting)
                    {
                        return (QuartzConfigurationSetting)extSetting.Extend;
                    }
                }
                else
                {
                    if (section.GetSetting(options.ConfigName) is ExtendConfigurationSetting extSetting)
                    {
                        return (QuartzConfigurationSetting)extSetting.Extend;
                    }
                }
            }

            return null;
        }

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
