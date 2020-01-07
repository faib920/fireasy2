using Fireasy.Common.Extensions;
using Fireasy.Common.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Fireasy.Common.Tests
{
    public class AAA
    {
        private static AAA insta;

        public AAA()
        {
            Console.WriteLine("dfsafsadf");
        }

        public static AAA GetInstance()
        {
            insta = SingletonLocker.Lock(ref insta, () => new AAA());
            return insta;
        }

        public void Test()
        {

        }
    }
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Test()
        {
            var dic1 = new Dictionary<string, string>();
            var dic2 = new Dictionary<Type, string>();

            foreach (var assn in typeof(UnitTest1).Assembly.GetReferencedAssemblies().Distinct())
            {
                Console.WriteLine(assn);
                var ass = Assembly.Load(assn);
                foreach (var type in ass.GetTypes())
                {
                    dic1.Add(type.AssemblyQualifiedName, type.FullName);
                    dic2.Add(type, type.FullName);
                }
            }

            var k = dic2.Last().Key;

            var t = TimeWatcher.Watch(() =>
            {
                dic1[k.AssemblyQualifiedName] = "dd";
            });
            Console.WriteLine(t);
            t = TimeWatcher.Watch(() =>
            {
                dic2[k] = "dd";
            });
            Console.WriteLine(t);
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
