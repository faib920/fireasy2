// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Data.Entity.Linq;
using Fireasy.Data.Entity.Tests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fireasy.Data.Entity.Tests
{
    [TestClass()]
    public class EntityQueryTests
    {
        private static DbContext db;
        private string par = "London";

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            InitConfig.Init();
            db = new DbContext();
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            db.Dispose();
        }

        [TestMethod]
        public void TestGet()
        {
            //var c = db.Customers.Get("ALFKI");
            //Assert.IsNotNull(c);

            var l = new long?[] { 2, 3, 4 };
            var list = db.Customers.Where(s => l.Contains(Convert.ToInt32(s.CustomerID))).ToList();
        }

        [TestMethod]
        public void TestCount()
        {
            var count = db.Customers.Count();
            Assert.AreEqual(91, count);
        }

        [TestMethod]
        public void TestCountPredicate()
        {
            var count = db.Customers.Count(c => c.City == "London");
            Assert.AreEqual(6, count);
        }

        [TestMethod]
        public void TestWhere()
        {
            var list = db.Customers.Where(c => c.City == "London").ToList();
            Assert.AreEqual(6, list.Count);
        }

        [TestMethod]
        public void TestWhere_SQL()
        {
            var list = db.Database.ExecuteEnumerable((SqlCommand)@"SELECT t0.CustomerID, t0.CompanyName, t0.ContactName, t0.ContactTitle, t0.Address, t0.City, t0.Region, t0.PostalCode, t0.Country, t0.Phone, t0.Fax
FROM Customers AS t0
WHERE (t0.City = 'London')").ToList();
            Assert.AreEqual(6, list.Count);
        }

        [TestMethod]
        public void TestWhereWithLocalVar()
        {
            var k = "London";
            var list = db.Customers.Where(c => c.City == k).ToList();
            Assert.AreEqual(6, list.Count);
        }

        [TestMethod]
        public void TestWhereWithGlobalVar()
        {
            var list = db.Customers.Where(c => c.City == par).ToList();
            Assert.AreEqual(6, list.Count);
        }

        [TestMethod]
        public void TestWhereTrue()
        {
            var list = db.Customers.Where(c => true).ToList();
            Assert.AreEqual(91, list.Count);
        }

        [TestMethod]
        public void TestAssertWhereTrue()
        {
            var list = db.Customers.AssertWhere(!string.IsNullOrEmpty(par), s => s.City == par).ToList();
            Assert.AreEqual(6, list.Count);
        }

        [TestMethod]
        public void TestCompareEntityEqual()
        {
            var alfki = new Customers { CustomerID = "ALFKI" };
            var list = db.Customers.Where(c => c == alfki).ToList();
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("ALFKI", list[0].CustomerID);
        }

        [TestMethod]
        public void TestCompareEntityNotEqual()
        {
            var alfki = new Customers { CustomerID = "ALFKI" };
            var list = db.Customers.Where(c => c != alfki).ToList();
            Assert.AreEqual(90, list.Count);
        }

        [TestMethod]
        public void TestCompareConstructedEqual()
        {
            var list = db.Customers.Where(c => new { x = c.City } == new { x = "London" }).ToList();
            Assert.AreEqual(6, list.Count);
        }

        [TestMethod]
        public void TestCompareConstructedMultiValueEqual()
        {
            var list = db.Customers.Where(c => new { x = c.City, y = c.Country } == new { x = "London", y = "UK" }).ToList();
            Assert.AreEqual(6, list.Count);
        }

        [TestMethod]
        public void TestCompareConstructedMultiValueNotEqual()
        {
            var list = db.Customers.Where(c => new { x = c.City, y = c.Country } != new { x = "London", y = "UK" }).ToList();
            Assert.AreEqual(85, list.Count);
        }

        [TestMethod]
        public void TestCompareEntityEntityEqualRelationship()
        {
            var alfki = new Customers { CustomerID = "ALFKI" };
            var testQuery = from o in db.Orders
                            from c in db.Customers
                            where o.Customers == c &&
                                  c.CustomerID == alfki.CustomerID
                            select o.Customers;

            var test = testQuery.ToList();

            Assert.AreEqual(6, test.Count);
        }

        [TestMethod]
        public void TestCompareEntityConstantEqualRelationship()
        {
            var alfki = new Customers { CustomerID = "ALFKI" };
            var testQuery = from o in db.Orders
                            where o.Customers == alfki
                            select o;

            var test = testQuery.ToList();

            Assert.AreEqual(6, test.Count);
        }

        [TestMethod]
        public void TestCompareEntityConstantDirectEqualRelationship()
        {
            var alfki = new Customers { CustomerID = "ALFKI" };
            var testQuery = from c in db.Customers
                            where c == alfki
                            select c;

            var test = testQuery.ToList();

            Assert.AreEqual(1, test.Count);
            Assert.AreEqual("ALFKI", test[0].CustomerID);
        }

        [TestMethod]
        public void TestCompareConstantEntityNestedRelationshipNegation()
        {
            var exclude = new Orders() { OrderID = 10702 };

            var testQuery = from c in db.Customers
                            from o in c.Orderses
                            from d in o.OrderDetailses
                            where o != exclude
                            select d;

            var test = testQuery.ToList();

            Assert.AreEqual(2153, test.Count);
        }

        [TestMethod]
        public void TestCompareTwoConstantEntityNestedRelationshipNegation()
        {
            var exclude = new Orders() { OrderID = 10702 };
            var alfki = new Customers() { CustomerID = "ALFKI" };

            var testQuery = from o in db.Orders
                            where o.Customers == alfki &&
                                  o != exclude
                            select o;

            var test = testQuery.ToList();
            Assert.AreEqual(5, test.Count);
            Assert.AreEqual("ALFKI", test[0].CustomerID);
        }

        [TestMethod]
        public void TestSelectScalar()
        {
            var list = db.Customers.Where(c => c.City == "London").Select(c => c.City).ToList();
            Assert.AreEqual(6, list.Count);
            Assert.AreEqual("London", list[0]);
            Assert.IsTrue(list.All(x => x == "London"));
        }

        [TestMethod]
        public void TestSelectAnonymousOne()
        {
            var list = db.Customers.Where(c => c.City == "London").Select(c => new { c.City }).ToList();
            Assert.AreEqual(6, list.Count);
            Assert.AreEqual("London", list[0].City);
            Assert.IsTrue(list.All(x => x.City == "London"));
        }

        [TestMethod]
        public void TestSelectAnonymousTwo()
        {
            var list = db.Customers.Where(c => c.City == "London").Select(c => new { c.City, c.Phone }).ToList();
            Assert.AreEqual(6, list.Count);
            Assert.AreEqual("London", list[0].City);
            Assert.IsTrue(list.All(x => x.City == "London"));
            Assert.IsTrue(list.All(x => x.Phone != null));
        }

        [TestMethod]
        public void TestSelectCustomerTable()
        {
            var list = db.Customers.ToList();
            Assert.AreEqual(91, list.Count);
        }

        [TestMethod]
        public void TestSelectAnonymousWithObject()
        {
            var list = db.Customers.Where(c => c.City == "London").Select(c => new { c.City, c }).ToList();
            Assert.AreEqual(6, list.Count);
            Assert.AreEqual("London", list[0].City);
            Assert.IsTrue(list.All(x => x.City == "London"));
            Assert.IsTrue(list.All(x => x.c.City == x.City));
        }

        [TestMethod]
        public void TestSelectAnonymousLiteral()
        {
            var list = db.Customers.Where(c => c.City == "London").Select(c => new { X = 10 }).ToList();
            Assert.AreEqual(6, list.Count);
            Assert.IsTrue(list.All(x => x.X == 10));
        }

        [TestMethod]
        public void TestSelectConstantInt()
        {
            var list = db.Customers.Select(c => 10).ToList();
            Assert.AreEqual(91, list.Count);
            Assert.IsTrue(list.All(x => x == 10));
        }

        [TestMethod]
        public void TestSelectConstantNullString()
        {
            var list = db.Customers.Select(c => (string)null).ToList();
            Assert.AreEqual(91, list.Count);
            Assert.IsTrue(list.All(x => x == null));
        }

        [TestMethod]
        public void TestSelectLocal()
        {
            int x = 10;
            var list = db.Customers.Select(c => x).ToList();
            Assert.AreEqual(91, list.Count);
            Assert.IsTrue(list.All(y => y == 10));
        }

        [TestMethod]
        public void TestSelectNestedCollection()
        {
            var list = (
                from c in db.Customers
                where c.CustomerID == "ALFKI"
                select db.Orders.Where(o => o.CustomerID == c.CustomerID).Select(o => o.OrderID)
                ).ToList();
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(6, list[0].Count());
        }

        [TestMethod]
        public void TestSelectNestedCollectionInAnonymousType()
        {
            var list = (
                from c in db.Customers
                where c.CustomerID == "ALFKI"
                select new { Foos = db.Orders.Where(o => o.CustomerID == c.CustomerID).Select(o => o.OrderID).ToList() }
                ).ToList();
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(6, list[0].Foos.Count);
        }

        [TestMethod]
        public void TestSelectNested()
        {
            var list = db.Orders.Select(s => new
            {
                s.CustomerID,
                CompanyName = db.Customers.FirstOrDefault(t => t.CustomerID == s.CustomerID).CompanyName
            }).ToList();
        }

        [TestMethod]
        public void TestJoinCustomerOrders()
        {
            var list = (
                from c in db.Customers
                where c.CustomerID == "ALFKI"
                join o in db.Orders on c.CustomerID equals o.CustomerID
                select new { c.ContactName, o.OrderID }
                ).ToList();
            Assert.AreEqual(6, list.Count);
        }

        [TestMethod]
        public void TestJoinMultiKey()
        {
            var list = (
                from c in db.Customers
                where c.CustomerID == "ALFKI"
                join o in db.Orders on new { a = c.CustomerID, b = c.CustomerID } equals new { a = o.CustomerID, b = o.CustomerID }
                select new { c, o }
                ).ToList();
            Assert.AreEqual(6, list.Count);
        }

        [TestMethod]
        public void TestJoinIntoCustomersOrdersCount()
        {
            var list = (
                from c in db.Customers
                where c.CustomerID == "ALFKI"
                join o in db.Orders on c.CustomerID equals o.CustomerID into ords
                select new { cust = c, ords = ords.Count() }
                ).ToList();
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(6, list[0].ords);
        }

        [TestMethod]
        public void TestJoinIntoDefaultIfEmpty()
        {
            var list = (
                from c in db.Customers
                where c.CustomerID == "PARIS"
                join o in db.Orders on c.CustomerID equals o.CustomerID into ords
                from o in ords.DefaultIfEmpty()
                select new { c, o }
                ).ToList();

            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(null, list[0].o);
        }

        [TestMethod]
        public void TestMultipleJoinsWithJoinConditionsInWhere()
        {
            // this should reduce to inner joins
            var list = (
                from c in db.Customers
                from o in db.Orders
                from d in db.OrderDetails
                where o.CustomerID == c.CustomerID && o.OrderID == d.OrderID
                where c.CustomerID == "ALFKI"
                select d
                ).ToList();

            Assert.AreEqual(12, list.Count);
        }

        [TestMethod]
        public void TestMultipleJoinsWithMissingJoinCondition()
        {
            // this should force a naked cross join
            var list = (
                from c in db.Customers
                from o in db.Orders
                from d in db.OrderDetails
                where o.CustomerID == c.CustomerID /*&& o.OrderID == d.OrderID*/
                where c.CustomerID == "ALFKI"
                select d
                ).ToList();

            Assert.AreEqual(12930, list.Count);
        }

        [TestMethod]
        public void TestOrderBy()
        {
            var list = db.Customers.OrderBy(c => c.CustomerID).Select(c => c.CustomerID).ToList();
            var sorted = list.OrderBy(c => c).ToList();
            Assert.AreEqual(91, list.Count);
            Assert.IsTrue(Enumerable.SequenceEqual(list, sorted));
        }

        [TestMethod]
        public void TestOrderByOrderBy()
        {
            var list = db.Customers.OrderBy(c => c.Phone).OrderBy(c => c.CustomerID).ToList();
            var sorted = list.OrderBy(c => c.CustomerID).ToList();
            Assert.AreEqual(91, list.Count);
            Assert.IsTrue(Enumerable.SequenceEqual(list, sorted));
        }

        [TestMethod]
        public void TestOrderByThenBy()
        {
            var list = db.Customers.OrderBy(c => c.CustomerID).ThenBy(c => c.Phone).ToList();
            var sorted = list.OrderBy(c => c.CustomerID).ThenBy(c => c.Phone).ToList();
            Assert.AreEqual(91, list.Count);
            Assert.IsTrue(Enumerable.SequenceEqual(list, sorted));
        }

        [TestMethod]
        public void TestOrderByDescending()
        {
            var list = db.Customers.OrderByDescending(c => c.CustomerID).ToList();
            var sorted = list.OrderByDescending(c => c.CustomerID).ToList();
            Assert.AreEqual(91, list.Count);
            Assert.IsTrue(Enumerable.SequenceEqual(list, sorted));
        }

        [TestMethod]
        public void TestOrderByDescendingThenBy()
        {
            var list = db.Customers.OrderByDescending(c => c.CustomerID).ThenBy(c => c.Country).ToList();
            var sorted = list.OrderByDescending(c => c.CustomerID).ThenBy(c => c.Country).ToList();
            Assert.AreEqual(91, list.Count);
            Assert.IsTrue(Enumerable.SequenceEqual(list, sorted));
        }

        [TestMethod]
        public void TestOrderByDescendingThenByDescending()
        {
            var list = db.Customers.OrderByDescending(c => c.CustomerID).ThenByDescending(c => c.Country).ToList();
            var sorted = list.OrderByDescending(c => c.CustomerID).ThenByDescending(c => c.Country).ToList();
            Assert.AreEqual(91, list.Count);
            Assert.IsTrue(Enumerable.SequenceEqual(list, sorted));
        }

        [TestMethod]
        public void TestOrderByJoin()
        {
            var list = (
                from c in db.Customers.OrderBy(c => c.CustomerID)
                join o in db.Orders.OrderBy(o => o.OrderID) on c.CustomerID equals o.CustomerID
                select new { c.CustomerID, o.OrderID }
                ).ToList();

            var sorted = list.OrderBy(x => x.CustomerID).ThenBy(x => x.OrderID);
            Assert.IsTrue(Enumerable.SequenceEqual(list, sorted));
        }

        [TestMethod]
        public void TestOrderBySelectMany()
        {
            var list = (
                from c in db.Customers.OrderBy(c => c.CustomerID)
                from o in db.Orders.OrderBy(o => o.OrderID)
                where c.CustomerID == o.CustomerID
                select new { c.CustomerID, o.OrderID }
                ).ToList();
            var sorted = list.OrderBy(x => x.CustomerID).ThenBy(x => x.OrderID).ToList();
            Assert.IsTrue(Enumerable.SequenceEqual(list, sorted));
        }

        [TestMethod]
        public void TestOrderByCount()
        {
            var list = db.Customers.OrderBy(c => c.Orderses.Count)
                .Join(db.Orders.DefaultIfEmpty(), s => s.CustomerID, s => s.CustomerID, (s, t) => s)
                .ToList();
            Assert.AreEqual(91, list.Count);
        }

        [TestMethod]
        public void TestCountProperty()
        {
            var list = db.Customers.Where(c => c.Orderses.Count > 0).ToList();
            Assert.AreEqual(89, list.Count);
        }

        [TestMethod]
        public void TestGroupBy()
        {
            var list = db.Customers.GroupBy(c => c.City).ToList();
            Assert.AreEqual(69, list.Count);
        }

        [TestMethod]
        public void TestGroupByOne()
        {
            var list = db.Customers.Where(c => c.City == "London").GroupBy(c => c.City).ToList();
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(6, list[0].Count());
        }

        [TestMethod]
        public void TestGroupBySelectMany()
        {
            var list = db.Customers.GroupBy(c => c.City).SelectMany(g => g).ToList();
            Assert.AreEqual(91, list.Count);
        }

        [TestMethod]
        public void TestGroupBySum()
        {
            var list = db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID).Select(g => g.Sum(o => (o.CustomerID == "ALFKI" ? 1 : 1))).ToList();
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(6, list[0]);
        }

        [TestMethod]
        public void TestGroupByCount()
        {
            var list = db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID).Select(g => g.Count()).ToList();
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(6, list[0]);
        }

        [TestMethod]
        public void TestGroupByCountAndKey()
        {
            var list = db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID).Select(g => new { g.Key, Count = g.Count() }).ToList();
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(6, list[0].Count);
            Assert.AreEqual("ALFKI", list[0].Key);
        }

        [TestMethod]
        public void TestGroupThenJoin()
        {
            var group = db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID).Select(g => new { g.Key, Count = g.Count() });
            var list = db.Orders.Join(group, s => s.CustomerID, s => s.Key, (s, t) => new { s.CustomerID, t.Count }).ToList();
        }

        [TestMethod]
        public void TestGroupThenJoin1()
        {
            var group = db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => new { o.EmployeeID, o.CustomerID }).Select(g => new { g.Key, Count = g.Count() });
            var list = db.Orders.Join(group, s => s.CustomerID, s => s.Key.CustomerID, (s, t) => new { s.CustomerID, t.Count }).ToList();
        }

        [TestMethod]
        public void TestGroupByLongCount()
        {
            var list = db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID).Select(g => g.LongCount()).ToList();
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(6L, list[0]);
        }

        [TestMethod]
        public void TestGroupBySumMinMaxAvg()
        {
            var list =
                db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID).Select(g =>
                    new
                    {
                        Sum = g.Sum(o => (o.CustomerID == "ALFKI" ? 1 : 1)),
                        Min = g.Min(o => o.OrderID),
                        Max = g.Max(o => o.OrderID),
                        Avg = g.Average(o => o.OrderID)
                    }).ToList();
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(6, list[0].Sum);
        }

        [TestMethod]
        public void TestGroupByWithResultSelector()
        {
            var list =
                db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID, (k, g) =>
                    new
                    {
                        Sum = g.Sum(o => (o.CustomerID == "ALFKI" ? 1 : 1)),
                        Min = g.Min(o => o.OrderID),
                        Max = g.Max(o => o.OrderID),
                        Avg = g.Average(o => o.OrderID)
                    }).ToList();
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(6, list[0].Sum);
        }

        [TestMethod]
        public void TestGroupByWithElementSelectorSum()
        {
            var list = db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID, o => (o.CustomerID == "ALFKI" ? 1 : 1)).Select(g => g.Sum()).ToList();
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(6, list[0]);
        }

        [TestMethod]
        public void TestGroupByWithElementSelector()
        {
            // note: groups are retrieved through a separately execute subquery per row
            var list = db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID, o => (o.CustomerID == "ALFKI" ? 1 : 1)).ToList();
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(6, list[0].Count());
            Assert.AreEqual(6, list[0].Sum());
        }

        [TestMethod]
        public void TestGroupByWithElementSelectorSumMax()
        {
            var list = db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID, o => (o.CustomerID == "ALFKI" ? 1 : 1)).Select(g => new { g.Key, Sum = g.Sum(), Max = g.Max() }).ToList();
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(6, list[0].Sum);
            Assert.AreEqual(1, list[0].Max);
        }

        [TestMethod]
        public void TestGroupByWithAnonymousElement()
        {
            var list = db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID, o => new { X = (o.CustomerID == "ALFKI" ? 1 : 1) }).Select(g => g.Sum(x => x.X)).ToList();
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(6, list[0]);
        }

        [TestMethod]
        public void TestGroupByWithTwoPartKey()
        {
            var list = db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => new { o.CustomerID, o.OrderDate }).Select(g => g.Sum(o => (o.CustomerID == "ALFKI" ? 1 : 1))).ToList();
            Assert.AreEqual(6, list.Count);
        }

        [TestMethod]
        public void TestGroupByWithCountInWhere()
        {
            var list = db.Customers.Where(a => a.Orderses.Count() > 15).GroupBy(a => a.City).ToList();
            Assert.AreEqual(9, list.Count);
        }

        [TestMethod]
        public void TestOrderByGroupBy()
        {
            // note: order-by is lost when group-by is applied (the sequence of groups is not ordered)
            var list = db.Orders.Where(o => o.CustomerID == "ALFKI").OrderBy(o => o.OrderID).GroupBy(o => o.CustomerID).ToList();
            Assert.AreEqual(1, list.Count);
            var grp = list[0].ToList();
            var sorted = grp.OrderBy(o => o.OrderID);
            Assert.IsTrue(Enumerable.SequenceEqual(grp, sorted));
        }

        [TestMethod]
        public void TestOrderByGroupBySelectMany()
        {
            var list = db.Orders.Where(o => o.CustomerID == "ALFKI").OrderBy(o => o.OrderID).GroupBy(o => o.CustomerID).SelectMany(g => g).ToList();
            Assert.AreEqual(6, list.Count);
            var sorted = list.OrderBy(o => o.OrderID).ToList();
            Assert.IsTrue(Enumerable.SequenceEqual(list, sorted));
        }

        [TestMethod]
        public void TestSumWithNoArg()
        {
            var sum = db.Orders.Where(o => o.CustomerID == "ALFKI").Select(o => (o.CustomerID == "ALFKI" ? 1 : 1)).Sum();
            Assert.AreEqual(6, sum);
        }

        [TestMethod]
        public void TestSumWithArg()
        {
            var sum = db.Orders.Where(o => o.CustomerID == "ALFKI").Sum(o => (o.CustomerID == "ALFKI" ? 1 : 1));
            Assert.AreEqual(6, sum);
        }

        [TestMethod]
        public void TestSumWithFunc()
        {
            var data = db.OrderDetails.Select(s => new { Money = SqlFunc.Min(s.Discount * s.Quantity), Quantity = SqlFunc.Sum(s.Quantity), Count = SqlFunc.Count() }).ToList();
            Assert.AreEqual(6, data.Count());
        }

        [TestMethod]
        public void TestCountWithNoPredicate()
        {
            var cnt = db.Orders.Count();
            Assert.AreEqual(830, cnt);
        }

        [TestMethod]
        public void TestCountWithPredicate()
        {
            var cnt = db.Orders.Count(o => o.CustomerID == "ALFKI");
            Assert.AreEqual(6, cnt);
        }

        [TestMethod]
        public void TestDistinctNoDupes()
        {
            var list = db.Customers.Distinct().ToList();
            Assert.AreEqual(91, list.Count);
        }

        [TestMethod]
        public void TestDistinctScalar()
        {
            var list = db.Customers.Select(c => c.City).Distinct().ToList();
            Assert.AreEqual(69, list.Count);
        }

        [TestMethod]
        public void TestOrderByDistinct()
        {
            // these are un-ordered, because using distinct negates any existing ordering.
            var list = db.Customers.Where(c => c.City.StartsWith("P")).OrderBy(c => c.City).Select(c => c.City).Distinct().ToList();
            Assert.AreEqual(2, list.Count);
            var sorted = list.OrderBy(x => x).ToList();
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(sorted[0], "Paris");
            Assert.AreEqual(sorted[1], "Portland");
        }

        [TestMethod]
        public void TestDistinctOrderBy()
        {
            var list = db.Customers.Where(c => c.City.StartsWith("P")).Select(c => c.City).Distinct().OrderBy(c => c).ToList();
            var sorted = list.OrderBy(x => x).ToList();
            Assert.AreEqual(list[0], sorted[0]);
            Assert.AreEqual(list[list.Count - 1], sorted[list.Count - 1]);
        }

        [TestMethod]
        public void TestDistinctGroupBy()
        {
            var list = db.Orders.Where(o => o.CustomerID == "ALFKI").Distinct().GroupBy(o => o.CustomerID).ToList();
            Assert.AreEqual(1, list.Count);
        }

        [TestMethod]
        public void TestGroupByDistinct()
        {
            // distinct after group-by should not do anything
            var list = db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID).Distinct().ToList();
            Assert.AreEqual(1, list.Count);
        }

        [TestMethod]
        public void TestDistinctCount()
        {
            var cnt = db.Customers.Distinct().Count();
            Assert.AreEqual(91, cnt);
        }

        [TestMethod]
        public void TestSelectDistinctCount()
        {
            // cannot do: SELECT COUNT(DISTINCT some-colum) FROM some-table
            // because COUNT(DISTINCT some-column) does not count nulls
            var cnt = db.Customers.Select(c => c.City).Distinct().Count();
            Assert.AreEqual(69, cnt);
        }

        [TestMethod]
        public void TestSelectSelectDistinctCount()
        {
            var cnt = db.Customers.Select(c => c.City).Select(c => c).Distinct().Count();
            Assert.AreEqual(69, cnt);
        }

        [TestMethod]
        public void TestDistinctCountPredicate()
        {
            var cnt = db.Customers.Select(c => new { c.City, c.Country }).Distinct().Count(c => c.City == "London");
            Assert.AreEqual(1, cnt);
        }

        [TestMethod]
        public void TestDistinctSumWithArg()
        {
            var sum = db.Orders.Where(o => o.CustomerID == "ALFKI").Distinct().Sum(o => (o.CustomerID == "ALFKI" ? 1 : 1));
            Assert.AreEqual(6, sum);
        }

        [TestMethod]
        public void TestSelectDistinctSum()
        {
            var sum = db.Orders.Where(o => o.CustomerID == "ALFKI").Select(o => o.OrderID).Distinct().Sum();
            Assert.AreEqual(64835, sum);
        }

        [TestMethod]
        public void TestSelectDistinctWithRelationship()
        {
            var cus = db.Orders.Include(s => s.Customers).Select(s => s.Customers).Distinct().ToList();
            Assert.AreEqual(89, cus.Count);
        }

        [TestMethod]
        public void TestTake()
        {
            var list = db.Orders.Take(5).ToList();
            Assert.AreEqual(5, list.Count);
        }

        [TestMethod]
        public void TestTakeDistinct()
        {
            // distinct must be forced to apply after top has been computed
            var list = db.Orders.OrderBy(o => o.CustomerID).Select(o => o.CustomerID).Take(5).Distinct().ToList();
            Assert.AreEqual(1, list.Count);
        }

        [TestMethod]
        public void TestDistinctTake()
        {
            // top must be forced to apply after distinct has been computed
            var list = db.Orders.OrderBy(o => o.CustomerID).Select(o => o.CustomerID).Distinct().Take(5).ToList();
            Assert.AreEqual(5, list.Count);
        }

        [TestMethod]
        public void TestDistinctTakeCount()
        {
            var cnt = db.Orders.Distinct().OrderBy(o => o.CustomerID).Select(o => o.CustomerID).Take(5).Count();
            Assert.AreEqual(5, cnt);
        }

        [TestMethod]
        public void TestTakeDistinctCount()
        {
            var cnt = db.Orders.OrderBy(o => o.CustomerID).Select(o => o.CustomerID).Take(5).Distinct().Count();
            Assert.AreEqual(1, cnt);
        }

        [TestMethod]
        public void TestFirst()
        {
            var first = db.Customers.OrderBy(c => c.ContactName).First();
            Assert.AreNotEqual(null, first);
            Assert.AreEqual("ROMEY", first.CustomerID);
        }

        [TestMethod]
        public void TestFirstPredicate()
        {
            var first = db.Customers.OrderBy(c => c.ContactName).First(c => c.City == "London");
            Assert.AreNotEqual(null, first);
            Assert.AreEqual("EASTC", first.CustomerID);
        }

        [TestMethod]
        public void TestWhereFirst()
        {
            var first = db.Customers.OrderBy(c => c.ContactName).Where(c => c.City == "London").First();
            Assert.AreNotEqual(null, first);
            Assert.AreEqual("EASTC", first.CustomerID);
        }

        [TestMethod]
        public void TestFirstOrDefault()
        {
            var first = db.Customers.OrderBy(c => c.ContactName).FirstOrDefault();
            Assert.AreNotEqual(null, first);
            Assert.AreEqual("ROMEY", first.CustomerID);
        }

        [TestMethod]
        public void TestFirstOrDefaultPredicate()
        {
            var first = db.Customers.OrderBy(c => c.ContactName).FirstOrDefault(c => c.City == "London");
            Assert.AreNotEqual(null, first);
            Assert.AreEqual("EASTC", first.CustomerID);
        }

        [TestMethod]
        public void TestWhereFirstOrDefault()
        {
            var first = db.Customers.OrderBy(c => c.ContactName).Where(c => c.City == "London").FirstOrDefault();
            Assert.AreNotEqual(null, first);
            Assert.AreEqual("EASTC", first.CustomerID);
        }

        [TestMethod]
        public void TestFirstOrDefaultPredicateNoMatch()
        {
            var first = db.Customers.OrderBy(c => c.ContactName).FirstOrDefault(c => c.City == "SpongeBob");
            Assert.AreEqual(null, first);
        }

        [TestMethod]
        public void TestReverse()
        {
            var list = db.Customers.OrderBy(c => c.ContactName).Reverse().ToList();
            Assert.AreEqual(91, list.Count);
            Assert.AreEqual("WOLZA", list[0].CustomerID);
            Assert.AreEqual("ROMEY", list[90].CustomerID);
        }

        [TestMethod]
        public void TestReverseReverse()
        {
            var list = db.Customers.OrderBy(c => c.ContactName).Reverse().Reverse().ToList();
            Assert.AreEqual(91, list.Count);
            Assert.AreEqual("ROMEY", list[0].CustomerID);
            Assert.AreEqual("WOLZA", list[90].CustomerID);
        }

        [TestMethod]
        public void TestReverseWhereReverse()
        {
            var list = db.Customers.OrderBy(c => c.ContactName).Reverse().Where(c => c.City == "London").Reverse().ToList();
            Assert.AreEqual(6, list.Count);
            Assert.AreEqual("EASTC", list[0].CustomerID);
            Assert.AreEqual("BSBEV", list[5].CustomerID);
        }

        [TestMethod]
        public void TestReverseTakeReverse()
        {
            var list = db.Customers.OrderBy(c => c.ContactName).Reverse().Take(5).Reverse().ToList();
            Assert.AreEqual(5, list.Count);
            Assert.AreEqual("CHOPS", list[0].CustomerID);
            Assert.AreEqual("WOLZA", list[4].CustomerID);
        }

        [TestMethod]
        public void TestReverseWhereTakeReverse()
        {
            var list = db.Customers.OrderBy(c => c.ContactName).Reverse().Where(c => c.City == "London").Take(5).Reverse().ToList();
            Assert.AreEqual(5, list.Count);
            Assert.AreEqual("CONSH", list[0].CustomerID);
            Assert.AreEqual("BSBEV", list[4].CustomerID);
        }

        [TestMethod]
        public void TestLast()
        {
            var last = db.Customers.OrderBy(c => c.ContactName).Last();
            Assert.AreNotEqual(null, last);
            Assert.AreEqual("WOLZA", last.CustomerID);
        }

        [TestMethod]
        public void TestLastPredicate()
        {
            var last = db.Customers.OrderBy(c => c.ContactName).Last(c => c.City == "London");
            Assert.AreNotEqual(null, last);
            Assert.AreEqual("BSBEV", last.CustomerID);
        }

        [TestMethod]
        public void TestWhereLast()
        {
            var last = db.Customers.OrderBy(c => c.ContactName).Where(c => c.City == "London").Last();
            Assert.AreNotEqual(null, last);
            Assert.AreEqual("BSBEV", last.CustomerID);
        }

        [TestMethod]
        public void TestLastOrDefault()
        {
            var last = db.Customers.OrderBy(c => c.ContactName).LastOrDefault();
            Assert.AreNotEqual(null, last);
            Assert.AreEqual("WOLZA", last.CustomerID);
        }

        [TestMethod]
        public void TestLastOrDefaultPredicate()
        {
            var last = db.Customers.OrderBy(c => c.ContactName).LastOrDefault(c => c.City == "London");
            Assert.AreNotEqual(null, last);
            Assert.AreEqual("BSBEV", last.CustomerID);
        }

        [TestMethod]
        public void TestWhereLastOrDefault()
        {
            var last = db.Customers.OrderBy(c => c.ContactName).Where(c => c.City == "London").LastOrDefault();
            Assert.AreNotEqual(null, last);
            Assert.AreEqual("BSBEV", last.CustomerID);
        }

        [TestMethod]
        public void TestLastOrDefaultNoMatches()
        {
            var last = db.Customers.OrderBy(c => c.ContactName).LastOrDefault(c => c.City == "SpongeBob");
            Assert.AreEqual(null, last);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestSingleFails()
        {
            var single = db.Customers.Single();
        }

        [TestMethod]
        public void TestSinglePredicate()
        {
            var single = db.Customers.Single(c => c.CustomerID == "ALFKI");
            Assert.AreNotEqual(null, single);
            Assert.AreEqual("ALFKI", single.CustomerID);
        }

        [TestMethod]
        public void TestWhereSingle()
        {
            var single = db.Customers.Where(c => c.CustomerID == "ALFKI").Single();
            Assert.AreNotEqual(null, single);
            Assert.AreEqual("ALFKI", single.CustomerID);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestSingleOrDefaultFails()
        {
            var single = db.Customers.SingleOrDefault();
        }

        [TestMethod]
        public void TestSingleOrDefaultPredicate()
        {
            var single = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI");
            Assert.AreNotEqual(null, single);
            Assert.AreEqual("ALFKI", single.CustomerID);
        }

        [TestMethod]
        public void TestWhereSingleOrDefault()
        {
            var single = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault();
            Assert.AreNotEqual(null, single);
            Assert.AreEqual("ALFKI", single.CustomerID);
        }

        [TestMethod]
        public void TestSingleOrDefaultNoMatches()
        {
            var single = db.Customers.SingleOrDefault(c => c.CustomerID == "SpongeBob");
            Assert.AreEqual(null, single);
        }

        [TestMethod]
        public void TestAnyTopLevel()
        {
            var any = db.Customers.Any();
            Assert.IsTrue(any);
        }

        [TestMethod]
        public void TestAnyWithSubquery()
        {
            var list = db.Customers.Where(c => c.Orderses.Any(o => o.CustomerID == "ALFKI")).ToList();
            Assert.AreEqual(1, list.Count);
        }

        [TestMethod]
        public void TestAnyWithSubqueryNoPredicate()
        {
            // customers with at least one order
            var list = db.Customers.Where(c => db.Orders.Where(o => o.CustomerID == c.CustomerID).Any()).ToList();
            Assert.AreEqual(89, list.Count);
        }

        [TestMethod]
        public void TestAnyWithLocalCollection()
        {
            // get customers for any one of these IDs
            string[] ids = new[] { "ALFKI", "WOLZA", "NOONE" };
            var list = db.Customers.Where(c => ids.Any(id => c.CustomerID == id)).ToList();
            Assert.AreEqual(2, list.Count);
        }

        [TestMethod]
        public void TestAllWithSubquery()
        {
            var list = db.Customers.Where(c => c.Orderses.All(o => o.CustomerID == "ALFKI")).ToList();
            // includes customers w/ no orders
            Assert.AreEqual(3, list.Count);
        }

        [TestMethod]
        public void TestAllWithLocalCollection()
        {
            // get all customers with a name that contains both 'm' and 'd'  (don't use vowels since these often depend on collation)
            string[] patterns = new[] { "m", "d" };

            var list = db.Customers.Where(c => patterns.All(p => c.ContactName.ToLower().Contains(p))).Select(c => c.ContactName).ToList();
            var local = db.Customers.AsEnumerable().Where(c => patterns.All(p => c.ContactName.ToLower().Contains(p))).Select(c => c.ContactName).ToList();

            Assert.AreEqual(local.Count, list.Count);
        }

        [TestMethod]
        public void TestAllTopLevel()
        {
            // all customers have name length > 0?
            var all = db.Customers.All(c => c.ContactName.Length > 0);
            Assert.IsTrue(all);
        }

        [TestMethod]
        public void TestAllTopLevelNoMatches()
        {
            // all customers have name with 'a'
            var all = db.Customers.All(c => c.ContactName.Contains("a"));
            Assert.IsFalse(all);
        }

        [TestMethod]
        public void TestContainsWithSubquery()
        {
            // this is the long-way to determine all customers that have at least one order
            var list = db.Customers.Where(c => db.Orders.Select(o => o.CustomerID).Contains(c.CustomerID)).ToList();
            Assert.AreEqual(89, list.Count);
        }

        [TestMethod]
        public void TestContainsWithLocalCollection()
        {
            string[] ids = new[] { "ALFKI", "WOLZA", "NOONE" };
            var list = db.Customers.Where(c => ids.Contains(c.CustomerID)).ToList();
            Assert.AreEqual(2, list.Count);
        }

        [TestMethod]
        public void TestContainsWithLocalList()
        {
            var ids = new List<string> { "ALFKI", "WOLZA", "NOONE" };
            var list = db.Customers.Where(c => ids.Contains(c.CustomerID)).ToList();
            Assert.AreEqual(2, list.Count);
        }

        [TestMethod]
        public void TestContainsTopLevel()
        {
            var contains = db.Customers.Select(c => c.CustomerID).Contains("ALFKI");
            Assert.IsTrue(contains);
        }

        [TestMethod]
        public void TestSkipTake()
        {
            var list = db.Customers.OrderBy(c => c.CustomerID).Skip(5).Take(10).ToList();
            Assert.AreEqual(10, list.Count);
            Assert.AreEqual("BLAUS", list[0].CustomerID);
            Assert.AreEqual("COMMI", list[9].CustomerID);
        }

        [TestMethod]
        public void TestDistinctSkipTake()
        {
            var list = db.Customers.Select(c => c.City).Distinct().OrderBy(c => c).Skip(5).Take(10).ToList();
            Assert.AreEqual(10, list.Count);
            var hs = new HashSet<string>(list);
            Assert.AreEqual(10, hs.Count);
        }

        [TestMethod]
        public void TestCoalesce()
        {
            var list = db.Customers.Select(c => new { City = (c.City == "London" ? null : c.City), Country = (c.CustomerID == "EASTC" ? null : c.Country) })
                         .Where(x => (x.City ?? "NoCity") == "NoCity").ToList();
            Assert.AreEqual(6, list.Count);
            Assert.AreEqual(null, list[0].City);
        }

        [TestMethod]
        public void TestCoalesce2()
        {
            var list = db.Customers.Select(c => new { City = (c.City == "London" ? null : c.City), Country = (c.CustomerID == "EASTC" ? null : c.Country) })
                         .Where(x => (x.City ?? x.Country ?? "NoCityOrCountry") == "NoCityOrCountry").ToList();
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(null, list[0].City);
            Assert.AreEqual(null, list[0].Country);
        }

        // framework function tests

        [TestMethod]
        public void TestStringLength()
        {
            var list = db.Customers.Where(c => c.City.Length == 7).ToList();
            Assert.AreEqual(9, list.Count);
        }

        [TestMethod]
        public void TestStringStartsWithLiteral()
        {
            var list = db.Customers.Where(c => c.ContactName.StartsWith("M")).ToList();
            Assert.AreEqual(12, list.Count);
        }

        [TestMethod]
        public void TestStringStartsWithColumn()
        {
            var list = db.Customers.Where(c => c.ContactName.StartsWith(c.ContactName)).ToList();
            Assert.AreEqual(91, list.Count);
        }

        [TestMethod]
        public void TestStringEndsWithLiteral()
        {
            var list = db.Customers.Where(c => c.ContactName.EndsWith("s")).ToList();
            Assert.AreEqual(9, list.Count);
        }

        [TestMethod]
        public void TestStringEndsWithColumn()
        {
            var list = db.Customers.Where(c => c.ContactName.EndsWith(c.ContactName)).ToList();
            Assert.AreEqual(91, list.Count);
        }

        [TestMethod]
        public void TestStringContainsLiteral()
        {
            var list = db.Customers.Where(c => c.ContactName.Contains("nd")).Select(c => c.ContactName).ToList();
            var local = db.Customers.AsEnumerable().Where(c => c.ContactName.ToLower().Contains("nd")).Select(c => c.ContactName).ToList();
            Assert.AreEqual(local.Count, list.Count);
        }

        [TestMethod]
        public void TestStringContainsColumn()
        {
            var list = db.Customers.Where(c => c.ContactName.Contains(c.ContactName)).ToList();
            Assert.AreEqual(91, list.Count);
        }

        [TestMethod]
        public void TestStringConcatImplicit2Args()
        {
            var list = db.Customers.Where(c => c.ContactName + "X" == "Maria AndersX").ToList();
            Assert.AreEqual(1, list.Count);
        }

        [TestMethod]
        public void TestStringConcatExplicit2Args()
        {
            var list = db.Customers.Where(c => string.Concat(c.ContactName, "X") == "Maria AndersX").ToList();
            Assert.AreEqual(1, list.Count);
        }

        [TestMethod]
        public void TestStringConcatExplicit3Args()
        {
            var list = db.Customers.Where(c => string.Concat(c.ContactName, "X", c.Country) == "Maria AndersXGermany").ToList();
            Assert.AreEqual(1, list.Count);
        }

        [TestMethod]
        public void TestStringConcatExplicitNArgs()
        {
            var list = db.Customers.Where(c => string.Concat(new string[] { c.ContactName, "X", c.Country }) == "Maria AndersXGermany").ToList();
            Assert.AreEqual(1, list.Count);
        }

        [TestMethod]
        public void TestStringIsNullOrEmpty()
        {
            var list = db.Customers.Select(c => c.City == "London" ? null : c.CustomerID).Where(x => string.IsNullOrEmpty(x)).ToList();
            Assert.AreEqual(6, list.Count);
        }

        [TestMethod]
        public void TestStringToUpper()
        {
            var str = db.Customers.Where(c => c.CustomerID == "ALFKI").Max(c => (c.CustomerID == "ALFKI" ? "abc" : "abc").ToUpper());
            Assert.AreEqual("ABC", str);
        }

        [TestMethod]
        public void TestStringToLower()
        {
            var str = db.Customers.Where(c => c.CustomerID == "ALFKI").Max(c => (c.CustomerID == "ALFKI" ? "ABC" : "ABC").ToLower());
            Assert.AreEqual("abc", str);
        }

        [TestMethod]
        public void TestStringSubstring()
        {
            var list = db.Customers.Where(c => c.City.Substring(0, 4) == "Seat").ToList();
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("Seattle", list[0].City);
        }

        [TestMethod]
        public void TestStringSubstringNoLength()
        {
            var list = db.Customers.Where(c => c.City.Substring(4) == "tle").ToList();
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("Seattle", list[0].City);
        }

        [TestMethod]
        public void TestStringIndexOf()
        {
            var n = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => c.ContactName.IndexOf("ar"));
            Assert.AreEqual(1, n);
        }

        [TestMethod]
        public void TestStringIndexOfChar()
        {
            var n = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => c.ContactName.IndexOf('r'));
            Assert.AreEqual(2, n);
        }

        [TestMethod]
        public void TestStringIndexOfWithStart()
        {
            var n = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => c.ContactName.IndexOf("a", 3));
            Assert.AreEqual(4, n);
        }

        [TestMethod]
        public void TestStringTrim()
        {
            var notrim = db.Customers.Where(c => c.CustomerID == "ALFKI").Max(c => ("  " + c.City + " "));
            var trim = db.Customers.Where(c => c.CustomerID == "ALFKI").Max(c => ("  " + c.City + " ").Trim());
            Assert.AreNotEqual(notrim, trim);
            Assert.AreEqual(notrim.Trim(), trim);
        }

        [TestMethod]
        public void TestDateTimeConstructYMD()
        {
            var dt = db.Customers.Where(c => c.CustomerID == "ALFKI").Max(c => new DateTime((c.CustomerID == "ALFKI") ? 1997 : 1997, 7, 4));
            Assert.AreEqual(1997, dt.Year);
            Assert.AreEqual(7, dt.Month);
            Assert.AreEqual(4, dt.Day);
            Assert.AreEqual(0, dt.Hour);
            Assert.AreEqual(0, dt.Minute);
            Assert.AreEqual(0, dt.Second);
        }

        [TestMethod]
        public void TestDateTimeConstructYMDHMS()
        {
            var dt = db.Customers.Where(c => c.CustomerID == "ALFKI").Max(c => new DateTime((c.CustomerID == "ALFKI") ? 1997 : 1997, 7, 4, 3, 5, 6));
            Assert.AreEqual(1997, dt.Year);
            Assert.AreEqual(7, dt.Month);
            Assert.AreEqual(4, dt.Day);
            Assert.AreEqual(3, dt.Hour);
            Assert.AreEqual(5, dt.Minute);
            Assert.AreEqual(6, dt.Second);
        }

        [TestMethod]
        public void TestDateTimeDay()
        {
            var v = db.Orders.Where(o => o.OrderDate == new DateTime(1997, 8, 25)).Take(1).Max(o => o.OrderDate.Value.Day);
            Assert.AreEqual(25, v);
        }

        [TestMethod]
        public void TestDateTimeMonth()
        {
            var v = db.Orders.Where(o => o.OrderDate == new DateTime(1997, 8, 25)).Take(1).Max(o => o.OrderDate.Value.Month);
            Assert.AreEqual(8, v);
        }

        [TestMethod]
        public void TestDateTimeConvert()
        {
            var v = db.Orders.Where(o => o.OrderDate == Convert.ToDateTime(o.CustomerID))
                .Take(1).Max(o => o.OrderDate.Value.Month);

            Assert.AreEqual(8, v);
        }

        [TestMethod]
        public void TestDateTimeYear()
        {
            Console.WriteLine(db.Orders.Where(o => o.OrderDate > new DateTime(1997, 8, 25)).Count());
            var v = db.Orders.Where(o => o.OrderDate == new DateTime(1997, 8, 25)).Take(1).Max(o => o.OrderDate.Value.Year);
            Assert.AreEqual(1997, v);
        }

        [TestMethod]
        public void TestDateTimeHour()
        {
            var hour = db.Customers.Where(c => c.CustomerID == "ALFKI").Max(c => new DateTime((c.CustomerID == "ALFKI") ? 1997 : 1997, 7, 4, 3, 5, 6).Hour);
            Assert.AreEqual(3, hour);
        }

        [TestMethod]
        public void TestDateTimeMinute()
        {
            var minute = db.Customers.Where(c => c.CustomerID == "ALFKI").Max(c => new DateTime((c.CustomerID == "ALFKI") ? 1997 : 1997, 7, 4, 3, 5, 6).Minute);
            Assert.AreEqual(5, minute);
        }

        [TestMethod]
        public void TestDateTimeSecond()
        {
            var second = db.Customers.Where(c => c.CustomerID == "ALFKI").Max(c => new DateTime((c.CustomerID == "ALFKI") ? 1997 : 1997, 7, 4, 3, 5, 6).Second);
            Assert.AreEqual(6, second);
        }

        [TestMethod]
        public void TestDateTimeDayOfWeek()
        {
            var dow = db.Orders.Where(o => o.OrderDate == new DateTime(1997, 8, 25)).Take(1).Max(o => o.OrderDate.Value.DayOfWeek);
            Assert.AreEqual(DayOfWeek.Monday, dow);
        }

        [TestMethod]
        public void TestDateTimeAddYears()
        {
            var od = db.Orders.FirstOrDefault(o => o.OrderDate == new DateTime(1997, 8, 25) && o.OrderDate.Value.AddYears(2).Year == 1999);
            Assert.AreNotEqual(null, od);
        }

        [TestMethod]
        public void TestDateTimeAddMonths()
        {
            var od = db.Orders.FirstOrDefault(o => o.OrderDate == new DateTime(1997, 8, 25) && o.OrderDate.Value.AddMonths(2).Month == 10);
            Assert.AreNotEqual(null, od);
        }

        [TestMethod]
        public void TestDateTimeAddDays()
        {
            var od = db.Orders.FirstOrDefault(o => o.OrderDate == new DateTime(1997, 8, 25) && o.OrderDate.Value.AddDays(2).Day == 27);
            Assert.AreNotEqual(null, od);
        }

        [TestMethod]
        public void TestDateTimeAddHours()
        {
            var od = db.Orders.FirstOrDefault(o => o.OrderDate == new DateTime(1997, 8, 25) && o.OrderDate.Value.AddHours(3).Hour == 3);
            Assert.AreNotEqual(null, od);
        }

        [TestMethod]
        public void TestDateTimeAddMinutes()
        {
            var od = db.Orders.FirstOrDefault(o => o.OrderDate == new DateTime(1997, 8, 25) && o.OrderDate.Value.AddMinutes(5).Minute == 5);
            Assert.AreNotEqual(null, od);
        }

        [TestMethod]
        public void TestDateTimeAddSeconds()
        {
            var od = db.Orders.FirstOrDefault(o => o.OrderDate == new DateTime(1997, 8, 25) && o.OrderDate.Value.AddSeconds(6).Second == 6);
            Assert.AreNotEqual(null, od);
        }

        [TestMethod]
        public void TestDateTimeDiffDays()
        {
            var od = db.Orders.FirstOrDefault(o => ((DateTime)o.OrderDate - DateTime.Today.AddDays(1)).Days == 1);
            Assert.AreNotEqual(null, od);
        }

        [TestMethod]
        public void TestMathAbs()
        {
            var neg1 = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Abs((c.CustomerID == "ALFKI") ? -1 : 0));
            var pos1 = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Abs((c.CustomerID == "ALFKI") ? 1 : 0));
            Assert.AreEqual(Math.Abs(-1), neg1);
            Assert.AreEqual(Math.Abs(1), pos1);
        }

        [TestMethod]
        public void TestMathAtan()
        {
            var zero = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Atan((c.CustomerID == "ALFKI") ? 0.0 : 0.0));
            var one = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Atan((c.CustomerID == "ALFKI") ? 1.0 : 1.0));
            Assert.AreEqual(Math.Atan(0.0), zero, 0.0001);
            Assert.AreEqual(Math.Atan(1.0), one, 0.0001);
        }

        [TestMethod]
        public void TestMathCos()
        {
            var zero = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Cos((c.CustomerID == "ALFKI") ? 0.0 : 0.0));
            var pi = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Cos((c.CustomerID == "ALFKI") ? Math.PI : Math.PI));
            Assert.AreEqual(Math.Cos(0.0), zero, 0.0001);
            Assert.AreEqual(Math.Cos(Math.PI), pi, 0.0001);
        }

        [TestMethod]
        public void TestMathSin()
        {
            var zero = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Sin((c.CustomerID == "ALFKI") ? 0.0 : 0.0));
            var pi = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Sin((c.CustomerID == "ALFKI") ? Math.PI : Math.PI));
            var pi2 = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Sin(((c.CustomerID == "ALFKI") ? Math.PI : Math.PI) / 2.0));
            Assert.AreEqual(Math.Sin(0.0), zero);
            Assert.AreEqual(Math.Sin(Math.PI), pi, 0.0001);
            Assert.AreEqual(Math.Sin(Math.PI / 2.0), pi2, 0.0001);
        }

        [TestMethod]
        public void TestMathTan()
        {
            var zero = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Tan((c.CustomerID == "ALFKI") ? 0.0 : 0.0));
            var pi = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Tan((c.CustomerID == "ALFKI") ? Math.PI : Math.PI));
            Assert.AreEqual(Math.Tan(0.0), zero, 0.0001);
            Assert.AreEqual(Math.Tan(Math.PI), pi, 0.0001);
        }

        [TestMethod]
        public void TestMathExp()
        {
            var zero = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Exp((c.CustomerID == "ALFKI") ? 0.0 : 0.0));
            var one = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Exp((c.CustomerID == "ALFKI") ? 1.0 : 1.0));
            var two = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Exp((c.CustomerID == "ALFKI") ? 2.0 : 2.0));
            Assert.AreEqual(Math.Exp(0.0), zero, 0.0001);
            Assert.AreEqual(Math.Exp(1.0), one, 0.0001);
            Assert.AreEqual(Math.Exp(2.0), two, 0.0001);
        }

        [TestMethod]
        public void TestMathLog()
        {
            var one = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Log((c.CustomerID == "ALFKI") ? 1.0 : 1.0));
            var e = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Log((c.CustomerID == "ALFKI") ? Math.E : Math.E));
            Assert.AreEqual(Math.Log(1.0), one, 0.0001);
            Assert.AreEqual(Math.Log(Math.E), e, 0.0001);
        }

        [TestMethod]
        public void TestMathSqrt()
        {
            var one = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Sqrt((c.CustomerID == "ALFKI") ? 1.0 : 1.0));
            var four = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Sqrt((c.CustomerID == "ALFKI") ? 4.0 : 4.0));
            var nine = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Sqrt((c.CustomerID == "ALFKI") ? 9.0 : 9.0));
            Assert.AreEqual(1.0, one);
            Assert.AreEqual(2.0, four);
            Assert.AreEqual(3.0, nine);
        }

        [TestMethod]
        public void TestMathPow()
        {
            // 2^n
            var zero = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Pow((c.CustomerID == "ALFKI") ? 2.0 : 2.0, 0.0));
            var one = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Pow((c.CustomerID == "ALFKI") ? 2.0 : 2.0, 1.0));
            var two = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Pow((c.CustomerID == "ALFKI") ? 2.0 : 2.0, 2.0));
            var three = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Pow((c.CustomerID == "ALFKI") ? 2.0 : 2.0, 3.0));
            Assert.AreEqual(1.0, zero);
            Assert.AreEqual(2.0, one);
            Assert.AreEqual(4.0, two);
            Assert.AreEqual(8.0, three);
        }

        [TestMethod]
        public void TestMathRoundDefault()
        {
            var four = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Round((c.CustomerID == "ALFKI") ? 3.4 : 3.4));
            var six = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Round((c.CustomerID == "ALFKI") ? 3.6 : 3.6));
            Assert.AreEqual(3.0, four);
            Assert.AreEqual(4.0, six);
        }

        [TestMethod]
        public void TestMathFloor()
        {
            // The difference between floor and truncate is how negatives are handled.  Floor drops the decimals and moves the
            // value to the more negative, so Floor(-3.4) is -4.0 and Floor(3.4) is 3.0.
            var four = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Floor((c.CustomerID == "ALFKI" ? 3.4 : 3.4)));
            var six = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Floor((c.CustomerID == "ALFKI" ? 3.6 : 3.6)));
            var nfour = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Floor((c.CustomerID == "ALFKI" ? -3.4 : -3.4)));
            Assert.AreEqual(Math.Floor(3.4), four);
            Assert.AreEqual(Math.Floor(3.6), six);
            Assert.AreEqual(Math.Floor(-3.4), nfour);
        }

        [TestMethod]
        public void TestDecimalFloor()
        {
            var four = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Floor((c.CustomerID == "ALFKI" ? 3.4m : 3.4m)));
            var six = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Floor((c.CustomerID == "ALFKI" ? 3.6m : 3.6m)));
            var nfour = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Floor((c.CustomerID == "ALFKI" ? -3.4m : -3.4m)));
            Assert.AreEqual(decimal.Floor(3.4m), four);
            Assert.AreEqual(decimal.Floor(3.6m), six);
            Assert.AreEqual(decimal.Floor(-3.4m), nfour);
        }

        [TestMethod]
        public void TestMathTruncate()
        {
            // The difference between floor and truncate is how negatives are handled.  Truncate drops the decimals, 
            // therefore a truncated negative often has a more positive value than non-truncated (never has a less positive),
            // so Truncate(-3.4) is -3.0 and Truncate(3.4) is 3.0.
            var four = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Truncate((c.CustomerID == "ALFKI") ? 3.4 : 3.4));
            var six = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Truncate((c.CustomerID == "ALFKI") ? 3.6 : 3.6));
            var neg4 = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Truncate((c.CustomerID == "ALFKI") ? -3.4 : -3.4));
            Assert.AreEqual(Math.Truncate(3.4), four);
            Assert.AreEqual(Math.Truncate(3.6), six);
            Assert.AreEqual(Math.Truncate(-3.4), neg4);
        }

        [TestMethod]
        public void TestStringCompareTo()
        {
            var lt = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => c.City.CompareTo("Seattle"));
            var gt = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => c.City.CompareTo("Aaa"));
            var eq = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => c.City.CompareTo("Berlin"));
            Assert.AreEqual(-1, lt);
            Assert.AreEqual(1, gt);
            Assert.AreEqual(0, eq);
        }

        [TestMethod]
        public void TestStringCompareToLT()
        {
            var cmpLT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Seattle") < 0);
            var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Berlin") < 0);
            Assert.AreNotEqual(null, cmpLT);
            Assert.AreEqual(null, cmpEQ);
        }

        [TestMethod]
        public void TestStringCompareToLE()
        {
            var cmpLE = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Seattle") <= 0);
            var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Berlin") <= 0);
            var cmpGT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Aaa") <= 0);
            Assert.AreNotEqual(null, cmpLE);
            Assert.AreNotEqual(null, cmpEQ);
            Assert.AreEqual(null, cmpGT);
        }

        [TestMethod]
        public void TestStringCompareToGT()
        {
            var cmpLT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Aaa") > 0);
            var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Berlin") > 0);
            Assert.AreNotEqual(null, cmpLT);
            Assert.AreEqual(null, cmpEQ);
        }

        [TestMethod]
        public void TestStringCompareToGE()
        {
            var cmpLE = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Seattle") >= 0);
            var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Berlin") >= 0);
            var cmpGT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Aaa") >= 0);
            Assert.AreEqual(null, cmpLE);
            Assert.AreNotEqual(null, cmpEQ);
            Assert.AreNotEqual(null, cmpGT);
        }

        [TestMethod]
        public void TestStringCompareToEQ()
        {
            var cmpLE = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Seattle") == 0);
            var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Berlin") == 0);
            var cmpGT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Aaa") == 0);
            Assert.AreEqual(null, cmpLE);
            Assert.AreNotEqual(null, cmpEQ);
            Assert.AreEqual(null, cmpGT);
        }

        [TestMethod]
        public void TestStringCompareToNE()
        {
            var cmpLE = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Seattle") != 0);
            var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Berlin") != 0);
            var cmpGT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Aaa") != 0);
            Assert.AreNotEqual(null, cmpLE);
            Assert.AreEqual(null, cmpEQ);
            Assert.AreNotEqual(null, cmpGT);
        }

        [TestMethod]
        public void TestStringCompare()
        {
            var lt = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => string.Compare(c.City, "Seattle"));
            var gt = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => string.Compare(c.City, "Aaa"));
            var eq = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => string.Compare(c.City, "Berlin"));
            Assert.AreEqual(-1, lt);
            Assert.AreEqual(1, gt);
            Assert.AreEqual(0, eq);
        }

        [TestMethod]
        public void TestStringCompareLT()
        {
            var cmpLT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Seattle") < 0);
            var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Berlin") < 0);
            Assert.AreNotEqual(null, cmpLT);
            Assert.AreEqual(null, cmpEQ);
        }

        [TestMethod]
        public void TestStringCompareLE()
        {
            var cmpLE = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Seattle") <= 0);
            var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Berlin") <= 0);
            var cmpGT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Aaa") <= 0);
            Assert.AreNotEqual(null, cmpLE);
            Assert.AreNotEqual(null, cmpEQ);
            Assert.AreEqual(null, cmpGT);
        }

        [TestMethod]
        public void TestStringCompareGT()
        {
            var cmpLT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Aaa") > 0);
            var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Berlin") > 0);
            Assert.AreNotEqual(null, cmpLT);
            Assert.AreEqual(null, cmpEQ);
        }

        [TestMethod]
        public void TestStringCompareGE()
        {
            var cmpLE = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Seattle") >= 0);
            var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Berlin") >= 0);
            var cmpGT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Aaa") >= 0);
            Assert.AreEqual(null, cmpLE);
            Assert.AreNotEqual(null, cmpEQ);
            Assert.AreNotEqual(null, cmpGT);
        }

        [TestMethod]
        public void TestStringCompareEQ()
        {
            var cmpLE = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Seattle") == 0);
            var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Berlin") == 0);
            var cmpGT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Aaa") == 0);
            Assert.AreEqual(null, cmpLE);
            Assert.AreNotEqual(null, cmpEQ);
            Assert.AreEqual(null, cmpGT);
        }

        [TestMethod]
        public void TestStringCompareNE()
        {
            var cmpLE = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Seattle") != 0);
            var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Berlin") != 0);
            var cmpGT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Aaa") != 0);
            Assert.AreNotEqual(null, cmpLE);
            Assert.AreEqual(null, cmpEQ);
            Assert.AreNotEqual(null, cmpGT);
        }

        [TestMethod]
        public void TestIntCompareTo()
        {
            // prove that x.CompareTo(y) works for types other than string
            var eq = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => (c.CustomerID == "ALFKI" ? 10 : 10).CompareTo(10));
            var gt = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => (c.CustomerID == "ALFKI" ? 10 : 10).CompareTo(9));
            var lt = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => (c.CustomerID == "ALFKI" ? 10 : 10).CompareTo(11));
            Assert.AreEqual(0, eq);
            Assert.AreEqual(1, gt);
            Assert.AreEqual(-1, lt);
        }

        [TestMethod]
        public void TestDecimalCompare()
        {
            // prove that type.Compare(x,y) works with decimal
            var eq = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Compare((c.CustomerID == "ALFKI" ? 10m : 10m), 10m));
            var gt = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Compare((c.CustomerID == "ALFKI" ? 10m : 10m), 9m));
            var lt = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Compare((c.CustomerID == "ALFKI" ? 10m : 10m), 11m));
            Assert.AreEqual(0, eq);
            Assert.AreEqual(1, gt);
            Assert.AreEqual(-1, lt);
        }

        [TestMethod]
        public void TestDecimalAdd()
        {
            var onetwo = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Add((c.CustomerID == "ALFKI" ? 1m : 1m), 2m));
            Assert.AreEqual(3m, onetwo);
        }

        [TestMethod]
        public void TestDecimalSubtract()
        {
            var onetwo = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Subtract((c.CustomerID == "ALFKI" ? 1m : 1m), 2m));
            Assert.AreEqual(-1m, onetwo);
        }

        [TestMethod]
        public void TestDecimalMultiply()
        {
            var onetwo = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Multiply((c.CustomerID == "ALFKI" ? 1m : 1m), 2m));
            Assert.AreEqual(2m, onetwo);
        }

        [TestMethod]
        public void TestDecimalDivide()
        {
            var onetwo = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Divide((c.CustomerID == "ALFKI" ? 1.0m : 1.0m), 2.0m));
            Assert.AreEqual(0.5m, onetwo);
        }

        [TestMethod]
        public void TestDecimalNegate()
        {
            var one = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Negate((c.CustomerID == "ALFKI" ? 1m : 1m)));
            Assert.AreEqual(-1m, one);
        }

        [TestMethod]
        public void TestDecimalRoundDefault()
        {
            var four = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Round((c.CustomerID == "ALFKI" ? 3.4m : 3.4m)));
            var six = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Round((c.CustomerID == "ALFKI" ? 3.5m : 3.5m)));
            Assert.AreEqual(3.0m, four);
            Assert.AreEqual(4.0m, six);
        }

        [TestMethod]
        public void TestDecimalTruncate()
        {
            // The difference between floor and truncate is how negatives are handled.  Truncate drops the decimals, 
            // therefore a truncated negative often has a more positive value than non-truncated (never has a less positive),
            // so Truncate(-3.4) is -3.0 and Truncate(3.4) is 3.0.
            var four = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Truncate((c.CustomerID == "ALFKI") ? 3.4m : 3.4m));
            var six = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Truncate((c.CustomerID == "ALFKI") ? 3.6m : 3.6m));
            var neg4 = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Truncate((c.CustomerID == "ALFKI") ? -3.4m : -3.4m));
            Assert.AreEqual(decimal.Truncate(3.4m), four);
            Assert.AreEqual(decimal.Truncate(3.6m), six);
            Assert.AreEqual(decimal.Truncate(-3.4m), neg4);
        }

        [TestMethod]
        public void TestDecimalLT()
        {
            // prove that decimals are treated normally with respect to normal comparison operators
            var alfki = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 1.0m : 3.0m) < 2.0m);
            Assert.AreNotEqual(null, alfki);
        }

        [TestMethod]
        public void TestIntLessThan()
        {
            var alfki = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 1 : 3) < 2);
            var alfkiN = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 3 : 1) < 2);
            Assert.AreNotEqual(null, alfki);
            Assert.AreEqual(null, alfkiN);
        }

        [TestMethod]
        public void TestIntLessThanOrEqual()
        {
            var alfki = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 1 : 3) <= 2);
            var alfki2 = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 2 : 3) <= 2);
            var alfkiN = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 3 : 1) <= 2);
            Assert.AreNotEqual(null, alfki);
            Assert.AreNotEqual(null, alfki2);
            Assert.AreEqual(null, alfkiN);
        }

        [TestMethod]
        public void TestIntGreaterThan()
        {
            var alfki = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 3 : 1) > 2);
            var alfkiN = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 1 : 3) > 2);
            Assert.AreNotEqual(null, alfki);
            Assert.AreEqual(null, alfkiN);
        }

        [TestMethod]
        public void TestIntGreaterThanOrEqual()
        {
            var alfki = db.Customers.Single(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 3 : 1) >= 2);
            var alfki2 = db.Customers.Single(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 3 : 2) >= 2);
            var alfkiN = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 1 : 3) > 2);
            Assert.AreNotEqual(null, alfki);
            Assert.AreNotEqual(null, alfki2);
            Assert.AreEqual(null, alfkiN);
        }

        [TestMethod]
        public void TestIntEqual()
        {
            var alfki = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 1 : 1) == 1);
            var alfkiN = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 1 : 1) == 2);
            Assert.AreNotEqual(null, alfki);
            Assert.AreEqual(null, alfkiN);
        }

        [TestMethod]
        public void TestIntNotEqual()
        {
            var alfki = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 2 : 2) != 1);
            var alfkiN = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 2 : 2) != 2);
            Assert.AreNotEqual(null, alfki);
            Assert.AreEqual(null, alfkiN);
        }

        [TestMethod]
        public void TestIntAdd()
        {
            var three = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => ((c.CustomerID == "ALFKI") ? 1 : 1) + 2);
            Assert.AreEqual(3, three);
        }

        [TestMethod]
        public void TestIntSubtract()
        {
            var negone = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => ((c.CustomerID == "ALFKI") ? 1 : 1) - 2);
            Assert.AreEqual(-1, negone);
        }

        [TestMethod]
        public void TestIntMultiply()
        {
            var six = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => ((c.CustomerID == "ALFKI") ? 2 : 2) * 3);
            Assert.AreEqual(6, six);
        }

        [TestMethod]
        public void TestIntDivide()
        {
            var one = (int)db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => ((c.CustomerID == "ALFKI") ? 3 : 3) / 2.0);
            Assert.AreEqual(1, one);
        }

        [TestMethod]
        public void TestIntModulo()
        {
            var three = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => ((c.CustomerID == "ALFKI") ? 7 : 7) % 4);
            Assert.AreEqual(3, three);
        }

        [TestMethod]
        public void TestIntLeftShift()
        {
            var eight = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => ((c.CustomerID == "ALFKI") ? 1 : 1) << 3);
            Assert.AreEqual(8, eight);
        }

        [TestMethod]
        public void TestIntRightShift()
        {
            var eight = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => ((c.CustomerID == "ALFKI") ? 32 : 32) >> 2);
            Assert.AreEqual(8, eight);
        }

        [TestMethod]
        public void TestIntBitwiseAnd()
        {
            var band = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => ((c.CustomerID == "ALFKI") ? 6 : 6) & 3);
            Assert.AreEqual(2, band);
        }

        [TestMethod]
        public void TestIntBitwiseOr()
        {
            var eleven = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => ((c.CustomerID == "ALFKI") ? 10 : 10) | 3);
            Assert.AreEqual(11, eleven);
        }

        [TestMethod]
        public void TestIntBitwiseExclusiveOr()
        {
            var zero = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => ((c.CustomerID == "ALFKI") ? 1 : 1) ^ 1);
            Assert.AreEqual(0, zero);
        }

        [TestMethod]
        public void TestIntBitwiseNot()
        {
            var bneg = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => ~((c.CustomerID == "ALFKI") ? -1 : -1));
            Assert.AreEqual(~-1, bneg);
        }

        [TestMethod]
        public void TestIntNegate()
        {
            var neg = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => -((c.CustomerID == "ALFKI") ? 1 : 1));
            Assert.AreEqual(-1, neg);
        }

        [TestMethod]
        public void TestAnd()
        {
            var custs = db.Customers.Where(c => c.Country == "USA" && c.City.StartsWith("A")).Select(c => c.City).ToList();
            Assert.AreEqual(2, custs.Count);
            Assert.IsTrue(custs.All(c => c.StartsWith("A")));
        }

        [TestMethod]
        public void TestOr()
        {
            var custs = db.Customers.Where(c => c.Country == "USA" || c.City.StartsWith("B")).Select(c => c.City).ToList();
            Assert.AreEqual(24, custs.Count);
        }

        [TestMethod]
        public void TestNot()
        {
            var custs = db.Customers.Where(c => !(c.Country == "USA")).Select(c => c.Country).ToList();
            Assert.AreEqual(78, custs.Count);
        }

        /*
        [TestMethod]
        public void TestEqualLiteralNull()
        {
            var q = db.Customers.Select(c => c.CustomerID == "ALFKI" ? null : c.CustomerID).Where(x => x == null);
            Assert.IsTrue(_provider.GetQueryText(q.Expression).Contains("IS NULL"));
            var n = q.Count();
            Assert.AreEqual(1, n);
        }

        [TestMethod]
        public void TestEqualLiteralNullReversed()
        {
            var q = db.Customers.Select(c => c.CustomerID == "ALFKI" ? null : c.CustomerID).Where(x => null == x);
            Assert.IsTrue(_provider.GetQueryText(q.Expression).Contains("IS NULL"));
            var n = q.Count();
            Assert.AreEqual(1, n);
        }

        [TestMethod]
        public void TestNotEqualLiteralNull()
        {
            var q = db.Customers.Select(c => c.CustomerID == "ALFKI" ? null : c.CustomerID).Where(x => x != null);
            Assert.IsTrue(_provider.GetQueryText(q.Expression).Contains("IS NOT NULL"));
            var n = q.Count();
            Assert.AreEqual(90, n);
        }

        [TestMethod]
        public void TestNotEqualLiteralNullReversed()
        {
            var q = db.Customers.Select(c => c.CustomerID == "ALFKI" ? null : c.CustomerID).Where(x => null != x);
            Assert.IsTrue(_provider.GetQueryText(q.Expression).Contains("IS NOT NULL"));
            var n = q.Count();
            Assert.AreEqual(90, n);
        }
         */

        [TestMethod]
        public void TestConditionalResultsArePredicates()
        {
            bool value = db.Orders.Where(c => c.CustomerID == "ALFKI").Max(c => (c.CustomerID == "ALFKI" ? string.Compare(c.CustomerID, "POTATO") < 0 : string.Compare(c.CustomerID, "POTATO") > 0));
            Assert.IsTrue(value);
        }

        [TestMethod]
        public void TestSelectManyJoined()
        {
            var cods =
                (from c in db.Customers
                 from o in db.Orders.Where(o => o.CustomerID == c.CustomerID)
                 select new { c.ContactName, o.OrderDate }).ToList();
            Assert.AreEqual(830, cods.Count);
        }

        [TestMethod]
        public void TestSelectManyJoinedDefaultIfEmpty()
        {
            var cods = (
                from c in db.Customers
                from o in db.Orders.Where(o => o.CustomerID == c.CustomerID).DefaultIfEmpty()
                select new { c.ContactName, o.OrderDate }
                ).ToList();
            Assert.AreEqual(832, cods.Count);
        }

        [TestMethod]
        public void TestSelectWhereAssociation()
        {
            var ords = (
                from o in db.Orders
                where o.Customers.City == "Seattle"
                select o
                ).ToList();
            Assert.AreEqual(14, ords.Count);
        }

        [TestMethod]
        public void TestSelectWhereAssociation11()
        {
            var details = db.OrderDetails.Where(s => s.ProductID == 12);

            var ords = db.Orders.Where(s => s.OrderID != 0).Select(s => new
            {
                Count = details.Where(t => t.OrderID == s.OrderID).Count()
            }).ToList();

            Assert.AreEqual(12, ords[0].Count);
        }

        [TestMethod]
        public void TestSelectWhereAssociationTwice()
        {
            var n = db.Orders.Where(c => c.CustomerID == "WHITC").Count();
            var ords = (
                from o in db.Orders
                where o.Customers.Country == "USA" && o.Customers.City == "Seattle"
                select o
                ).ToList();
            Assert.AreEqual(n, ords.Count);
        }

        [TestMethod]
        public void TestSelectAssociation()
        {
            var custs = (
                from o in db.Orders
                where o.CustomerID == "ALFKI"
                select o.Customers
                ).ToList();
            Assert.AreEqual(6, custs.Count);
            Assert.IsTrue(custs.All(c => c.CustomerID == "ALFKI"));
        }

        [TestMethod]
        public void TestSelectAssociations()
        {
            var doubleCusts = (
                from o in db.Orders
                where o.CustomerID == "ALFKI"
                select new { A = o.Customers, B = o.Customers }
                ).ToList();

            Assert.AreEqual(6, doubleCusts.Count);
            Assert.IsTrue(doubleCusts.All(c => c.A.CustomerID == "ALFKI" && c.B.CustomerID == "ALFKI"));
        }

        [TestMethod]
        public void TestSelectAssociationsWhereAssociations()
        {
            var stuff = (
                from o in db.Orders
                where o.Customers.Country == "USA"
                where o.Customers.City != "Seattle"
                select new { A = o.Customers, B = o.Customers }
                ).ToList();
            Assert.AreEqual(108, stuff.Count);
        }


        [TestMethod]
        public void TestSelectScalarFunc()
        {
            var list = db.Customers.Where(c => c.City == "London").Select(c => c.City.Substring(0, 2)).ToList();
            Assert.AreEqual(6, list.Count);
            Assert.AreEqual("Lo", list[0]);
            Assert.IsTrue(list.All(x => x == "Lo"));
        }

        [TestMethod]
        public void TestSelectSegment()
        {
            var pager = new DataPager(10);
            var list = db.OrderDetails
                .AssertWhere(true, s => true)
                .Select(s => s.OrderID).Segment(pager).ToList();
            Assert.AreEqual(10, list.Count);
            Assert.AreEqual(2155, pager.RecordCount);
        }

        [TestMethod]
        public void TestTowLevelRelationship()
        {
            var list = db.OrderDetails.Where(s => s.Orders.Customers.City.StartsWith("London")).ToList();
            Assert.AreEqual(112, list.Count);
        }

        [TestMethod]
        public void TestTowLevelRelationshipRet()
        {
            var list = db.OrderDetails.Where(s => s.Orders.Customers.City == "London").Select(s => s.Orders.Customers.CompanyName).ToList();
            Assert.AreEqual(112, list.Count);
        }

        [TestMethod]
        public void TestNullableHasValue()
        {
            var list = db.Orders.Where(s => !s.OrderDate.HasValue).ToList();
            Assert.AreEqual(0, list.Count);
        }

        [TestMethod]
        public void TestOrderByWithDefinition()
        {
            var list = db.Orders.OrderBy("OrderDate", SortOrder.Descending, s => s.OrderBy(t => t.EmployeeID).OrderByDescending(t => t.CustomerID)).ToList();
            Console.WriteLine(list[0].CustomerID);
        }

        [TestMethod]
        public void TestOrderByWithDefinitionEmpty()
        {
            var list = db.Orders.OrderBy(SortDefinition.Empty, s => s.OrderBy(t => t.EmployeeID).ThenByDescending(t => t.CustomerID.Substring(0, 5))).ToList();
            Console.WriteLine(list[0].Customers.Address);
        }

        [TestMethod]
        public void TestGroupJoin()
        {
            var tt = db.Customers.GroupBy(s => s.CustomerID);

            var d = from s in db.Orders
                    join t in tt on s.CustomerID equals t.Key
                    select t;

            Console.Write(d.ToList());
        }

        [TestMethod]
        public void TestExtendAs()
        {
            var list = db.Orders.Select(s => s.ExtendAs<OrderEx>(() => new { Name1 = s.Customers.CompanyName.Substring(0, 1) })).ToList();
            Console.WriteLine(list[0].Name1);
            Console.WriteLine(list[0].OrderID);
        }

        [TestMethod]
        public void TestTo()
        {
            var list = db.Orders.Select(s => s.To<OrderEx>(null)).ToList();
            Console.WriteLine(list[0].Name1);
            Console.WriteLine(list[0].OrderID);
        }

        [TestMethod]
        public void TestJoinExtendAs()
        {
            var list = db.Orders
                .Join(db.Customers.DefaultIfEmpty(), t => t.CustomerID, t => t.CustomerID, (s, t) =>
                        s.ExtendAs<OrderEx>(() => new { Name1 = t.City, DDD = s.Freight })
                    )
                .ToList();
            Console.WriteLine(list[0].Name1);
            Console.WriteLine(list[0].OrderID);
        }

        [TestMethod]
        public void TestJoinExtendAs1()
        {
            var list = db.Orders
                .Join(db.Customers.DefaultIfEmpty(), t => t.CustomerID, t => t.CustomerID, (s, t) =>
                        new OrderEx { Name1 = t.City, DDD = s.Freight }
                    )
                .ToList();
            Console.WriteLine(list[0].Name1);
            Console.WriteLine(list[0].OrderID);
        }

        private class OrderEx
        {
            public int OrderID { get; set; }

            public string Name1 { get; set; }

            public decimal? DDD { get; set; }

            public int Any { get; set; }
        }

        [TestMethod]
        public void TestIncludeSingle()
        {
            var list = db.Orders.Include(s => s.Customers).Where(s => s.CustomerID == "ALFKI").ToList();
            Assert.AreEqual("ALFKI", list[0].Customers.CustomerID);
        }

        [TestMethod]
        public void TestIncludeSingle1()
        {
            var list = db.OrderDetails.Include(s => s.Orders.Customers)
                .Where(s => s.Orders.CustomerID == "ALFKI").Select(s => new { s.Orders.Customers.CustomerID }).ToList();
            Assert.AreEqual("ALFKI", list[0].CustomerID);
        }

        //[TestMethod]
        public void TestIncludeDetails()
        {
            var list = db.Orders.Include(s => s.OrderDetailses).Where(s => s.CustomerID == "ALFKI").ToList();
            foreach (var i in list)
            {
                Console.WriteLine(i.OrderDetailses.Count);
            }
        }

        [TestMethod]
        public void TestIncludeDeepField()
        {
            var list = db.OrderDetails.Include(s => s.Orders.Customers).Where(s => s.Quantity == 1).ToList();
            Assert.AreEqual(10259, list[0].Orders.OrderID);
            Assert.AreEqual("CENTC", list[0].Orders.Customers.CustomerID);
        }

        [TestMethod]
        public void TestCustomersAssociateOrders()
        {
            var custs = db.Customers.Associate(s => s.Orderses.Where(o => (o.OrderID & 1) == 0))
                .Where(c => c.CustomerID == "ALFKI")
                .Select(c => new { CustomerID = c.CustomerID, FilteredOrdersCount = c.Orderses.Count() }).ToList();
            Assert.AreEqual(1, custs.Count);
            Assert.AreEqual(3, custs[0].FilteredOrdersCount);
        }

        //[TestMethod]
        public void TestCustomersIncludeThenAssociateOrders()
        {
            var custs = db.Customers.Include(c => c.Orderses)
                .Associate(c => c.Orderses.Where(o => (o.OrderID & 1) == 0))
                .Where(c => c.CustomerID == "ALFKI").ToList();
            Assert.AreEqual(1, custs.Count);
            Assert.AreEqual(3, custs[0].Orderses.Count);
        }

        [TestMethod]
        public void TestCustomersApplyFilter()
        {
            db.Apply<Customers>(seq => seq.Where(c => c.City == "London"));
            var custs = db.Customers.ToList();
            Assert.AreEqual(6, custs.Count);
        }

        [TestMethod]
        public void TestInterface()
        {
            var query1 = db.Set(typeof(P1)) as IQueryable<ITestEntity>;
            var list1 = query1.Where(s => s.Name == "").ToList();
            var query2 = db.Set(typeof(P2)) as IQueryable<ITestEntity>;
            var list2 = query2.Where(s => s.Name == "").ToList();
        }
    }

    public interface ITestEntity
    {
        string Name { get; set; }
    }

    public class P1 : LightEntity<P1>, ITestEntity
    {
        [PropertyMapping]
        public virtual string Name { get; set; }
    }

    public class P2 : LightEntity<P2>, ITestEntity
    {
        [PropertyMapping]
        public virtual string Name { get; set; }
    }
}
