using Fireasy.Common.Ioc;
using Fireasy.Common.Subscribes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Common.Tests.Subscribes
{
    [TestClass]
    public class RabbitMQSubscribeTests
    {
        public RabbitMQSubscribeTests()
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
            var subMgr = SubscribeManagerFactory.CreateManager("rabbit");
            var r = new Random();
            subMgr.AddSubscriber<TestSubject>(s =>
            {
                //throw new Exception();
                //Thread.Sleep(r.Next(0, 500));
                Console.WriteLine("1:" + s.Key);
                if (r.Next(10) < 5)
                {
                    //throw new Exception();
                }
            });
            //subMgr.AddSubscriber<TestSubject>("a", s =>
            //{
            //    //Thread.Sleep(r.Next(0, 500));
            //    Console.WriteLine("2:" + s.Key);
            //});

            subMgr.Publish(new TestSubject { Key = "fireasy1" });
            //subMgr.Publish(new TestSubject { Key = "fireasy2" });
            //subMgr.Publish(new TestSubject { Key = "fireasy3" });
            //subMgr.Publish(new TestSubject { Key = "fireasy4" });
            //subMgr.Publish(new TestSubject { Key = "fireasy5" });
            //subMgr.Publish(new TestSubject { Key = "fireasy6" });
            //subMgr.Publish("a", new TestSubject { Key = "fireasy7" });
            //subMgr.Publish("a", new TestSubject { Key = "fireasy8" });

            Thread.Sleep(5000);
        }

        [TestMethod]
        public void TestStatus()
        {
            Console.WriteLine(DateTime.Now);
            var subMgr = SubscribeManagerFactory.CreateManager("rabbit");
            subMgr.AddSubscriber<TestSubject>(s =>
            {
                Console.WriteLine("accept " + s.Key);
            });

            var i = 0;
            while (i++ < 40)
            {
                try
                {
                    subMgr.Publish(new TestSubject { Key = i.ToString() });
                    Console.WriteLine("send " + i);
                }
                catch
                {
                    Console.WriteLine("无法连接到rabbitmq");
                }

                Thread.Sleep(1000);
            }
        }

        [TestMethod]
        public void TestAsync()
        {
            var subMgr = SubscribeManagerFactory.CreateManager("rabbit");
            var r = new Random();
            subMgr.AddAsyncSubscriber<TestSubject>(async s =>
            {
                //throw new Exception();
                Thread.Sleep(r.Next(0, 500));
                Console.WriteLine("1:" + s.Key);
                if (r.Next(10) < 5)
                {
                    //throw new Exception();
                }
                await Task.Run(() => { });
            });
            subMgr.AddAsyncSubscriber<TestSubject>("a", async s =>
            {
                Thread.Sleep(r.Next(0, 500));
                Console.WriteLine("2:" + s.Key);
                await Task.Run(() => { });
            });

            subMgr.Publish(new TestSubject { Key = "fireasy1" });
            subMgr.Publish(new TestSubject { Key = "fireasy2" });
            subMgr.Publish(new TestSubject { Key = "fireasy3" });
            subMgr.Publish(new TestSubject { Key = "fireasy4" });
            subMgr.Publish(new TestSubject { Key = "fireasy5" });
            subMgr.Publish(new TestSubject { Key = "fireasy6" });
            subMgr.Publish("a", new TestSubject { Key = "fireasy7" });
            subMgr.Publish("a", new TestSubject { Key = "fireasy8" });

            Thread.Sleep(10000);
        }

        [Topic("Fireasy.Common.Tests.TestSubject")]
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
