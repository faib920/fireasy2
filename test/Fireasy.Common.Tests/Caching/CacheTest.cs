using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fireasy.Common.Caching;
using System.Linq;

namespace Fireasy.Common.Tests
{
    /// <summary>
    /// CacheTest 的摘要说明
    /// </summary>
    [TestClass]
    public class CacheTest
    {
        [TestMethod]
        public void TestHit()
        {
            var cache = CacheManagerFactory.CreateManager();

            if (cache is MemoryCacheManager mem)
            {
                mem.Capacity = 1000;
            }

            for (var i = 0; i < 999; i++)
            {
                cache.Add("a" + i, i);
            }

            Assert.AreEqual(cache.Get("a88"), 88);
            Assert.AreEqual(cache.Get("a2"), 2);
            Assert.AreEqual(cache.Get("a2"), 2);

            for (var i = 0; i < 99; i++)
            {
                cache.Add("b" + i, i);
            }

            Assert.AreEqual(cache.Get("a2"), 2);
            Assert.IsFalse(cache.Contains("a0"));
            Assert.IsFalse(cache.Contains("a5"));
        }
    }
}
