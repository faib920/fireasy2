// -----------------------------------------------------------------------
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

namespace Fireasy.Common.Logging
{
    /// <summary>
    /// 复合的日志记录器，可以将信息分发到不同的记录器中。
    /// </summary>
    [ConfigurationSetting(typeof(ComplexLoggingSetting))]
    public class ComplexLogger : ILogger, IConfigurationSettingHostService
    {
        private ComplexLoggingSetting setting;
        private List<ComplexLoggerPair> logPairs = new List<ComplexLoggerPair>();

        void ILogger.Debug(object message, Exception exception)
        {
            logPairs.Where(s => IsConfigured(s.Level, LogLevel.Debug)).ForEach(s => s.Logger.Debug(message, exception));
        }

        void ILogger.Error(object message, Exception exception)
        {
            logPairs.Where(s => IsConfigured(s.Level, LogLevel.Error)).ForEach(s => s.Logger.Error(message, exception));
        }

        void ILogger.Fatal(object message, Exception exception)
        {
            logPairs.Where(s => IsConfigured(s.Level, LogLevel.Fatal)).ForEach(s => s.Logger.Fatal(message, exception));
        }

        void ILogger.Info(object message, Exception exception)
        {
            logPairs.Where(s => IsConfigured(s.Level, LogLevel.Info)).ForEach(s => s.Logger.Info(message, exception));
        }

        void ILogger.Warn(object message, Exception exception)
        {
            logPairs.Where(s => IsConfigured(s.Level, LogLevel.Warn)).ForEach(s => s.Logger.Warn(message, exception));
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
            this.setting = (ComplexLoggingSetting)setting;
            foreach (var item in this.setting.Pairs)
            {
                logPairs.Add(new ComplexLoggerPair { Level = item.Level, Logger = item.LogType.New<ILogger>() });
            }
        }

        IConfigurationSettingItem IConfigurationSettingHostService.GetSetting()
        {
            return setting;
        }
    }
}
