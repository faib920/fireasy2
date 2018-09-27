using Fireasy.Common.Caching;
using Fireasy.Common.Extensions;
using Fireasy.Common.Serialization;
using Fireasy.Data.Entity.Linq;
using Fireasy.Data.Entity.Linq.Expressions;
using Fireasy.Data.Entity.Linq.Translators;
using Fireasy.Data.Entity.Subscribes;
using Fireasy.Data.Entity.Tests.Models;
using Fireasy.Data.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization.Formatters.Binary;

namespace Fireasy.Data.Entity.Tests
{
    [TestClass]
    public class EntityRespositoryTest
    {
        public EntityRespositoryTest()
        {
            InitConfig.Init();
        }

        [TestMethod]
        public void TestGet()
        {
            using (var context = new DbContext())
            {
                var product = context.Products.Get(12);
                int? a = null;
                var product1 = context.Products.Get(a);
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
                Assert.AreEqual(42, detail.ProductID);
            }
        }

        [TestMethod]
        public void TestLazyLoad()
        {
            OrderDetails detail;
            using (var db = new DbContext())
            {
                detail = db.OrderDetails.FirstOrDefault();
            }

            Assert.AreEqual("VINET", detail.Orders.CustomerID);
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

        [TestMethod]
        public void TestIncludeCascade()
        {
            using (var db = new DbContext())
            {
                var details = db.OrderDetails
                            .Include(s => s.Orders.Customers)
                            .Take(3);

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

        public Products TestCache2()
        {
            var cacheMgr = CacheManagerFactory.CreateManager();

            using (var db = new DbContext())
            {
                return GetProduct(() => db.Products.FirstOrDefault());
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
                            .FirstOrDefault(s => s.OrderID == 1 && s.ProductID == 1);

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
        public void TestSelectAll()
        {
            using (var db = new DbContext())
            {
                var list = db.OrderDetails.Select(s => new OrderDetailsEx
                {
                    OrderID = s.OrderID,
                    ProductID = s.ProductID,
                    Quantity = s.Quantity,
                    UnitPrice = s.UnitPrice,
                    Discount = s.Discount,
                    ProductName = s.Products.ProductName
                })
                    .ToList();

                Assert.AreEqual("Queso Cabrales", list[0].ProductName);

                var ss = db.OrderDetails.Select(s => new { s.ProductID, a = db.Orders.FirstOrDefault(t => t.OrderID == s.OrderID).OrderDate }).ToList();
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
                        ProductName = s.Products.ProductName
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
                    .Where(s => s.s.OrderID != 1)
                    .Select(s =>
                        s.s.ExtendAs<OrderDetailsEx>(() => new OrderDetailsEx
                        {
                            ProductName = s.s.Products.ProductName,
                            OrderDesc = s.t.OrderDate.ToString()
                        }))
                    .Distinct()
                    .ToList();

                Assert.AreEqual("Queso Cabrales", list[0].ProductName);
            }
        }

        [TestMethod]
        public void TestJoinExtendAs()
        {
            using (var db = new DbContext())
            {
                var list = db.OrderDetails
                    .Join(db.Products.DefaultIfEmpty(),
                        s => s.ProductID, s => s.ProductID, (s, t) => new { detail = s, product = t })
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

        public class OrderDetailsEx : OrderDetails
        {
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
                var customer = new Customers
                {
                    CustomerID = "DD",
                    CompanyName = "kunming",
                    City = "kunming"
                };

                db.Customers.Insert(customer);
            }
        }

        [TestMethod]
        public void TestInsertByFactory()
        {
            using (var db = new DbContext())
            {
                db.Customers.Insert(() => new Customers
                {
                    CustomerID = "DD",
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
                var customer = Customers.New();
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
                var customer = Customers.Wrap(() => new Customers
                {
                    CustomerID = "F3",
                    CompanyName = "kunming",
                    City = "kunming"
                });

                db.Customers.Insert(customer);
            }
        }

        [TestMethod]
        public void TestInsertWithDetails()
        {
            var order = new Orders
            {
                OrderDate = DateTime.Now,
                OrderDetailses = new EntitySet<OrderDetails>(),
            };

            //构造5条明细
            for (var i = 0; i < 5; i++)
            {
                order.OrderDetailses.Add(new OrderDetails
                {
                    ProductID = 1 + i,
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
        public void TestUpdateNoEvents()
        {
            using (var db = new DbContext())
            {
                var order = db.Orders.FirstOrDefault();
                order.ShipName = null;
                db.Orders.ConfigOptions(s => s.NotifyEvents = false).Update(order);
            }
        }

        [TestMethod]
        public void TestUpdateNoValidate()
        {
            using (var db = new DbContext())
            {
                var order = db.Orders.FirstOrDefault();
                order.ShipName = "211111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111";
                db.Orders.ConfigOptions(s => s.ValidateEntity = false).Update(order);
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
                var order = Orders.Wrap(() => new Orders { OrderID = 11092 });
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
                    var product = Products.New(); ;

                    product.Photo = new byte[] { 45, 55, 34, 67, 133, 54, 213 };
                    sss.Add(product);
                }

                db.Products.Batch(sss, (u, s) => u.Insert(s));
            }
        }

        [TestMethod]
        public void TestRemoveAndAddDetail()
        {
            using (var db = new DbContext())
            {
                var order = db.Orders.Get(11092);

                order.OrderDetailses.RemoveAt(0);
                order.OrderDetailses.Add(new OrderDetails { ProductID = 1 });

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
        public void TestUpdateAsync()
        {
            using (var db = new DbContext())
            {
                var order = db.Orders.FirstOrDefault();
                order.ShipName = "cc" + new Random(DateTime.Now.Millisecond).Next(100);
                db.Orders.UpdateAsync(order);
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
        public void TestUpdatByCalculator()
        {
            using (var db = new DbContext())
            {
                db.Orders.Update(s => new Orders { Freight = s.Freight * 100 }, s => s.OrderDate >= DateTime.Now);
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
                db.Database.Log = (c, p) => Console.WriteLine(c.Output());

                db.Orders.Delete(s => s.OrderDate > DateTime.Now);
            }
        }

        [TestMethod]
        public void TestBatchInsert()
        {
            using (var db = new DbContext())
            {
                var list = new List<Products>();

                for (var i = 0; i < 3; i++)
                {
                    var d = Products.New();
                    //d.ProductID = 944 + i;
                    d.ProductName = "aa1111";
                    //d.Orders = null;
                    d.Discontinued = true;
                    list.Add(d);
                }

                db.Products.Batch(list, (u, s) => u.Insert(s));
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
                    var d = Depts.Wrap(() => new Depts { DeptID = i + 50, DeptName = "a" + i });
                    list.Add(d);
                }

                list[1].DeptCode = "test";

                db.Depts.Batch(list, (u, s) => u.Update(s));
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
                    var d = Products.New();
                    d.ProductID = 1;
                    //d.Orders = null;
                    list.Add(d);
                }

                db.Products.Batch(list, (u, s) => u.Delete(s, true));
                Console.WriteLine(11);
            }
        }

        [TestMethod]
        public void TestDataBatchInsert()
        {
            using (var db = new DbContext())
            {
                var list = new List<Orders>();

                for (var i = 0; i < 10000; i++)
                {
                    var order = Orders.New();
                    order.OrderDate = DateTime.Now;
                    list.Add(order);
                }

                db.Orders.BatchInsert(list);
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
                var list = context.Products.Where(s => Funcs.TestFunc(3, s.ProductID));

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
                    .Select(s => new OrderResult
                    {
                        Date = s.Orders.OrderDate.Value.ToLongDateString(),
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
                var func = new Func<OrderDetails, object>(s => new { s.ProductID, s.OrderID });

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
                sorting.Replace("ReorderLevelName", "Products.ReorderLevel");

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
                var dt = context.Database.ExecuteDataTable((SqlCommand)"select * from products", null, d);
            }
        }

        [TestMethod]
        public void TestTransaction()
        {
            using (var scope = new EntityTransactionScope())
            using (var context = new DbContext())
            {
                TestAnyMethod(context.Database);
                scope.Complete();
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
            EntityPersistentSubscribeManager.AddSubscriber(subject =>
            {
                new EntitySubscriber().Accept(subject);
            });

            using (var db = new DbContext())
            {
                var list = new List<Depts>();

                for (var i = 0; i < 3; i++)
                {
                    var d = Depts.New();
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
            EntityPersistentSubscribeManager.AddSubscriber(subject =>
            {
                new EntitySubscriber().Accept(subject);
            });

            using (var db = new DbContext())
            {
                var order = db.Orders.FirstOrDefault();
                order.ShipName = "2";
                db.Orders.Update(order);
            }
        }

        [TestMethod]
        public void TestSubscriberForUpdateLinq()
        {
            EntityPersistentSubscribeManager.AddSubscriber(subject =>
            {
                new EntitySubscriber().Accept(subject);
            });

            using (var db = new DbContext())
            {
                db.Orders.Update(new Orders { ShipName = "1" }, s => s.ShipName == "1");
            }
        }

        private class EntitySubscriber : EntityPersistentSubscriber
        {
            protected override void OnBeforeUpdate(IEntity entity)
            {
                Console.WriteLine(entity);
            }

            protected override void OnUpdate(Type entityType)
            {
                Console.WriteLine(entityType);
            }

            protected override void OnBeforeBatch(IEnumerable<IEntity> entities, EntityPersistentOperater operater)
            {
                Console.WriteLine(operater);
            }
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
