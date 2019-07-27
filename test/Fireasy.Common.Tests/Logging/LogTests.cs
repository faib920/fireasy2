using Fireasy.Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Fireasy.Common.Tests.Logging
{
    public class TestLog : ILogger
    {
        public void Debug(object message, Exception exception = null)
        {
        }

        public void Error(object message, Exception exception = null)
        {
        }

        public void Fatal(object message, Exception exception = null)
        {
        }

        public void Info(object message, Exception exception = null)
        {
        }

        public void Warn(object message, Exception exception = null)
        {
        }
    }

    [TestClass]
    public class LogTests
    {
        public LogTests()
        {
            InitConfig.Init();
        }

        [TestMethod]
        public void TestParallel()
        {
            var a = LoggerFactory.CreateLogger();
            Parallel.For(1, 5, t =>
            {
                a.Info(t + "----------");
            });

            Console.WriteLine("end");
        }
    }
}
