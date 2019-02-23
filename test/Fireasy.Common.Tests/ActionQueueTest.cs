using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
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
            var ss = "dfasdfaf";
            var dd = 1;
            ActionQueue.Push(() =>
            {
                Console.WriteLine(str + ss + dd);
                throw new Exception("error");
            }, 2);

            Thread.Sleep(5000);
        }
    }
}
