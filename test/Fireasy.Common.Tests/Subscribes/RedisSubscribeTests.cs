using Fireasy.Common.Caching;
using Fireasy.Common.Subscribes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Threading;

namespace Fireasy.Common.Tests.Subscribes
{
    [TestClass]
    public class RedisSubscribeTests
    {
        public RedisSubscribeTests()
        {
            InitConfig.Init();
        }

        [TestMethod]
        public void Test()
        {
            var subMgr = SubscribeManagerFactory.CreateManager("redis");
            var cacheMgr = CacheManagerFactory.CreateManager("redis");

            subMgr.AddSubscriber<TestSubject>("a", s =>
            {
                Console.WriteLine("1:--" + s.Key);
            });
            subMgr.AddSubscriber<TestSubject>(s =>
            {
                //throw new Exception();
                Console.WriteLine("2:--" + s.Key);
            });

            subMgr.Publish(new TestSubject { Key = "fireasy1" });
            subMgr.Publish(new TestSubject { Key = "fireasy2" });

            //subMgr.RemoveSubscriber<TestSubject>();

            subMgr.Publish("a", new TestSubject { Key = "new fireasy1" });
            subMgr.Publish(new TestSubject { Key = "new fireasy2" });

            Thread.Sleep(5000);
        }

        [TestMethod]
        public void TestSubscriber()
        {
            var subMgr = SubscribeManagerFactory.CreateManager("redis");

            subMgr.Discovery<TestSubject>(this.GetType().Assembly);
            subMgr.AddSubscriber<TestSubject>(new SubjectSubscriber1());
            subMgr.Publish(new TestSubject { Key = "fireasy1" });

            Thread.Sleep(2000);
        }

        [Topic("test11")]
        public class TestSubject
        {
            public string Key { get; set; }
        }

        public class SubjectSubscriber1 : ISubscriber<TestSubject>
        {
            public void Accept(TestSubject subject)
            {
                Console.WriteLine($"subscriber1: {subject.Key}");
            }
        }

        public class SubjectSubscriber2 : ISubscriber<TestSubject>
        {
            public void Accept(TestSubject subject)
            {
                Console.WriteLine($"subscriber2: {subject.Key}");
            }
        }
    }
}


