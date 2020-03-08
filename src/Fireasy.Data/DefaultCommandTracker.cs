// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Subscribes;
using Fireasy.Data.Extensions;
using System;
using System.Data;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Data
{
    /// <summary>
    /// 默认的命令执行跟踪器。使用 __dbtrack 目录中的文本文件来记录日志。
    /// </summary>
    internal class DefaultCommandTracker : ICommandTracker
    {
        private string logPath;
        private readonly ISubscribeManager subscribeMgr = DefaultSubscribeManager.Instance;

        internal readonly static ICommandTracker Instance = new DefaultCommandTracker();

        protected DefaultCommandTracker()
        {
            subscribeMgr.AddSubscriber<CommandTrackerSubject>(s =>
                {
                    using var writer = new StreamWriter(s.FileName, true, Encoding.Default);
                    writer.WriteLine(s.Content);
                });
        }

        void ICommandTracker.Write(IDbCommand command, TimeSpan period)
        {
            CreateDirectory();

            var fileName = Path.Combine(logPath, DateTime.Today.ToString("yyyy-MM-dd") + ".log");
            var content = $@"time: {DateTime.Now}
sql: {command.Output()}
timer(s): {period}
===========================================================
";

            subscribeMgr.Publish(new CommandTrackerSubject { FileName = fileName, Content = content });
        }

        void ICommandTracker.Fail(IDbCommand command, Exception exception)
        {
            CreateDirectory();

            var fileName = Path.Combine(logPath, DateTime.Today.ToString("yyyy-MM-dd") + ".error.log");
            var content = $@"time: {DateTime.Now}
sql: {command.Output()}
error: {exception.Message}
===========================================================
";

            subscribeMgr.Publish(new CommandTrackerSubject { FileName = fileName, Content = content });
        }

        async Task ICommandTracker.WriteAsync(IDbCommand command, TimeSpan period, CancellationToken cancellationToken)
        {
            CreateDirectory();

            var fileName = Path.Combine(logPath, DateTime.Today.ToString("yyyy-MM-dd") + ".log");
            var content = $@"time: {DateTime.Now}
sql: {command.Output()}
timer(s): {period}
===========================================================
";

            await subscribeMgr.PublishAsync(new CommandTrackerSubject { FileName = fileName, Content = content });
        }

        async Task ICommandTracker.FailAsync(IDbCommand command, Exception exception, CancellationToken cancellationToken)
        {
            CreateDirectory();

            var fileName = Path.Combine(logPath, DateTime.Today.ToString("yyyy-MM-dd") + ".error.log");
            var content = $@"time: {DateTime.Now}
sql: {command.Output()}
error: {exception.Message}
===========================================================
";

            await subscribeMgr.PublishAsync(new CommandTrackerSubject { FileName = fileName, Content = content });
        }

        private void CreateDirectory()
        {
            logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "__dbtrack");
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }
        }

        private class CommandTrackerSubject
        {
            public string FileName { get; set; }

            public string Content { get; set; }
        }
    }
}
