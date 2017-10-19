using Fireasy.Common.Extensions;
using Fireasy.Data.Entity.Linq;
using Fireasy.Data.Entity.Linq.Expressions;
using Fireasy.Data.Entity.Linq.Translators;
using Fireasy.Data.Entity.Tests.Models;
using Fireasy.Data.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Tests
{
    [TestClass]
    public class EntityRespositoryTest
    {
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
                Console.WriteLine(dept.Attributes);
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
                var list = db.OrderDetails.Select(s =>
                    s.ExtendAs<OrderDetailsEx>(() => new OrderDetailsEx
                    {
                        ProductName = s.Products.ProductName
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

        public class OrderDetailsEx : OrderDetails
        {
            public string ProductName { get; set; }

            public IEnumerable<string> List { get; set; }

            public int Count { get; set; }
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
                    UnitPrice = 0.3,
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
                var list = new List<Depts>();

                for (var i = 0; i < 3; i++)
                {
                    var d = Depts.New();
                    d.DeptName = "测试" + i;
                    list.Add(d);
                }

                db.Depts.Batch(list, (u, s) => u.Insert(s));
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
                //使用 string 的自定义扩展方法筛选以 ee 结尾的数据
                var list = context.Products.Where(s => s.ProductName.RightString(2) == "ee");

                foreach (var item in list)
                {
                    Console.WriteLine(item.ProductName);
                }
            }
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
        private class RightStringBinder : IMethodCallBinder
        {
            public Expression Bind(DbExpressionVisitor visitor, MethodCallExpression callExp)
            {
                var arguments = visitor.Visit(callExp.Arguments);

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
            public Expression Bind(DbExpressionVisitor visitor, MethodCallExpression callExp)
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

                var arguments = visitor.Visit(callExp.Arguments);
                var userId = (arguments[0] as ConstantExpression).Value.ToString();
                var sqlExp = new SqlExpression(string.Format(sql, userId));
                return new InExpression(arguments[1], new Expression[] { sqlExp });
            }
        }
    }
}
