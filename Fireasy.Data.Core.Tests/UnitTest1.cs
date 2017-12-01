using Fireasy.Data.Provider;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Fireasy.Data.Core.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            using (var db = DatabaseFactory.CreateDatabase())
            {
                var list = db.ExecuteEnumerable((SqlCommand)"select ProductId, ProductName from products").ToList();
                foreach (dynamic item in list)
                {
                    Console.WriteLine(item.ProductName);
                }
            }
        }
    }
}
