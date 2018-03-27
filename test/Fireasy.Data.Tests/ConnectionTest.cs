using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fireasy.Data.Tests
{
    [TestClass]
    public class ConnectionTest
    {
        [TestMethod]
        public void TestMsSql()
        {
            using (var db = DatabaseFactory.CreateDatabase("mssql"))
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
        public void TestCluster()
        {
            using (var db = DatabaseFactory.CreateDatabase("cluster"))
            {
                var exception = db.TryConnect();
                Assert.IsNull(exception);
            }
        }
    }
}
