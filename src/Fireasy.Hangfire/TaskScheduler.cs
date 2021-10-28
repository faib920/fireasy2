// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
using Fireasy.Common.Ioc;
using Fireasy.Common.Tasks;
using Hangfire;
using Hangfire.Common;
#if NETSTANDARD
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Fireasy.Common.Options;
using Fireasy.Common.Tasks.Configuration;
#endif
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Hangfire
{
    [ConfigurationSetting(typeof(HangfireConfigurationSetting))]
    public class TaskScheduler : DisposableBase, ITaskScheduler, IConfigurationSettingHostService, IServiceProviderAccessor
    {
        private readonly CancellationTokenSource _stopToken = new CancellationTokenSource();
        private HangfireConfigurationSetting _setting;
        private bool _isRunning;

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
            ServiceProvider = serviceProvider;
        }

#if NETSTANDARD
        /// <summary>
        /// 初始化 <see cref="TaskScheduler"/> 类的新实例。
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="options"></param>
        public TaskScheduler(IServiceProvider serviceProvider, IOptionsMonitor<HangfireScheduleOptions> options)
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
                    if (string.IsNullOrEmpty(jobOpt.CronExpression))
                    {
                        BackgroundJob.Schedule(() => DoExecute(jobOpt.Executor, null), TimeSpan.Zero);
                    }
                    else
                    {
                        RecurringJob.AddOrUpdate(() => DoExecute(jobOpt.Executor, null), jobOpt.CronExpression);
                    }
                }
                else if (jobOpt.AsyncExecutor != null)
                {
                    if (string.IsNullOrEmpty(jobOpt.CronExpression))
                    {
                        BackgroundJob.Schedule(() => DoExecuteAsync(jobOpt.AsyncExecutor, null), TimeSpan.Zero);
                    }
                    else
                    {
                        RecurringJob.AddOrUpdate(() => DoExecuteAsync(jobOpt.AsyncExecutor, null), jobOpt.CronExpression);
                    }
                }
            }
        }
#endif

        /// <summary>
        /// 获取或设置应用程序服务提供者实例。
        /// </summary>
        public IServiceProvider ServiceProvider { get; set; }

        public void Start(StartOptions options, Action<IServiceProvider> executor)
        {
            if (options.Period != TimeSpan.Zero)
            {
                RecurringJob.AddOrUpdate(() => DoExecute(executor, options), ConvertToCron(options.Period));
            }
            else
            {
                BackgroundJob.Schedule(() => DoExecute(executor, options), TimeSpan.Zero);
            }
        }

        public void StartAsync(StartOptions options, Func<IServiceProvider, CancellationToken, Task> executor)
        {
            if (options.Period != TimeSpan.Zero)
            {
                RecurringJob.AddOrUpdate(() => DoExecuteAsync(executor, options), ConvertToCron(options.Period));
            }
            else
            {
                BackgroundJob.Schedule(() => DoExecuteAsync(executor, options), TimeSpan.Zero);
            }
        }

        public void StartExecutor<T>(StartOptions<T> options) where T : ITaskExecutor
        {
            if (options.Period != TimeSpan.Zero)
            {
                RecurringJob.AddOrUpdate(() => DoExecute(options), ConvertToCron(options.Period));
            }
            else
            {
                BackgroundJob.Schedule(() => DoExecute(options), TimeSpan.Zero);
            }
        }

        public void StartExecutorAsync<T>(StartOptions<T> options) where T : IAsyncTaskExecutor
        {
            if (options.Period != TimeSpan.Zero)
            {
                Thread.Sleep(options.Delay);
                RecurringJob.AddOrUpdate(() => DoExecuteAsync(options), ConvertToCron(options.Period));
            }
            else
            {
                BackgroundJob.Schedule(() => DoExecuteAsync(options), TimeSpan.Zero);
            }
        }

        public void Start()
        {
            if (_isRunning)
            {
                return;
            }

            _isRunning = true;
        }

        public void Stop()
        {
            _stopToken.Cancel();
            _isRunning = false;
        }

        private void StartJob(HangfireJobSetting setting)
        {
            RecurringJob.AddOrUpdate(() => AA.Test(), setting.CronExpression, TimeZoneInfo.Local);


            if (typeof(ITaskExecutor).IsAssignableFrom(setting.ExecutorType))
            {
                RecurringJob.AddOrUpdate(setting.ExecutorType.FullName, () => DoExecute(setting), setting.CronExpression, TimeZoneInfo.Local);
            }
            else if (typeof(IAsyncTaskExecutor).IsAssignableFrom(setting.ExecutorType))
            {
                RecurringJob.AddOrUpdate(setting.ExecutorType.FullName, () => DoExecuteAsync(setting), setting.CronExpression, TimeZoneInfo.Local);
            }
        }

        void IConfigurationSettingHostService.Attach(IConfigurationSettingItem setting)
        {
            if (_setting == null)
            {
                _setting = (HangfireConfigurationSetting)setting;

                _setting.Jobs.ForEach(s => StartJob(s));
            }
        }

        IConfigurationSettingItem IConfigurationSettingHostService.GetSetting()
        {
            return _setting;
        }

        public void DoExecute(Action<IServiceProvider> executor, StartOptions options)
        {
            if (options?.Delay.TotalMilliseconds > 0)
            {
                Thread.Sleep(options.Delay);
            }

            using var scope = ServiceProvider.TryCreateScope();
            executor(scope.ServiceProvider);
        }

        public async Task DoExecuteAsync(Func<IServiceProvider, CancellationToken, Task> executor, StartOptions options)
        {
            if (options?.Delay.TotalMilliseconds > 0)
            {
                await Task.Delay(options.Delay);
            }

            using var scope = ServiceProvider.TryCreateScope();
            await executor(scope.ServiceProvider, _stopToken.Token);
        }

        public void DoExecute<T>(StartOptions<T> options) where T : ITaskExecutor
        {
            if (options?.Delay.TotalMilliseconds > 0)
            {
                Thread.Sleep(options.Delay);
            }

            using var scope = ServiceProvider.TryCreateScope();
            var context = new TaskExecuteContext(scope.ServiceProvider, options?.Arguments, _stopToken.Token);
            var executor = typeof(T).New<T>(scope.ServiceProvider);
            options.Initializer?.Invoke(executor);
            executor.Execute(context);
        }

        public async Task DoExecuteAsync<T>(StartOptions<T> options) where T : IAsyncTaskExecutor
        {
            if (options?.Delay.TotalMilliseconds > 0)
            {
                await Task.Delay(options.Delay);
            }

            using var scope = ServiceProvider.TryCreateScope();
            var context = new TaskExecuteContext(scope.ServiceProvider, options?.Arguments, _stopToken.Token);
            var executor = typeof(T).New<T>(scope.ServiceProvider);
            options.Initializer?.Invoke(executor);
            await executor.ExecuteAsync(context);
        }

        public void DoExecute(HangfireJobSetting setting)
        {
            using var scope = ServiceProvider.TryCreateScope();
            var context = new TaskExecuteContext(scope.ServiceProvider, setting.Arguments, _stopToken.Token);
            var executor = setting.ExecutorType.New<ITaskExecutor>(scope.ServiceProvider);
            executor.Execute(context);
        }

        public async Task DoExecuteAsync(HangfireJobSetting setting)
        {
            using var scope = ServiceProvider.TryCreateScope();
            var context = new TaskExecuteContext(scope.ServiceProvider, setting.Arguments, _stopToken.Token);
            var executor = setting.ExecutorType.New<IAsyncTaskExecutor>(scope.ServiceProvider);
            await executor.ExecuteAsync(context);
        }

        private string ConvertToCron(TimeSpan period)
        {
            if (period.Seconds != 0)
            {
                throw new ArgumentException("Seconds not allowed for daily recurrence.");
            }

            return $"{period.Minutes} {period.Hours} * * *";
        }

#if NETSTANDARD
        private HangfireConfigurationSetting GetQuartzSetting(HangfireScheduleOptions options)
        {
            var section = ConfigurationUnity.GetSection<TaskScheduleConfigurationSection>();

            if (section != null)
            {
                if (!options.IsConfigured() || string.IsNullOrEmpty(options.ConfigName))
                {
                    var matchSetting = section.Settings.FirstOrDefault(s => s.Value.SchedulerType == typeof(TaskScheduler)).Value;
                    if (matchSetting != null && section.GetSetting(matchSetting.Name) is ExtendConfigurationSetting extSetting)
                    {
                        return (HangfireConfigurationSetting)extSetting.Extend;
                    }
                }
                else
                {
                    if (section.GetSetting(options.ConfigName) is ExtendConfigurationSetting extSetting)
                    {
                        return (HangfireConfigurationSetting)extSetting.Extend;
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

    public class AA
    {

        public static void Test()
        {
            Console.WriteLine(DateTime.Now);
        }

    }
}
