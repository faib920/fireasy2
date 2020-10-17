using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Common.Tests
{
    [TestClass]
    public class ActionQueueTest
    {
        private string str = "ddd";

        [TestMethod]
        public void TestException()
        {
            TT();
            Console.WriteLine(22222);
        }

        private async Task<int> TT()
        {
            Thread.Sleep(1000);
            Console.WriteLine(111111);
            return await Task.Run(() =>
            {
                return 1;
            });
        }
    }
}
