using Fireasy.Data.Extensions;
#if NETSTANDARD
using Microsoft.Extensions.Configuration;
#endif
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Data.Tests
{
    [TestClass]
    public class DatabaseTest
    {
        private const string instanceName = "mysql";

        public DatabaseTest()
        {
            InitConfig.Init();
        }

        [TestMethod]
        public void TestScopedDatabase()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            using (var scope = new DatabaseScope(db))
            {
                TestFromScope();
            }
        }

        public void TestFromScope()
        {
            var db = DatabaseFactory.GetDatabaseFromScope();
            Assert.IsNotNull(db);
        }

        [TestMethod]
        public void TestReturnParameter()
        {
            var parameters = new ParameterCollection();
            parameters.Add("name", "admin");
            parameters.Add("type", 1);
            parameters.Add("password", "123");

            parameters.AddOut<string>("ret", 100);

            using (var database = DatabaseFactory.CreateDatabase("mysql"))
            {
                database.ExecuteNonQuery((ProcedureCommand)"sp_check_login", parameters);

                Assert.AreEqual("succeed", parameters["ret"].Value);
            }
        }

        [TestMethod]
        public void TestExecuteNonQuery()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var parameters = new ParameterCollection();
                parameters.Add("city", "__");
                var result = db.ExecuteNonQuery((SqlCommand)"delete from batch", parameters);
                Console.WriteLine($"执行完毕 结果为{result}");
            }
        }

        [TestMethod]
        public async Task TestExecuteNonQueryAsync()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var parameters = new ParameterCollection();
                parameters.Add("city", "London111");
                await db.ExecuteNonQueryAsync((SqlCommand)"delete from batch");
            }
        }

        [TestMethod]
        public void TestFillDataSet()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var ds = new DataSet();
                var paper = new DataPager(5, 0);
                var parameters = new ParameterCollection();
                parameters.Add("city", new[] { "London", "dff" });
                db.FillDataSet(ds, (SqlCommand)"select city from customers where city in (@city)", segment: paper, parameters: parameters);
                Assert.AreEqual(5, ds.Tables[0].Rows.Count);
            }
        }

        [TestMethod]
        public void TestExecuteScalr()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                db.BeginTransaction();
                var parameters = new ParameterCollection();
                parameters.Add("city", "London");
                var result = db.ExecuteScalar<string>((SqlCommand)"select city from customers where city = @city ", parameters);
                Console.WriteLine($"执行完毕 结果为{result}");

                db.CommitTransaction();
            }
        }

        [TestMethod]
        public void TestExecuteScalrWithParameters()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var parameters = new ParameterCollection(new { city = "London" });
                var result = db.ExecuteScalar<string>((SqlCommand)"select city from customers where city = @city and city <> @city", parameters);
                Console.WriteLine($"执行完毕 结果为{result}");
            }
        }

        [TestMethod]
        public async Task TestExecuteScalrAsync()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var parameters = new ParameterCollection();
                parameters.Add("city", "Berlin");
                await db.ExecuteScalarAsync<string>((SqlCommand)"select * from batch", parameters);
            }
        }

        [TestMethod]
        public void TestExecuteReader()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var paper = new DataPager(2, 0);
                var parameters = new ParameterCollection();
                parameters.Add("city", "London");
                using (var reader = db.ExecuteReader((SqlCommand)"select city from customers where city <> @city", paper, parameters))
                {
                    while (reader.Read())
                    {
                        Console.WriteLine(reader.GetValue(0));
                    }
                }
            }
        }

        [TestMethod]
        public async Task TestExecuteReaderAsync()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var paper = new DataPager(10, 0);
                var parameters = new ParameterCollection();
                parameters.Add("city", "London");
                using (var reader = await db.ExecuteReaderAsync((SqlCommand)"select * from Bag", paper, parameters, cancellationToken: CancellationToken.None))
                {
                    Console.WriteLine("ex" + Thread.CurrentThread.ManagedThreadId);

                    while (await ((DbDataReader)reader).ReadAsync(CancellationToken.None))
                    {
                        Console.WriteLine("read" + Thread.CurrentThread.ManagedThreadId);
                        Console.WriteLine(reader.GetValue(0));
                    }
                }

                Console.WriteLine("后续代码" + Thread.CurrentThread.ManagedThreadId);
            }
        }

        [TestMethod]
        public void ExecuteDataTableByPage()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var sql = new SqlCommand("SELECT * FROM Customers");
                var pager = new DataPager(5, 2);
                var table = db.ExecuteDataTable(sql, segment: pager);
                table.EachRows((r, i) => Console.WriteLine("CustomerID: {0}", r[0]));
            };
        }

        [TestMethod]
        public void TestExecuteEnumerable()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss fff"));
                var result = db.ExecuteEnumerable<Customer>((SqlCommand)"select * from customers").ToList();
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss fff"));
                result = db.ExecuteEnumerable<Customer>((SqlCommand)"select * from customers").ToList();
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss fff"));
                Console.WriteLine($"执行完毕 结果为{result.Count()}");
            }
        }

        [TestMethod]
        public void TestExecuteEnumerableSingle()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss fff"));
                var result = db.ExecuteEnumerable<int>((SqlCommand)"select customerid from customers");
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss fff"));
                Console.WriteLine($"执行完毕 结果为{result.Count()}");
            }
        }

        [TestMethod]
        public void TestExecuteDynamicEnumerable()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var pager = new DataPager(5, 0);
                var result = db.ExecuteEnumerable((SqlCommand)"select * from orders limit 1").ToList();
                var d = (DateTime)result[0].OrderDate;
                Console.WriteLine($"执行完毕 结果为{result.Count()}");
            }
        }

        [TestMethod]
        public void TestExecuteEnumerableByPager()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var sql = (SqlCommand)"select * from customers";

                var pager = new DataPager(5, 0);

                var result = db.ExecuteEnumerable<Customer>(sql, pager);
                Console.WriteLine($"执行完毕 结果为{result.Count()}");

                pager.CurrentPageIndex++;
            }
        }

        [TestMethod]
        public void TestExecuteEnumerableByExpiration()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var sql = (SqlCommand)"select * from customers";

                var pager = new DataPager(5, 0);

                //查询总记录数，同时采用20秒缓存，因此可以不用每次都查询总记录数
                pager.Evaluator = new TotalRecordEvaluator
                {
                    //Expiration = TimeSpan.FromSeconds(20)
                };

                while (true)
                {
                    var result = db.ExecuteEnumerable<Customer>(sql, pager);
                    Console.WriteLine($"执行完毕 结果为{result.Count()}");

                    if (pager.CurrentPageIndex + 1 >= pager.PageCount)
                    {
                        break;
                    }

                    pager.CurrentPageIndex++;
                }
            }
        }

        [TestMethod]
        public void TestExecuteEnumerableByTryNextEvaluator()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var sql = (SqlCommand)"select * from customers";

                //每页5条
                var pager = new DataPager(10, 0);
                pager.Evaluator = new TryNextEvaluator();

                //记录当前查询出的记录数
                var recordCount = 0;

                while (true)
                {
                    var result = db.ExecuteEnumerable<Customer>(sql, pager);
                    Console.WriteLine($"执行完毕 结果为{result.Count()}");

                    //判断是否到最后一页
                    if (recordCount + pager.PageSize > pager.RecordCount)
                    {
                        break;
                    }

                    //赋值新记录数
                    recordCount = pager.RecordCount;
                    pager.CurrentPageIndex++;
                }
            }
        }

        [TestMethod]
        public async Task TestExecuteEnumerableAsync()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var paper = new DataPager(10, 0);
                var rr = await db.ExecuteEnumerableAsync((SqlCommand)"select * from bag", paper);
                foreach (var r in rr)
                {
                    Console.WriteLine(r.BagID);
                }
            }
        }

        [TestMethod]
        public void TestStoreProcedure()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var pager = new DataPager(3, 1);
                var parameters = new ParameterCollection();
                parameters.Add("in1", 10).AddOut<int>("out1");

                var rr = db.ExecuteNonQuery((ProcedureCommand)"test", parameters);

                Console.WriteLine(parameters[1].Value);
            }
        }

        [TestMethod]
        public void TestExcelQuery()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var list = db.ExecuteEnumerable((SqlCommand)"select * from [orders$] where VAL(DATEPART('yyyy', orderdate)) = 2008");
                foreach (var r in list)
                {
                    Console.WriteLine(r);
                }
            }
        }

        public class Customer
        {
            public string CustomerId { get; set; }

            public string City { get; set; }

            public Size Test1 { get; set; }
        }

        public enum TestEnum
        {
            A,
            B
        }
    }
}
