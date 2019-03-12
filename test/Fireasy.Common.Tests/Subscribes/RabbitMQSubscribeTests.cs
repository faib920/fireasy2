using Fireasy.Common.Subscribes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Threading;

namespace Fireasy.Common.Tests.Subscribes
{
    [TestClass]
    public class RabbitMQSubscribeTests
    {
        public RabbitMQSubscribeTests()
        {
            InitConfig.Init();
        }

        [TestMethod]
        public void Test()
        {
            var subMgr = SubscribeManagerFactory.CreateManager("rabbit");
            var r = new Random();
            subMgr.AddSubscriber<TestSubject>(s =>
            {
                //throw new Exception();
                Thread.Sleep(r.Next(0, 1000));
                Console.WriteLine("1:" + s.Key);
            });
            subMgr.AddSubscriber<TestSubject>(s =>
            {
                Thread.Sleep(r.Next(0, 1000));
                Console.WriteLine("2:" + s.Key);
            });

            subMgr.Publish(new TestSubject { Key = "fireasy1" });
            subMgr.Publish(new TestSubject { Key = "fireasy2" });
            subMgr.Publish(new TestSubject { Key = "fireasy3" });
            subMgr.Publish(new TestSubject { Key = "fireasy4" });
            subMgr.Publish(new TestSubject { Key = "fireasy5" });
            subMgr.Publish(new TestSubject { Key = "fireasy6" });
            subMgr.Publish(new TestSubject { Key = "fireasy7" });
            subMgr.Publish(new TestSubject { Key = "fireasy8" });

            Thread.Sleep(5000);
        }

        public class TestSubject
        {
            public string Key { get; set; }
        }

        public class TestSubject1
        {
            public string Key { get; set; }
        }
    }


}
