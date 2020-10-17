﻿using Fireasy.Common.Caching;
using Fireasy.Common.Subscribes;
using Fireasy.Common.Threading;
using Fireasy.Redis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
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
        public async Task TestTryGet()
        {
            var cacheMgr = CacheManagerFactory.CreateManager("redis");
            var value = await cacheMgr.TryGetAsync("test1", () => Task.FromResult(100));
            value = await cacheMgr.TryGetAsync("test2", () => Task.FromResult(100));
            value = await cacheMgr.TryGetAsync("test3", () => Task.FromResult(100));
            value = await cacheMgr.TryGetAsync("test4", () => Task.FromResult(100));
            Assert.AreEqual(100, value);
        }

        [TestMethod]
        public async Task TestStatus()
        {
            var cacheMgr = CacheManagerFactory.CreateManager("redis");

            var i = 0;
            while (i++ < 100)
            {
                try
                {
                    var value = await cacheMgr.TryGetAsync("test1", () => Task.FromResult(100));
                    Console.WriteLine(value);
                }
                catch
                {
                    Console.WriteLine("无法连接到redis");
                }

                Thread.Sleep(1000);
            }
        }

        [TestMethod]
        public async Task TestContains()
        {
            var cacheMgr = CacheManagerFactory.CreateManager("redis");
            var value = await cacheMgr.TryGetAsync("test1", () => Task.FromResult(100));
            var value1 = await cacheMgr.TryGetAsync("test3", () => Task.FromResult(100));
            Thread.Sleep(10000);
            var value2 = await cacheMgr.TryGetAsync("test4", () => Task.FromResult(100));
            //Assert.AreEqual(true, await cacheMgr.ContainsAsync("test1"));
            //Assert.AreEqual(false, await cacheMgr.ContainsAsync("test2"));

            //var keys = cacheMgr.GetKeys("test*");
            //Console.WriteLine(keys.Count());
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

            Parallel.For(0, 10, (i, s) => Console.WriteLine(cacheMgr.TryGet("test", () => Get(), () => new RelativeTime(TimeSpan.FromSeconds(5)))));
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
                    var cacheMgr = CacheManagerFactory.CreateManager("redis") as IEnhancedCacheManager;
                    var inc = cacheMgr.TryIncrement("inc1", () => 0);
                    Console.WriteLine(inc);
                });
        }

        [TestMethod]
        public void TestDecrement()
        {
            Parallel.For(0, 10, i =>
                {
                    var cacheMgr = CacheManagerFactory.CreateManager("redis") as IEnhancedCacheManager;
                    var dec = cacheMgr.TryDecrement("dec1", () => 100);
                    Console.WriteLine(dec);
                });
        }

        [TestMethod]
        public void TestString()
        {
            var cacheMgr = CacheManagerFactory.CreateManager("redis");
            var str = "fireasy";
            cacheMgr.Add("tt1", str);

            str = cacheMgr.Get<string>("tt1");
            Assert.AreEqual("fireasy", str);
        }

        [TestMethod]
        public void TestBytes()
        {
            var cacheMgr = CacheManagerFactory.CreateManager("redis");
            var test = new Test1 { A = new byte[] { 45, 33, 45, 23, 122, 178, 213 }, B = "fireasy" };
            cacheMgr.Add("tt2", test);

            test = cacheMgr.Get<Test1>("tt2");
            Assert.AreEqual("fireasy", test.B);
        }

        [TestMethod]
        public void TestHashset()
        {
            var cacheMgr = CacheManagerFactory.CreateManager("redis");
            var set = cacheMgr.GetHashSet<string, Test1>("bee");
            var a1 = set.TryGet("1", () => new Test1 { B = "aaaa1" });
            var a2 = set.TryGet("2", () => new Test1 { B = "bbbb1" });
        }

        public class Test1
        {
            public byte[] A { get; set; }

            public string B { get; set; }

            public string C { get; set; }
        }
    }
}
