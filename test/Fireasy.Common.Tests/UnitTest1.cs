using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fireasy.Common;
using System.Collections.Generic;
using Fireasy.Common.Extensions;
using System.Linq.Expressions;
using System.Dynamic;
using System.Text;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Fireasy.Common.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
        }

        private bool A1()
        {
            return false;
        }

        private bool A2()
        {
            return false;
        }

        public class Data1
        {
            public string Name { get; set; }
        }

        public class Data2
        {
            public string Name { get; set; }

            public int Age { get; set; }

            public string Sex { get; set; }
        }

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
