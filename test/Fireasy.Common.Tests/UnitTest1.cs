using Fireasy.Common.ComponentModel;
using Fireasy.Common.Extensions;
using Fireasy.Common.Localization;
using Fireasy.Common.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Fireasy.Common.Tests
{
    [TestClass]
    public class UnitTest1
    {
        static ConcurrentDictionary<string, int> dict = new ConcurrentDictionary<string, int>();
        static SafetyDictionary<string, int> sdict = new SafetyDictionary<string, int>();

        private Dictionary<string, string> dictionary = new Dictionary<string, string>();
        private Person _person = null;
        private object _locker = new object();

        public class Person
        {
            public Person()
            {
                Console.WriteLine(DateTime.Now);
            }
        }


        [TestMethod]
        public void TestSingletonLock()
        {
            Parallel.For(0, 5, i =>
            {
                _person = SingletonLocker.Lock(ref _person, _locker, () => new Person());
            });
        }

        [TestMethod]
        public void TestLockRead()
        {
            var locker = new ReadWriteLocker();
            Parallel.For(0, 5, i =>
            {
                locker.LockWrite(() =>
                {
                    dictionary.Add("fireasy", "true");
                });

                Console.WriteLine(dictionary.Count);
            });
        }

        [TestMethod]
        public async Task TestAsyncLock()
        {
            var locker = new AsyncLocker();
            Parallel.For(0, 5, async i =>
            {
                using (locker.LockAsync())
                {
                    await AddKey();
                }

                Console.WriteLine(dictionary.Count);
            });
        }

        private async Task AddKey()
        {
            await Task.Run(() => dictionary.Add("fireasy", "true"));
        }

        [TestMethod]
        public async Task TestTimeWatcherAsync()
        {
            var time = await TimeWatcher.WatchAsync(CallOtherAsync);

            Console.WriteLine(time);
        }

        private async Task CallOtherAsync()
        {
            //比较耗时的业务处理方法
        }

        [TestMethod]
        public void TestWatchApart()
        {
            var times = TimeWatcher.WatchApart(CallOther1, CallOther2);

            foreach (var time in times)
            {
                Console.WriteLine(time);
            }
        }

        private void CallOther1()
        {
            //比较耗时的业务处理方法
        }

        private void CallOther2()
        {
            //比较耗时的业务处理方法
        }

        [TestMethod]
        public void Test11()
        {
            var rr = new XmlStringLocalizerManager();
            //rr.CultureInfo = CultureInfo.GetCultureInfo("en-US");
            var locc = rr.GetLocalizer("Resource", typeof(XmlStringLocalizerManager).Assembly);
            var tt = locc["OrderNotExists", "dfsfd"];
            Console.WriteLine(tt);
        }

        [TestMethod]
        public void Test()
        {
            var t1 = TimeWatcher.Watch(() =>
            {
                for (var i = 0; i < 10000; i++)
                {
                    Console.WriteLine($"{2222}fadfasdfasf{4454545}ffdafdf{33}ffdfad{44}");
                }
            });
            Console.WriteLine(t1);
            var t2 = TimeWatcher.Watch(() =>
            {
                for (var i = 0; i < 10000; i++)
                {
                    Console.WriteLine(string.Concat(2222, "fadfasdfasf", 4454545, "ffdafdf", 33, "ffdfad", 44));
                }
            });
            Console.WriteLine(t2);
            var t3 = TimeWatcher.Watch(() =>
            {
                var sb = new StringBuilder();
                for (var i = 0; i < 10000; i++)
                {
                    Console.WriteLine(string.Format("{0}fadfasdfasf{1}ffdafdf{2}ffdfad{3}", 2222, 4454545, 33, 44));
                }
            });
            Console.WriteLine(t3);
        }

        String CreateKey(int numBytes)
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] buff = new byte[numBytes];

            rng.GetBytes(buff);
            return BytesToHexString(buff);
        }

        String BytesToHexString(byte[] bytes)
        {
            StringBuilder hexString = new StringBuilder(64);

            for (int counter = 0; counter < bytes.Length; counter++)
            {
                hexString.Append(String.Format("{0:X2}", bytes[counter]));
            }
            return hexString.ToString();
        }

        [TestMethod]
        public async Task TestScope()
        {
            using (var scope = new AScope())
            {
                await Test1();
            }
        }

        private async Task<int> Test1()
        {
            Console.WriteLine(AScope.Current);
            return await Task.Run(Test2);
        }

        private int Test2()
        {
            Console.WriteLine(AScope.Current);
            return 1;
        }

        private class AScope : Scope<AScope>
        {

        }

        [TestMethod]
        public void Test1111()
        {
            Assert.IsFalse(typeof(string).IsPrimitive);
            Assert.IsFalse(typeof(AAF).IsPrimitive);
            Assert.IsTrue(typeof(int).IsPrimitive);
            Assert.IsFalse(typeof(DateTime).IsPrimitive);
        }

        public enum AAF
        {

        }
    }
}
