using Fireasy.Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fireasy.Common.Tests.Logging
{
    [TestClass]
    public class NLogTests
    {
        public NLogTests()
        {
            InitConfig.Init();
        }

        [TestMethod]
        public void TestInfo()
        {
            var log = LoggerFactory.CreateLogger("nlog");
            log.Info("fireasy output.");
        }

        [TestMethod]
        public void TestError()
        {
            var log = LoggerFactory.CreateLogger("nlog");
            log.Error("fireasy output.", new System.IO.FileNotFoundException("file not found.", "c:\\a.txt"));
        }

        [TestMethod]
        public void TestSubError()
        {
            var log = LoggerFactory.CreateLogger("nlog").GetLogger<NLogTests>();
            log.Error("fireasy output.", new System.IO.FileNotFoundException("file not found.", "c:\\a.txt"));
        }

    }
}
