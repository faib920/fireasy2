using Fireasy.Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fireasy.Common.Tests.Logging
{
    [TestClass]
    public class Log4netTests
    {
        public Log4netTests()
        {
            InitConfig.Init();
        }

        [TestMethod]
        public void TestInfo()
        {
            var log = LoggerFactory.CreateLogger("log4net");
            log.Info("fireasy output.");
        }

        [TestMethod]
        public void TestError()
        {
            var log = LoggerFactory.CreateLogger("log4net");
            log.Error("fireasy output.", new System.IO.FileNotFoundException("file not found.", "c:\\a.txt"));
        }
    }
}
