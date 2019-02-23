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
        [TestMethod]
        public void TestParallel()
        {
            Parallel.For(0, 1000, i => LoggerFactory.CreateLogger().Info("dfafafdadf"));
        }
    }
}
