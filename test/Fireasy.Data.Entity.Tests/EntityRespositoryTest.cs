﻿using Fireasy.Common;
using Fireasy.Common.Caching;
using Fireasy.Common.Extensions;
using Fireasy.Common.Ioc;
using Fireasy.Common.Reflection;
using Fireasy.Common.Serialization;
using Fireasy.Data.Entity.Linq;
using Fireasy.Data.Entity.Linq.Expressions;
using Fireasy.Data.Entity.Linq.Translators;
using Fireasy.Data.Entity.Subscribes;
using Fireasy.Data.Entity.Tests.Models;
using Fireasy.Data.Entity.Validation;
using Fireasy.Data.Extensions;
using Fireasy.Data.Provider;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Data.Entity.Tests
{
    [TestClass]
    public class EntityRespositoryTest
    {
        public EntityRespositoryTest()
        {
            InitConfig.Init();
        }

        private class TestScope : Scope<TestScope>
        {
            public string Guid { get; set; }
        }

        [TestMethod]
        public void TestGet()
        {
            using (var context = new DbContext())
            {
                //context.Orders.Insert(new Orders { CustomerID = "ALFKI", EmployeeID = 2, OrderDate = DateTime.Now });
                context.Orders.Update(() => new Orders { CustomerID = "ALFKI", EmployeeID = 2, OrderDate = DateTime.Now }, s => false);
                //var rr = context.Orders.Where(s => s.Customers.CustomerID.StartsWith("_")).Take(10).ToList();
            }
        }

        [TestMethod]
        public void TestClone()
        {
            using (var context = new DbContext())
            {
                var rr = context.Orders.FirstOrDefault();
                var rr1 = new Orders { OrderID = 33, CustomerID = "dsfsaf" };

                Console.WriteLine(rr);
                var tt = rr1.Clone();
                Console.WriteLine(tt);
                context.Orders.Insert(tt);
            }
        }

        [TestMethod]
        public async Task TestGetAsync()
        {
            ThreadPool.SetMaxThreads(10, 10);
            using (var context = new DbContext())
            {
                var customers = await context.Customers.GetAsync("ALFKI");
            }
        }

        [TestMethod]
        public async Task TestSelectByConstructorAsync()
        {
            using (var context = new DbContext())
            {
                var customers = await context.Customer1s.FirstOrDefaultAsync();
            }
        }

        [TestMethod]
        public void TestScope1()
        {
            Parallel.For(1, 3, i => TestGetScopeAsync(i).AsSync());
        }

        [TestMethod]
        public async Task TestGetScopeAsync(int index)
        {
            var guid = Guid.NewGuid().ToString();

            using (var scope = new TestScope() { Guid = guid })
            using (var context = new DbContext())
            {
                Console.WriteLine("ThreadId(" + index + "): " + Thread.CurrentThread.ManagedThreadId + " guid:" + TestScope.Current.Guid);

                var customers = await context.Customers.GetAsync("ALFKI");

                Console.WriteLine("ThreadId(" + index + "): " + Thread.CurrentThread.ManagedThreadId + " guid:" + TestScope.Current.Guid);

                customers.Address = "a11";

                var r = await context.Customers.InsertOrUpdateAsync(customers);

                Console.WriteLine("ThreadId(" + index + "): " + Thread.CurrentThread.ManagedThreadId + " guid:" + TestScope.Current.Guid);

                //Assert.AreEqual(1, r);
            }
        }

        [TestMethod]
        public void TestAny()
        {
            using (var context = new DbContext())
            {
                var ret = context.Customers.Any(s => s.CustomerID == "ALFKI");
                Assert.IsTrue(ret);
            }
        }

        [TestMethod]
        public async Task TestAnyAsync()
        {
            using (var context = new DbContext())
            {
                var ret = await context.Customers.AnyAsync(s => s.CustomerID == "ALFKI");
                Assert.IsTrue(ret);
            }
        }

        [TestMethod]
        public void TestAll()
        {
            using (var context = new DbContext())
            {
                var tt = "a";
                var ret = context.Customers.Where(s => tt == "a" ? s.CustomerID == "ALFKI" : s.CustomerID == "AA").ToList();
                //Assert.IsFalse(ret);
            }
        }

        [TestMethod]
        public async Task TestAllAsync()
        {
            using (var context = new DbContext())
            {
                var t = context.Database.ExecuteNonQueryAsync((SqlCommand)"ddd").AsSync();

                var ret = await context.Customers.AllAsync(s => s.CustomerID == "ALFKI");
                Assert.IsFalse(ret);
            }
        }

        [TestMethod]
        public async Task TestAverageAsync()
        {
            using (var context = new DbContext())
            {
                var ret = await context.OrderDetails.AverageAsync(s => s.Quantity);
                Console.WriteLine(ret);
            }
        }

        [TestMethod]
        public async Task TestAverageAsync1()
        {
            using (var context = new DbContext())
            {
                var ret = await context.OrderDetails.Select(s => s.Quantity).AverageAsync();
                Console.WriteLine(ret);
            }
        }

        [TestMethod]
        public async Task TestCountAsync()
        {
            using (var context = new DbContext())
            {
                var ret = await context.OrderDetails.Where(s => s.Quantity > 30).CountAsync();
                Console.WriteLine(ret);
            }
        }

        [TestMethod]
        public async Task TestCountAsync1()
        {
            using (var context = new DbContext())
            {
                var ret = await context.OrderDetails.CountAsync(s => s.Quantity > 30);
                Console.WriteLine(ret);
            }
        }

        [TestMethod]
        public async Task TestSumAsync()
        {
            using (var context = new DbContext())
            {
                var ret = await context.OrderDetails.SumAsync(s => s.Quantity);
                Console.WriteLine(ret);
                ret = await context.OrderDetails.Where(s => true).SumAsync(s => s.Quantity);
                Console.WriteLine(ret);
            }
        }

        [TestMethod]
        public async Task TestSumAsync1()
        {
            using (var context = new DbContext())
            {
                var ret = await context.OrderDetails.Select(s => s.Quantity).SumAsync();
                Console.WriteLine(ret);
            }
        }

        [TestMethod]
        public async Task TestSumAsync2()
        {
            using (var context = new DbContext())
            {
                var ret = await context.Orders.Select(s => new { s.OrderID, Qu = s.OrderDetailses.Sum(t => t.Quantity) }).ToListAsync();
                Console.WriteLine(ret);
            }
        }

        [TestMethod]
        public async Task TestMinAsync()
        {
            using (var context = new DbContext())
            {
                var ret = await context.OrderDetails.MinAsync(s => s.Quantity);
                Console.WriteLine(ret);
            }
        }

        [TestMethod]
        public async Task TestMinAsync1()
        {
            using (var context = new DbContext())
            {
                var ret = await context.OrderDetails.Select(s => s.Quantity).MinAsync();
                Console.WriteLine(ret);
            }
        }

        [TestMethod]
        public async Task TestMaxAsync()
        {
            using (var context = new DbContext())
            {
                var ret = await context.OrderDetails.MaxAsync(s => s.Quantity);
                Console.WriteLine(ret);
            }
        }

        [TestMethod]
        public async Task TestMaxAsync1()
        {
            using (var context = new DbContext())
            {
                var ret = await context.OrderDetails.Select(s => s.Quantity).MaxAsync();
                Console.WriteLine(ret);
            }
        }

        [TestMethod]
        public async Task TestFirstOrDefaultAsync()
        {
            using (var context = new DbContext())
            {
                var ret = await context.Customers.FirstOrDefaultAsync(s => s.CustomerID == "ALFKI");
                Assert.IsNotNull(ret);
            }
        }

        [TestMethod]
        public async Task TestFirstAsync()
        {
            using (var context = new DbContext())
            {
                var ret = await context.Customers.FirstAsync(s => s.CustomerID == "ALFKI");
                Assert.IsNotNull(ret);
            }
        }

        [TestMethod]
        public async Task TestGroupby()
        {
            using (var context = new DbContext())
            {
                var ret = await context.Orders.GroupBy(s => s.CustomerID, (s, t) => new { s, c = t.Count() }).ToDictionaryAsync(s => s.s, s => s.c);
            }
        }

        [TestMethod]
        public async Task TestBetween()
        {
            using (var context = new DbContext())
            {
                var list = await context.Orders
                    .Where(s => s.OrderDate.IsBetween(DateTime.Now.AddDays(-1), DateTime.Now))
                    .ToListAsync();
            }
        }

        [TestMethod]
        public async Task TestContains()
        {
            var c = new int[] { 1 };
            using (var context = new DbContext())
            {
                var ret = await context.Orders.Where(s => c.Contains(s.CustomerID.Length)).ToListAsync();
            }
        }

        [TestMethod]
        public void TestTrans()
        {
            using (var context = new DbContext())
            {
                context.BeginTransaction();
                TestTrans1();
                context.CommitTransaction();
            }
        }

        private void TestTrans1()
        {
            using (var context = new DbContext1())
            {
                context.BeginTransaction();

                TestTrans2();
                TestTrans3();

                context.CommitTransaction();
            }
        }

        private void TestTrans2()
        {
            using (var context = new DbContext1())
            {
                context.BeginTransaction();

                context.Database.ExecuteNonQuery((SqlCommand)"delete from orders where orderid=-1");

                context.CommitTransaction();
            }
        }

        private void TestTrans3()
        {
            using (var context = new DbContext())
            {
                context.BeginTransaction();

                context.Database.ExecuteNonQuery((SqlCommand)"delete from orders where orderid=-1");

                context.CommitTransaction();
            }
        }

        [TestMethod]
        public async Task TestTransScope()
        {
            using (var scope = new EntityTransactionScope())
            using (var context = new DbContext())
            {
                await context.Products.DeleteAsync(s => false);
            }
        }

        [TestMethod]
        public void TestInterfaceGet()
        {
            using (var context = new DbContext())
            {
                var queryable = (IQueryable<IOrder>)context.Set(typeof(Orders));

                var c = queryable.Where(s => s.OrderID == 10251).Count();

                Assert.AreEqual(1, c);

                var q = queryable.BatchOr(new long[] { }, (s, t) => s.OrderID == t);

                Assert.AreEqual(3, q.Count());
            }
        }

        [TestMethod]
        public void TestArray()
        {
            using (var context = new DbContext())
            {
                var dept = context.Depts.FirstOrDefault();
                //Console.WriteLine(dept.Attributes);
            }
        }

        [TestMethod]
        public void TestGetByMultiKeys()
        {
            using (var context = new DbContext())
            {
                var detail = context.OrderDetails.Get(10248, 42);
                Assert.AreEqual(10248, detail.OrderID);
                Assert.AreEqual(42, detail.Product1ID);
            }
        }

        [TestMethod]
        public async Task TestGetByMultiKeysAsync()
        {
            using (var context = new DbContext())
            {
                var detail = await context.OrderDetails.GetAsync(10248, 42);
                Assert.AreEqual(10248, detail.OrderID);
                Assert.AreEqual(42, detail.Product1ID);
            }
        }

        [TestMethod]
        public void TestLazyLoad()
        {
            OrderDetails detail;
            using (var db = new DbContext())
            {
                SerializeOption.GlobalConverters.Add(new LightEntityJsonConverter());
                db.BeginTransaction();

                detail = db.OrderDetails.FirstOrDefault();
                Assert.AreEqual("VINET", detail.Orders.CustomerID);

                using (var db1 = new DbContext())
                {
                    var detail1 = db1.OrderDetails.FirstOrDefault();
                    Assert.AreEqual("VINET", detail1.Orders.CustomerID);
                }

                db.RollbackTransaction();
            }

            using (var db = new DbContext())
            {
                detail = db.OrderDetails.FirstOrDefault();
                Assert.AreEqual("VINET", detail.Orders.CustomerID);
            }
        }

        [TestMethod]
        public void TestLazyLoadDetails()
        {
            Orders order;
            using (var db = new DbContext())
            {
                order = db.Orders.FirstOrDefault();
            }

            Assert.AreEqual(3, order.OrderDetailses.Count);
        }

        [TestMethod]
        public void TestLazyLoadLoop()
        {
            using (var db = new DbContext())
            {
                //将生产 1 + 5 次查询
                foreach (var detail in db.OrderDetails.Take(5))
                {
                    Assert.IsFalse(string.IsNullOrEmpty(detail.Orders.CustomerID));
                }
            }
        }

#if NETCOREAPP3_0
        [TestMethod]
        public async Task TestForeachAsync()
        {
            using (var db = new DbContext())
            {
                await foreach (var detail in db.Customers.Where(s => s.City != "")
                    .Segment(new DataPager(2,1))
                    .AsAsyncEnumerable())
                {
                    Console.WriteLine(detail.CustomerID);
                }
            }
        }
#endif

        [TestMethod]
        public void TestIncludeCascade()
        {
            using (var db = new DbContext())
            {
                var details = db.OrderDetails
                            .Include(s => s.Orders.Customers)
                            .CacheExecution(true).Take(5);

                foreach (var detail in details)
                {
                    Console.WriteLine(detail.Orders.Customers.Address);
                }
            }
        }

        [TestMethod]
        public async Task TestIncludeCascadeAsync()
        {
            using (var db = new DbContext())
            {
                var details = await db.OrderDetails
                            .Include(s => s.Orders.Customers)
                            .Take(2)
                            .ToListAsync();

                foreach (var detail in details)
                {
                    Console.WriteLine(detail.Orders.Customers.Address);
                }
            }
        }

        public Products TestCache1()
        {
            var cacheMgr = CacheManagerFactory.CreateManager();

            using (var db = new DbContext())
            {
                if (cacheMgr.Contains("p"))
                {
                    return (Products)cacheMgr.Get("p");
                }

                var product = db.Products.FirstOrDefault();
                cacheMgr.Add("p", product);
                return product;
            }
        }

        [TestMethod]
        public void TestCache2()
        {
            using (var db = new DbContext())
            {
                var a1 = new DataPager(10, 2);
                var a2 = new DataPager(10, 2);

                Console.WriteLine(TimeWatcher.Watch(() =>
                {
                    var list1 = db.Orders
                        .Segment(a1)
                        .CacheParsing(true, TimeSpan.FromDays(1))
                        .CacheExecution(false, TimeSpan.FromDays(1))
                        .AsNoTracking()
                        .ToList();
                }));

                Console.WriteLine(TimeWatcher.Watch(() =>
                {
                    var list2 = db.Orders
                        .Segment(a2)
                        .CacheParsing(true, TimeSpan.FromDays(1))
                        .CacheExecution(false, TimeSpan.FromDays(1))
                        .AsNoTracking()
                        .ToList();
                }));
            }
        }

        [TestMethod]
        public async Task TestCacheAsync2()
        {
            using (var db = new DbContext())
            {
                var a1 = new DataPager(10, 2);
                var a2 = new DataPager(10, 2);

                Console.WriteLine(await TimeWatcher.WatchAsync(async () =>
                {
                    var list1 = await db.Orders
                        .Segment(a1)
                        .CacheParsing(true, TimeSpan.FromDays(1))
                        .CacheExecution(true, TimeSpan.FromDays(1))
                        .AsNoTracking()
                        .ToListAsync();
                }));

                Console.WriteLine(await TimeWatcher.WatchAsync(async () =>
                {
                    var list2 = await db.Orders
                        .Segment(a2)
                        .CacheParsing(true, TimeSpan.FromDays(1))
                        .CacheExecution(true, TimeSpan.FromDays(1))
                        .AsNoTracking()
                        .ToListAsync();
                }));

                //await db.Orders.DeleteAsync(s => false);
            }
        }

        private Products GetProduct(Func<Products> factory)
        {
            var cacheMgr = CacheManagerFactory.CreateManager();

            if (cacheMgr.Contains("p"))
            {
                return (Products)cacheMgr.Get("p");
            }

            var product = factory();
            cacheMgr.Add("p", product);
            return product;
        }

        [TestMethod]
        public void TestInclude()
        {
            using (var db = new DbContext())
            {
                var details = db.OrderDetails
                            .Include(s => s.Products)
                            .Include(s => s.Orders);

                foreach (var detail in details)
                {
                    Console.WriteLine(detail.Products.ProductName);
                    Console.WriteLine(detail.Orders.OrderDate);
                }
            }
        }

        [TestMethod]
        public void TestIncludeGet()
        {
            using (var db = new DbContext())
            {
                var detail = db.OrderDetails
                            .Include(s => s.Products)
                            .Include(s => s.Orders)
                            .FirstOrDefault();

                Assert.AreEqual(detail.Orders.CustomerID, "VINET");
            }
        }

        [TestMethod]
        public void TestAssociate()
        {
            using (var db = new DbContext())
            {
                db.Associate<Customers>(s => s.Orderses.Where(o => o.OrderDate >= DateTime.Now));

                var customers = db.Customers
                    .Where(c => c.CustomerID == "ALFKI")
                    .Select(c => new
                    {
                        CustomerID = c.CustomerID,
                        FilteredOrdersCount = c.Orderses.Count()
                    })
                    .ToList();

                Assert.AreEqual(1, customers.Count);

                //未筛选前是 7 条数据
                Assert.AreEqual(0, customers[0].FilteredOrdersCount);
            }
        }

        [TestMethod]
        public void TestAssociate1()
        {
            using (var db = new DbContext())
            {
                var customers = db.Customers.Associate(s => s.Orderses.Where(o => o.OrderDate >= DateTime.Now))
                    .Where(c => c.CustomerID == "ALFKI")
                        .Select(c => new
                        {
                            CustomerID = c.CustomerID,
                            FilteredOrdersCount = c.Orderses.Count()
                        })
                    .ToList();

                Assert.AreEqual(1, customers.Count);

                //未筛选前是 7 条数据
                Assert.AreEqual(0, customers[0].FilteredOrdersCount);
            }
        }

        [TestMethod]
        public void TestApply()
        {
            using (var db = new DbContext())
            {
                db.Apply<Customers>(u => u.Where(s => s.CustomerID == "ALFKI"));

                var customers = db.Customers
                    .ToList();

                //不加限定条件为 88 条数据
                Assert.AreEqual(1, customers.Count);
            }
        }

        [TestMethod]
        public void TestPaging()
        {
            using (var db = new DbContext())
            {
                var pager = new DataPager(50, 2);
                var products = db.Products.Segment(pager).ToList();
                Assert.AreEqual(77, pager.RecordCount);
                Assert.AreEqual(2, pager.PageCount); // 77 / 50 余数加1
            }
        }

        [TestMethod]
        public async Task TestPagingAsync()
        {
            using (var db = new DbContext())
            {
                var pager = new DataPager(50, 2);
                var products = await db.OperateLog.Segment(pager).ToListAsync();
                var p = await db.OperateLog.GetAsync(1);
                p.UserName = "a";
                await db.OperateLog.UpdateAsync(p);
                p.UserName = "a1";
                await db.OperateLog.UpdateAsync(p);
                p.UserName = "a2";
                await db.OperateLog.UpdateAsync(p);
                p.UserName = "a3";
                await db.OperateLog.UpdateAsync(p);
                Console.WriteLine("------------------");
                //Assert.AreEqual(77, pager.RecordCount);
                //Assert.AreEqual(2, pager.PageCount); // 77 / 50 余数加1
            }
        }

        [TestMethod]
        public void TestPagingByTryNextEvaluator()
        {
            using (var db = new DbContext())
            {
                var pager1 = new DataPager(500, 3);
                pager1.Evaluator = new TryNextEvaluator();
                var detail1 = db.OrderDetails.Segment(pager1).ToList();
                Console.WriteLine(pager1.PageCount);
            }
        }

        [TestMethod]
        public void TestPaginalList()
        {
            using (var db = new DbContext())
            {
                var pager = new DataPager(50, 0);
                var result = db.Orders.Segment(pager).ToPaginalResult();
                Console.WriteLine(result.Pages);
            }
        }

        [TestMethod]
        public void TestTowQuery()
        {
            using (var db = new DbContext())
            {
                var query = db.Orders;
                var list1 = query.ToList();
                var list2 = query.ToList();
            }
        }

        [TestMethod]
        public void TestTowDiffQuery()
        {
            using (var db = new DbContext())
            {
                var query1 = db.Orders.Where(s => s.CustomerID == "AA");
                var query2 = db.Orders.Where(s => s.CustomerID == "BB");
                var list1 = query1.ToList();
                var list2 = query2.ToList();
            }
        }

        [TestMethod]
        public void TestOneQuery()
        {
            using (var db = new DbContext())
            {
                var query = db.Orders;
                var list1 = (query as IListSource).GetList();
                var list2 = (query as IListSource).GetList();
            }
        }

        [TestMethod]
        public void TestAsHasTracking()
        {
            using (var db = new DbContext())
            {
                var result = db.Customers.FirstOrDefault(s => s.CustomerID == "ALFKI");
                result.City = "kunming";
                result.CompanyName = "fireasy";

                var modifiedPropeties = result.GetModifiedProperties();

                Assert.AreEqual(2, modifiedPropeties.Length);
                Assert.AreEqual("Berlin", result.GetOldValue("City"));
            }
        }

        [TestMethod]
        public void TestAsNoTracking()
        {
            using (var db = new DbContext())
            {
                var result = db.Customers.AsNoTracking().FirstOrDefault(s => s.CustomerID == "ALFKI");
                result.City = "kunming";
                result.CompanyName = "fireasy";

                var modifiedPropeties = result.GetModifiedProperties();

                Assert.AreEqual(0, modifiedPropeties.Length);
                Assert.AreEqual(PropertyValue.Empty, result.GetOldValue("City"));
            }
        }

        [TestMethod]
        public async Task TestAsync()
        {
            using (var db = new DbContext())
            {
                await db.Customers.UpdateAsync(() => new Customers("dfdf"), s => s.Address == "");
            }
        }

        [TestMethod]
        public void TestSelectAll()
        {
            using (var db = new DbContext())
            {
                var list = db.OrderDetails.Select(s => new OrderDetailsEx
                {
                    OrderID = s.OrderID,
                    Product1ID = s.Product1ID,
                    Quantity = s.Quantity,
                    UnitPrice = s.UnitPrice,
                    Discount = s.Discount,
                    ProductName = s.Products.ProductName
                })
                    .ToList();

                Assert.AreEqual("Queso Cabrales", list[0].ProductName);

                var ss = db.OrderDetails.Select(s => new { s.Product1ID, a = db.Orders.FirstOrDefault(t => t.OrderID == s.OrderID).OrderDate }).ToList();
            }
        }

        [TestMethod]
        public void TestJoinNewConstructor()
        {
            using (var db = new DbContext())
            {
                var list = db.OrderDetails
                    .Join(db.Products.DefaultIfEmpty(),
                        s => s.Product1ID, s => s.Id, (s, t) => new { detail = s, product = t })
                    .Select(s => new OrderDetailsEx(s.detail.OrderID) { Product1ID = s.product.Id })
                    .ToList();
                Assert.AreEqual("Queso Cabrales", list[0].ProductName);
            }
        }

        [TestMethod]
        public void TestExtend()
        {
            using (var db = new DbContext())
            {
                var list = db.OrderDetails.Select(s =>
                    s.Extend(() => new
                    {
                        ProductName = s.Products.ProductName,
                        BName = s.Products1.ProductName
                    }))
                    .ToList();

                Assert.AreEqual("Queso Cabrales", list[0].ProductName);
            }
        }

        [TestMethod]
        public void TestExtendAs()
        {
            using (var db = new DbContext())
            {
                var list = db.OrderDetails
                    .Join(db.Orders, s => s.OrderID, s => s.OrderID, (s, t) => new { s, t })
                    //.Where(s => s.s.OrderID != 1)
                    .Select(s =>
                        s.s.ExtendAs<OrderDetailsEx>(() => new OrderDetailsEx
                        {
                            ProductName = s.s.Products.ProductName,
                            OrderDesc = s.t.OrderDate.ToString()
                        }))
                    //.Distinct()
                    .ToList();

                Assert.AreEqual("Queso Cabrales", list[0].ProductName);
            }
        }

        [TestMethod]
        public void TestLeftJoin()
        {
            using (var db = new DbContext())
            {
                var list = db.OrderDetails
                    .LeftJoin(db.Products,
                        s => s.Product1ID, s => s.Id, (s, t) => new { detail = s, product = t })
                    .Select(s => new { s.product.ProductName, s.detail.OrderID })
                    .ToList();
                Assert.AreEqual("Queso Cabrales", list[0].ProductName);
            }
        }

        [TestMethod]
        public void TestRightJoin()
        {
            using (var db = new DbContext())
            {
                var list = db.OrderDetails
                    .RightJoin(db.Products,
                        s => s.Product1ID, s => s.Id, (s, t) => new { detail = s, product = t })
                    .Select(s => new { s.product.ProductName, s.detail.OrderID })
                    .ToList();
                //Assert.AreEqual("Queso Cabrales", list[0].ProductName);
            }
        }

        [TestMethod]
        public void TestJoinExtendAs()
        {
            using (var db = new DbContext())
            {
                var list = db.OrderDetails
                    .Join(db.Products.DefaultIfEmpty(),
                        s => s.Product1ID, s => s.Id, (s, t) => new { detail = s, product = t })
                    .Select(s => s.detail.ExtendAs<OrderDetailsEx>(() => new OrderDetailsEx
                    {
                        ProductName = s.product.ProductName
                    }))
                    .ToList();
                Assert.AreEqual("Queso Cabrales", list[0].ProductName);
            }
        }

        [TestMethod]
        public void TestExtendAs1()
        {
            using (var db = new DbContext())
            {
                var items = new List<string> { "df", "cc" };
                var list = db.OrderDetails.Select(s =>
                    s.ExtendAs<OrderDetailsEx>(() => new OrderDetailsEx
                    {
                        ProductName = items.FirstOrDefault(),
                        List = items.OrderBy(t => t).Where(t => true).ToList(),
                        Count = items.Count(t => true)
                    }))
                    .ToList();

                Assert.AreEqual("cc", list[0].ProductName);
            }
        }

        [TestMethod]
        public void TestExtendSelect()
        {
            using (var db = new DbContext())
            {
                var list = db.OrderDetails
                    .ExtendSelect(s => new OrderDetailsEx { ProductName = s.Products.ProductName })
                    .ToList();

                Assert.AreEqual("Queso Cabrales", list[0].ProductName);
            }
        }

        [TestMethod]
        public void TestSubSelect()
        {
            using (var db = new DbContext())
            {
                var list = db.OrderDetails
                    .Select(s => new { s.OrderID, s.Products.ProductName, db.Orders.FirstOrDefault(t => t.OrderID == s.OrderID).OrderDate, Count = db.Orders.Count(t => t.OrderID == s.OrderID) })
                    .ToList();

                Assert.AreEqual("Queso Cabrales", list[0].ProductName);
            }
        }

        [TestMethod]
        public void TestUseSql()
        {
            using (var db = new DbContext())
            {
                var list = db.OrderDetails
                    .Where("ProductID = @pid", par => par.Add("pid", 4))
                    .Where("OrderId <> 0")
                    .ToList();
            }
        }

        public class OrderDetailsEx : OrderDetails
        {
            public OrderDetailsEx()
            {
            }

            public OrderDetailsEx(long id)
            {
                OrderID = id;
            }

            public string ProductName { get; set; }

            public IEnumerable<string> List { get; set; }

            public int Count { get; set; }

            public string OrderDesc { get; set; }
        }

        [TestMethod]
        public void TestInsert()
        {
            using (var db = new DbContext())
            {
                var customer = new Orders
                {
                    CustomerID = "A211133",
                    EmployeeID = 1,
                    //OrderDate = DateTime.Now,
                    //ShipVia = 3
                };

                db.Orders.Insert(customer);
            }
        }

        [TestMethod]
        public async Task TestInsertAsync()
        {
            using (var db = new DbContext())
            {
                var customer = new Customers
                {
                    CustomerID = "766v",
                    CompanyName = "a2"

                };

                var ret = await db.Customers.InsertAsync(customer);
                Assert.AreEqual(1, ret);
            }
        }

        [TestMethod]
        public void TestInsertByFactory()
        {
            using (var db = new DbContext())
            {
                db.Customers.Insert(() => new Customers
                {
                    CustomerID = "DD1",
                    CompanyName = "kunming",
                    City = "kunming"
                });
            }
        }

        [TestMethod]
        public async Task TestInsertByFactoryAsync()
        {
            using (var db = new DbContext())
            {
                await db.Customers.InsertAsync(() => new Customers
                {
                    CustomerID = "DD1",
                    CompanyName = "kunming",
                    City = "kunming"
                });
            }
        }

        [TestMethod]
        public void TestInsertAndRetPk()
        {
            using (var db = new DbContext())
            {
                var order = new Orders
                {
                    CustomerID = "ALFKI",
                };

                var ret = db.Orders.Insert(order);
                Assert.AreEqual(ret, order.OrderID);
            }
        }

        [TestMethod]
        public void TestInsertByNew()
        {
            using (var db = new DbContext())
            {
                var customer = db.New<Customers>();
                customer.CustomerID = "F2";
                customer.CompanyName = "kunming";
                customer.City = "kunming";

                db.Customers.Insert(customer);
            }
        }

        [TestMethod]
        public void TestInsertByWrap()
        {
            using (var db = new DbContext())
            {
                var customer = db.Wrap(() => new Customers
                {
                    CustomerID = "F3",
                    CompanyName = "kunming",
                    City = "kunming"
                });

                db.Customers.Insert(customer);
            }
        }

        [TestMethod]
        public async Task TestInsertByInitializerAsync()
        {
            using (var db = new DbContext())
            {
                await db.Customers.InsertAsync(c =>
                {
                    c.CustomerID = "DD1";
                    c.CompanyName = "kunming";
                    c.City = "kunming";
                });
            }
        }

        [TestMethod]
        public void TestInsertWithDetails()
        {
            var order = new Orders
            {
                OrderDate = DateTime.Now,
                CustomerID = "ALFKI",
                EmployeeID = 5,
                ShipVia = 3,
                OrderDetailses = new EntitySet<OrderDetails>(),
            };

            //构造5条明细
            for (var i = 0; i < 5; i++)
            {
                order.OrderDetailses.Add(new OrderDetails
                {
                    Product1ID = 1 + i,
                    UnitPrice = 1,
                    Quantity = 12,
                    Discount = 0.99,
                });
            }

            using (var db = new DbContext())
            {
                db.Orders.Insert(order);
            }
        }

        [TestMethod]
        public void TestUpdate()
        {
            using (var db = new DbContext())
            {
                var order = db.Orders.FirstOrDefault();
                order.ShipName = null;
                db.Orders.Update(order);
            }
        }

        [TestMethod]
        public void TestUpdateByExtension()
        {
            using (var db = new DbContext())
            {
                db.Orders
                    .AssertWhere(true, s => s.EmployeeID != 1L)
                    .Where(s => s.OrderDate > DateTime.Now)
                    .Where(s => false)
                    .OrderBy(s => s.CustomerID)
                    .Update(() => new Orders { CustomerID = "aa" });

                db.Orders
                    .AssertWhere(true, s => s.EmployeeID != 1L)
                    .Where(s => s.OrderDate > DateTime.Now)
                    .Where(s => false)
                    .OrderBy(s => s.CustomerID)
                    .Update(p => { p.CustomerID = "aa"; });
            }
        }

        [TestMethod]
        public void TestUpdateNoEvents()
        {
            using (var db = new DbContext())
            {
                var order = db.Orders.FirstOrDefault();
                order.ShipName = null;
                //db.Orders.ConfigOptions(s => s.NotifyEvents = false).Update(order);
            }
        }

        [TestMethod]
        public void TestUpdateNoValidate()
        {
            using (var db = new DbContext())
            {
                var order = db.Orders.FirstOrDefault();
                order.ShipName = "211111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111";
                //db.Orders.ConfigOptions(s => s.ValidateEntity = true).Update(order);
            }
        }

        [TestMethod]
        public void TestUpdateByNewEntity()
        {
            using (var db = new DbContext())
            {
                var order = new Orders { OrderID = 11092 };
                order.ShipName = "fireasy";
                db.Orders.Update(order);
            }
        }

        [TestMethod]
        public void TestUpdateByWrap()
        {
            using (var db = new DbContext())
            {
                var order = db.Wrap(() => new Orders { OrderID = 11092 });
                order.ShipName = "fireasy";
                db.Orders.Update(order);
            }
        }

        [TestMethod]
        public void TestUpdateReference()
        {
            using (var db = new DbContext())
            {
                var detail = db.OrderDetails.FirstOrDefault();
                detail.Orders.ShipName = "fireasy";
                db.OrderDetails.Update(detail);
            }
        }

        [TestMethod]
        public void TestUpdateSecondReference()
        {
            using (var db = new DbContext())
            {
                var order = db.Orders.FirstOrDefault();

                if (order.OrderDetailses.Count > 0)
                {
                    order.OrderDetailses[0].Products.UnitsInStock = 45;
                }

                db.Orders.Update(order);
            }
        }

        [TestMethod]
        public void TestUpdateBytes()
        {
            using (var db = new DbContext())
            {
                var sss = new List<Products>();

                for (var i = 0; i < 2; i++)
                {
                    var product = db.New<Products>(); ;

                    //product.Photo = new byte[] { 45, 55, 34, 67, 133, 54, 213 };
                    sss.Add(product);
                }

                db.Products.Batch(sss, (u, s) => u.Insert(s));
            }
        }

        [TestMethod]
        public void TestUpdateByNoPrimary()
        {
            using (var db = new DbContext())
            {
                var order = db.Orders.FirstOrDefault();
                order.ShipName = null;
                db.Orders.Update(order);
            }
        }

        [TestMethod]
        public void TestRemoveAndAddDetail()
        {
            using (var db = new DbContext())
            {
                var order = db.Orders.Get(11092);

                order.OrderDetailses.RemoveAt(0);
                order.OrderDetailses.Add(new OrderDetails { Product1ID = 1 });

                db.Orders.Update(order);
            }
        }

        [TestMethod]
        public void TestRemoveDetail()
        {
            using (var db = new DbContext())
            {
                var order = db.Orders.Get(11092);

                order.OrderDetailses.RemoveAt(0);

                db.Orders.Update(order);
            }
        }

        [TestMethod]
        public void TestClearDetail()
        {
            using (var db = new DbContext())
            {
                var order = db.Orders.Get(11092);

                order.OrderDetailses = null;

                db.Orders.Update(order);
            }
        }

        [TestMethod]
        public async Task TestUpdateAsync()
        {
            using (var db = new DbContext())
            {
                var order = await db.Orders.OrderBy(s => s.OrderDate).FirstOrDefaultAsync(s => true);
                order.ShipName = "cc" + new Random(DateTime.Now.Millisecond).Next(100);
                await db.Orders.UpdateAsync(order);
                Console.WriteLine("后续代码");
            }
        }

        [TestMethod]
        public void TestUpdateWhere()
        {
            using (var db = new DbContext())
            {
                db.Orders.Update(() => new Orders { Freight = 1 }, s => s.OrderDate >= DateTime.Now);
            }
        }

        [TestMethod]
        public void TestUpdateWhereByObject()
        {
            using (var db = new DbContext())
            {
                var order = new Orders { Freight = 1 };
                db.Orders.Update(order, s => s.OrderDate >= DateTime.Now);
            }
        }

        [TestMethod]
        public async Task TestUpdateWhereByObjectAsync()
        {
            using (var db = new DbContext())
            {
                var order = new Orders { Freight = 1 };
                await db.Orders.UpdateAsync(order, s => s.OrderDate >= DateTime.Now);
            }
        }

        [TestMethod]
        public void TestUpdatByCalculator()
        {
            using (var db = new DbContext())
            {
                db.Orders.Update(s => new Orders { Freight = s.Freight * 100 }, s => s.OrderDate >= DateTime.Now);
            }
        }

        [TestMethod]
        public async Task TestUpdatByCalculatorAsync()
        {
            using (var db = new DbContext())
            {
                await db.Orders.UpdateAsync(s => new Orders { Freight = s.Freight * 100 }, s => s.OrderDate >= DateTime.Now);
            }
        }

        [TestMethod]
        public void TestDelete()
        {
            using (var db = new DbContext())
            {
                var order = db.Orders.Get(-1);
                if (order != null)
                {
                    db.Orders.Delete(order, true);
                }
            }
        }

        [TestMethod]
        public void TestDeleteByPrimaryKeys()
        {
            using (var db = new DbContext())
            {
                db.Orders.Delete(-1);
                db.OrderDetails.Delete(-1, 1);
            }
        }

        [TestMethod]
        public void TestDeleteWhere()
        {
            using (var db = new DbContext())
            {
                db.Orders.Delete(s => s.OrderDate > DateTime.Now);
            }
        }

        [TestMethod]
        public void TestDeleteByExtension()
        {
            using (var db = new DbContext())
            {
                db.Orders
                    .Where(s => s.OrderDate > DateTime.Now)
                    .Delete();
            }
        }

        [TestMethod]
        public async Task TestDeleteWhereAsync()
        {
            using (var db = new DbContext())
            {
                await db.Customers.DeleteAsync(s => false);
            }
        }

        [TestMethod]
        public void TestBatchInsert()
        {
            using (var db = new DbContext())
            {
                var list = new List<Orders>();

                for (var i = 0; i < 10; i++)
                {
                    var d = db.New<Orders>();
                    //d.ProductName = "aa" + i;
                    //d.Discontinued = 0;
                    d.CustomerID = "dfsdf";
                    d.OrderDate = DateTime.Now;
                    list.Add(d);
                }

                //list[6].QuantityPerUnit = "dd";

                db.Orders.Batch(list, (u, s) => u.Insert(s), new BatchOperateOptions(BatchCheckModifiedKinds.Everyone));

                //var p = db.Wrap(() => new Products { ProductName = "b11", Discontinued = 0 });
                //db.Products.Insert(p);
                //Console.WriteLine(p.ProductID);
            }
        }

        [TestMethod]
        public async Task TestBatchInsertAsync()
        {
            using (var db = new DbContext())
            {
                var list = new List<Products>();

                for (var i = 0; i < 3; i++)
                {
                    var d = db.New<Products>();
                    d.ProductName = "aa1111";
                    d.Discontinued = 0;
                    list.Add(d);
                }

                await db.Products.BatchAsync(list, (u, s) => u.Insert(s));
            }
        }

        [TestMethod]
        public void TestBatchUpdate()
        {
            using (var db = new DbContext())
            {
                var list = new List<Depts>();

                for (var i = 0; i < 3; i++)
                {
                    var d = db.Wrap(() => new Depts { DeptID = i + 50, DeptName = "a" + i });
                    list.Add(d);
                }

                list[1].DeptCode = "test";

                db.Depts.Batch(list, (u, s) => u.Update(s));
            }
        }

        [TestMethod]
        public async Task TestBatchUpdateAsync()
        {
            using (var db = new DbContext())
            {
                var list = new List<Depts>();

                for (var i = 0; i < 3; i++)
                {
                    var d = db.Wrap(() => new Depts { DeptID = i + 50, DeptName = "a" + i });
                    list.Add(d);
                }

                list[1].DeptCode = "test";

                await db.Depts.BatchAsync(list, (u, s) => u.Update(s));
            }
        }

        [TestMethod]
        public void TestBatchDelete()
        {
            using (var db = new DbContext())
            {
                var list = new List<Products>();

                for (var i = 0; i < 3; i++)
                {
                    var d = db.New<Products>();
                    d.Id = 1;
                    //d.Orders = null;
                    list.Add(d);
                }

                db.Products.Batch(list, (u, s) => u.Delete(s, true));
                Console.WriteLine(11);
            }
        }

        [TestMethod]
        public async Task TestBatchDeleteAsync()
        {
            using (var db = new DbContext())
            {
                var list = new List<Products>();

                for (var i = 0; i < 3; i++)
                {
                    var d = db.New<Products>();
                    d.Id = 100 + i;
                    //d.Orders = null;
                    list.Add(d);
                }

                await db.Products.BatchAsync(list, (u, s) => u.Delete(s, true));
                Console.WriteLine(11);
            }
        }

        [TestMethod]
        public void TestDataBatchInsert()
        {
            using (var db = new DbContext())
            {
                var list = new List<Products>();

                for (var i = 0; i < 10; i++)
                {
                    var p = db.New<Products>();
                    p.ProductName = "aa";
                    p.Discontinued = 0;
                    list.Add(p);
                }

                db.Products.BatchInsert(list);
            }
        }

        [TestMethod]
        public async Task TestDataBatchInsertAsync()
        {
            using (var db = new DbContext())
            {
                var list = new List<Products>();

                for (var i = 0; i < 10; i++)
                {
                    var p = db.New<Products>();
                    p.ProductName = "aa";
                    p.Discontinued = 0;
                    list.Add(p);
                }

                await db.Products.BatchInsertAsync(list);
            }
        }

        [TestMethod]
        public void TestExpression()
        {
            using (var context = new DbContext())
            {
                var lambda = Builder.GetLambda<Products>(s => s.ProductName == "fireasy");

                var q1 = Where(context.Products, lambda);

                var list = q1.ToList();

                Console.WriteLine(list.Count);
            }
        }

        [TestMethod]
        public void TestCustomBind()
        {
            using (var context = new DbContext())
            {
                var list = context.Products.Where(s => RightString(s.ProductName, 12) == "ee");

                foreach (var item in list)
                {
                    Console.WriteLine(item.ProductName);
                }
            }
        }

        [MethodCallBind(typeof(Funcs.RightStringBinder))]
        public string RightString(string str, int length)
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestInBind()
        {
            using (var context = new DbContext())
            {
                var userId = 8;

                //判断 deptId 是否在用户的数据权限范围之内
                var list = context.Depts.Where(s => Funcs.CheckDataPermission(userId, (int)s.DeptID));

                foreach (var item in list)
                {
                    Console.WriteLine(item.DeptName);
                }
            }
        }

        [TestMethod]
        public void TestFunction()
        {
            using (var context = new DbContext())
            {
                //调用自定义函数
                var list = context.Products.Where(s => Funcs.TestFunc(3, s.Id));

                foreach (var item in list)
                {
                    Console.WriteLine(item.ProductName);
                }
            }
        }

        [TestMethod]
        public void TestOverrideBind()
        {
            TranslateUtils.AddMethodBinder<ConvertBinder>(m => m.DeclaringType == typeof(Convert) && m.Name == "ToInt32");

            using (var context = new DbContext())
            {
                var list = context.OrderDetails.Where(s => Convert.ToInt32(s.Discount) == 1).ToList();
            }
        }

        [TestMethod]
        public void TestComplexSelect()
        {
            using (var context = new DbContext())
            {
                var func = new Func<string, string>(s => s.Substring(0, 1));

                var list = context.OrderDetails
                    .Take(2)
                    .Select(s => new OrderResult(s.Orders.OrderDate.Value.ToLongDateString())
                    {
                        //Date = s.Orders.OrderDate.Value.ToLongDateString(),
                        RecorderLevel = s.Products.ReorderLevel.GetDescription(),
                        Phone = s.Orders.Customers.Phone,
                        FirstName = func(s.Orders.Customers.CompanyName),
                        ShortDate = s.Orders.OrderDate.Value.ToString("G")
                    });

                foreach (var item in list)
                {
                    Console.WriteLine(item);
                }
            }
        }

        [TestMethod]
        public void TestInvokeSelect()
        {
            using (var context = new DbContext())
            {
                var func = new Func<OrderDetails, object>(s => new { s.Product1ID, s.OrderID });

                var list = context.OrderDetails
                    .Take(2)
                    .Select(func);

                foreach (var item in list)
                {
                    Console.WriteLine(item);
                }
            }
        }

        [TestMethod]
        public void TestGeneralWhere()
        {
            using (var context = new DbContext())
            {
                DateTime? startTime = null;
                DateTime? endTime = null;
                var state = 0;

                IQueryable<Orders> query = context.Orders;
                if (startTime != null)
                {
                    query = query.Where(s => s.OrderDate >= startTime);
                }
                if (endTime != null)
                {
                    query = query.Where(s => s.OrderDate <= endTime);
                }
                if (state == 0)
                {
                    query = query.Where(s => s.RequiredDate == DateTime.Now);
                }
                else
                {
                    query = query.Where(s => s.RequiredDate >= DateTime.Now);
                }

                foreach (var item in query)
                {
                    Console.WriteLine(item);
                }
            }
        }

        [TestMethod]
        public void TestAssertWhere()
        {
            using (var context = new DbContext())
            {
                DateTime? startTime = null;
                DateTime? endTime = null;
                var state = 0;

                var list = context.Orders
                    .AssertWhere(startTime != null, s => s.OrderDate >= startTime)
                    .AssertWhere(endTime != null, s => s.OrderDate <= endTime)
                    .AssertWhere(state == 0, s => s.RequiredDate == DateTime.Now, s => s.RequiredDate >= DateTime.Now);

                foreach (var item in list)
                {
                    Console.WriteLine(item);
                }
            }
        }

        [TestMethod]
        public void TestSwitchWhere()
        {
            using (var context = new DbContext())
            {
                var value = "aa";

                var list = context.Orders
                    .SwitchWhere(value, b => b
                        .When("aa", s => s.ShipCity == "kunming")
                        .When("bb", s => s.ShipName == "fireasy")
                        .When("cc", s => s.OrderDate >= DateTime.Today));

                foreach (var item in list)
                {
                    Console.WriteLine(item);
                }
            }
        }

        [TestMethod]
        public void TestOrderByExtend()
        {
            using (var context = new DbContext())
            {
                var sorting = new SortDefinition();
                sorting.Member = "OrderDate";
                sorting.Order = SortOrder.Descending;

                var list = context.Orders
                    .Select(s => new { s.OrderDate, CompanyName = s.Customers.CompanyName })
                    .OrderBy(sorting)
                    .ToList();

                Console.WriteLine(list.Count);
            }
        }

        [TestMethod]
        public void TestOrderByExtendDefault()
        {
            using (var context = new DbContext())
            {
                var sorting = new SortDefinition();
                //sorting.Member = "OrderDate";
                //sorting.Order = SortOrder.Descending;

                var list = context.Orders
                    .Select(s => new { s.OrderDate, CompanyName = s.Customers.CompanyName })
                    .OrderBy(sorting, u => u.OrderByDescending(s => s.OrderDate))
                    .ToList();

                Console.WriteLine(list.Count);
            }
        }

        [TestMethod]
        public void TestOrderByExtendReplace()
        {
            using (var context = new DbContext())
            {
                var sorting = new SortDefinition();
                sorting.Member = "ReorderLevelName";
                //sorting.Replace("ReorderLevelName", "Products.ReorderLevel");
                sorting.Replace<OrderDetails>("ReorderLevelName", s => s.Products.ReorderLevel);

                var list = context.OrderDetails
                    .Select(s => s.ExtendAs<OrderDetails>(() => new OrderDetails
                    {
                        ReorderLevelName = s.Products.ReorderLevel.GetDescription(),
                    }))
                    .OrderBy(sorting)
                    .ToList();

                Console.WriteLine(list.Count);
            }
        }

        [TestMethod]
        public void TestOrderByExtend1()
        {
            using (var context = new DbContext())
            {
                var sorting = new SortDefinition();
                sorting.Member = "CompanyName";

                var list = context.Orders
                    .Select(s => new { s.OrderDate, CompanyName = s.Customers.CompanyName })
                    .OrderBy(sorting)
                    .ToList();

                Console.WriteLine(list.Count);
            }
        }

        [TestMethod]
        public void TestOrderByExtend2()
        {
            using (var context = new DbContext())
            {
                var sorting = new SortDefinition();
                sorting.Member = "ProductName";

                var list = context.OrderDetails
                    .Select(s => s.ExtendAs<OrderDetailsEx>(() => new OrderDetailsEx { ProductName = s.Orders.CustomerID }))
                    .OrderBy(sorting)
                    .ToList();

                Console.WriteLine(list.Count);
            }
        }

        [TestMethod]
        public void TestExecuteSql()
        {
            using (var context = new DbContext())
            {
                var d = new DataPager(10, 0);
                var dt = context.Database.ExecuteEnumerable<Customers>((SqlCommand)"select * from customers", d);
            }
        }

        [TestMethod]
        public async Task TestExecuteSqlAsync()
        {
            using (var context = new DbContext())
            {
                var d = new DataPager(10, 0);
                var dt = await context.Database.ExecuteEnumerableAsync<dynamic>((SqlCommand)"select * from OperateLog", d);
            }
        }

        [TestMethod]
        public void TestTransaction()
        {
            //using (var scope = new EntityTransactionScope())
            using (var context = new DbContext())
            {
                context.Products.FirstOrDefault();
                //TestAnyMethod(context.Database);
                //scope.Complete();
            }
        }

        [TestMethod]
        public void TestBatchOr()
        {
            var countries = new string[] { "France", "Spain" };
            using (var context = new DbContext())
            {
                var list = context.Customers.BatchOr(countries, (s, t) => s.Country.Contains(t)).ToList();
                Assert.AreEqual(16, list.Count);
            }
        }

        [TestMethod]
        public void TestBatchAnd()
        {
            var countries = new string[] { "France", "Spain" };
            using (var context = new DbContext())
            {
                var list = context.Customers.BatchAnd(countries, (s, t) => s.Country.Contains(t)).ToList();
                Assert.AreEqual(0, list.Count);
            }
        }

        [TestMethod]
        public void TestEntitySet()
        {
            using (var context = new DbContext())
            {
                var ss = context.Orders.Take(5).Select(s => new Orders { OrderID = s.OrderID, OrderDetailses = s.OrderDetailses.Take(1).ToEntitySet() }).ToList();
            }
        }

        private string key1 = "Lond";
        private string key2 = "Lond";

        [TestMethod]
        public void TestTranslateCache()
        {
            InnerTestTranslateCache(key1);
        }

        private void InnerTestTranslateCache(string str)
        {
            using (var context = new DbContext())
            {
                var ids1 = context.Customers.Where(s => s.City == str).Select(s => s.CustomerID);
                var ids2 = context.Customers.Where(s => s.City == "AA").Select(s => s.CustomerID);

                var customer1 = context.Orders.Where(s => s.CustomerID == str && ids1.Contains(s.CustomerID)).FirstOrDefault();
                var customer2 = context.Orders.Where(s => ids2.Contains(s.CustomerID)).FirstOrDefault();
                var customer3 = context.Orders.Where(s => ids1.Contains(s.CustomerID)).FirstOrDefault();
                var customer4 = context.Orders.Where(s => ids2.Contains(s.CustomerID)).FirstOrDefault();
            }
        }

        [TestMethod]
        public void TestXmlSerialize()
        {
            using (var context = new DbContext())
            {
                var customer = context.Customers.FirstOrDefault();

                var ser = new XmlSerializer();
                var str = ser.Serialize(customer);
                Console.WriteLine(str);
            }
        }

        [TestMethod]
        public void TestBinSerialize()
        {
            using (var context = new DbContext())
            {
                var customer = context.Customers.FirstOrDefault();

                var ser = new BinaryFormatter();
                using (var m = new MemoryStream())
                {
                    ser.Serialize(m, customer);
                }
            }
        }

        [TestMethod]
        public void TestNewtonsoftSerialize()
        {
            using (var context = new DbContext())
            {
                var customer = context.Customers.FirstOrDefault();

                var str = JsonConvert.SerializeObject(customer, new Newtonsoft.LazyObjectJsonConverter());
                Console.WriteLine(str);
            }
        }

        [TestMethod]
        public void TestSubscriberForBatch()
        {
            PersistentSubscribeManager.AddSubscriber(subject =>
            {
                new MyEntityPersistentSubscriber().Accept(subject);
            });

            using (var db = new DbContext())
            {
                var list = new List<Depts>();

                for (var i = 0; i < 3; i++)
                {
                    var d = db.New<Depts>();
                    d.DeptID = i + 50;
                    d.DeptName = "a" + i;
                    list.Add(d);
                }

                list[1].DeptCode = "test";

                db.Depts.Batch(list, (u, s) => u.Update(s));
            }

        }

        [TestMethod]
        public void TestSubscriberForUpdate()
        {
            PersistentSubscribeManager.AddSubscriber(subject =>
            {
                new MyEntityPersistentSubscriber().Accept(subject);
            }, typeof(Orders));

            using (var db = new DbContext())
            {
                var order = db.Orders.FirstOrDefault();
                order.ShipName = "12443";
                db.Orders.Update(order);
            }
        }

        [TestMethod]
        public void TestSubscriberForAsyncUpdate()
        {
            PersistentSubscribeManager.AddAsyncSubscriber(subject => new MyAsyncEntityPersistentSubscriber().AcceptAsync(subject));

            using (var db = new DbContext())
            {
                var order = db.Orders.FirstOrDefault();
                order.ShipName = "1235";
                db.Orders.Update(order);
            }
        }

        [TestMethod]
        public void TestSubscriberForUpdateLinq()
        {
            PersistentSubscribeManager.AddSubscriber(subject =>
            {
                new MyEntityPersistentSubscriber().Accept(subject);
            });

            using (var db = new DbContext())
            {
                db.Orders.Update(new Orders { ShipName = "1" }, s => s.ShipName == "1");
            }
        }

        private class MyEntityPersistentSubscriber : PersistentSubscriber
        {
            protected override bool OnBeforeUpdate(IEntity entity)
            {
                return true;
            }

            protected override void OnAfterCreate(IEntity entity)
            {
                Log(entity, PersistentOperator.Create);
            }

            protected override void OnAfterUpdate(IEntity entity)
            {
                Log(entity, PersistentOperator.Update);
            }

            protected override void OnAfterRemove(IEntity entity)
            {
                Log(entity, PersistentOperator.Remove);
            }

            protected override void OnCreate(Type entityType)
            {
                ClearCache(entityType);
            }

            protected override void OnUpdate(Type entityType)
            {
                ClearCache(entityType);
            }

            protected override void OnRemove(Type entityType)
            {
                ClearCache(entityType);
            }

            /// <summary>
            /// 清理与实体类型相关的缓存。
            /// </summary>
            /// <param name="entityType"></param>
            private void ClearCache(Type entityType)
            {
                //约定数据缓存都以实体类型名称作为前缀，方便清理
                var pattern = entityType.Name + ":*";
                var cacheMgr = CacheManagerFactory.CreateManager();
                foreach (var key in cacheMgr.GetKeys(pattern))
                {
                    cacheMgr.Remove(key);
                }
            }

            /// <summary>
            /// 记录操作日志。
            /// </summary>
            /// <param name="entity"></param>
            /// <param name="operater"></param>
            private void Log(IEntity entity, PersistentOperator operater)
            {

                var sb = new StringBuilder();
                if (operater == PersistentOperator.Create)
                {
                    sb.Append("新增数据");
                }
                else if (operater == PersistentOperator.Update)
                {
                    sb.Append("修改数据");
                    foreach (var p in entity.GetModifiedProperties())
                    {
                        sb.Append($",{p} 由 {entity.GetOldValue(p)} 修改为 {entity.GetValue(p)}");
                    }
                }
                else if (operater == PersistentOperator.Remove)
                {
                    //获取主键
                    var pk = PropertyUnity.GetPrimaryProperties(entity.EntityType).FirstOrDefault();
                    sb.Append($"删除数据，主键为 {entity.GetValue(pk)}");
                }

                //记录日志
            }
        }

        private class MyAsyncEntityPersistentSubscriber : AsyncPersistentSubscriber
        {
            protected override async Task<bool> OnBeforeUpdateAsync(IEntity entity)
            {
                return true;
            }

            protected override async Task OnAfterUpdateAsync(IEntity entity)
            {
            }
        }

        public interface IBaseModel
        {
            int CreateUserId { get; set; }

            DateTime CreateTime { get; set; }
        }

        private void TestAnyMethod(IDatabase db)
        {
            //事件期间，不同的DbContext对象共用一个IDatabase对象
            using (var context = new DbContext())
            {
                Assert.IsNotNull(context.Database.Transaction);
                context.BeginTransaction();
                Assert.AreEqual(db, context.Database);
                context.CommitTransaction();
            }
        }

        private class OrderResult
        {
            public OrderResult(string date)
            {
                Date = date;
            }
            public string Date { get; set; }
            public string RecorderLevel { get; set; }
            public string Phone { get; set; }
            public string FirstName { get; set; }
            public string ShortDate { get; set; }
        }

        public class Builder
        {
            public static LambdaExpression GetLambda<T>(Expression<Func<T, bool>> expression)
            {
                return expression;
            }
        }

        private IQueryable<T> Where<T>(IQueryable<T> queryable, LambdaExpression expression)
        {
            var where = Expression.Call(typeof(Queryable), "Where", new[] { typeof(T) }, queryable.Expression, expression);

            return (IQueryable<T>)queryable.Provider.CreateQuery(where);
        }
    }

    /// <summary>
    /// 自定义函数库。
    /// </summary>
    public static class Funcs
    {
        /// <summary>
        /// 取字符串的右边n个字符。
        /// </summary>
        /// <param name="str">字符串。</param>
        /// <param name="length">长度。</param>
        /// <returns></returns>
        [MethodCallBind(typeof(RightStringBinder))]
        public static string RightString(this string str, int length)
        {
            throw new InvalidOperationException("只能在Linq中使用。");
        }

        [FunctionBind("testfun")]
        public static bool TestFunc(int a1, long a2)
        {
            throw new InvalidOperationException("只能在Linq中使用。");
        }

        /// <summary>
        /// 检查用户的数据权限。
        /// </summary>
        /// <param name="userId">用户ID。</param>
        /// <param name="deptId">科室ID。</param>
        /// <returns></returns>
        [MethodCallBind(typeof(CheckDataPermissionBinder))]
        public static bool CheckDataPermission(int userId, int deptId)
        {
            throw new InvalidOperationException("只能在Linq中使用。");
        }

        /// <summary>
        /// 方法 RightString 的绑定。
        /// </summary>
        public class RightStringBinder : IMethodCallBinder
        {
            public Expression Bind(MethodCallBindContext context)
            {
                var arguments = context.Visitor.Visit(context.Expression.Arguments);

                // ret = str.Substring(str.Length - length, length)
                var lenExp = Expression.MakeMemberAccess(arguments[0], typeof(string).GetProperty("Length"));
                var startExp = Expression.Subtract(lenExp, arguments[1]);
                return Expression.Call(arguments[0], "Substring", new Type[0], startExp, arguments[1]);
            }
        }

        /// <summary>
        /// 方法 CheckDataPermission 的绑定。
        /// </summary>
        private class CheckDataPermissionBinder : IMethodCallBinder
        {
            public Expression Bind(MethodCallBindContext context)
            {
                var sql = @"
              select
                t.dept_id
              from
                system_dept_permission t
              where
                role_id in (
                  select role_id
                  from system_manager_role t
                  where t.user_id = {0})
              union (
                select t.dept_id
                from system_user t
                where user_id = {0}
              )";

                var arguments = context.Visitor.Visit(context.Expression.Arguments);
                var userId = (arguments[0] as ConstantExpression).Value.ToString();
                var sqlExp = new SqlExpression(string.Format(sql, userId));
                return new InExpression(arguments[1], new Expression[] { sqlExp });
            }
        }
    }

    public class ConvertBinder : IMethodCallBinder
    {
        public Expression Bind(MethodCallBindContext context)
        {
            return new SqlExpression("(cast discount as integer)", typeof(int));
        }
    }
}
