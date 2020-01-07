using Fireasy.App.Licence;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Fireasy.Common.Tests
{
    [TestClass]
    public class LicenceTests
    {
        [TestMethod]
        public void TestGenerateLocalKey()
        {
            var mgr = new DefaultLicenseManager();
            var localKey = mgr.GetLocalKey(new LocalKeyOption 
                { 
                    AppKey = "test",
                    EncryptKind = LocalKeyEncryptKinds.SHA256,
                    MachineKeyKind = MachineKeyKinds.MacAddress
                });

            Console.WriteLine(localKey);
        }

        [TestMethod]
        public void TestGenerateLicenseKey()
        {
            var mgr = new DefaultLicenseManager();
            var licenseKey = mgr.GetLicenseKey(new LicenceGenerateOption
            {
                EncryptKind = LicenseEncryptKinds.RSA,
                LocalKey = "AQLZlMVvXQktYVIxFY5IEd/6iIHOcgCevhYg6FhkZ3f6QQ==",
                PrivateKey = "<RSAKeyValue><Modulus>23GxSa98EllX44bQXrqT+G1oA6si6Ll0PKhcOdRLkYfZ6mCDX3gY1KNYW7VisAYqokYSxVoVM5g7mH7K3rZ2b3XAemX/KuW8Efl5BHlqY220Uau9JumoisgdeQVxrw84eWUZG1571VNDJFCXh/430EVHZpEhokD3xLZnJFssMik=</Modulus><Exponent>AQAB</Exponent><P>5C7E/BclATAZeGOMeazJmQihfyNQ9X6f7oaG0zfUQ8uI9PHx9xhrtots6h3nlWpTmv3g0Ni9F3voVN6Exr54jw==</P><Q>9jI0k37ulDEt7d1bGsd0gmNBKTwL2z7FStffWIKzUGG+OLkj1/yPkmKfTZnobcLDmgrjRI03LlK2XvHrmBtVxw==</Q><DP>NS79dLBETLmURi4VsMpoFoYfdg2aMg34YtTyGcraa47F9ilqNyh2+o4WrZ6YuoeUwvxHaQYLVKzpotZWZihA2w==</DP><DQ>e7GHxt9MQPpgiBTh9BdoCyaRE4WOs23bNBo3pcGtE+K37sneH2NOQw732MZOB++FrUFz37pPkh+Oby9+3eJF+w==</DQ><InverseQ>WPkr9zesGjTX+g50HwWzCKSDiN3RLdwEP6Rr4Uy4te3cFl6/9xVvrydOhurxaRcwAPld9reZ+WTRzuowflZA0Q==</InverseQ><D>emJP465w3bLiJ0yajRo1SAmMRu98ypoTT3j95mqVspY3XFVzLx1Fi+GFd7ATV+Q4hsPZ2CLDTEEMi6G69raAGeTbQHlTpuLzn6K3WrX+6RWYvAD+n6WCDzIrMhIl6Wjf96OeQo+VZuj4SDYnHu5tLrEQu0gShZdbfY1YCw0fzfk=</D></RSAKeyValue>",
            });

            Console.WriteLine(licenseKey);
        }

        [TestMethod]
        public void TestVerifyLicenseKey()
        {
            var mgr = new DefaultLicenseManager();
            var ret = mgr.VerifyLicenseKey(new LicenceVerifyOption
            {
                AppKey = "test",
                EncryptKind = LicenseEncryptKinds.RSA,
                LocalKey = "AQLZlMVvXQktYVIxFY5IEd/6iIHOcgCevhYg6FhkZ3f6QQ==",
                LicenseKey = "2S0BXnEvbioHOVH/8xf2kk3UXnMW3AFLPjnDoKvdvx/F04pFPNo8kOhUuaGOCyKDY3azjNlY121+yoFfCMOTMn73BepoxcDFe+vyWxHRj1sBFmlsYNzhZf1CbJRMfG4EHHzyBn4I5hAixwdcBT0HCFH/Y0448zSqfHXdZjwX4RU=",
                PublicKey = "<RSAKeyValue><Modulus>23GxSa98EllX44bQXrqT+G1oA6si6Ll0PKhcOdRLkYfZ6mCDX3gY1KNYW7VisAYqokYSxVoVM5g7mH7K3rZ2b3XAemX/KuW8Efl5BHlqY220Uau9JumoisgdeQVxrw84eWUZG1571VNDJFCXh/430EVHZpEhokD3xLZnJFssMik=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>",
            });

            Assert.IsTrue(ret);
        }

        [TestMethod]
        public void TestVerifyLicenseKeyNoVerifyLocalKey()
        {
            var mgr = new DefaultLicenseManager();
            var ret = mgr.VerifyLicenseKey(new LicenceVerifyOption
            {
                AppKey = "test1111111111",
                EncryptKind = LicenseEncryptKinds.RSA,
                VerifyLocalKey = false,
                LocalKey = "AQLZlMVvXQktYVIxFY5IEd/6iIHOcgCevhYg6FhkZ3f6QQ==",
                LicenseKey = "2S0BXnEvbioHOVH/8xf2kk3UXnMW3AFLPjnDoKvdvx/F04pFPNo8kOhUuaGOCyKDY3azjNlY121+yoFfCMOTMn73BepoxcDFe+vyWxHRj1sBFmlsYNzhZf1CbJRMfG4EHHzyBn4I5hAixwdcBT0HCFH/Y0448zSqfHXdZjwX4RU=",
                PublicKey = "<RSAKeyValue><Modulus>23GxSa98EllX44bQXrqT+G1oA6si6Ll0PKhcOdRLkYfZ6mCDX3gY1KNYW7VisAYqokYSxVoVM5g7mH7K3rZ2b3XAemX/KuW8Efl5BHlqY220Uau9JumoisgdeQVxrw84eWUZG1571VNDJFCXh/430EVHZpEhokD3xLZnJFssMik=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>",
            });

            Assert.IsTrue(ret);
        }

        [TestMethod]
        public void TestSaveLicenseData()
        {
            var localKey = "AQLZlMVvXQktYVIxFY5IEd/6iIHOcgCevhYg6FhkZ3f6QQ==";
            var licenseKey = "2S0BXnEvbioHOVH/8xf2kk3UXnMW3AFLPjnDoKvdvx/F04pFPNo8kOhUuaGOCyKDY3azjNlY121+yoFfCMOTMn73BepoxcDFe+vyWxHRj1sBFmlsYNzhZf1CbJRMfG4EHHzyBn4I5hAixwdcBT0HCFH/Y0448zSqfHXdZjwX4RU=";

            var data = new LicenceData
            {
                LocalKey = localKey,
                LicenceKey = licenseKey
            };

            var store = new DefaultLicenseDataStore();
            store.Save("test", data);
        }

        [TestMethod]
        public void TestLoadLicenseData()
        {
            var store = new DefaultLicenseDataStore();
            var data = store.Load<LicenceData>("test");
            Assert.IsNotNull(data);

            var mgr = new DefaultLicenseManager();
            var ret = mgr.VerifyLicenseKey(new LicenceVerifyOption
            {
                AppKey = "test",
                EncryptKind = LicenseEncryptKinds.RSA,
                LocalKey = data.LocalKey,
                LicenseKey = data.LicenceKey,
                PublicKey = "<RSAKeyValue><Modulus>23GxSa98EllX44bQXrqT+G1oA6si6Ll0PKhcOdRLkYfZ6mCDX3gY1KNYW7VisAYqokYSxVoVM5g7mH7K3rZ2b3XAemX/KuW8Efl5BHlqY220Uau9JumoisgdeQVxrw84eWUZG1571VNDJFCXh/430EVHZpEhokD3xLZnJFssMik=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>",
            });

            Assert.IsTrue(ret);
        }

        [TestMethod]
        public void TestSaveCustomLicenseData()
        {
            var localKey = "AQLZlMVvXQktYVIxFY5IEd/6iIHOcgCevhYg6FhkZ3f6QQ==";
            var licenseKey = "2S0BXnEvbioHOVH/8xf2kk3UXnMW3AFLPjnDoKvdvx/F04pFPNo8kOhUuaGOCyKDY3azjNlY121+yoFfCMOTMn73BepoxcDFe+vyWxHRj1sBFmlsYNzhZf1CbJRMfG4EHHzyBn4I5hAixwdcBT0HCFH/Y0448zSqfHXdZjwX4RU=";

            var data = new CustomLicenseData
            {
                LocalKey = localKey,
                LicenceKey = licenseKey,
                Name = "fireasy",
                RegisterTime = DateTime.Now
            };

            var store = new DefaultLicenseDataStore();
            store.Save("test", data);
        }

        [TestMethod]
        public void TestLoadCustomLicenseData()
        {
            var store = new DefaultLicenseDataStore();
            var data = store.Load<CustomLicenseData>("test");
            Assert.IsNotNull(data);

            var mgr = new DefaultLicenseManager();
            var ret = mgr.VerifyLicenseKey(new LicenceVerifyOption
            {
                AppKey = "test",
                EncryptKind = LicenseEncryptKinds.RSA,
                LocalKey = data.LocalKey,
                LicenseKey = data.LicenceKey,
                PublicKey = "<RSAKeyValue><Modulus>23GxSa98EllX44bQXrqT+G1oA6si6Ll0PKhcOdRLkYfZ6mCDX3gY1KNYW7VisAYqokYSxVoVM5g7mH7K3rZ2b3XAemX/KuW8Efl5BHlqY220Uau9JumoisgdeQVxrw84eWUZG1571VNDJFCXh/430EVHZpEhokD3xLZnJFssMik=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>",
            });

            Assert.IsTrue(ret);
        }

        [Serializable]
        public class CustomLicenseData : LicenceData
        {
            public string Name { get; set; }

            public DateTime RegisterTime { get; set; }
        }
    }
}
