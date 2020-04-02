using Fireasy.Common.ComponentModel;
using Fireasy.Common.Extensions;
using Fireasy.Common.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
    }
}
