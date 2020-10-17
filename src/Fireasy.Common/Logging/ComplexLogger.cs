﻿// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
using Fireasy.Common.Logging.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Common.Logging
{
    /// <summary>
    /// 复合的日志记录器，可以将信息分发到不同的记录器中。
    /// </summary>
    [ConfigurationSetting(typeof(ComplexLoggingSetting))]
    public class ComplexLogger : ILogger, IConfigurationSettingHostService
    {
        private ComplexLoggingSetting _setting;
        private readonly List<ComplexLoggerPair> _logPairs = new List<ComplexLoggerPair>();

        public ILogger GetLogger<T>() where T : class
        {
            throw new NotImplementedException();
        }

        void ILogger.Debug(object message, Exception exception)
        {
            _logPairs.Where(s => IsConfigured(s.Level, LogLevel.Debug)).ForEach(s => s.Logger.Debug(message, exception));
        }

        Task ILogger.DebugAsync(object message, Exception exception, CancellationToken cancellationToken)
        {
            foreach (var log in _logPairs.Where(s => IsConfigured(s.Level, LogLevel.Debug)))
            {
                log.Logger.DebugAsync(message, exception, cancellationToken);
            }

            return null;
        }

        void ILogger.Error(object message, Exception exception)
        {
            _logPairs.Where(s => IsConfigured(s.Level, LogLevel.Error)).ForEach(s => s.Logger.Error(message, exception));
        }

        Task ILogger.ErrorAsync(object message, Exception exception, CancellationToken cancellationToken)
        {
            foreach (var log in _logPairs.Where(s => IsConfigured(s.Level, LogLevel.Error)))
            {
                log.Logger.DebugAsync(message, exception, cancellationToken);
            }

            return null;
        }

        void ILogger.Fatal(object message, Exception exception)
        {
            _logPairs.Where(s => IsConfigured(s.Level, LogLevel.Fatal)).ForEach(s => s.Logger.Fatal(message, exception));
        }

        Task ILogger.FatalAsync(object message, Exception exception, CancellationToken cancellationToken)
        {
            foreach (var log in _logPairs.Where(s => IsConfigured(s.Level, LogLevel.Fatal)))
            {
                log.Logger.DebugAsync(message, exception, cancellationToken);
            }

            return null;
        }

        void ILogger.Info(object message, Exception exception)
        {
            _logPairs.Where(s => IsConfigured(s.Level, LogLevel.Info)).ForEach(s => s.Logger.Info(message, exception));
        }

        Task ILogger.InfoAsync(object message, Exception exception, CancellationToken cancellationToken)
        {
            foreach (var log in _logPairs.Where(s => IsConfigured(s.Level, LogLevel.Info)))
            {
                log.Logger.DebugAsync(message, exception, cancellationToken);
            }

            return null;
        }

        void ILogger.Warn(object message, Exception exception)
        {
            _logPairs.Where(s => IsConfigured(s.Level, LogLevel.Warn)).ForEach(s => s.Logger.Info(message, exception));
        }

        Task ILogger.WarnAsync(object message, Exception exception, CancellationToken cancellationToken)
        {
            foreach (var log in _logPairs.Where(s => IsConfigured(s.Level, LogLevel.Warn)))
            {
                log.Logger.DebugAsync(message, exception, cancellationToken);
            }

            return null;
        }

        ILogger<T> ILogger.Create<T>() where T : class
        {
            throw new NotImplementedException();
        }

        private bool IsConfigured(LogLevel current, LogLevel standard)
        {
            if (current == LogLevel.Default)
            {
                return LogEnvironment.IsConfigured(standard);
            }

            return current.HasFlag(standard);
        }

        void IConfigurationSettingHostService.Attach(IConfigurationSettingItem setting)
        {
            _setting = (ComplexLoggingSetting)setting;
            foreach (var item in _setting.Pairs)
            {
                _logPairs.Add(new ComplexLoggerPair { Level = item.Level, Logger = item.LogType.New<ILogger>() });
            }
        }

        IConfigurationSettingItem IConfigurationSettingHostService.GetSetting()
        {
            return _setting;
        }
    }
}
