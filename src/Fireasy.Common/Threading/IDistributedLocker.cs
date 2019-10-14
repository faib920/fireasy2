// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Threading.Tasks;

namespace Fireasy.Common.Threading
{
    /// <summary>
    /// 分布式锁。
    /// </summary>
    public interface IDistributedLocker
    {
        /// <summary>
        /// 提供一个令牌对某执行期间进行上锁。
        /// </summary>
        /// <param name="token">锁的令牌。</param>
        /// <param name="timeout">锁的过期时间。</param>
        /// <param name="action"></param>
        void Lock(string token, TimeSpan timeout, Action action);

        /// <summary>
        /// 提供一个令牌对某执行期间进行上锁。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="token">锁的令牌。</param>
        /// <param name="timeout">锁的过期时间。</param>
        /// <param name="func"></param>
        /// <returns></returns>
        T Lock<T>(string token, TimeSpan timeout, Func<T> func);

        /// <summary>
        /// 异步的，提供一个令牌对某执行期间进行上锁。
        /// </summary>
        /// <param name="token">锁的令牌。</param>
        /// <param name="timeout">锁的过期时间。</param>
        /// <param name="task"></param>
        /// <returns></returns>
        Task LockAsync(string token, TimeSpan timeout, Task task);

        /// <summary>
        /// 异步的，提供一个令牌对某执行期间进行上锁。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="token">锁的令牌。</param>
        /// <param name="timeout">锁的过期时间。</param>
        /// <param name="func"></param>
        /// <returns></returns>
        Task<T> LockAsync<T>(string token, TimeSpan timeout, Func<Task<T>> func);
    }
}
