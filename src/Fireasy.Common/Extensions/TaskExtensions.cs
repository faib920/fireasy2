// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Threading.Tasks;

namespace Fireasy.Common.Extensions
{
    /*
    /// <summary>
    /// Task 相关的扩展。
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// 将此任务作为同步调用。
        /// </summary>
        /// <param name="task"></param>
        public static void AsSync(this Task task)
        {
#if NETFRAMEWORK
            task.ConfigureAwait(false).GetAwaiter().GetResult();
#else
            task.ConfigureAwait(false).GetAwaiter().GetResult();
#endif
        }

        /// <summary>
        /// 将此任务作为同步调用并返回值。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <returns></returns>
        public static T AsSync<T>(this Task<T> task)
        {
#if NETFRAMEWORK
            return task.Result;
#else
            return task.ConfigureAwait(false).GetAwaiter().GetResult();
#endif
        }
    }
    */
}
