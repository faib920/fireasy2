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
using SLogger = Serilog;

namespace Fireasy.Serilog
{
    public class Logger : ILogger
    {
        private readonly SLogger.ILogger _logger = new SLogger.LoggerConfiguration().CreateLogger();

        public ILogger<T> Create<T>() where T : class
        {
            throw new NotImplementedException();
        }

        public void Debug(object message, Exception exception = null)
        {
            _logger.Debug(exception, (string)message);
        }

        public Task DebugAsync(object message, Exception exception = null, CancellationToken cancellationToken = default)
        {
            _logger.Debug(exception, (string)message);
            return TaskCompatible.CompletedTask;
        }

        public void Error(object message, Exception exception = null)
        {
            _logger.Error(exception, (string)message);
        }

        public Task ErrorAsync(object message, Exception exception = null, CancellationToken cancellationToken = default)
        {
            _logger.Error(exception, (string)message);
            return TaskCompatible.CompletedTask;
        }

        public void Fatal(object message, Exception exception = null)
        {
            _logger.Fatal(exception, (string)message);
        }

        public Task FatalAsync(object message, Exception exception = null, CancellationToken cancellationToken = default)
        {
            _logger.Fatal(exception, (string)message);
            return TaskCompatible.CompletedTask;
        }

        public void Info(object message, Exception exception = null)
        {
            _logger.Information(exception, (string)message);
        }

        public Task InfoAsync(object message, Exception exception = null, CancellationToken cancellationToken = default)
        {
            _logger.Information(exception, (string)message);
            return TaskCompatible.CompletedTask;
        }

        public void Warn(object message, Exception exception = null)
        {
            _logger.Warning(exception, (string)message);
        }

        public Task WarnAsync(object message, Exception exception = null, CancellationToken cancellationToken = default)
        {
            _logger.Warning(exception, (string)message);
            return TaskCompatible.CompletedTask;
        }
    }
}
