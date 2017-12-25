#if !NET40 && !NET35
// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fireasy.Data
{
    /// <summary>
    /// 异步任务管理器。无法继承此类。
    /// </summary>
    public sealed class AsyncTaskManager : Scope<AsyncTaskManager>
    {
        private List<Task> tasks = new List<Task>();
        private IDatabase database;

        /// <summary>
        /// 初始化 <see cref="AsyncTaskManager"/> 类的新实例。
        /// </summary>
        /// <param name="database">要托管的 <see cref="IDatabase"/> 对象。</param>
        public AsyncTaskManager(IDatabase database)
        {
            this.database = database;
        }

        /// <summary>
        /// 获取或设置是否提交事务。
        /// </summary>
        public bool IsCommitTransaction { get; set; }

        internal Task<T> AddTask<T>(Task<T> task)
        {
            tasks.Add(task);
            task.ContinueWith(t =>
                {
                    tasks.Remove(t);
                    if (tasks.Count == 0)
                    {
                        if (IsCommitTransaction)
                        {
                            database.CommitTransaction();
                        }

                        database.Dispose();
                    }
                });
            return task;
        }

        internal Task AddTask(Task task)
        {
            tasks.Add(task);
            return task.ContinueWith(t =>
                {
                    tasks.Remove(t);
                    if (tasks.Count == 0)
                    {
                        database.Dispose();
                    }
                });
        }

        /// <summary>
        /// 获取是否有异步任务。
        /// </summary>
        public bool HasTasks
        {
            get
            {
                return tasks.Count > 0;
            }
        }

        /// <summary>
        /// 使用异步任务管理器接管任务对象。
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public static Task Adapter(Task task)
        {
            if (Current == null)
            {
                return task;
            }

            return Current.AddTask(task);
        }

        /// <summary>
        /// 使用异步任务管理器接管任务对象。
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public static Task<T> Adapter<T>(Task<T> task)
        {
            if (Current == null)
            {
                return task;
            }

            return Current.AddTask<T>(task);
        }
    }
}
#endif