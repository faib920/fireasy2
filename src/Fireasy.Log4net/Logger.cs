// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Logging;
using log4net;
using log4net.Config;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#if NETSTANDARD
using Microsoft.Extensions.Options;
#endif

namespace Fireasy.Log4net
{
    /// <summary>
    /// 基于 log4net 的日志管理器。
    /// </summary>
    public class Logger : ILogger
    {
        private readonly ILog _logger;
        private readonly Log4netOptions _options;

        public Logger()
            : this(null, null)
        {
        }

#if NETSTANDARD
        public Logger(IOptions<Log4netOptions> options)
            : this(null, options.Value)
        {
        }
#endif

        public Logger(Type type, Log4netOptions options)
        {
            _options = options;

            var repository = LogManager.GetAllRepositories().FirstOrDefault(s => s.Name == "fireasy") ?? LogManager.CreateRepository("fireasy");

            XmlConfigurator.Configure(repository,
                new FileInfo(options == null || string.IsNullOrEmpty(options.XmlFile) ? "log4net.config" : options.XmlFile));

            _logger = type == null ? LogManager.GetLogger("fireasy", string.Empty) :
                LogManager.GetLogger("fireasy", type);
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
                _logger.Error(message, exception);
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
                _logger.Info(message, exception);
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
                _logger.Warn(message, exception);
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
                _logger.Debug(message, exception);
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
                _logger.Fatal(message, exception);
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
                _logger.Error(message, exception);
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
                _logger.Info(message, exception);
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
                _logger.Warn(message, exception);
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
                _logger.Debug(message, exception);
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
                _logger.Fatal(message, exception);
            }
        }

        public ILogger<T> Create<T>() where T : class
        {
            throw new NotImplementedException();
        }
    }

    public class Logger<T> : Logger, ILogger<T> where T : class
    {
        public Logger(Log4netOptions options)
            : base(typeof(T), options)
        {
        }

#if NETSTANDARD
        public Logger(IOptions<Log4netOptions> options)
            : base(typeof(T), options.Value)
        {
        }
#endif
    }
}
