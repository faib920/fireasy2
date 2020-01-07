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
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Test()
        {

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
