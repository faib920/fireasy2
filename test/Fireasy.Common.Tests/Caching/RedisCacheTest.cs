using Fireasy.Common.Caching;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Common.Tests.Caching
{
    [TestClass]
    public class RedisCacheTest
    {
        public RedisCacheTest()
        {
            InitConfig.Init();
        }

        [TestMethod]
        public void TestTryGet()
        {
            var cacheMgr = CacheManagerFactory.CreateManager("redis");
            var value = cacheMgr.TryGet("test1", () => 100);
            Assert.AreEqual(100, value);
        }

        [TestMethod]
        public void TestContains()
        {
            var cacheMgr = CacheManagerFactory.CreateManager("redis");
            var value = cacheMgr.TryGet("test1", () => 100);
            Assert.AreEqual(true, cacheMgr.Contains("test1"));
            Assert.AreEqual(false, cacheMgr.Contains("test2"));
        }

        [TestMethod]
        public void TestExpireTimes()
        {
            var cacheMgr = CacheManagerFactory.CreateManager("redis");
            var value = cacheMgr.TryGet("test7", () => 100, () => new RelativeTime(TimeSpan.FromSeconds(10)));
            Console.WriteLine(cacheMgr.GetExpirationTime("test7"));
        }

        [TestMethod]
        public void TestExpired()
        {
            var cacheMgr = CacheManagerFactory.CreateManager("redis");
            var value = cacheMgr.TryGet("test3", () => 100, () => new RelativeTime(TimeSpan.FromSeconds(2)));
            Assert.AreEqual(true, cacheMgr.Contains("test3"));
            Thread.Sleep(3000);
            Assert.AreEqual(false, cacheMgr.Contains("test3"));
        }

        [TestMethod]
        public void TestClear()
        {
            var cacheMgr = CacheManagerFactory.CreateManager("redis");
            cacheMgr.Add("test4", 100);
            cacheMgr.Add("test5", 100);
            cacheMgr.Add("test6", 100);

            cacheMgr.Clear();

            Assert.AreEqual(false, cacheMgr.Contains("test4"));
            Assert.AreEqual(false, cacheMgr.Contains("test5"));
            Assert.AreEqual(false, cacheMgr.Contains("test6"));
        }

        [TestMethod]
        public void TestParallel()
        {
            var cacheMgr = CacheManagerFactory.CreateManager("redis");

            int Get()
            {
                Thread.Sleep(100);
                Console.WriteLine("get");
                return 100;
            }

            Parallel.For(0, 5, (i, s) => Console.WriteLine(cacheMgr.TryGet("test", () => Get(), () => new RelativeTime(TimeSpan.FromSeconds(5)))));
        }

        [TestMethod]
        public void TestAdvanceDelay()
        {
            var cacheMgr = CacheManagerFactory.CreateManager("redis");
            var value = cacheMgr.TryGet("test1", () => 100, () => new RelativeTime(TimeSpan.FromSeconds(1)));
            Thread.Sleep(1100);
            value = cacheMgr.TryGet("test1", () => 100, () => new RelativeTime(TimeSpan.FromSeconds(1)));
            Thread.Sleep(720);
            value = cacheMgr.TryGet("test1", () => 100, () => new RelativeTime(TimeSpan.FromSeconds(1)));
        }

        [TestMethod]
        public void TestIncrement()
        {
            Parallel.For(0, 10, i =>
                {
                    var cacheMgr = CacheManagerFactory.CreateManager("redis") as IDistributedCacheManager;
                    var inc = cacheMgr.TryIncrement("inc1", () => 0);
                    Console.WriteLine(inc);
                });
        }

        [TestMethod]
        public void TestDecrement()
        {
            Parallel.For(0, 10, i =>
                {
                    var cacheMgr = CacheManagerFactory.CreateManager("redis") as IDistributedCacheManager;
                    var dec = cacheMgr.TryDecrement("dec1", () => 100);
                    Console.WriteLine(dec);
                });
        }
    }
}
