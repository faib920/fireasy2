// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.NLog
{
    /// <summary>
    /// 基于 log4net 的日志管理器。
    /// </summary>
    public class Logger : ILogger
    {
        private readonly global::NLog.ILogger log;
        private readonly NLogOptions options;
        private static readonly ConcurrentDictionary<Type, ILogger> loggers = new ConcurrentDictionary<Type, ILogger>();

        protected Logger(Type type)
            : this (type, null)
        {
        }

        public Logger()
            : this(null)
        {
        }

        public Logger(Type type, NLogOptions options)
        {
            this.options = options;

            if (options != null && !string.IsNullOrEmpty(options.XmlFile))
            {
                global::NLog.LogManager.Configuration = new global::NLog.Config.XmlLoggingConfiguration(options.XmlFile, true);
            }

            log = type == null ? global::NLog.LogManager.GetLogger("fireasy") :
                global::NLog.LogManager.GetLogger("fireasy", type);
        }

        public ILogger GetLogger<T>() where T : class
        {
            return loggers.GetOrAdd(typeof(T), k =>new Logger(k, options));
        }

        /// <summary>
        /// 记录错误信息到日志。
        /// </summary>
        /// <param name="message">要记录的信息。</param>
        /// <param name="exception">异常对象。</param>
        public void Error(object message, Exception exception = null)
        {
            if (LogEnvironment.IsConfigured(LogLevel.Error))
            {
                log.Error(exception, message?.ToString());
            }
        }

        /// <summary>
        /// 记录一般的信息到日志。
        /// </summary>
        /// <param name="message">要记录的信息。</param>
        /// <param name="exception">异常对象。</param>
        public void Info(object message, Exception exception = null)
        {
            if (LogEnvironment.IsConfigured(LogLevel.Info))
            {
                log.Info(exception, message?.ToString());
            }
        }

        /// <summary>
        /// 记录警告信息到日志。
        /// </summary>
        /// <param name="message">要记录的信息。</param>
        /// <param name="exception">异常对象。</param>
        public void Warn(object message, Exception exception = null)
        {
            if (LogEnvironment.IsConfigured(LogLevel.Warn))
            {
                log.Warn(exception, message?.ToString());
            }
        }

        /// <summary>
        /// 记录调试信息到日志。
        /// </summary>
        /// <param name="message">要记录的信息。</param>
        /// <param name="exception">异常对象。</param>
        public void Debug(object message, Exception exception = null)
        {
            if (LogEnvironment.IsConfigured(LogLevel.Debug))
            {
                log.Debug(exception, message?.ToString());
            }
        }

        /// <summary>
        /// 记录致命信息到日志。
        /// </summary>
        /// <param name="message">要记录的信息。</param>
        /// <param name="exception">异常对象。</param>
        public void Fatal(object message, Exception exception = null)
        {
            if (LogEnvironment.IsConfigured(LogLevel.Fatal))
            {
                log.Fatal(exception, message?.ToString());
            }
        }

        /// <summary>
        /// 异步的，记录错误信息到日志。
        /// </summary>
        /// <param name="message">要记录的信息。</param>
        /// <param name="exception">异常对象。</param>
        public async Task ErrorAsync(object message, Exception exception = null, CancellationToken cancellationToken = default)
        {
            if (LogEnvironment.IsConfigured(LogLevel.Error))
            {
                log.Error(exception, message?.ToString());
            }
        }

        /// <summary>
        /// 异步的，记录一般的信息到日志。
        /// </summary>
        /// <param name="message">要记录的信息。</param>
        /// <param name="exception">异常对象。</param>
        public async Task InfoAsync(object message, Exception exception = null, CancellationToken cancellationToken = default)
        {
            if (LogEnvironment.IsConfigured(LogLevel.Info))
            {
                log.Info(exception, message?.ToString());
            }
        }

        /// <summary>
        /// 异步的，记录警告信息到日志。
        /// </summary>
        /// <param name="message">要记录的信息。</param>
        /// <param name="exception">异常对象。</param>
        public async Task WarnAsync(object message, Exception exception = null, CancellationToken cancellationToken = default)
        {
            if (LogEnvironment.IsConfigured(LogLevel.Warn))
            {
                log.Warn(exception, message?.ToString());
            }
        }

        /// <summary>
        /// 异步的，记录调试信息到日志。
        /// </summary>
        /// <param name="message">要记录的信息。</param>
        /// <param name="exception">异常对象。</param>
        public async Task DebugAsync(object message, Exception exception = null, CancellationToken cancellationToken = default)
        {
            if (LogEnvironment.IsConfigured(LogLevel.Debug))
            {
                log.Debug(exception, message?.ToString());
            }
        }

        /// <summary>
        /// 异步的，记录致命信息到日志。
        /// </summary>
        /// <param name="message">要记录的信息。</param>
        /// <param name="exception">异常对象。</param>
        public async Task FatalAsync(object message, Exception exception = null, CancellationToken cancellationToken = default)
        {
            if (LogEnvironment.IsConfigured(LogLevel.Fatal))
            {
                log.Fatal(exception, message?.ToString());
            }
        }
    }
}
