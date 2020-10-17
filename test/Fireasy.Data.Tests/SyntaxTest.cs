using Fireasy.Data.Provider;
using Fireasy.Data.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Fireasy.Data.Tests
{
    [TestClass]
    public class SyntaxTest
    {
        private const string instanceName = "access";

        public SyntaxTest()
        {
            InitConfig.Init();
        }

        [TestMethod]
        public void TestStringSubstring()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var syntax = db.Provider.GetService<ISyntaxProvider>();
                var sql = $"select *, {syntax.String.Substring("productname", 2, 1)} as c from {syntax.DelimitTable("products")}";

                foreach (var item in db.ExecuteEnumerable((SqlCommand)sql))
                {
                    Console.WriteLine(item.c);
                }
            }
        }

        [TestMethod]
        public void TestStringSubstring1()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var syntax = db.Provider.GetService<ISyntaxProvider>();
                var sql = $"select *, {syntax.String.Substring("productname", 2)} as c from {syntax.DelimitTable("products")}";

                foreach (var item in db.ExecuteEnumerable((SqlCommand)sql))
                {
                    Console.WriteLine(item.c);
                }
            }
        }

        [TestMethod]
        public void TestStringIndexOf()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var syntax = db.Provider.GetService<ISyntaxProvider>();
                var sql = $"select *, {syntax.String.IndexOf("productname", "'C'")} as c from {syntax.DelimitTable("products")}";

                foreach (var item in db.ExecuteEnumerable((SqlCommand)sql))
                {
                    Console.WriteLine(item.c);
                }
            }
        }

        [TestMethod]
        public void TestStringToUpper()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var syntax = db.Provider.GetService<ISyntaxProvider>();
                var sql = $"select *, {syntax.String.ToUpper("productname")} as c from {syntax.DelimitTable("products")}";

                foreach (var item in db.ExecuteEnumerable((SqlCommand)sql))
                {
                    Console.WriteLine(item.c);
                }
            }
        }

        [TestMethod]
        public void TestStringToLower()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var syntax = db.Provider.GetService<ISyntaxProvider>();
                var sql = $"select *, {syntax.String.ToLower("productname")} as c from {syntax.DelimitTable("products")}";

                foreach (var item in db.ExecuteEnumerable((SqlCommand)sql))
                {
                    Console.WriteLine(item.c);
                }
            }
        }

        [TestMethod]
        public void TestStringLength()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var syntax = db.Provider.GetService<ISyntaxProvider>();
                var sql = $"select *, {syntax.String.Length("productname")} as c from {syntax.DelimitTable("products")}";

                foreach (var item in db.ExecuteEnumerable((SqlCommand)sql))
                {
                    Console.WriteLine(item.c);
                }
            }
        }

        [TestMethod]
        public void TestStringConcat()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var syntax = db.Provider.GetService<ISyntaxProvider>();
                var sql = $"select *, {syntax.String.Concat("productname", "'-'", "productid")} as c from {syntax.DelimitTable("products")}";

                foreach (var item in db.ExecuteEnumerable((SqlCommand)sql))
                {
                    Console.WriteLine(item.c);
                }
            }
        }

        [TestMethod]
        public void TestStringTrim()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var syntax = db.Provider.GetService<ISyntaxProvider>();
                var sql = $"select *, {syntax.String.Trim("productname")} as c from {syntax.DelimitTable("products")}";

                foreach (var item in db.ExecuteEnumerable((SqlCommand)sql))
                {
                    Console.WriteLine(item.c);
                }
            }
        }

        [TestMethod]
        public void TestStringReplace()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var syntax = db.Provider.GetService<ISyntaxProvider>();
                var sql = $"select *, {syntax.String.Replace("productname", "'C'", "'_'")} as c from {syntax.DelimitTable("products")}";

                foreach (var item in db.ExecuteEnumerable((SqlCommand)sql))
                {
                    Console.WriteLine(item.c);
                }
            }
        }

        [TestMethod]
        public void TestStringReverse()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var syntax = db.Provider.GetService<ISyntaxProvider>();
                var sql = $"select *, {syntax.String.Reverse("productname")} as c from {syntax.DelimitTable("products")}";

                foreach (var item in db.ExecuteEnumerable((SqlCommand)sql))
                {
                    Console.WriteLine(item.c);
                }
            }
        }

        [TestMethod]
        public void TestDateTimeYear()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var syntax = db.Provider.GetService<ISyntaxProvider>();
                var sql = $"select *, {syntax.DateTime.Year("orderdate")} as c from {syntax.DelimitTable("orders")}";

                foreach (var item in db.ExecuteEnumerable((SqlCommand)sql))
                {
                    Console.WriteLine(item.c);
                }
            }
        }

        [TestMethod]
        public void TestDateTimeMonth()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var syntax = db.Provider.GetService<ISyntaxProvider>();
                var sql = $"select *, {syntax.DateTime.Month("orderdate")} as c from {syntax.DelimitTable("orders")}";

                foreach (var item in db.ExecuteEnumerable((SqlCommand)sql))
                {
                    Console.WriteLine(item.c);
                }
            }
        }

        [TestMethod]
        public void TestDateTimeDay()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var syntax = db.Provider.GetService<ISyntaxProvider>();
                var sql = $"select *, {syntax.DateTime.Day("orderdate")} as c from {syntax.DelimitTable("orders")}";

                foreach (var item in db.ExecuteEnumerable((SqlCommand)sql))
                {
                    Console.WriteLine(item.c);
                }
            }
        }

        [TestMethod]
        public void TestDateTimeHour()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var syntax = db.Provider.GetService<ISyntaxProvider>();
                var sql = $"select *, {syntax.DateTime.Hour("orderdate")} as c from {syntax.DelimitTable("orders")}";

                foreach (var item in db.ExecuteEnumerable((SqlCommand)sql))
                {
                    Console.WriteLine(item.c);
                }
            }
        }

        [TestMethod]
        public void TestDateTimeMinute()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var syntax = db.Provider.GetService<ISyntaxProvider>();
                var sql = $"select *, {syntax.DateTime.Minute("orderdate")} as c from {syntax.DelimitTable("orders")}";

                foreach (var item in db.ExecuteEnumerable((SqlCommand)sql))
                {
                    Console.WriteLine(item.c);
                }
            }
        }

        [TestMethod]
        public void TestDateTimeSecond()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var syntax = db.Provider.GetService<ISyntaxProvider>();
                var sql = $"select *, {syntax.DateTime.Second("orderdate")} as c from {syntax.DelimitTable("orders")}";

                foreach (var item in db.ExecuteEnumerable((SqlCommand)sql))
                {
                    Console.WriteLine(item.c);
                }
            }
        }

        [TestMethod]
        public void TestDateTimeMillisecond()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var syntax = db.Provider.GetService<ISyntaxProvider>();
                var sql = $"select *, {syntax.DateTime.Millisecond("orderdate")} as c from {syntax.DelimitTable("orders")}";

                foreach (var item in db.ExecuteEnumerable((SqlCommand)sql))
                {
                    Console.WriteLine(item.c);
                }
            }
        }

        [TestMethod]
        public void TestDateTimeDayOfYear()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var syntax = db.Provider.GetService<ISyntaxProvider>();
                var sql = $"select *, {syntax.DateTime.DayOfYear("orderdate")} as c from {syntax.DelimitTable("orders")}";

                foreach (var item in db.ExecuteEnumerable((SqlCommand)sql))
                {
                    Console.WriteLine(item.c);
                }
            }
        }

        [TestMethod]
        public void TestDateTimeDayOfWeek()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var syntax = db.Provider.GetService<ISyntaxProvider>();
                var sql = $"select *, {syntax.DateTime.DayOfWeek("orderdate")} as c from {syntax.DelimitTable("orders")}";

                foreach (var item in db.ExecuteEnumerable((SqlCommand)sql))
                {
                    Console.WriteLine(item.c);
                }
            }
        }

        [TestMethod]
        public void TestDateTimeWeekOfYear()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var syntax = db.Provider.GetService<ISyntaxProvider>();
                var sql = $"select *, {syntax.DateTime.WeekOfYear("orderdate")} as c from {syntax.DelimitTable("orders")}";

                foreach (var item in db.ExecuteEnumerable((SqlCommand)sql))
                {
                    Console.WriteLine(item.c);
                }
            }
        }

        [TestMethod]
        public void TestDateTimeSystemTime()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var syntax = db.Provider.GetService<ISyntaxProvider>();
                var sql = $"select *, {syntax.DateTime.Now()} as c from {syntax.DelimitTable("orders")}";

                foreach (var item in db.ExecuteEnumerable((SqlCommand)sql))
                {
                    Console.WriteLine(item.c);
                }
            }
        }

        [TestMethod]
        public void TestDateTimeNew()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var syntax = db.Provider.GetService<ISyntaxProvider>();
                var sql = $"select *, {syntax.DateTime.New(2019, 1, 1)} as c from {syntax.DelimitTable("orders")}";

                foreach (var item in db.ExecuteEnumerable((SqlCommand)sql))
                {
                    Console.WriteLine(item.c);
                }
            }
        }

        [TestMethod]
        public void TestDateTimeDiffDays()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var syntax = db.Provider.GetService<ISyntaxProvider>();
                var sql = $"select *, {syntax.DateTime.DiffDays("orderdate", syntax.DateTime.Now())} as c from {syntax.DelimitTable("orders")}";

                foreach (var item in db.ExecuteEnumerable((SqlCommand)sql))
                {
                    Console.WriteLine(item.c);
                }
            }
        }

        [TestMethod]
        public void TestDateTimeDiffHours()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var syntax = db.Provider.GetService<ISyntaxProvider>();
                var sql = $"select *, {syntax.DateTime.DiffHours("orderdate", syntax.DateTime.Now())} as c from {syntax.DelimitTable("orders")}";

                foreach (var item in db.ExecuteEnumerable((SqlCommand)sql))
                {
                    Console.WriteLine(item.c);
                }
            }
        }

        [TestMethod]
        public void TestDateTimeDiffMinutes()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var syntax = db.Provider.GetService<ISyntaxProvider>();
                var sql = $"select *, {syntax.DateTime.DiffMinutes("orderdate", syntax.DateTime.Now())} as c from {syntax.DelimitTable("orders")}";

                foreach (var item in db.ExecuteEnumerable((SqlCommand)sql))
                {
                    Console.WriteLine(item.c);
                }
            }
        }

        [TestMethod]
        public void TestDateTimeDiffSeconds()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var syntax = db.Provider.GetService<ISyntaxProvider>();
                var sql = $"select *, {syntax.DateTime.DiffSeconds("orderdate", syntax.DateTime.Now())} as c from {syntax.DelimitTable("orders")}";

                foreach (var item in db.ExecuteEnumerable((SqlCommand)sql))
                {
                    Console.WriteLine(item.c);
                }
            }
        }

        [TestMethod]
        public void TestDateTimeAddYears()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var syntax = db.Provider.GetService<ISyntaxProvider>();
                var sql = $"select *, {syntax.DateTime.AddYears("orderdate", 1)} as c from {syntax.DelimitTable("orders")}";

                foreach (var item in db.ExecuteEnumerable((SqlCommand)sql))
                {
                    Console.WriteLine(item.c);
                }
            }
        }

        [TestMethod]
        public void TestDateTimeAddMonths()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var syntax = db.Provider.GetService<ISyntaxProvider>();
                var sql = $"select *, {syntax.DateTime.AddMonths("orderdate", 1)} as c from {syntax.DelimitTable("orders")}";

                foreach (var item in db.ExecuteEnumerable((SqlCommand)sql))
                {
                    Console.WriteLine(item.c);
                }
            }
        }

        [TestMethod]
        public void TestDateTimeAddHours()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var syntax = db.Provider.GetService<ISyntaxProvider>();
                var sql = $"select *, {syntax.DateTime.AddHours("[orderdate]", 1)} as c from {syntax.DelimitTable("orders")}";

                foreach (var item in db.ExecuteEnumerable((SqlCommand)sql))
                {
                    Console.WriteLine(item.c);
                }
            }
        }

        [TestMethod]
        public void TestDateTimeAddMinutes()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var syntax = db.Provider.GetService<ISyntaxProvider>();
                var sql = $"select *, {syntax.DateTime.AddMinutes("orderdate", 1)} as c from {syntax.DelimitTable("orders")}";

                foreach (var item in db.ExecuteEnumerable((SqlCommand)sql))
                {
                    Console.WriteLine(item.c);
                }
            }
        }

        [TestMethod]
        public void TestDateTimeAddSeconds()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var syntax = db.Provider.GetService<ISyntaxProvider>();
                var sql = $"select *, {syntax.DateTime.AddSeconds("orderdate", 1)} as c from {syntax.DelimitTable("orders")}";

                foreach (var item in db.ExecuteEnumerable((SqlCommand)sql))
                {
                    Console.WriteLine(item.c);
                }
            }
        }

        [TestMethod]
        public void TestMathAbs()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var syntax = db.Provider.GetService<ISyntaxProvider>();
                var sql = $"select *, {syntax.Math.Abs("-200")} as c from {syntax.DelimitTable("orders")}";

                foreach (var item in db.ExecuteEnumerable((SqlCommand)sql))
                {
                    Console.WriteLine(item.c);
                }
            }
        }

        [TestMethod]
        public void TestMathFloor()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var syntax = db.Provider.GetService<ISyntaxProvider>();
                var sql = $"select *, {syntax.Math.Floor("0.67")} as c from {syntax.DelimitTable("orders")}";

                foreach (var item in db.ExecuteEnumerable((SqlCommand)sql))
                {
                    Console.WriteLine(item.c);
                }
            }
        }

        [TestMethod]
        public void TestMathTruncate()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var syntax = db.Provider.GetService<ISyntaxProvider>();
                var sql = $"select *, {syntax.Math.Truncate("0.67")} as c from {syntax.DelimitTable("orders")}";

                foreach (var item in db.ExecuteEnumerable((SqlCommand)sql))
                {
                    Console.WriteLine(item.c);
                }
            }
        }

        [TestMethod]
        public void TestMathSin()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var syntax = db.Provider.GetService<ISyntaxProvider>();
                var sql = $"select *, {syntax.Math.Sin("0.67")} as c from {syntax.DelimitTable("orders")}";

                foreach (var item in db.ExecuteEnumerable((SqlCommand)sql))
                {
                    Console.WriteLine(item.c);
                }
            }
        }

        [TestMethod]
        public void TestMathCos()
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var syntax = db.Provider.GetService<ISyntaxProvider>();
                var sql = $"select *, {syntax.Math.Cos("0.67")} as c from {syntax.DelimitTable("orders")}";

                foreach (var item in db.ExecuteEnumerable((SqlCommand)sql))
                {
                    Console.WriteLine(item.c);
                }
            }
        }
    }
}
