using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fireasy.Common.Caching;
using System.Linq;
using System.Threading;

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
                //mem.Capacity = 1000;
            }

            for (var i = 0; i < 100; i++)
            {
                cache.Add("a" + i, i);
            }

            Assert.AreEqual(cache.Get("a0"), 0);

            for (var i = 0; i < 200; i++)
            {
                cache.Add("b" + i, i);
            }

            Assert.AreEqual(cache.Get("a88"), 88);
            Assert.AreEqual(cache.Get("a2"), 2);
            Assert.AreEqual(cache.Get("a2"), 2);

            for (var i = 0; i < 99; i++)
            {
                cache.Add("b" + i, i);
            }

            Thread.Sleep(6000);

            //Assert.AreEqual(cache.Get("a2"), 2);
            //Assert.IsFalse(cache.Contains("a0"));
            //Assert.IsFalse(cache.Contains("a5"));
        }

        public class PersonCache
        {
            public string Name { get; set; }
        }

        [TestMethod]
        public void TestAddWithRemovedCallback()
        {
            var manager = CacheManagerFactory.CreateManager();

            //10分钟之后过期
            manager.Add("testKey1", new PersonCache { Name = "fireasy" }, TimeSpan.FromMinutes(10), (key, value) =>
            {
                Console.WriteLine($"{key} 已被移除。");
            });
        }

        [TestMethod]
        public void TestInitializeHashSet()
        {
            var manager = CacheManagerFactory.CreateManager();

            var hashSet = manager.GetHashSet<string, PersonCache>("personSet", () =>
            {
                return new[]
                {
                    Tuple.Create("01", new PersonCache { Name = "fireasy01" }, NeverExpired.Instance),
                    Tuple.Create("02", new PersonCache { Name = "fireasy02" }, NeverExpired.Instance),
                    Tuple.Create("03", new PersonCache { Name = "fireasy03" }, NeverExpired.Instance),
                    Tuple.Create("04", new PersonCache { Name = "fireasy04" }, NeverExpired.Instance)
                };
            });

            if (hashSet.TryGet("01", out PersonCache person))
            {
                Assert.AreEqual("fireasy01", person.Name);
            }
        }
    }
}
