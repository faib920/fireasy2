using Fireasy.Common.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Fireasy.Common.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Test()
        {
            Assert.AreEqual("bodies", "body".ToPlural());
            Assert.AreEqual("people", "people".ToPlural());
            Assert.AreEqual("girls", "girl".ToPlural());
            Console.WriteLine("----");
            Assert.AreEqual("body", "bodies".ToSingular());
            Assert.AreEqual("people", "people".ToSingular());
            Assert.AreEqual("girl", "girls".ToSingular());
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
    }
}
