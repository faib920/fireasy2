using Fireasy.Common.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;

namespace Fireasy.Common.Tests.Security
{
    [TestClass()]
    public class SecurityTest
    {
        [TestMethod]
        public void TestMD5()
        {
            var c = CryptographyFactory.Create(CryptoAlgorithm.MD5);
            var bytes = c.Encrypt("fireasy", Encoding.UTF8);
            Assert.AreEqual("P2OjkZ/a9Oe0Yss1rRnlzw==", Convert.ToBase64String(bytes));
        }

        [TestMethod]
        public void TestSHA1()
        {
            var c = CryptographyFactory.Create(CryptoAlgorithm.SHA1);
            var bytes = c.Encrypt("fireasy", Encoding.UTF8);
            Assert.AreEqual("Ce+S8twtqAqw3IunLaJwGJsbA0k=", Convert.ToBase64String(bytes));
        }

        [TestMethod]
        public void TestSHA256()
        {
            var c = CryptographyFactory.Create(CryptoAlgorithm.SHA256);
            var bytes = c.Encrypt("fireasy", Encoding.UTF8);
            Assert.AreEqual("ajYzysdQ8Dt4CGBSHAQVf0jSO62d8A8rsUEXAYkWhA8=", Convert.ToBase64String(bytes));
        }

        [TestMethod]
        public void TestSHA384()
        {
            var c = CryptographyFactory.Create(CryptoAlgorithm.SHA384);
            var bytes = c.Encrypt("fireasy", Encoding.UTF8);
            Assert.AreEqual("JIojBzlQO8t8h/BmKwHHbKhV5l1it/tRuPnRvJC2KMXBv8rvTSpiQLSOc63zJ4vW", Convert.ToBase64String(bytes));
        }

        [TestMethod]
        public void TestSHA512()
        {
            var c = CryptographyFactory.Create(CryptoAlgorithm.SHA512);
            var bytes = c.Encrypt("fireasy", Encoding.UTF8);
            Assert.AreEqual("zdRjlmz3FE4LLBlcsaOBrPwTDQ4SBnbIlTXfTiSlB16+GDegxsq9xpNUgwv9oetpnmOWUjs+7vWWupEXNmfkXQ==", Convert.ToBase64String(bytes));
        }

        [TestMethod]
        public void TestRC2()
        {
            var c = CryptographyFactory.Create(CryptoAlgorithm.RC2);
            var bytes = c.Encrypt("fireasy", Encoding.UTF8);
            Assert.AreEqual("6jaAgqeW3dw=", Convert.ToBase64String(bytes));
        }

        [TestMethod]
        public void TestAES()
        {
            var c = CryptographyFactory.Create(CryptoAlgorithm.AES);
            var bytes = c.Encrypt("fireasy", Encoding.UTF8);
            Assert.AreEqual("LA+IcPGDn5F1/e5oHXfRBw==", Convert.ToBase64String(bytes));
        }

        [TestMethod]
        public void TestRC4()
        {
            var c = CryptographyFactory.Create(CryptoAlgorithm.RC4);
            var bytes = c.Encrypt("fireasy", Encoding.UTF8);
            Assert.AreEqual("hUWUuUSlmg==", Convert.ToBase64String(bytes));
        }

        [TestMethod]
        public void TestDES()
        {
            var c = CryptographyFactory.Create(CryptoAlgorithm.DES);
            var bytes = c.Encrypt("fireasy", Encoding.UTF8);
            Assert.AreEqual("NcnuOjI/5dI=", Convert.ToBase64String(bytes));
        }

        [TestMethod]
        public void TestTDES()
        {
            var c = CryptographyFactory.Create(CryptoAlgorithm.TripleDES);
            var bytes = c.Encrypt("fireasy", Encoding.UTF8);
            Assert.AreEqual("whjkbt+wvN8=", Convert.ToBase64String(bytes));
        }

        [TestMethod]
        public void TestRSA()
        {
            var c = CryptographyFactory.CreateAsymmetric(CryptoAlgorithm.RSA);

            c.PublicKey = c.GeneratePublicKey();
            c.PrivateKey = c.GeneratePrivateKey();

            var md5 = CryptographyFactory.CreateHash(CryptoAlgorithm.SHA1);
            var bytes = md5.Encrypt(Encoding.UTF8.GetBytes("fireasy"));

            var signatures = c.CreateSignature(bytes, "SHA1");
            Assert.IsTrue(c.VerifySignature(bytes, signatures, "SHA1"));
        }

        [TestMethod]
        public void TestDSA()
        {
            var c = CryptographyFactory.CreateAsymmetric(CryptoAlgorithm.DSA);

            c.PublicKey = c.GeneratePublicKey();
            c.PrivateKey = c.GeneratePrivateKey();

            var md5 = CryptographyFactory.CreateHash(CryptoAlgorithm.SHA1);
            var bytes = md5.Encrypt(Encoding.UTF8.GetBytes("fireasy"));

            var signatures = c.CreateSignature(bytes, "SHA1");
            Assert.IsTrue(c.VerifySignature(bytes, signatures, "SHA1"));
        }
    }
}
