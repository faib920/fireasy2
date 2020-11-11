// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;
using Fireasy.Common.Configuration;
using Fireasy.Data.Entity.Linq;
using Fireasy.Data.Entity.Linq.Translators;
using Fireasy.Data.Entity.Query;
using Fireasy.Data.Entity.Tests.Models;
using Fireasy.Data.Provider;
using Fireasy.Data.Syntax;
#if NETCOREAPP2_0
using Microsoft.Extensions.Configuration;
#endif
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Tests
{
    [TestClass()]
    public class TranslationTests
    {
        private const string Var1 = "lon";
        private DbContext db;

        [TestInitialize]
        public void Init()
        {
            InitConfig.Init();
            db = new DbContext();
        }

        [TestCleanup]
        public void Dispose()
        {
            db.Dispose();
        }

        private void TestQuery(IQueryable q)
        {
            Console.WriteLine(TimeWatcher.Watch(() =>
                {
                    var result = (q.Provider as ITranslateSupport).Translate(q.Expression);
                    var queryText = result.QueryText;

                    foreach (Parameter par in result.Parameters)
                    {
                        queryText += "\r\n-- " + par.ParameterName + "=" + par.Value;
                    }

                    Console.WriteLine(queryText);
#if Q
                    var r = q.GetEnumerator();
                    while (r.MoveNext())
                    {
                    }
#endif
                }));
        }

        private void TestQuery(Expression<Func<object>> query)
        {
            Console.WriteLine(TimeWatcher.Watch(() =>
                {
                    var exp = query.Body;
                    if (exp.NodeType == ExpressionType.Convert &&
                        exp.Type == typeof(object))
                    {
                        exp = ((UnaryExpression)exp).Operand;
                    }

                    var svr = db.GetService<IContextService>();
                    var pro = db.GetService<IProvider>();
                    var enq = new EntityQueryProvider(svr);
                    var result = enq.Translate(exp);
                    var queryText = result.QueryText;
                    if (result.DataSegment != null)
                    {
                        queryText = pro.GetService<ISyntaxProvider>().Segment(queryText, result.DataSegment);
                    }

                    foreach (Parameter par in result.Parameters)
                    {
                        queryText += "\r\n-- " + par;
                    }
                    Console.WriteLine(queryText);

#if Q
                        Console.WriteLine(enq.Execute(exp));
#endif
                }));
        }

        [TestMethod()]
        public void TestWhere()
        {
            TestQuery(db.Customers.Where(c => c.City == "London"));
        }

        [TestMethod()]
        public void TestWhereStrings()
        {
            TestQuery(db.Customers.Where(c => c.City == new string('a', 10)));
        }

        [TestMethod()]
        public void TestWhereVar()
        {
            var par = "London";
            TestQuery(db.Customers.Where(c => c.City == par));
        }

        [TestMethod()]
        public void TestWhereVar1()
        {
            TestQuery(db.Customers.Where(c => c.City == Var1));
        }

        [TestMethod()]
        public void TestWhereVar2()
        {
            var par = new Customers { City = "dfafd" };
            TestQuery(db.Customers.Where(c => c.City == par.City));
        }

        [TestMethod()]
        public void TestWhereVar3()
        {
            TestQuery(db.Customers.Where(c => c.City == new Customers { City = "dfafd" }.City));
        }

        [TestMethod()]
        public void TestWhereVar4()
        {
            var id = 55;
            TestQuery(db.Products.Where(c => c.CategoryID == id));
        }

        [TestMethod()]
        public void TestWhereSum()
        {
            TestQuery(db.Customers.Where(c => c.Orderses.Sum(s => s.OrderID) > 1000));
        }

        [TestMethod()]
        public void TestWhereCount()
        {
            TestQuery(
                from c in db.Customers
                where db.Orders.Count() > 100
                select c
                );
        }

        [TestMethod()]
        public void TestWhereTrue()
        {
            TestQuery(db.Customers.Where(c => true));
        }

        [TestMethod()]
        public void TestWhereFalse()
        {
            TestQuery(db.Customers.Where(c => false));
        }

        [TestMethod()]
        public void TestSelectScalar()
        {
            TestQuery(db.Customers.Select(c => c.City));
        }

        [TestMethod()]
        public void TestSelectAnonymousOne()
        {
            TestQuery(db.Customers.Select(c => new { c.City }));
        }

        [TestMethod()]
        public void TestSelectAnonymousFunc()
        {
            TestQuery(db.Customers.Select(c => new { Country1 = c.Country, A = c.City.Length, B = c.City.Substring(0, 10) + c.Address }));
        }

        [TestMethod()]
        public void TestSelectAnonymousTwo()
        {
            TestQuery(db.Customers.Select(c => new { A = c.City, B = c.Phone }));
        }

        [TestMethod()]
        public void TestSelectAnonymousThree()
        {
            TestQuery(db.Customers.Select(c => new { c.City, c.Phone, c.Country }));
        }

        [TestMethod()]
        public void TestSelectCustomersTable()
        {
            TestQuery(db.Customers);
        }

        [TestMethod()]
        public void TestSelectCustomeridentity()
        {
            TestQuery(db.Customers.Select(c => c));
        }

        [TestMethod()]
        public void TestSelectAnonymousWithObject()
        {
            TestQuery(db.Customers.Select(c => new { c.City, c }));
        }

        [TestMethod()]
        public void TestSelectAnonymousNested()
        {
            TestQuery(db.Customers.Select(c => new { c.City, Country = new { c.Country } }));
        }

        [TestMethod()]
        public void TestSelectAnonymousEmpty()
        {
            TestQuery(db.Customers.Select(c => new { }));
        }

        [TestMethod()]
        public void TestSelectAnonymousLiteral()
        {
            TestQuery(db.Customers.Select(c => new { c.Address, X = 10 }));
        }

        [TestMethod()]
        public void TestSelectConstantInt()
        {
            TestQuery(db.Customers.Select(c => 0));
        }

        [TestMethod()]
        public void TestSelectConstantNullString()
        {
            TestQuery(db.Customers.Select(c => (string)null));
        }

        [TestMethod()]
        public void TestSelectLocal()
        {
            int x = 10;
            TestQuery(db.Customers.Select(c => x));
        }

        [TestMethod()]
        public void TestSelectNestedCollection()
        {
            TestQuery(
                from c in db.Customers
                where c.CustomerID == "ALFKI"
                select db.Orders.Where(o => o.CustomerID == c.CustomerID && o.OrderDate.Value.Year == 1997).Select(o => o.OrderID)
                );
        }

        [TestMethod()]
        public void TestSelectNestedCollectionInAnonymousType()
        {
            TestQuery(
                from c in db.Customers
                where c.CustomerID == "ALFKI"
                select new { Foos = db.Orders.Where(o => o.CustomerID == c.CustomerID && o.OrderDate.Value.Year == 1997).Select(o => o.OrderID) }
                );
        }

        [TestMethod()]
        public void TestJoinCustomersOrderss()
        {
            TestQuery(
                from c in db.Customers
                join o in db.Orders on c.CustomerID equals o.CustomerID
                select new { Contactname = c.ContactName, Orderid = o.OrderID }
                );
        }

        [TestMethod()]
        public void TestJoinLeft()
        {
            TestQuery(
                from c in db.Customers
                join o in db.Orders.DefaultIfEmpty() on c.CustomerID equals o.CustomerID
                select new { Contactname = c.ContactName, Orderid = o.OrderID }
                );
        }

        [TestMethod()]
        public void TestJoinLeft1()
        {
            TestQuery(
                from c in db.Customers
                join o in db.Orders on c.CustomerID equals o.CustomerID into o1
                from o11 in o1.DefaultIfEmpty()
                join t in db.OrderDetails on o11.OrderID equals t.OrderID into t1
                from t11 in t1.DefaultIfEmpty()
                select new { Contactname = c.ContactName, Orderid = o11.OrderID, Productid = t11.ProductID }
                );
        }

        [TestMethod()]
        public void TestJoinLeft2()
        {
            TestQuery(
                from c in db.Customers
                join o in db.Orders.DefaultIfEmpty() on c.CustomerID equals o.CustomerID
                join t in db.OrderDetails.DefaultIfEmpty() on o.OrderID equals t.OrderID
                select new { Contactname = c.ContactName, Orderid = o.OrderID, Productid = t.ProductID }
                );
        }

        [TestMethod()]
        public void TestJoinRight()
        {
            TestQuery(
                from c in db.Customers.DefaultIfEmpty()
                join o in db.Orders on c.CustomerID equals o.CustomerID
                select new { Contactname = c.ContactName, Orderid = o.OrderID }
                );
        }

        [TestMethod()]
        public void TestJoinCustomersByColumn()
        {
            TestQuery(
                from c in db.Customers
                join o in db.Orders on c.CustomerID equals o.CustomerID
                select new { CustomersPhone = c.Phone }
                );
        }

        [TestMethod()]
        public void TestSelectManyCustomersOrderss()
        {
            TestQuery(
                from c in db.Customers
                from o in db.Orders
                where c.CustomerID == o.CustomerID
                select new { Contactname = c.ContactName, Orderid = o.OrderID }
                );
        }

        [TestMethod()]
        public void TestOrderBy()
        {
            TestQuery(
                db.Customers.OrderBy(c => c.CustomerID)
                );
        }

        [TestMethod()]
        public void TestOrderLinqBy()
        {
            TestQuery(
                db.Customers.OrderBy("Customerid").ThenBy("Address", SortOrder.Ascending)
                );
        }

        [TestMethod()]
        public void TestOrderBySelect()
        {
            TestQuery(
                db.Customers.OrderBy(c => c.CustomerID).Select(c => c.ContactName)
                );
        }

        [TestMethod()]
        public void TestOrderBySelectSingle()
        {
            TestQuery(
                () => db.Customers.OrderBy(c => c.CustomerID).Select(c => c.ContactName).FirstOrDefault()
                );
        }

        [TestMethod()]
        public void TestOrderByOrderBy()
        {
            TestQuery(
                db.Customers.OrderBy(c => c.CustomerID).OrderBy(c => c.Country).Select(c => c.City)
                );
        }

        [TestMethod()]
        public void TestOrderByThenBy()
        {
            TestQuery(
                db.Customers.OrderBy(c => c.CustomerID).ThenBy(c => c.Country).Select(c => c.City)
                );
        }

        [TestMethod()]
        public void TestOrderByMutil()
        {
            TestQuery(
                db.Customers.OrderBy(c => new { Customerid = c.CustomerID, Companyname = c.CompanyName })
                );
        }

        [TestMethod()]
        public void TestOrderByThenByMutil()
        {
            TestQuery(
                db.Customers.OrderBy(c => c.Fax).ThenBy(c => new { Customerid = c.CustomerID, Companyname = c.CompanyName })
                );
        }

        [TestMethod()]
        public void TestOrderByMutilJoin()
        {
            TestQuery(
                db.Orders.OrderBy(c => new { Orderid = c.OrderID, Customerid = c.Customers.CustomerID })
                );
        }

        [TestMethod()]
        public void TestOrderByMutilDescending()
        {
            TestQuery(
                db.Customers.OrderByDescending(c => new { Customerid = c.CustomerID, Companyname = c.CompanyName })
                );
        }

        [TestMethod()]
        public void TestOrderByDescending()
        {
            TestQuery(
                db.Customers.OrderByDescending(c => c.CustomerID).Select(c => c.City)
                );
        }

        [TestMethod()]
        public void TestOrderByDescendingThenBy()
        {
            TestQuery(
                db.Customers.OrderByDescending(c => c.CustomerID).ThenBy(c => c.Country).Select(c => c.City)
                );
        }

        [TestMethod()]
        public void TestOrderByDescendingThenByDescending()
        {
            TestQuery(
                db.Customers.OrderByDescending(c => c.CustomerID).ThenByDescending(c => c.Country).Select(c => c.City)
                );
        }

        [TestMethod()]
        public void TestOrderByJoin()
        {
            TestQuery(
                from c in db.Customers.OrderBy(c => c.CustomerID)
                join o in db.Orders.OrderBy(o => o.OrderID) on c.CustomerID equals o.CustomerID
                select new { Customerid = c.CustomerID, Orderid = o.OrderID }
                );
        }

        [TestMethod()]
        public void TestOrderBySelectMany()
        {
            TestQuery(
                from c in db.Customers.OrderBy(c => c.CustomerID)
                from o in db.Orders.OrderBy(o => o.OrderID)
                where c.CustomerID == o.CustomerID
                select new { Contactname = c.ContactName, Orderid = o.OrderID }
                );

        }

        [TestMethod()]
        public void TestGroupBy()
        {
            TestQuery(
                db.Customers.GroupBy(c => c.City)
                );
        }

        [TestMethod()]
        public void TestGroupBySelectMany()
        {
            TestQuery(
                db.Customers.GroupBy(c => c.City).SelectMany(g => g)
                );
        }

        [TestMethod()]
        public void TestGroupBySum()
        {
            TestQuery(
                db.Orders.GroupBy(o => o.CustomerID).Select(g => g.Sum(o => o.OrderID))
                );
        }

        [TestMethod()]
        public void TestGroupByCount()
        {
            TestQuery(
                db.Orders.GroupBy(o => o.CustomerID).Select(g => g.Count())
                );
        }

        [TestMethod()]
        public void TestGroupBySumMinMaxAvg()
        {
            TestQuery(
                db.Orders.GroupBy(o => o.CustomerID).Select(g =>
                    new
                    {
                        Key = g.Key,
                        Sum = g.Sum(o => o.OrderID),
                        Min = g.Min(o => o.OrderID),
                        Max = g.Max(o => o.OrderDate),
                        Avg = g.Average(o => o.OrderID)
                    }).OrderBy(s => s.Max)
                );
        }

        [TestMethod()]
        public void TestGroupByWithResultSelector()
        {
            TestQuery(
                db.Orders.GroupBy(o => o.CustomerID, (k, g) =>
                    new
                    {
                        Sum = g.Sum(o => o.OrderID),
                        Min = g.Min(o => o.OrderID),
                        Max = g.Max(o => o.OrderID),
                        Avg = g.Average(o => o.OrderID)
                    })
                );
        }

        [TestMethod()]
        public void TestGroupByWithElementSelectorSum()
        {
            TestQuery(
                db.Orders.GroupBy(o => o.CustomerID, o => o.OrderID).Select(g => g.Sum())
                );
        }

        [TestMethod()]
        public void TestGroupByWithElementSelector()
        {
            // note: groups are retrieved through a separately execute subquery per row
            TestQuery(
                db.Orders.GroupBy(o => o.CustomerID, o => o.OrderID)
                );
        }

        [TestMethod()]
        public void TestGroupByWithElementSelectorSumMax()
        {
            TestQuery(
                db.Orders.GroupBy(o => o.CustomerID, o => o.OrderID).Select(g => new { Sum = g.Sum(), Max = g.Max() })
                );
        }

        [TestMethod()]
        public void TestGroupByWithAnonymousElement()
        {
            TestQuery(
                db.Orders.GroupBy(o => o.CustomerID, o => new { Orderid = o.OrderID }).Select(g => g.Sum(x => x.Orderid))
                );
        }

        [TestMethod()]
        public void TestGroupByWithTwoPartKey()
        {
            TestQuery(
                db.Orders.GroupBy(o => new { Customerid = o.CustomerID, Orderdate = o.OrderDate }).Select(g => g.Sum(o => o.OrderID))
                );
        }

        [TestMethod()]
        public void TestOrderByGroupBy()
        {
            // note: order-by is lost when group-by is applied (the sequence of groups is not ordered)
            TestQuery(
                db.Orders.OrderBy(o => o.OrderID).GroupBy(o => o.CustomerID).Select(g => g.Sum(o => o.OrderID))
                );
        }

        [TestMethod()]
        public void TestOrderByGroupBySelectMany()
        {
            // note: order-by is preserved within grouped sub-collections
            TestQuery(
                db.Orders.OrderBy(o => o.OrderID).GroupBy(o => o.CustomerID).SelectMany(g => g)
                );
        }

        [TestMethod()]
        public void TestOrderByGroupHaving()
        {
            // note: order-by is preserved within grouped sub-collections
            TestQuery(
                db.Orders.OrderBy(o => o.OrderID).GroupBy(o => o.CustomerID).Where(s => s.Count() > 0 || s.Sum(t => t.Freight) > 0).Select(s => s.Sum(t => t.Freight))
                );
        }

        [TestMethod()]
        public void TestSumWithNoArg()
        {
            TestQuery(
                () => db.Orders.Select(o => o.OrderID).Sum()
                );
        }

        [TestMethod()]
        public void TestSumWithArg()
        {
            TestQuery(
                () => db.Orders.Sum(o => o.OrderID)
                );
        }

        [TestMethod()]
        public void TestSumComplex()
        {
            TestQuery(
                () => db.Orders.Select(s => new { CompanyName = s.Customers.CompanyName, Sum = s.OrderDetailses.Sum(t => t.UnitPrice) })
                );
        }

        [TestMethod()]
        public void TestCountWithNoPredicate()
        {
            TestQuery(
                () => db.Orders.Count()
                );
        }

        [TestMethod()]
        public void TestCountWithPredicate()
        {
            TestQuery(
                () => db.Orders.Count(o => o.CustomerID == "ALFKI")
                );
        }

        [TestMethod()]
        public void TestDistinct()
        {
            TestQuery(
                db.Customers.Distinct()
                );
        }

        [TestMethod()]
        public void TestDistinctScalar()
        {
            TestQuery(
                db.Customers.Select(c => c.City).Distinct()
                );
        }

        [TestMethod()]
        public void TestOrderByDistinct()
        {
            TestQuery(
                db.Customers.OrderBy(c => c.CustomerID).Select(c => c.City).Distinct()
                );
        }

        [TestMethod()]
        public void TestDistinctOrderBy()
        {
            TestQuery(
                db.Customers.Select(c => c.City).Distinct().OrderBy(c => c)
                );
        }

        [TestMethod()]
        public void TestDistinctGroupBy()
        {
            TestQuery(
                db.Orders.Distinct().GroupBy(o => o.CustomerID)
                );
        }

        [TestMethod()]
        public void TestGroupByDistinct()
        {
            TestQuery(
                db.Orders.GroupBy(o => o.CustomerID).Distinct()
                );

        }

        [TestMethod()]
        public void TestDistinctCount()
        {
            TestQuery(
                () => db.Customers.Distinct().Count()
                );
        }

        [TestMethod()]
        public void TestSelectDistinctCount()
        {
            // cannot do: SELECT COUNT(DISTINCT some-colum) FROM some-table
            // because COUNT(DISTINCT some-column) does not count nulls
            TestQuery(
                () => db.Customers.Select(c => c.City).Distinct().Count()
                );
        }

        [TestMethod()]
        public void TestSelectSelectDistinctCount()
        {
            TestQuery(
                () => db.Customers.Select(c => c.City).Select(c => c).Distinct().Count()
                );
        }

        [TestMethod()]
        public void TestDistinctCountPredicate()
        {
            TestQuery(
                () => db.Customers.Distinct().Count(c => c.CustomerID == "ALFKI")
                );
        }

        [TestMethod()]
        public void TestDistinctSumWithArg()
        {
            TestQuery(
                () => db.Orders.Distinct().Sum(o => o.OrderID)
                );
        }

        [TestMethod()]
        public void TestSelectDistinctSum()
        {
            TestQuery(
                () => db.Orders.Select(o => o.OrderID).Distinct().Sum()
                );
        }

        [TestMethod()]
        public void TestTake()
        {
            TestQuery(
                db.Orders.Take(5)
                );
        }

        [TestMethod()]
        public void TestTakeDistinct()
        {
            // distinct must be forced to apply after top has been computed
            TestQuery(
                db.Orders.Take(5).Distinct()
                );
        }

        [TestMethod()]
        public void TestDistinctTake()
        {
            // top must be forced to apply after distinct has been computed
            TestQuery(
                db.Orders.Distinct().Take(5)
                );
        }

        [TestMethod()]
        public void TestDistinctTakeCount()
        {
            TestQuery(
                () => db.Orders.Distinct().Take(5).Count()
                );
        }

        [TestMethod()]
        public void TestTakeDistinctCount()
        {
            TestQuery(
                () => db.Orders.Take(5).Distinct().Count()
                );
        }

        [TestMethod()]
        public void TestSkip()
        {
            TestQuery(
                db.Customers.OrderBy(c => c.ContactName).Skip(5)
                );
        }

        [TestMethod()]
        public void TestSkipTake()
        {
            TestQuery(
                db.Customers.OrderBy(c => c.ContactName).Skip(5).Take(10)
                );
        }

        [TestMethod()]
        public void TestTakeSkip()
        {
            TestQuery(
                db.Customers.OrderBy(c => c.ContactName).Take(10).Skip(5)
                );
        }

        [TestMethod()]
        public void TestSkipDistinct()
        {
            TestQuery(
                db.Customers.OrderBy(c => c.ContactName).Skip(5).Distinct()
                );
        }

        [TestMethod()]
        public void TestDistinctSkip()
        {
            TestQuery(
                db.Customers.Distinct().OrderBy(c => c.ContactName).Skip(5)
                );
        }

        [TestMethod()]
        public void TestSkipTakeDistinct()
        {
            TestQuery(
                db.Customers.OrderBy(c => c.ContactName).Skip(5).Take(10).Distinct()
                );
        }

        [TestMethod()]
        public void TestTakeSkipDistinct()
        {
            TestQuery(
                db.Customers.OrderBy(c => c.ContactName).Take(10).Skip(5).Distinct()
                );
        }

        [TestMethod()]
        public void TestDistinctSkipTake()
        {
            TestQuery(
                db.Customers.Distinct().OrderBy(c => c.ContactName).Skip(5).Take(10)
                );
        }


        [TestMethod()]
        public void TestFirst()
        {
            TestQuery(
                () => db.Customers.First()
                );
        }

        [TestMethod()]
        public void TestFirstPredicate()
        {
            TestQuery(
                () => db.Customers.First(c => c.CustomerID == "ALFKI")
                );
        }

        [TestMethod()]
        public void TestWhereFirst()
        {
            TestQuery(
                () => db.Customers.Where(c => c.CustomerID == "ALFKI").First()
                );
        }

        [TestMethod()]
        public void TestFirstOrDefault()
        {
            TestQuery(
                () => db.Customers.FirstOrDefault()
                );
        }

        [TestMethod()]
        public void TestFirstOrDefaultPredicate()
        {
            TestQuery(
                () => db.Customers.FirstOrDefault(c => c.CustomerID == "ALFKI")
                );
        }

        [TestMethod()]
        public void TestWhereFirstOrDefault()
        {
            TestQuery(
                () => db.Customers.Where(c => c.CustomerID == "ALFKI").FirstOrDefault()
                );
        }

        [TestMethod()]
        public void TestSegment()
        {
            var pager = new DataPager(10);
            TestQuery(
                () => db.Customers.OrderBy(s => s.City).Segment(pager)
                );
        }

        [TestMethod()]
        public void TestSingle()
        {
            TestQuery(
                () => db.Customers.Single()
                );
        }

        [TestMethod()]
        public void TestSinglePredicate()
        {
            TestQuery(
                () => db.Customers.Single(c => c.CustomerID == "ALFKI")
                );
        }

        [TestMethod()]
        public void TestWhereSingle()
        {
            TestQuery(
                () => db.Customers.Where(c => c.CustomerID == "ALFKI").Single()
                );
        }

        [TestMethod()]
        public void TestSingleOrDefault()
        {
            TestQuery(
                () => db.Customers.SingleOrDefault()
                );
        }

        [TestMethod()]
        public void TestSingleOrDefaultPredicate()
        {
            TestQuery(
                () => db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI")
                );
        }

        [TestMethod()]
        public void TestWhereSingleOrDefault()
        {
            TestQuery(
                () => db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault()
                );
        }

        [TestMethod()]
        public void TestAnyWithSubquery()
        {
            TestQuery(
                db.Customers.Where(c => db.Orders.Where(o => o.CustomerID == c.CustomerID).Any(o => o.OrderDate.Value.Year == 1997))
                );
        }

        [TestMethod()]
        public void TestAnyWithSubqueryNoPredicate()
        {
            TestQuery(
                db.Customers.Where(c => db.Orders.Where(o => o.CustomerID == c.CustomerID).Any())
                );
        }

        [TestMethod()]
        public void TestAnyWithLocalCollection()
        {
            string[] ids = new[] { "ABCDE", "ALFKI" };
            TestQuery(
                db.Customers.Where(c => ids.Any(id => c.CustomerID == id))
                );
        }

        [TestMethod()]
        public void TestAnyTopLevel()
        {
            TestQuery(
                () => db.Customers.Any()
                );
        }

        [TestMethod()]
        public void TestAllWithSubquery()
        {
            TestQuery(
                db.Customers.Where(c => db.Orders.Where(o => o.CustomerID == c.CustomerID).All(o => o.OrderDate.Value.Year == 1997))
                );
        }

        [TestMethod()]
        public void TestAllWithLocalCollection()
        {
            string[] patterns = new[] { "a", "e" };

            TestQuery(
                db.Customers.Where(c => patterns.All(p => c.ContactName.Contains(p)))
                );
            TestQuery(
                db.Customers.BatchAnd(patterns, (c, p) => c.CompanyName.Contains(p))
                );
        }

        [TestMethod()]
        public void TestAllTopLevel()
        {
            TestQuery(
                () => db.Customers.All(c => c.ContactName.StartsWith("a"))
                );
        }

        [TestMethod()]
        public void TestContainsWithSubquery()
        {
            TestQuery(
                db.Customers.Where(c => db.Orders.Select(o => o.CustomerID).Contains(c.CustomerID))
                );
        }

        [TestMethod()]
        public void TestContainsWithSubquery1()
        {
            TestQuery(
                db.Customers.Where(c => db.Orders
                    .Where(v => db.Products.Select(p => p.ProductName).Contains(v.CustomerID))
                    .Select(o => o.CustomerID)
                    .Contains(c.CustomerID))
            );
        }

        [TestMethod()]
        public void TestContainsWithOutquery1()
        {
            var products = db.Products.Select(p => p.ProductName);
            var orders = db.Orders.Where(v => products.Contains(v.CustomerID)).Select(o => o.CustomerID);
            TestQuery(
                db.Customers.Where(c => orders.Contains(c.CustomerID))
                );
        }

        [TestMethod()]
        public void TestContainsWithOutquery()
        {
            var ss = db.Orders.Select(o => o.CustomerID);
            TestQuery(
                db.Customers.Where(c => ss.Contains(c.CustomerID))
                );
        }

        [TestMethod()]
        public void TestContainsWithLocalCollection()
        {
            string[] ids = new[] { "ABCDE", "ALFKI" };
            TestQuery(
                db.Customers.Where(c => ids.Contains(c.CustomerID))
                );
        }

        [TestMethod()]
        public void TestContainsTopLevel()
        {
            TestQuery(
                () => db.Customers.Select(c => c.CustomerID).Contains("ALFKI")
                );
        }

        // framework function tests

        [TestMethod()]
        public void TestStringLength()
        {
            TestQuery(db.Customers.Where(c => c.City.Length == 7));
        }

        [TestMethod()]
        public void TestStringStartsWithLiteral()
        {
            TestQuery(db.Customers.Where(c => c.ContactName.StartsWith("M")));
        }

        [TestMethod()]
        public void TestStringStartsWithColumn()
        {
            TestQuery(db.Customers.Where(c => c.ContactName.StartsWith(c.ContactName)));
        }

        [TestMethod()]
        public void TestStringEndsWithLiteral()
        {
            TestQuery(db.Customers.Where(c => c.ContactName.EndsWith("s")));
        }

        [TestMethod()]
        public void TestStringEndsWithColumn()
        {
            TestQuery(db.Customers.Where(c => c.ContactName.EndsWith(c.ContactName)));
        }

        [TestMethod()]
        public void TestStringContainsLiteral()
        {
            TestQuery(db.Customers.Where(c => c.ContactName.Contains("and")));
        }

        [TestMethod()]
        public void TestStringContainsColumn()
        {
            TestQuery(db.Customers.Where(c => c.ContactName.Contains(c.ContactName)));
        }

        [TestMethod()]
        public void TestStringConcatImplicit2Args()
        {
            TestQuery(db.Customers.Where(c => c.ContactName + "X" == "X"));
        }

        [TestMethod()]
        public void TestStringConcatExplicit2Args()
        {
            TestQuery(db.Customers.Where(c => string.Concat(c.ContactName, "X") == "X"));
        }

        [TestMethod()]
        public void TestStringConcatExplicit3Args()
        {
            TestQuery(db.Customers.Where(c => string.Concat(c.ContactName, "X", c.Country) == "X"));
        }

        [TestMethod()]
        public void TestStringConcatExplicitNArgs()
        {
            TestQuery(db.Customers.Where(c => string.Concat(new string[] { c.ContactName, "X", c.Country }) == "X"));
        }

        [TestMethod()]
        public void TestStringIsNullOrEmpty()
        {
            TestQuery(db.Customers.Where(c => string.IsNullOrEmpty(c.City)));
        }

        [TestMethod()]
        public void TestStringToUpper()
        {
            TestQuery(db.Customers.Where(c => c.City.ToUpper() == "SEATTLE"));
        }

        [TestMethod()]
        public void TestStringToLower()
        {
            TestQuery(db.Customers.Where(c => c.City.ToLower() == "seattle"));
        }

        [TestMethod()]
        public void TestStringReplace()
        {
            TestQuery(db.Customers.Where(c => c.City.Replace("ea", "ae") == "Saettle"));
        }

        [TestMethod()]
        public void TestStringReplaceChars()
        {
            TestQuery(db.Customers.Where(c => c.City.Replace("e", "y") == "Syattly"));
        }

        [TestMethod()]
        public void TestStringSubstring()
        {
            TestQuery(db.Customers.Where(c => c.City.Substring(0, 4) == "Seat"));
        }

        [TestMethod()]
        public void TestStringSubstringNest()
        {
            TestQuery(db.Customers.Where(c => c.City.Substring(0, c.City.Length + 6) == "Seat"));
        }

        [TestMethod()]
        public void TestStringSubstringNoLength()
        {
            TestQuery(db.Customers.Where(c => c.City.Substring(4) == "tle"));
        }

        [TestMethod()]
        public void TestStringRemove()
        {
            TestQuery(db.Customers.Where(c => c.City.Remove(1, 2) == "Sttle"));
        }

        [TestMethod()]
        public void TestStringRemoveNoCount()
        {
            TestQuery(db.Customers.Where(c => c.City.Remove(4) == "Seat"));
        }

        [TestMethod()]
        public void TestStringIndexOf()
        {
            TestQuery(db.Customers.Where(c => c.City.IndexOf("tt") == 4));
        }

        [TestMethod()]
        public void TestStringIndexOfChar()
        {
            TestQuery(db.Customers.Where(c => c.City.IndexOf('t') == 4));
        }

        [TestMethod()]
        public void TestStringTrim()
        {
            TestQuery(db.Customers.Where(c => c.City.Trim() == "Seattle"));
        }

        [TestMethod()]
        public void TestStringToString()
        {
            TestQuery(db.Customers.Where(c => c.City.ToString() == "Seattle"));
        }

        [TestMethod()]
        public void TestParseDecimal()
        {
            TestQuery(db.Customers.Where(c => decimal.Parse(c.CustomerID) == 56));
        }

        [TestMethod()]
        public void TestParseDateTime()
        {
            TestQuery(db.Customers.Where(c => DateTime.Parse(c.City) == DateTime.Now));
        }

        [TestMethod()]
        public void TestParseInt32()
        {
            TestQuery(db.Customers.Where(c => int.Parse(c.CustomerID) == 44));
        }

        [TestMethod()]
        public void TestConvertInt32()
        {
            TestQuery(db.Products.Where(c => Convert.ToInt32(c.Id) == 44));
        }

        [TestMethod()]
        public void TestChangeType()
        {
            TestQuery(db.Products.Where(c => (int)Convert.ChangeType(c.Id, typeof(int)) == 44));
        }

        [TestMethod()]
        public void TestChangeType1()
        {
            TestQuery(db.Products.Where(c => (int)Convert.ChangeType(c.Id, TypeCode.Int32) == 44));
        }

        [TestMethod()]
        public void TestRegexIsMatch()
        {
            //TestQuery(db.Products.Where(c => Regex.IsMatch(c.ProductName, @"\d+")));
            //TestQuery(db.Products.Where(c => Regex.IsMatch("aa", c.ProductName)));
        }

        [TestMethod()]
        public void TestDateTimeConstructYMD()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate == new DateTime(o.OrderDate.Value.Year, 1, 1)));
        }

        [TestMethod()]
        public void TestDateTimeConstructYMD1()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate == new DateTime(o.OrderDate.Value.Year, o.OrderDate.Value.Month, 1)));
        }

        [TestMethod()]
        public void TestDateTimeConstructYMD2()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate == new DateTime(2009, o.OrderDate.Value.Month, 1)));
        }

        //public void TestDateTimeConstructYMDHMS()
        //{
        //    TestQuery(db.Orderss.Where(o => o.Orderdate == new DateTime(o.Orderdate.Value.Year, 1, 1, 10, 25, 55)));
        //}

        [TestMethod()]
        public void TestDateTimeConstruct()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate == new DateTime(2007, 1, 1)));
        }

        [TestMethod()]
        public void TestDateTimeNow()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate == DateTime.Now));
        }

        [TestMethod()]
        public void TestDateTimeParse()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate == DateTime.Parse("2009-4-5")));
        }

        [TestMethod()]
        public void TestDateTimeParse1()
        {
            TestQuery(db.Orders.Select(s => DateTime.Parse(s.ShipCity)));
        }

        [TestMethod()]
        public void TestDateTimeMax()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate == DateTime.MaxValue));
        }

        [TestMethod()]
        public void TestDateTimeDay()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate.Value.Day == 5));
        }

        [TestMethod()]
        public void TestDateTimeMonth()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate.Value.Month == 12));
        }

        [TestMethod()]
        public void TestDateTimeYear()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate.Value.Year == 1997));
        }

        [TestMethod()]
        public void TestDateTimeHour()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate.Value.Hour == 6));
        }

        [TestMethod()]
        public void TestDateTimeMinute()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate.Value.Minute == 32));
        }

        [TestMethod()]
        public void TestDateTimeSecond()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate.Value.Second == 47));
        }

        [TestMethod()]
        public void TestDateTimeMillisecond()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate.Value.Millisecond == 200));
        }

        [TestMethod()]
        public void TestDateTimeToShortTimeString()
        {
            TestQuery(db.Orders.Where(s => s.OrderDate.Value.ToShortTimeString() == ""));
        }

        [TestMethod()]
        public void TestDateTimeDiff()
        {
            TestQuery(db.Orders.Select(s => s.OrderDate.Value - s.RequiredDate));
        }

        [TestMethod()]
        public void TestDateTimeDiff1()
        {
            TestQuery(db.Orders.Where(s => (s.OrderDate.Value - s.RequiredDate.Value).TotalHours > 0));
        }

        [TestMethod()]
        public void TestDateTimeDayOfWeek()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate.Value.DayOfWeek == DayOfWeek.Friday));
        }

        [TestMethod()]
        public void TestDateTimeDayOfYear()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate.Value.DayOfYear == 360));
        }

        [TestMethod()]
        public void TestMathAbs()
        {
            TestQuery(db.Orders.Where(o => Math.Abs(o.OrderID) == 10));
        }

        [TestMethod()]
        public void TestMathAcos()
        {
            TestQuery(db.Orders.Where(o => Math.Acos(o.OrderID) == 0));
        }

        [TestMethod()]
        public void TestMathAsin()
        {
            TestQuery(db.Orders.Where(o => Math.Asin(o.OrderID) == 0));
        }

        [TestMethod()]
        public void TestMathAtan()
        {
            TestQuery(db.Orders.Where(o => Math.Atan(o.OrderID) == 0));
        }

        [TestMethod()]
        public void TestMathAtan2()
        {
            //TestQuery(db.Orderss.Where(o => Math.Atan2(o.Orderid, 3) == 0));
        }

        [TestMethod()]
        public void TestMathCos()
        {
            TestQuery(db.Orders.Where(o => Math.Cos(o.OrderID) == 0));
        }

        [TestMethod()]
        public void TestMathSin()
        {
            TestQuery(db.Orders.Where(o => Math.Sin(o.OrderID) == 0));
        }

        [TestMethod()]
        public void TestMathTan()
        {
            TestQuery(db.Orders.Where(o => Math.Tan(o.OrderID) == 0));
        }

        [TestMethod()]
        public void TestMathExp()
        {
            TestQuery(db.Orders.Where(o => Math.Exp(o.OrderID < 1000 ? 1 : 2) == 0));
        }

        [TestMethod()]
        public void TestMathLog()
        {
            TestQuery(db.Orders.Where(o => Math.Log(o.OrderID) == 0));
        }

        [TestMethod()]
        public void TestMathLog10()
        {
            TestQuery(db.Orders.Where(o => Math.Log10(o.OrderID) == 0));
        }

        [TestMethod()]
        public void TestMathSqrt()
        {
            TestQuery(db.Orders.Where(o => Math.Sqrt(o.OrderID) == 0));
        }

        [TestMethod()]
        public void TestMathCeiling()
        {
            TestQuery(db.Orders.Where(o => Math.Ceiling((double)o.OrderID) == 0));
        }

        [TestMethod()]
        public void TestMathFloor()
        {
            TestQuery(db.Orders.Where(o => Math.Floor((double)o.OrderID) == 0));
        }

        [TestMethod()]
        public void TestMathPow()
        {
            TestQuery(db.Orders.Where(o => Math.Pow(o.OrderID < 1000 ? 1 : 2, 3) == 0));
        }

        [TestMethod()]
        public void TestMathRoundDefault()
        {
            TestQuery(db.Orders.Where(o => Math.Round((decimal)o.OrderID) == 0));
        }

        [TestMethod()]
        public void TestMathRoundToPlace()
        {
            TestQuery(db.Orders.Where(o => Math.Round((decimal)o.OrderID, 2) == 0));
        }

        [TestMethod()]
        public void TestMathTruncate()
        {
            TestQuery(db.Orders.Where(o => Math.Truncate((double)o.OrderID) == 0));
        }

        [TestMethod()]
        public void TestStringCompareToLT()
        {
            TestQuery(db.Customers.Where(c => c.City.CompareTo("Seattle") < 0));
        }

        [TestMethod()]
        public void TestStringCompareToLE()
        {
            TestQuery(db.Customers.Where(c => c.City.CompareTo("Seattle") <= 0));
        }

        [TestMethod()]
        public void TestStringCompareToGT()
        {
            TestQuery(db.Customers.Where(c => c.City.CompareTo("Seattle") > 0));
        }

        [TestMethod()]
        public void TestStringCompareToGE()
        {
            TestQuery(db.Customers.Where(c => c.City.CompareTo("Seattle") >= 0));
        }

        [TestMethod()]
        public void TestStringCompareToEQ()
        {
            TestQuery(db.Customers.Where(c => c.City.CompareTo("Seattle") == 0));
        }

        [TestMethod()]
        public void TestStringCompareToNE()
        {
            TestQuery(db.Customers.Where(c => c.City.CompareTo("Seattle") != 0));
        }

        [TestMethod()]
        public void TestStringCompareLT()
        {
            TestQuery(db.Customers.Where(c => string.Compare(c.City, "Seattle") < 0));
        }

        [TestMethod()]
        public void TestStringCompareLE()
        {
            TestQuery(db.Customers.Where(c => string.Compare(c.City, "Seattle") <= 0));
        }

        [TestMethod()]
        public void TestStringCompareGT()
        {
            TestQuery(db.Customers.Where(c => string.Compare(c.City, "Seattle") > 0));
        }

        [TestMethod()]
        public void TestStringCompareGE()
        {
            TestQuery(db.Customers.Where(c => string.Compare(c.City, "Seattle") >= 0));
        }

        [TestMethod()]
        public void TestStringCompareEQ()
        {
            TestQuery(db.Customers.Where(c => string.Compare(c.City, "Seattle") == 0));
        }

        [TestMethod()]
        public void TestStringCompareNE()
        {
            TestQuery(db.Customers.Where(c => string.Compare(c.City, "Seattle") != 0));
        }

        [TestMethod()]
        public void TestIntCompareTo()
        {
            // prove that x.CompareTo(y) works for types other than string
            TestQuery(db.Orders.Where(o => o.OrderID.CompareTo(1000) == 0));
        }

        [TestMethod()]
        public void TestDecimalCompare()
        {
            // prove that type.Compare(x,y) works with decimal
            TestQuery(db.Orders.Where(o => decimal.Compare((decimal)o.OrderID, 0.0m) == 0));
        }

        [TestMethod()]
        public void TestDecimalAdd()
        {
            TestQuery(db.Orders.Where(o => decimal.Add(o.OrderID, 0.0m) == 0.0m));
        }

        [TestMethod()]
        public void TestDecimalSubtract()
        {
            TestQuery(db.Orders.Where(o => decimal.Subtract(o.OrderID, 0.0m) == 0.0m));
        }

        [TestMethod()]
        public void TestDecimalMultiply()
        {
            TestQuery(db.Orders.Where(o => decimal.Multiply(o.OrderID, 1.0m) == 1.0m));
        }

        [TestMethod()]
        public void TestDecimalDivide()
        {
            TestQuery(db.Orders.Where(o => decimal.Divide(o.OrderID, 1.0m) == 1.0m));
        }

        [TestMethod()]
        public void TestDecimalRemainder()
        {
            TestQuery(db.Orders.Where(o => decimal.Remainder(o.OrderID, 1.0m) == 0.0m));
        }

        [TestMethod()]
        public void TestDecimalNegate()
        {
            TestQuery(db.Orders.Where(o => decimal.Negate(o.OrderID) == 1.0m));
        }

        [TestMethod()]
        public void TestDecimalCeiling()
        {
            TestQuery(db.Orders.Where(o => decimal.Ceiling(o.OrderID) == 0.0m));
        }

        [TestMethod()]
        public void TestDecimalFloor()
        {
            TestQuery(db.Orders.Where(o => decimal.Floor(o.OrderID) == 0.0m));
        }

        [TestMethod()]
        public void TestDecimalRoundDefault()
        {
            TestQuery(db.Orders.Where(o => decimal.Round(o.OrderID) == 0m));
        }

        [TestMethod()]
        public void TestDecimalRoundPlaces()
        {
            TestQuery(db.Orders.Where(o => decimal.Round(o.OrderID, 2) == 0.00m));
        }

        [TestMethod()]
        public void TestDecimalTruncate()
        {
            TestQuery(db.Orders.Where(o => decimal.Truncate(o.OrderID) == 0m));
        }

        [TestMethod()]
        public void TestDecimalLT()
        {
            // prove that decimals are treated normally with respect to normal comparison operators
            TestQuery(db.Orders.Where(o => ((decimal)o.OrderID) < 0.0m));
        }

        [TestMethod()]
        public void TestIntLessThan()
        {
            TestQuery(db.Orders.Where(o => o.OrderID < 0));
        }

        [TestMethod()]
        public void TestIntLessThanOrEqual()
        {
            TestQuery(db.Orders.Where(o => o.OrderID <= 0));
        }

        [TestMethod()]
        public void TestIntGreaterThan()
        {
            TestQuery(db.Orders.Where(o => o.OrderID > 0));
        }

        [TestMethod()]
        public void TestIntGreaterThanOrEqual()
        {
            TestQuery(db.Orders.Where(o => o.OrderID >= 0));
        }

        [TestMethod()]
        public void TestIntEqual()
        {
            TestQuery(db.Orders.Where(o => o.OrderID == 0));
        }

        [TestMethod()]
        public void TestIntNotEqual()
        {
            TestQuery(db.Orders.Where(o => o.OrderID != 0));
        }

        [TestMethod()]
        public void TestIntAdd()
        {
            TestQuery(db.Orders.Where(o => o.OrderID + 0 == 0));
        }

        [TestMethod()]
        public void TestIntSubtract()
        {
            TestQuery(db.Orders.Where(o => o.OrderID - 0 == 0));
        }

        [TestMethod()]
        public void TestIntMultiply()
        {
            TestQuery(db.Orders.Where(o => o.OrderID * 1 == 1));
        }

        [TestMethod()]
        public void TestIntDivide()
        {
            TestQuery(db.Orders.Where(o => o.OrderID / 1 == 1));
        }

        [TestMethod()]
        public void TestIntModulo()
        {
            TestQuery(db.Orders.Where(o => o.OrderID % 1 == 0));
        }

        [TestMethod()]
        public void TestIntLeftShift()
        {
            TestQuery(db.Orders.Where(o => o.OrderID << 1 == 0));
        }

        [TestMethod()]
        public void TestIntRightShift()
        {
            TestQuery(db.Orders.Where(o => o.OrderID >> 1 == 0));
        }

        [TestMethod()]
        public void TestIntBitwiseAnd()
        {
            TestQuery(db.Orders.Where(o => (o.OrderID & 1) == 0));
        }

        [TestMethod()]
        public void TestIntBitwiseOr()
        {
            TestQuery(db.Orders.Where(o => (o.OrderID | 1) == 1));
        }

        [TestMethod()]
        public void TestIntBitwiseExclusiveOr()
        {
            TestQuery(db.Orders.Where(o => (o.OrderID ^ 1) == 1));
        }

        [TestMethod()]
        public void TestIntBitwiseNot()
        {
            TestQuery(db.Orders.Where(o => ~o.OrderID == 0));
        }

        [TestMethod()]
        public void TestIntNegate()
        {
            TestQuery(db.Orders.Where(o => -o.OrderID == -1));
        }

        [TestMethod()]
        public void TestAnd()
        {
            TestQuery(db.Orders.Where(o => o.OrderID > 0 && o.OrderID < 2000));
        }

        [TestMethod()]
        public void TestOr()
        {
            TestQuery(db.Orders.Where(o => o.OrderID < 5 || o.OrderID > 10));
        }

        [TestMethod()]
        public void TestNot()
        {
            TestQuery(db.Orders.Where(o => (o.OrderID != 0)));
        }

        [TestMethod()]
        public void TestEqualNull()
        {
            TestQuery(db.Customers.Where(c => c.City == null));
        }

        [TestMethod()]
        public void TestEqualNullReverse()
        {
            TestQuery(db.Customers.Where(c => null == c.City));
        }

        [TestMethod()]
        public void TestCoalsce()
        {
            TestQuery(db.Customers.Where(c => (c.City ?? "Seattle") == "Seattle"));
        }

        [TestMethod()]
        public void TestCoalesce2()
        {
            TestQuery(db.Customers.Where(c => (c.City ?? c.Country ?? "Seattle") == "Seattle"));
        }

        [TestMethod()]
        public void TestConditional()
        {
            TestQuery(db.Orders.Where(o => (o.CustomerID == "ALFKI" ? 1000 : 0) == 1000));
        }

        [TestMethod()]
        public void TestConditional2()
        {
            TestQuery(db.Orders.Where(o => (o.CustomerID == "ALFKI" ? 1000 : o.CustomerID == "ABCDE" ? 2000 : 0) == 1000));
        }

        [TestMethod()]
        public void TestConditionalTestIsValue()
        {
            TestQuery(db.Orders.Where(o => (((bool)(object)o.OrderID) ? 100 : 200) == 100));
        }

        [TestMethod()]
        public void TestConditionalResultsArePredicates()
        {
            TestQuery(db.Orders.Where(o => (o.CustomerID == "ALFKI" ? o.OrderID < 10 : o.OrderID > 10)));
        }

        [TestMethod()]
        public void TestSelectManyJoined()
        {
            TestQuery(
                from c in db.Customers
                from o in db.Orders.Where(o => o.CustomerID == c.CustomerID)
                select new { Contactname = c.ContactName, Orderdate = o.OrderDate }
                );
        }

        [TestMethod()]
        public void TestSelectManyJoinedDefaultIfEmpty()
        {
            TestQuery(
                from c in db.Customers
                from o in db.Orders.Where(o => o.CustomerID == c.CustomerID).DefaultIfEmpty()
                select new { Contactname = c.ContactName, Orderdate = o.OrderDate }
                );
        }

        [TestMethod()]
        public void TestSelectWhereAssociation()
        {
            TestQuery(
                from o in db.Orders
                where o.Customers.City == "Seattle"
                select o
                );
        }

        [TestMethod()]
        public void TestSelectWhereAssociations()
        {
            TestQuery(
                from o in db.Orders
                where o.Customers.City == "Seattle" && o.Customers.Phone != "555 555 5555"
                select o
                );
        }

        [TestMethod()]
        public void TestSelectWhereAssociationTwice()
        {
            TestQuery(
                from o in db.Orders
                where o.Customers.City == "Seattle" && o.Customers.Phone != "555 555 5555"
                select o
                );
        }

        [TestMethod()]
        public void TestSelectAssociation()
        {
            TestQuery(
                from o in db.Orders
                select o.Customers
                );
        }

        [TestMethod()]
        public void TestSelectAssociation1()
        {
            TestQuery(
                from o in db.Orders
                select o.Customers.ContactName
                );
        }

        [TestMethod()]
        public void TestSelectAssociation2()
        {
            TestQuery(
                from o in db.Orders
                orderby o.Customers.City
                select new { Companyname = o.Customers.CompanyName, Contactname = o.Customers.ContactName }
                );
        }

        [TestMethod()]
        public void TestSelectAssociations()
        {
            TestQuery(
                from o in db.Orders
                select new { A = o.Customers, B = o.Customers }
                );
        }

        [TestMethod()]
        public void TestSelectAssociationsWhereAssociations()
        {
            TestQuery(
                from o in db.Orders
                where o.Customers.City == "Seattle"
                where o.Customers.Phone != "555 555 5555"
                select new { A = o.Customers, B = o.Customers }
                );
        }

        [TestMethod()]
        public void TestCompareDateTimesWithDifferentNullability()
        {
            TestQuery(
                from o in db.Orders
                where o.OrderDate < DateTime.Today && ((DateTime?)o.OrderDate) < DateTime.Today
                select o
                );
        }

        [TestMethod]
        public void TestCompareEntityEqual()
        {
            var alfki = new Customers { CustomerID = "ALFKI" };
            TestQuery(
                db.Customers.Where(c => c == alfki)
                );
        }

        [TestMethod]
        public void TestCompareEntityNotEqual()
        {
            var alfki = new Customers { CustomerID = "ALFKI" };
            TestQuery(
                db.Customers.Where(c => c != alfki)
                );
        }

        [TestMethod]
        public void TestCompareConstructedMultiValueEqual()
        {
            TestQuery(
                db.Customers.Where(c => new { x = c.City, y = c.Country } == new { x = "London", y = "UK" })
                );
        }

        [TestMethod]
        public void TestCompareConstructedMultiValueNotEqual()
        {
            TestQuery(
                db.Customers.Where(c => new { x = c.City, y = c.Country } != new { x = "London", y = "UK" })
                );
        }

        [TestMethod]
        public void TestCompareConstructed()
        {
            TestQuery(
                db.Customers.Where(c => new { x = c.City } == new { x = "London" })
                );
        }

        [TestMethod()]
        public void TestContainsWithEmptyLocalList()
        {
            var ids = new string[0];
            TestQuery(
                from c in db.Customers
                where ids.Contains(c.CustomerID)
                select c
                );
        }

        [TestMethod()]
        public void TestContainsWithSubQuery()
        {
            var n = "London";
            var custsInLondon = db.Customers.Where(c => c.City == n).Select(c => c.CustomerID);

            TestQuery(
                from c in db.Customers
                where custsInLondon.Contains(c.CustomerID)
                select c
                );
        }

        [TestMethod()]
        public void TestCombineQueriesDeepNesting()
        {
            var custs = db.Customers.Where(c => c.ContactName.StartsWith("xxx"));
            var ords = db.Orders.Where(o => custs.Any(c => c.CustomerID == o.CustomerID));
            TestQuery(
                db.OrderDetails.Where(d => ords.Any(o => o.OrderID == d.OrderID))
                );
        }

        [TestMethod()]
        public void TestOuterApply()
        {
            var customers = db.Customers.Where(s => true);
            TestQuery(
                db.Orders.Select(s => new { Customerid = s.CustomerID, Allow = db.Customers.FirstOrDefault(t => t.Address == s.CustomerID) != null })
                );
        }

        [TestMethod()]
        public void TestLetWithSubquery()
        {
            TestQuery(
                from customers in db.Customers
                let orderss =
                    from order in db.Orders
                    where order.CustomerID == customers.CustomerID
                    select order
                select new
                {
                    customers = customers,
                    orderssCount = orderss.Count(),
                }
                );
        }

        [TestMethod()]
        public void TestSelectDelFlag1()
        {
            TestQuery(from o in db.Products select o);
        }

        [TestMethod()]
        public void TestSelectDelFlag2()
        {
            TestQuery(from o in db.Products where o.Id > 20 select o);
        }

        [TestMethod()]
        public void TestSelectDelFlag3()
        {
            TestQuery(from o in db.OrderDetails where o.Products.Id > 20 select o);
        }

        [TestMethod()]
        public virtual void TestBigQueryWithOrderingGroupingAndNestedGroupCounts()
        {
            TestQuery(db.Customers
                          .OrderBy(c => c.City)
                          .Take(10)
                          .GroupBy(c => c.City)
                          .OrderBy(g => g.Key)
                          .Select(g => new { Key = g.Key, ItemCount = g.Count(), HasSubGroups = false, Items = g }));
        }
    }
}
