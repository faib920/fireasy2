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

            subMgr.AddSubscriber<TestSubject>(s =>
            {
                Console.WriteLine("1:" + s.Key);
            });
            subMgr.AddSubscriber<TestSubject>(s =>
            {
                Console.WriteLine("2:" + s.Key);
            });
            subMgr.AddSubscriber(typeof(TestSubject).FullName, (s) =>
            {
                Console.WriteLine(Encoding.UTF8.GetString(s));
            });

            subMgr.Publish(new TestSubject { Key = "fireasy1" });
            subMgr.Publish(new TestSubject { Key = "fireasy2" });

            Thread.Sleep(2000);
        }

        public class TestSubject
        {
            public string Key { get; set; }
        }
    }


}
