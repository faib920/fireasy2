// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Configuration;
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
        static LogLevel? _level = null;

        public static LogLevel Level
        {
            get
            {
                if (_level == null)
                {
                    var section = ConfigurationUnity.GetSection<LoggingConfigurationSection>();
                    if (section != null)
                    {
                        _level = section.Level;
                    }
                    else
                    {
                        _level = LogLevel.Default;
                    }
                }

                return _level.Value;
            }
        }

        public static bool IsConfigured(LogLevel level)
        {
            return Level == LogLevel.Default || Level.HasFlag(level);
        }

        public static LogLevel GetLevel(string levels)
        {
            var level = LogLevel.Default;

            if (string.IsNullOrEmpty(levels))
            {
                return level;
            }

            foreach (var segment in levels.Split(new[] { '|', ';', ',' }))
            {
                if (segment.Equals("info", StringComparison.InvariantCultureIgnoreCase))
                {
                    level |= LogLevel.Info;
                }
                else if (segment.Equals("error", StringComparison.InvariantCultureIgnoreCase))
                {
                    level |= LogLevel.Error;
                }
                else if (segment.Equals("debug", StringComparison.InvariantCultureIgnoreCase))
                {
                    level |= LogLevel.Debug;
                }
                else if (segment.Equals("warn", StringComparison.InvariantCultureIgnoreCase))
                {
                    level |= LogLevel.Warn;
                }
                else if (segment.Equals("warn", StringComparison.InvariantCultureIgnoreCase))
                {
                    level |= LogLevel.Fatal;
                }
            }

            return level;
        }
    }
}
