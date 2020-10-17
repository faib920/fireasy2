﻿// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Subscribes;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Common.Logging
{
    /// <summary>
    /// 默认的日志管理器，将日志记录到文本文件中。
    /// </summary>
    public class DefaultLogger : ILogger
    {
        protected static readonly string _logFilePath;
        private static readonly ISubscribeManager _subscribeMgr = DefaultSubscribeManager.Instance;

        /// <summary>
        /// 获取 <see cref="DefaultLogger"/> 的静态实例。
        /// </summary>
        public static readonly DefaultLogger Instance = new DefaultLogger();

        /// <summary>
        /// 初始化 <see cref="DefaultLogger"/> 类的新实例。
        /// </summary>
        static DefaultLogger()
        {
            _logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log");
            _subscribeMgr.AddSubscriber<DefaultLoggerSubject>(s =>
               {
                   using var writer = new StreamWriter(s.FileName, true, Encoding.Default);
                   writer.WriteLine(s.Content);
               });
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
                Write("error", message, exception);
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
                await WriteAsync("error", message, exception, cancellationToken);
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
                Write("info", message, exception);
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
                await WriteAsync("info", message, exception, cancellationToken);
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
                Write("warn", message, exception);
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
                await WriteAsync("warn", message, exception, cancellationToken);
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
                Write("debug", message, exception);
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
                await WriteAsync("debug", message, exception, cancellationToken);
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
                Write("fatal", message, exception);
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
                await WriteAsync("fatal", message, exception, cancellationToken);
            }
        }

        /// <summary>
        /// 创建一个泛型的日志管理器。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ILogger<T> Create<T>() where T : class
        {
            return DefaultLogger<T>.Instance;
        }

        protected virtual string CreateLogFileName(string logType)
        {
            var path = Path.Combine(_logFilePath, logType);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return Path.Combine(path, DateTime.Today.ToString("yyyy-MM-dd") + ".log");
        }

        /// <summary>
        /// 将抛出的异常写入到日志记录器。
        /// </summary>
        /// <param name="logType">信息类别。</param>
        /// <param name="message">异常的说明信息。</param>
        /// <param name="exception">应用程序异常。</param>
        private void Write(string logType, object message, Exception exception)
        {
            var content = GetLogContent(message, exception);
            var fileName = CreateLogFileName(logType);

            _subscribeMgr.Publish(new DefaultLoggerSubject { FileName = fileName, Content = content });
        }

        /// <summary>
        /// 异步的，将抛出的异常写入到日志记录器。
        /// </summary>
        /// <param name="logType">信息类别。</param>
        /// <param name="message">异常的说明信息。</param>
        /// <param name="exception">应用程序异常。</param>
        private async Task WriteAsync(string logType, object message, Exception exception, CancellationToken cancellationToken = default)
        {
            var content = GetLogContent(message, exception);
            var fileName = CreateLogFileName(logType);

            await _subscribeMgr.PublishAsync(new DefaultLoggerSubject { FileName = fileName, Content = content }, cancellationToken);
        }

        private string GetLogContent(object message, Exception exception)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Time: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"));
            sb.AppendLine();
            if (message != null)
            {
                sb.AppendLine(message.ToString());
            }

            sb.AppendLine();

            if (exception != null)
            {
                sb.AppendLine("--Exceptions--");

                if (exception is AggregateException aggExp)
                {
                    foreach (var e in aggExp.InnerExceptions)
                    {
                        RecursiveWriteException(sb, e);
                    }
                }
                else if (exception is ReflectionTypeLoadException loadExp)
                {
                    foreach (var e in loadExp.LoaderExceptions)
                    {
                        RecursiveWriteException(sb, e);
                    }
                }
                else
                {
                    RecursiveWriteException(sb, exception);
                }
            }

            sb.AppendLine("*****************************************************************");

            return sb.ToString();
        }

        private static void RecursiveWriteException(StringBuilder builder, Exception exception)
        {
            var curExp = exception;
            var ident = 0;
            while (curExp != null)
            {
                var prefix = new string(' ', (ident++) * 2);
                builder.AppendLine(string.Concat(prefix, curExp.GetType().Name, " => ", curExp.Message));

                if (curExp.StackTrace != null)
                {
                    builder.AppendLine();
                    builder.AppendLine(string.Concat(prefix, "----Begin StackTrack----"));
                    builder.AppendLine(string.Concat(prefix, curExp.StackTrace));
                    builder.AppendLine(string.Concat(prefix, "----End StackTrack----"));
                }

                curExp = curExp.InnerException;
            }
        }

        private class DefaultLoggerSubject
        {
            public string FileName { get; set; }

            public string Content { get; set; }
        }
    }

    /// <summary>
    /// 默认的日志管理器，将日志记录到文本文件中。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DefaultLogger<T> : DefaultLogger, ILogger<T> where T : class
    {
        /// <summary>
        /// 获取 <see cref="DefaultLogger{T}"/> 的静态实例。
        /// </summary>
        public static readonly DefaultLogger<T> Instance = new DefaultLogger<T>();
    }
}
