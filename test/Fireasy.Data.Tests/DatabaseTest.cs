using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Fireasy.Data.Tests
{
    [TestClass]
    public class DatabaseTest
    {
        [TestMethod]
        public void TestExecuteNonQuery()
        {
            using (var db = DatabaseFactory.CreateDatabase())
            {
                var parameters = new ParameterCollection();
                parameters.Add("city", "__");
                var result = db.ExecuteNonQuery((SqlCommand)"delete from customers where city = @city", parameters);
                Console.WriteLine($"执行完毕 结果为{result}");
            }
        }

        [TestMethod]
        public void TestExecuteNonQueryAsync()
        {
            using (var db = DatabaseFactory.CreateDatabase())
            {
                var parameters = new ParameterCollection();
                parameters.Add("city", "London");
                var task = db.ExecuteNonQueryAsync((SqlCommand)"delete from customers where city = @city", parameters);
                task.ContinueWith(t =>
                    {
                        Console.WriteLine($"执行完毕 结果为{t.Result}");
                    });

                Console.WriteLine("后续代码");
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
        public void TestExecuteScalrAsync()
        {
            using (var db = DatabaseFactory.CreateDatabase())
            {
                var parameters = new ParameterCollection();
                parameters.Add("city", "London");
                var task = db.ExecuteScalarAsync<string>((SqlCommand)"select city from customers where city = @city", parameters);
                task.ContinueWith(t =>
                    {
                        Console.WriteLine($"执行完毕 结果为{t.Result}");
                    });

                Console.WriteLine("后续代码");
            }
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
                var paper = new DataPager(2, 0);
                var parameters = new ParameterCollection();
                parameters.Add("city", "London");
                using (var reader = await db.ExecuteReaderAsync((SqlCommand)"select city from customers where city <> @city", paper, parameters))
                {
                    while (reader.Read())
                    {
                        Console.WriteLine(reader.GetValue(0));
                    }
                }

                Console.WriteLine("后续代码");
            }
        }

        [TestMethod]
        public void TestExecuteEnumerable()
        {
            using (var db = DatabaseFactory.CreateDatabase())
            {
                var result = db.ExecuteEnumerable<Customer>((SqlCommand)"select * from customers");
                Console.WriteLine($"执行完毕 结果为{result.Count()}");
            }
        }

        [TestMethod]
        public async Task TestExecuteEnumerableAsync()
        {
            using (var db = DatabaseFactory.CreateDatabase())
            {
                var paper = new DataPager(2, 0);
                var result = await db.ExecuteEnumerableAsync<Customer>((SqlCommand)"select * from customers", paper);
                Console.WriteLine($"执行完毕 结果为{result.Count()}");

                Console.WriteLine("后续代码");
            }
        }

        public class Customer
        {
            public string CustomerId { get; set; }

            public string City { get; set; }
        }
    }
}
