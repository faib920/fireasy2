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
using Fireasy.Common;
using Fireasy.Data.Extensions;

namespace Fireasy.Data
{
    /// <summary>
    /// 默认的命令执行跟踪器。使用 __dbtrack 目录中的文本文件来记录日志。
    /// </summary>
    internal class DefaultCommandTracker : ICommandTracker, IDisposable
    {
        private readonly ReadWriteLocker locker = new ReadWriteLocker();
        private string logPath;

        internal static ICommandTracker Instance = new DefaultCommandTracker();

        void ICommandTracker.Write(IDbCommand command, TimeSpan period)
        {
            CreateDirectory();

            locker.LockWrite(() =>
                {
                    var fileName = Path.Combine(logPath, DateTime.Today.ToString("yyyy-MM-dd") + ".log");
                    using (var stream = new StreamWriter(fileName, true))
                    {
                        stream.WriteLine("time: " + DateTime.Now);
                        stream.WriteLine("sql: " + command.Output());
                        stream.WriteLine("timer(s): " + period);
                        stream.WriteLine("===========================================================");
                        stream.WriteLine();
                        stream.Flush();
                    }
                });
        }

        void ICommandTracker.Fail(IDbCommand command, Exception exception)
        {
            CreateDirectory();

            locker.LockWrite(() =>
                {
                    var fileName = Path.Combine(logPath, DateTime.Today.ToString("yyyy-MM-dd") + ".error.log");
                    using (var stream = new StreamWriter(fileName, true))
                    {
                        stream.WriteLine("time: " + DateTime.Now);
                        stream.WriteLine("sql: " + command.Output());
                        stream.WriteLine("error: " + exception.Message);
                        stream.WriteLine("===========================================================");
                        stream.WriteLine();
                        stream.Flush();
                    }
                });
        }

        private void CreateDirectory()
        {
            logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "__dbtrack");
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }
        }

        public void Dispose()
        {
            if (locker != null)
            {
                locker.Dispose();
            }
        }
    }
}
