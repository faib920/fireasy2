using Fireasy.Common.Configuration;
using Fireasy.Data.Extensions;
using Fireasy.Data.Provider;
#if NETSTANDARD
using Microsoft.Extensions.Configuration;
#endif
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Data.Tests
{
    [TestClass]
    public class DatabaseTest
    {
        public DatabaseTest()
        {
            InitConfig.Init();
        }

        [TestMethod]
        public void TestExecuteNonQuery()
        {
            using (var db = DatabaseFactory.CreateDatabase())
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
            using (var db = DatabaseFactory.CreateDatabase())
            {
                Console.WriteLine("前面代码" + Thread.CurrentThread.ManagedThreadId);

                var parameters = new ParameterCollection();
                parameters.Add("city", "London111");
                await db.ExecuteNonQueryAsync((SqlCommand)"delete from batch");

                Console.WriteLine("后续代码" + Thread.CurrentThread.ManagedThreadId);
                DoSomthings();
            }
        }

        [TestMethod]
        public void TestFillDataSet()
        {
            using (var db = DatabaseFactory.CreateDatabase())
            {
                var ds = new DataSet();
                var paper = new DataPager(5, 0);
                var parameters = new ParameterCollection();
                parameters.Add("city", "London");
                db.FillDataSet(ds, (SqlCommand)"select city from customers where city <> @city", segment: paper, parameters: parameters);
                Assert.AreEqual(5, ds.Tables[0].Rows.Count);
            }
        }

        [TestMethod]
        public void TestExecuteScalr()
        {
            using (var db = DatabaseFactory.CreateDatabase())
            {
                var parameters = new ParameterCollection();
                parameters.Add("city", "London");
                var result = db.ExecuteScalar<string>((SqlCommand)"select city from customers where city = @city and city <> ?city", parameters);
                Console.WriteLine($"执行完毕 结果为{result}");
            }
        }

        [TestMethod]
        public void TestExecuteScalrWithParameters()
        {
            using (var db = DatabaseFactory.CreateDatabase())
            {
                var parameters = new ParameterCollection(new { city = "London" });
                var result = db.ExecuteScalar<string>((SqlCommand)"select city from customers where city = @city and city <> @city", parameters);
                Console.WriteLine($"执行完毕 结果为{result}");
            }
        }

        [TestMethod]
        public async Task TestExecuteScalrAsync()
        {
            using (var db = DatabaseFactory.CreateDatabase())
            {
                var parameters = new ParameterCollection();
                parameters.Add("city", "Berlin");
                var task = await db.ExecuteScalarAsync<string>((SqlCommand)"select * from batch", parameters);

                Console.WriteLine("后续代码" + Thread.CurrentThread.ManagedThreadId);
                DoSomthings();
            }
        }

        private void DoSomthings()
        {
            Console.WriteLine("后续代码");
            Console.WriteLine("后续代码");
            Console.WriteLine("后续代码");
            Console.WriteLine("后续代码");
        }

        [TestMethod]
        public void TestExecuteReader()
        {
            using (var db = DatabaseFactory.CreateDatabase())
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
            using (var db = DatabaseFactory.CreateDatabase())
            {
                var paper = new DataPager(10, 0);
                var parameters = new ParameterCollection();
                parameters.Add("city", "London");
                using (var reader = await db.ExecuteReaderAsync((SqlCommand)"select * from Bag", paper, parameters, CancellationToken.None))
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
            using (var database = DatabaseFactory.CreateDatabase())
            {
                var sql = new SqlCommand("SELECT * FROM Customers");
                var pager = new DataPager(5, 2);
                var table = database.ExecuteDataTable(sql, segment: pager);
                table.EachRow((r, i) => Console.WriteLine("CustomerID: {0}", r[0]));
            };
        }

        [TestMethod]
        public void TestExecuteEnumerable()
        {
            using (var db = DatabaseFactory.CreateDatabase())
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
            using (var db = DatabaseFactory.CreateDatabase())
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
            using (var db = DatabaseFactory.CreateDatabase())
            {
                var pager = new DataPager(5, 0);
                var result = db.ExecuteEnumerable((SqlCommand)"select * from customers").ToList();
                Console.WriteLine($"执行完毕 结果为{result.Count()}");
            }
        }

        [TestMethod]
        public void TestExecuteEnumerableByPager()
        {
            using (var db = DatabaseFactory.CreateDatabase("mssql"))
            {
                var sql = @"
select r.StockedID, 
			r.OrgID,
			o.Code OrgCode,
			o.Name OrgName,
			o.FullName OrgFullName,
			r.StoreID,
			s.Name StoreName,
			r.KindID,
			k.Name KindName,
			r.Amount,
			(select top 1 BatchNo from GrainStockRecordRice where KindID=r.KindID and StoreID=r.StoreID and State=1 order by OperateTime desc) BatchNo 
from  GrainStocked r 
inner join SysOrg o on r.OrgID = o.OrgID 
inner join Kind k on r.KindID = k.KindID 
inner join Store s on r.StoreID = s.StoreID  
where r.State=1 and o.Attribute=3 and o.SchemaID=15 and o.Code like '00%'
order by o.Code";

                var pager = new DataPager(5, 0);
                var result = db.ExecuteEnumerable<Customer>((SqlCommand)sql, pager);
                Console.WriteLine($"执行完毕 结果为{result.Count()}");
            }
        }

        [TestMethod]
        public void TestExecuteEnumerableByExpiration()
        {
            using (var db = DatabaseFactory.CreateDatabase())
            {
                var sql = (SqlCommand)"select * from customers";

                var pager = new DataPager(5, 0);

                //查询总记录数，同时采用20秒缓存，因此可以不用每次都查询总记录数
                pager.Evaluator = new TotalRecordEvaluator
                {
                    Expiration = TimeSpan.FromSeconds(20)
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
            using (var db = DatabaseFactory.CreateDatabase())
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
            using (var db = DatabaseFactory.CreateDatabase())
            {
                var paper = new DataPager(10, 0);
                var rr = await db.ExecuteEnumerableAsync((SqlCommand)"select * from bag", paper);
                foreach (var r in rr)
                {
                    Console.WriteLine(r.BagID);
                }
                Console.WriteLine("后续代码" + Thread.CurrentThread.ManagedThreadId);
            }
        }

        public class Customer
        {
            public string CustomerId { get; set; }

            public string City { get; set; }

            public TestEnum Test1 { get; set; }
        }

        public enum TestEnum
        {
            A,
            B
        }
    }
}
