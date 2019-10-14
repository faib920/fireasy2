// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Fireasy.Common.Threading;
using Fireasy.Data.Extensions;

namespace Fireasy.Data
{
    /// <summary>
    /// 默认的命令执行跟踪器。使用 __dbtrack 目录中的文本文件来记录日志。
    /// </summary>
    internal class DefaultCommandTracker : ICommandTracker
    {
        private readonly AsyncLocker asyncLocker = new AsyncLocker();
        private string logPath;

        internal static ICommandTracker Instance = new DefaultCommandTracker();

        async Task ICommandTracker.WriteAsync(IDbCommand command, TimeSpan period, CancellationToken cancellationToken)
        {
            CreateDirectory();

            using (var locker = await asyncLocker.LockAsync())
            {
                var fileName = Path.Combine(logPath, DateTime.Today.ToString("yyyy-MM-dd") + ".log");
                using (var stream = new StreamWriter(fileName, true))
                {
                    await stream.WriteLineAsync("time: " + DateTime.Now);
                    await stream.WriteLineAsync("sql: " + command.Output());
                    await stream.WriteLineAsync("timer(s): " + period);
                    await stream.WriteLineAsync("===========================================================");
                    await stream.WriteLineAsync();
                    await stream.FlushAsync();
                }
            }
        }

        async Task ICommandTracker.FailAsync(IDbCommand command, Exception exception, CancellationToken cancellationToken)
        {
            CreateDirectory();

            using (var locker = await asyncLocker.LockAsync())
            {
                var fileName = Path.Combine(logPath, DateTime.Today.ToString("yyyy-MM-dd") + ".error.log");
                using (var stream = new StreamWriter(fileName, true))
                {
                    await stream.WriteLineAsync("time: " + DateTime.Now);
                    await stream.WriteLineAsync("sql: " + command.Output());
                    await stream.WriteLineAsync("error: " + exception.Message);
                    await stream.WriteLineAsync("===========================================================");
                    await stream.WriteLineAsync();
                    await stream.FlushAsync();
                }
            }
        }

        private void CreateDirectory()
        {
            logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "__dbtrack");
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }
        }
    }
}
