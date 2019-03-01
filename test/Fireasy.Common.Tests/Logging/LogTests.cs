using Fireasy.Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fireasy.Common.Tests.Logging
{
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
            Parallel.For(0, 1, i => LoggerFactory.CreateLogger().Warn("dfafafdadf"));
        }
    }
}
