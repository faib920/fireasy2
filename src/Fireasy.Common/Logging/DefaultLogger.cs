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
using System.IO;
using System.Text;
#if NET35
using Fireasy.Common.Extensions;
#endif

namespace Fireasy.Common.Logging
{
    /// <summary>
    /// 默认的日志管理器，将日志记录到文本文件中。
    /// </summary>
    public sealed class DefaultLogger : ILogger
    {
        private static readonly string logFilePath;
        private static readonly ReadWriteLocker locker = new ReadWriteLocker();
        private LogLevel level;

        /// <summary>
        /// 获取 <see cref="DefaultLogger"/> 的静态实例。
        /// </summary>
        public static readonly DefaultLogger Instance = new DefaultLogger();

        /// <summary>
        /// 初始化 <see cref="DefaultLogger"/> 类的新实例。
        /// </summary>
        static DefaultLogger()
        {
            logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log");
        }

        private DefaultLogger()
        {
            var section = ConfigurationUnity.GetSection<LoggingConfigurationSection>();
            level = section == null ? LogLevel.Default : section.Level;
        }

        /// <summary>
        /// 记录错误信息到日志。
        /// </summary>
        /// <param name="message">要记录的信息。</param>
        /// <param name="exception">异常对象。</param>
        public void Error(string message, Exception exception = null)
        {
            if (level == LogLevel.Default || level.HasFlag(LogLevel.Error))
            {
                Write("error", message, exception);
            }
        }

        /// <summary>
        /// 记录一般的信息到日志。
        /// </summary>
        /// <param name="message">要记录的信息。</param>
        /// <param name="exception">异常对象。</param>
        public void Info(string message, Exception exception = null)
        {
            if (level == LogLevel.Default || level.HasFlag(LogLevel.Info))
            {
                Write("info", message, exception);
            }
        }

        /// <summary>
        /// 记录警告信息到日志。
        /// </summary>
        /// <param name="message">要记录的信息。</param>
        /// <param name="exception">异常对象。</param>
        public void Warn(string message, Exception exception = null)
        {
            if (level == LogLevel.Default || level.HasFlag(LogLevel.Warn))
            {
                Write("warn", message, exception);
            }
        }

        /// <summary>
        /// 记录调试信息到日志。
        /// </summary>
        /// <param name="message">要记录的信息。</param>
        /// <param name="exception">异常对象。</param>
        public void Debug(string message, Exception exception = null)
        {
            if (level == LogLevel.Default || level.HasFlag(LogLevel.Debug))
            {
                Write("debug", message, exception);
            }
        }

        /// <summary>
        /// 记录致命信息到日志。
        /// </summary>
        /// <param name="message">要记录的信息。</param>
        /// <param name="exception">异常对象。</param>
        public void Fatal(string message, Exception exception = null)
        {
            if (level == LogLevel.Default || level.HasFlag(LogLevel.Fatal))
            {
                Write("fatal", message, exception);
            }
        }

        /// <summary>
        /// 将抛出的异常写入到日志记录器。
        /// </summary>
        /// <param name="logType">信息类别。</param>
        /// <param name="message">异常的说明信息。</param>
        /// <param name="exception">应用程序异常。</param>
        private void Write(string logType, string message, Exception exception)
        {
            locker.LockWrite(() =>
                {
                    var fileName = CreateLogFileName(logType);
                    using (var writer = new StreamWriter(fileName, true, Encoding.GetEncoding(0)))
                    {
                        writer.WriteLine("Time: " + DateTime.Now);
                        writer.WriteLine();
                        if (message != null)
                        {
                            writer.WriteLine(message);
                        }

                        writer.WriteLine();

                        if (exception != null)
                        {
                            writer.WriteLine("--Exceptions--");

#if !NET35
                            var aggExp = exception as AggregateException;
                            if (aggExp != null && aggExp.InnerExceptions.Count > 0)
                            {
                                foreach (var e in aggExp.InnerExceptions)
                                {
                                    WriteException(writer, e);
                                }
                            }
                            else
                            {
                                WriteException(writer, exception);
                            }
#else
                            WriteException(writer, exception);
#endif
                        }

                        writer.WriteLine("*****************************************************************");
                    }
                });
        }

        private string CreateLogFileName(string logType)
        {
            var path = Path.Combine(logFilePath, logType);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return Path.Combine(path, DateTime.Today.ToString("yyyy-MM-dd") + ".log");
        }

        private static void WriteException(StreamWriter writer, Exception exception)
        {
            var e = exception;
            var ident = 0;
            while (e != null)
            {
                var prefix = new string(' ', (ident++) * 2);
                writer.WriteLine(prefix + e.GetType().Name + " => " + e.Message);
                
                if (e.StackTrace != null)
                {
                    writer.WriteLine();
                    writer.WriteLine("----Begin StackTrack----");
                    writer.WriteLine(e.StackTrace);
                    writer.WriteLine("----End StackTrack----");
                }

                e = e.InnerException;
            }
        }
    }
}
