using Fireasy.Common.Caching;
using Fireasy.Common.Ioc;
using Fireasy.Common.Subscribes;
using Fireasy.Common.Subscribes.Persistance;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Common.Tests.Subscribes
{
    [TestClass]
    public class RedisSubscribeTests
    {
        public RedisSubscribeTests()
        {
            InitConfig.Init();
        }

        public class TopicNameNormalizer : Common.Subscribes.ITopicNameNormalizer
        {
            public string NormalizeName(string topicName)
            {
                return "tt_" + topicName;
            }
        }

        [TestMethod]
        public void Test()
        {
            ContainerUnity.GetContainer().RegisterSingleton<ITopicNameNormalizer, TopicNameNormalizer>();
            var subMgr = SubscribeManagerFactory.CreateManager("redis");
            var cacheMgr = CacheManagerFactory.CreateManager("redis");
            var r = new Random();

            subMgr.AddSubscriber<TestSubject>("a", s =>
            {
                Console.WriteLine("1:--" + s.Key);
            });
            subMgr.AddSubscriber<TestSubject>(s =>
            {
                if (r.Next(10) < 5)
                {
                    Console.WriteLine(111);
                    //throw new Exception();
                }
                Console.WriteLine("2:--" + s.Key);
            });

            subMgr.Publish(new TestSubject { Key = "fireasy1" });
            subMgr.Publish(new TestSubject { Key = "fireasy2" });

            //subMgr.RemoveSubscriber<TestSubject>();

            subMgr.Publish("a", new TestSubject { Key = "new fireasy1" });
            subMgr.Publish(new TestSubject { Key = "new fireasy2" });

            Thread.Sleep(15000);
        }

        [TestMethod]
        public void TestAsync()
        {
            var subMgr = SubscribeManagerFactory.CreateManager("redis");
            var cacheMgr = CacheManagerFactory.CreateManager("redis");

            subMgr.AddAsyncSubscriber<TestSubject>("a", async s =>
            {
                Console.WriteLine("1:--" + s.Key);
                await Task.Run(() => { });
            });
            subMgr.AddAsyncSubscriber<TestSubject>(async s =>
            {
                //throw new Exception();
                Console.WriteLine("2:--" + s.Key);
                await Task.Run(() => { });
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

            subMgr.AddSubscribers(this.GetType().Assembly);
            subMgr.AddSubscriber<TestSubject>(new SubjectSubscriber1());
            subMgr.Publish(new TestSubject { Key = "fireasy1" });

            Thread.Sleep(5000);
        }

        [TestMethod]
        public void TestPersistent()
        {
            var message = Encoding.UTF8.GetBytes("{ \"content\": \"fireasy\" }");
            ISubjectPersistance persistent = new LocalFilePersistance();
            persistent.SaveSubject("test", new StoredSubject("test", message));
            persistent.ReadSubjects("test", o =>
            {
                Console.WriteLine(o.Body);
                return false;
            });
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

        public class SubjectSubscribeHandler : ISubscribeHandler
        {
            public async Task Test(TestSubject subject)
            {
                Console.WriteLine($"subscriber handler1: {subject.Key}");
            }

            public void Test11(TestSubject subject)
            {
                Console.WriteLine($"subscriber handler2: {subject.Key}");
            }
        }
    }
}


