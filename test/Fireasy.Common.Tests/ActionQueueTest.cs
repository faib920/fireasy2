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
        [TestMethod]
        public void TestException()
        {
            ActionQueue.SetPeriod(TimeSpan.FromMilliseconds(500));
            ActionQueue.Push(() =>
            {
                Console.WriteLine("yes");
                throw new Exception("error");
            });
            ActionQueue.ExceptionHandler = (action, exp) =>
            {
                Console.WriteLine(exp.Message);
            };

            Thread.Sleep(5000);
        }
    }
}
