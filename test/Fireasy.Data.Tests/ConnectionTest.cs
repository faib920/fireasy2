using Fireasy.Common.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace Fireasy.Data.Tests
{
    [TestClass]
    public class ConnectionTest
    {
        public ConnectionTest()
        {
            InitConfig.Init();
        }

        [TestMethod]
        public void TestMsSql()
        {
            using (var db = DatabaseFactory.CreateDatabase())
            {
                var exception = db.TryConnect();
                Assert.IsNull(exception);
            }
        }

        [TestMethod]
        public void TestMySql()
        {
            using (var db = DatabaseFactory.CreateDatabase("mysql"))
            {
                var exception = db.TryConnect();
                Assert.IsNull(exception);
            }
        }

        [TestMethod]
        public void TestSqlite()
        {
            using (var db = DatabaseFactory.CreateDatabase("sqlite"))
            {
                var exception = db.TryConnect();
                Assert.IsNull(exception);
            }
        }

        [TestMethod]
        public void TestOracle()
        {
            using (var db = DatabaseFactory.CreateDatabase("oracle"))
            {
                var exception = db.TryConnect();
                Assert.IsNull(exception);
            }
        }

        [TestMethod]
        public void TestOracle1()
        {
            using (var db = DatabaseFactory.CreateDatabase("oracle1"))
            {
                var exception = db.TryConnect();
                Assert.IsNull(exception);
            }
        }

        [TestMethod]
        public void TestOracle2()
        {
            using (var db = DatabaseFactory.CreateDatabase("oracle2"))
            {
                var exception = db.TryConnect();
                Assert.IsNull(exception);
            }
        }

        [TestMethod]
        public void TestFirebird()
        {
            using (var db = DatabaseFactory.CreateDatabase("firebird"))
            {
                var exception = db.TryConnect();
                Assert.IsNull(exception);
            }
        }

        [TestMethod]
        public void TestPostgreSql()
        {
            using (var db = DatabaseFactory.CreateDatabase("pqsql"))
            {
                var exception = db.TryConnect();
                Assert.IsNull(exception);
            }
        }

        [TestMethod]
        public void TestDB2()
        {
            using (var db = DatabaseFactory.CreateDatabase("db2"))
            {
                var exception = db.TryConnect();
                Assert.IsNull(exception);
            }
        }

        [TestMethod]
        public void TestXmlStore()
        {
            using (var db = DatabaseFactory.CreateDatabase("xmlStore"))
            {
                var exception = db.TryConnect();
                Assert.IsNull(exception);
            }
        }

        [TestMethod]
        public void TestJsonStore()
        {
            using (var db = DatabaseFactory.CreateDatabase("jsonStore"))
            {
                var exception = db.TryConnect();
                Assert.IsNull(exception);
            }
        }

        [TestMethod]
        public void TestCluster()
        {
            using (var db = DatabaseFactory.CreateDatabase("cluster"))
            {
                var exception = db.TryConnect();
                Assert.IsNull(exception);
            }
        }

        [TestMethod]
        public void TestEncrypt()
        {
            var mthEncrypt = Type.GetType("Fireasy.Data.ConnectionStringEncryptHelper, Fireasy.Data").GetMethod("Encrypt", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var mthDecrypt = Type.GetType("Fireasy.Data.ConnectionStringEncryptHelper, Fireasy.Data").GetMethod("Decrypt", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

            Console.WriteLine(mthEncrypt.Invoke(null, new[] { "Data Source=" }));

            Console.WriteLine(mthDecrypt.Invoke(null, new[] { "" }));
        }
    }
}
