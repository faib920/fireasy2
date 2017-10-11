using Fireasy.Common;
using Fireasy.Common.Extensions;
using Fireasy.Common.Serialization;
using Fireasy.Data.Entity.Linq;
using Fireasy.Data.Entity.Linq.Expressions;
using Fireasy.Data.Entity.Linq.Translators;
using Fireasy.Data.Entity.Tests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Tests
{
    [TestClass]
    public class EntityTest
    {

        [TestMethod]
        public void TestModified()
        {
            var customer = new Customers
            {
                ContactName = "fireasy"
            };
            Assert.IsFalse(customer.IsModified("ContactName"));
            Assert.IsFalse(customer.IsModified("CustomerID"));
            Assert.AreEqual(customer.ContactName, "fireasy");
        }

        [TestMethod]
        public void TestPropertyValue()
        {
            PropertyValue v1 = 22;
            PropertyValue v2 = PropertyValue.Empty;
            Assert.IsTrue(v1 == 22);
        }

        [TestMethod]
        public void TestModified_Mark()
        {
            var customer = new Customers
            {
                CustomerID = null,
                ContactName = "fireasy"
            };

            customer.Modified(s => s.CustomerID).Modified(s => s.ContactName);

            Assert.IsTrue(customer.IsModified("ContactName"));
            Assert.IsTrue(customer.IsModified("CustomerID"));
            Assert.AreEqual(customer.ContactName, "fireasy");
        }

        [TestMethod]
        public void TestModified_New()
        {
            var product = Products.New();
            product.ProductName = "fireasy";

            Assert.IsTrue(product.IsModified("ProductName"));
            Assert.IsFalse(product.IsModified("ProductID"));
            Assert.AreEqual(product.ProductName, "fireasy");
        }

        [TestMethod]
        public void TestModified_Wrap()
        {
            var customer = Customers.Wrap(() => new Customers
            {
                ContactName = "fireasy"
            });
            Assert.IsTrue(customer.IsModified("ContactName"));
            Assert.IsFalse(customer.IsModified("CustomerID"));
            Assert.AreEqual(customer.ContactName, "fireasy");
        }

        [TestMethod]
        public void TestModified_Context()
        {
            using (var db = new DbContext())
            {
                var order = db.Orders.FirstOrDefault();
                order.ShipName = "fireasy";
                Assert.IsTrue(order.IsModified("ShipName"));
                Assert.IsFalse(order.IsModified("OrderID"));
                Assert.AreEqual(order.ShipName, "fireasy");
            }
        }

        [TestMethod]
        public void TestGetValue()
        {
            var customer = new Customers { ContactName = "fireasy" };
            var value = customer.GetValue(PropertyUnity.GetProperty(typeof(Customers), "ContactName"));
            Assert.AreEqual(value, "fireasy");

            var order = Orders.Wrap(() => new Orders { CustomerID = "11" });
            value = order.GetValue(PropertyUnity.GetProperty(typeof(Orders), "CustomerID"));
            Assert.AreEqual(value, "11");
        }

        [TestMethod]
        public void TestGetProperty()
        {
            var customer = Customers.New();
            var property1 = PropertyUnity.GetProperty(customer.GetType(), "CustomerID");
            var property2 = PropertyUnity.GetProperty(typeof(Customers), "CustomerID");
            Assert.IsNotNull(property1);
            Assert.IsNotNull(property2);
            Assert.AreEqual(property1, property2);
        }

        [TestMethod]
        public void TestNormalize()
        {
            var customer = Customers.Wrap(() => new Customers
            {
                ContactName = "fireasy"
            });

            customer = customer.Normalize("t1");
            Assert.AreEqual("t1", customer.CustomerID);
        }

        [TestMethod]
        public void TestEnumValue()
        {
            using (var db = new DbContext())
            {
                var product = db.Products.FirstOrDefault();
                Assert.AreEqual(product.ReorderLevel, RecorderLevel.B);
            }
        }
    }
}
