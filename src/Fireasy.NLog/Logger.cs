// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Logging;
using Fireasy.Common.Threading;
using System;
using System.Threading;
using System.Threading.Tasks;
#if NETSTANDARD
using Microsoft.Extensions.Options;
#endif

namespace Fireasy.NLog
{
    /// <summary>
    /// 基于 log4net 的日志管理器。
    /// </summary>
    public class Logger : ILogger
    {
        private readonly global::NLog.ILogger _logger;
        private readonly NLogOptions _options;

        public Logger()
            : this(null, null)
        {
        }

#if NETSTANDARD
        public Logger(IOptions<NLogOptions> options)
            : this(null, options.Value)
        {
        }
#endif

        public Logger(Type type, NLogOptions options)
        {
            _options = options;

            if (options != null && !string.IsNullOrEmpty(options.XmlFile))
            {
                global::NLog.LogManager.Configuration = new global::NLog.Config.XmlLoggingConfiguration(options.XmlFile, true);
            }

            _logger = type == null ? global::NLog.LogManager.GetLogger("fireasy") :
                global::NLog.LogManager.GetLogger("fireasy", type);
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
                _logger.Error(exception, message?.ToString());
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
                _logger.Info(exception, message?.ToString());
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
                _logger.Warn(exception, message?.ToString());
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
                _logger.Debug(exception, message?.ToString());
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
                _logger.Fatal(exception, message?.ToString());
            }
        }

        /// <summary>
        /// 异步的，记录错误信息到日志。
        /// </summary>
        /// <param name="message">要记录的信息。</param>
        /// <param name="exception">异常对象。</param>
        public Task ErrorAsync(object message, Exception exception = null, CancellationToken cancellationToken = default)
        {
            if (LogEnvironment.IsConfigured(LogLevel.Error))
            {
                _logger.Error(exception, message?.ToString());
            }

            return TaskCompatible.CompletedTask;
        }

        /// <summary>
        /// 异步的，记录一般的信息到日志。
        /// </summary>
        /// <param name="message">要记录的信息。</param>
        /// <param name="exception">异常对象。</param>
        public Task InfoAsync(object message, Exception exception = null, CancellationToken cancellationToken = default)
        {
            if (LogEnvironment.IsConfigured(LogLevel.Info))
            {
                _logger.Info(exception, message?.ToString());
            }

            return TaskCompatible.CompletedTask;
        }

        /// <summary>
        /// 异步的，记录警告信息到日志。
        /// </summary>
        /// <param name="message">要记录的信息。</param>
        /// <param name="exception">异常对象。</param>
        public Task WarnAsync(object message, Exception exception = null, CancellationToken cancellationToken = default)
        {
            if (LogEnvironment.IsConfigured(LogLevel.Warn))
            {
                _logger.Warn(exception, message?.ToString());
            }

            return TaskCompatible.CompletedTask;
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
                _logger.Debug(exception, message?.ToString());
            }
        }

        /// <summary>
        /// 异步的，记录致命信息到日志。
        /// </summary>
        /// <param name="message">要记录的信息。</param>
        /// <param name="exception">异常对象。</param>
        public Task FatalAsync(object message, Exception exception = null, CancellationToken cancellationToken = default)
        {
            if (LogEnvironment.IsConfigured(LogLevel.Fatal))
            {
                _logger.Fatal(exception, message?.ToString());
            }

            return TaskCompatible.CompletedTask;
        }

        public ILogger<T> Create<T>() where T : class
        {
            return new Logger<T>(_options);
        }
    }

    public class Logger<T> : Logger, ILogger<T> where T : class
    {
        public Logger(NLogOptions options)
            : base(typeof(T), options)
        {
        }

#if NETSTANDARD
        public Logger(IOptions<NLogOptions> options)
            : base(typeof(T), options.Value)
        {
        }
#endif
    }
}
