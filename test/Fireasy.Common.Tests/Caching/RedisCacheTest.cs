using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fireasy.Common.Caching;
using System.Threading;

namespace Fireasy.Common.Tests.Caching
{
    [TestClass]
    public class RedisCacheTest
    {
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
    }
}
