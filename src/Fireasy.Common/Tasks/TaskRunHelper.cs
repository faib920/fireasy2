// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using System.Reflection;

namespace Fireasy.Common.Tasks
{
    public class TaskRunHelper
    {
        private class MethodCache
        {
            internal protected static readonly MethodInfo Start = typeof(ITaskScheduler).GetMethod(nameof(ITaskScheduler.StartExecutor));
            internal protected static readonly MethodInfo StartAsync = typeof(ITaskScheduler).GetMethod(nameof(ITaskScheduler.StartExecutorAsync));
        }

        public static void Run(ITaskScheduler scheduler, TaskExecutorDefiniton definition)
        {
            if (definition == null || definition.ExecutorType == null)
            {
                return;
            }

            var startOption = typeof(StartOptions<>).MakeGenericType(definition.ExecutorType).New(definition.Delay, definition.Period);
            if (typeof(IAsyncTaskExecutor).IsAssignableFrom(definition.ExecutorType))
            {
                MethodCache.StartAsync.MakeGenericMethod(definition.ExecutorType).Invoke(scheduler, new object[] { startOption });
            }
            else if (typeof(ITaskExecutor).IsAssignableFrom(definition.ExecutorType))
            {
                MethodCache.Start.MakeGenericMethod(definition.ExecutorType).Invoke(scheduler, new object[] { startOption });
            }
        }
    }
}
