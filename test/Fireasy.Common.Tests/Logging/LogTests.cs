using Fireasy.Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
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
            a.Info("dddd");
        }
    }
}
