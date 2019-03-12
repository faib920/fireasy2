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

namespace Fireasy.Common.Logging
{
    /// <summary>
    /// 日志级别。
    /// </summary>
    [Flags]
    public enum LogLevel
    {
        Default = 0,
        Info = 1,
        Debug = 2,
        Warn = 4,
        Error = 8,
        Fatal = 16
    }

    public static class LogEnvironment
    {
        static LogLevel _level = LogLevel.Default;

        static LogEnvironment()
        {
            var section = ConfigurationUnity.GetSection<LoggingConfigurationSection>();
            if (section != null)
            {
                _level = section.Level;
            }
        }

        public static LogLevel Level { get; } = _level;

        public static bool IsConfigured(LogLevel level)
        {
            return _level == LogLevel.Default || _level.HasFlag(level);
        }
    }

}
