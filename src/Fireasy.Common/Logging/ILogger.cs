// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Common.Logging
{
    /// <summary>
    /// 提供日志记录的方法。
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// 记录错误信息到日志。
        /// </summary>
        /// <param name="message">要记录的信息。</param>
        /// <param name="exception">异常对象。</param>
        void Error(object message, Exception exception = null);

        /// <summary>
        /// 异步的，记录错误信息到日志。
        /// </summary>
        /// <param name="message">要记录的信息。</param>
        /// <param name="exception">异常对象。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        Task ErrorAsync(object message, Exception exception = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 记录一般的信息到日志。
        /// </summary>
        /// <param name="message">要记录的信息。</param>
        /// <param name="exception">异常对象。</param>
        void Info(object message, Exception exception = null);

        /// <summary>
        /// 异步的，记录一般的信息到日志。
        /// </summary>
        /// <param name="message">要记录的信息。</param>
        /// <param name="exception">异常对象。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        Task InfoAsync(object message, Exception exception = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 记录警告信息到日志。
        /// </summary>
        /// <param name="message">要记录的信息。</param>
        /// <param name="exception">异常对象。</param>
        void Warn(object message, Exception exception = null);

        /// <summary>
        /// 异步的，记录警告信息到日志。
        /// </summary>
        /// <param name="message">要记录的信息。</param>
        /// <param name="exception">异常对象。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        Task WarnAsync(object message, Exception exception = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 记录调试信息到日志。
        /// </summary>
        /// <param name="message">要记录的信息。</param>
        /// <param name="exception">异常对象。</param>
        void Debug(object message, Exception exception = null);

        /// <summary>
        /// 异步的，记录调试信息到日志。
        /// </summary>
        /// <param name="message">要记录的信息。</param>
        /// <param name="exception">异常对象。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        Task DebugAsync(object message, Exception exception = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 记录致命信息到日志。
        /// </summary>
        /// <param name="message">要记录的信息。</param>
        /// <param name="exception">异常对象。</param>
        void Fatal(object message, Exception exception = null);

        /// <summary>
        /// 异步的，记录致命信息到日志。
        /// </summary>
        /// <param name="message">要记录的信息。</param>
        /// <param name="exception">异常对象。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        Task FatalAsync(object message, Exception exception = null, CancellationToken cancellationToken = default);
    }
}
