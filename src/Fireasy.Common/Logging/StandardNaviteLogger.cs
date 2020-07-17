// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Common.Logging
{
    /// <summary>
    /// 使用 <see cref="Microsoft.Extensions.Logging.ILogger{T}"/> 处理日志。
    /// </summary>
    public class StandardNaviteLogger : ILogger
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Microsoft.Extensions.Logging.ILogger<StandardNaviteLogger> _logger;

        public StandardNaviteLogger(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = serviceProvider.GetService<Microsoft.Extensions.Logging.ILogger<StandardNaviteLogger>>();
        }

        public void Debug(object message, Exception exception = null)
        {
            _logger.LogDebug(message.ToString());
        }

        public async Task DebugAsync(object message, Exception exception = null, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug(message.ToString());
        }

        public void Error(object message, Exception exception = null)
        {
            _logger.LogError(message.ToString());
        }

        public async Task ErrorAsync(object message, Exception exception = null, CancellationToken cancellationToken = default)
        {
            _logger.LogError(message.ToString());
        }

        public void Fatal(object message, Exception exception = null)
        {
            _logger.LogCritical(message.ToString());
        }

        public async Task FatalAsync(object message, Exception exception = null, CancellationToken cancellationToken = default)
        {
            _logger.LogCritical(message.ToString());
        }

        public void Info(object message, Exception exception = null)
        {
            _logger.LogInformation(message.ToString());
        }

        public async Task InfoAsync(object message, Exception exception = null, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(message.ToString());
        }

        public void Warn(object message, Exception exception = null)
        {
            _logger.LogWarning(message.ToString());
        }

        public async Task WarnAsync(object message, Exception exception = null, CancellationToken cancellationToken = default)
        {
            _logger.LogWarning(message.ToString());
        }

        public ILogger<T> Create<T>() where T : class
        {
            return new StandardNaviteLogger<T>(_serviceProvider);
        }
    }

    public class StandardNaviteLogger<T> : ILogger<T> where T : class
    {
        private readonly Microsoft.Extensions.Logging.ILogger<T> _logger;

        public StandardNaviteLogger(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetService<Microsoft.Extensions.Logging.ILogger<T>>();
        }

        public void Debug(object message, Exception exception = null)
        {
            _logger.LogDebug(message.ToString());
        }

        public async Task DebugAsync(object message, Exception exception = null, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug(message.ToString());
        }

        public void Error(object message, Exception exception = null)
        {
            _logger.LogError(message.ToString());
        }

        public async Task ErrorAsync(object message, Exception exception = null, CancellationToken cancellationToken = default)
        {
            _logger.LogError(message.ToString());
        }

        public void Fatal(object message, Exception exception = null)
        {
            _logger.LogCritical(message.ToString());
        }

        public async Task FatalAsync(object message, Exception exception = null, CancellationToken cancellationToken = default)
        {
            _logger.LogCritical(message.ToString());
        }

        public void Info(object message, Exception exception = null)
        {
            _logger.LogInformation(message.ToString());
        }

        public async Task InfoAsync(object message, Exception exception = null, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(message.ToString());
        }

        public void Warn(object message, Exception exception = null)
        {
            _logger.LogWarning(message.ToString());
        }

        public async Task WarnAsync(object message, Exception exception = null, CancellationToken cancellationToken = default)
        {
            _logger.LogWarning(message.ToString());
        }
    }
}
#endif