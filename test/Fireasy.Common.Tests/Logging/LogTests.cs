using Fireasy.Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
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
        public async Task TestParallel()
        {
            Parallel.For(0, 5, async s =>
            {
                await DefaultLogger.Instance.InfoAsync("111----------");
            });
        }
    }
}
