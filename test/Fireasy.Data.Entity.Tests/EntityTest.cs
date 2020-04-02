using Fireasy.Common;
using Fireasy.Common.Extensions;
using Fireasy.Common.Serialization;
using Fireasy.Data.Entity.Dynamic;
using Fireasy.Data.Entity.Linq;
using Fireasy.Data.Entity.Linq.Expressions;
using Fireasy.Data.Entity.Linq.Translators;
using Fireasy.Data.Entity.Metadata;
using Fireasy.Data.Entity.Properties;
using Fireasy.Data.Entity.Tests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Fireasy.Data.Entity.Tests
{
    [TestClass]
    public class EntityTest
    {
        [TestMethod]
        public void Test()
        {
            var dic1 = new Dictionary<string, string>();
            var dic2 = new Dictionary<Type, string>();

            foreach (var assn in typeof(EntityTest).Assembly.GetReferencedAssemblies().Distinct())
            {
                var ass = Assembly.Load(assn);
                foreach (var type in ass.GetTypes())
                {
                    dic1.Add(type.AssemblyQualifiedName, type.FullName);
                    dic2.Add(type, type.FullName);
                }
            }

            var k = dic2.Last().Key;

            var t = TimeWatcher.Watch(() =>
            {
                dic1[k.AssemblyQualifiedName] = "dd";
            });
            Console.WriteLine(t);
            t = TimeWatcher.Watch(() =>
            {
                dic2[k] = "dd";
            });
            Console.WriteLine(t);
        }

        [TestMethod]
        public void TestModified()
        {
            var customer = new Customers
            {
                ContactName = "fireasy"
            };
            Assert.IsFalse(customer.IsModified(s => s.ContactName));
            Assert.IsFalse(customer.IsModified(s => s.CustomerID));
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

            Assert.IsTrue(customer.IsModified(s => s.ContactName));
            Assert.IsTrue(customer.IsModified(s => s.CustomerID));
            Assert.AreEqual(customer.ContactName, "fireasy");
        }

        [TestMethod]
        public void TestModified_New()
        {
            using (var db = new DbContext())
            {
                var product = db.New<Products>();
                product.ProductName = "fireasy";

                Assert.IsTrue(product.IsModified(s => s.ProductName));
                Assert.IsFalse(product.IsModified(s => s.ProductID));

                product.Modified(s => s.ProductName, false);

                Assert.IsFalse(product.IsModified(s => s.ProductName));

                Assert.AreEqual(product.ProductName, "fireasy");
            }
        }

        [TestMethod]
        public void TestModified_Wrap()
        {
            using (var db = new DbContext())
            {
                var customer = db.Wrap(() => new Customers
                {
                    ContactName = "fireasy"
                });
                Assert.IsTrue(customer.IsModified("ContactName"));
                Assert.IsFalse(customer.IsModified("CustomerID"));
                Assert.AreEqual(customer.ContactName, "fireasy");
            }
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

        [EntityMapping("dd")]
        public class Customers1 : Customers
        {
            /// <summary>
            /// 获取或设置。
            /// </summary>

            [PropertyMapping(ColumnName = "PostCode", Description = "", Length = 6, IsNullable = false)]
            public virtual string PostCode { get; set; }
        }

        [TestMethod]
        public void TestGetValue()
        {
            using (var db = new DbContext())
            {
                var customer = new Customers { PostalCode = "fireasy" };
                var value = customer.GetValue(PropertyUnity.GetProperty((customer as IEntity).EntityType, "PostalCode"));
                Assert.AreEqual(value, "fireasy");

                EntityMetadataUnity.GetEntityMetadata(typeof(Customers));
                EntityMetadataUnity.GetEntityMetadata(typeof(Customers1));

                Console.WriteLine(PropertyUnity.GetProperties(typeof(Customers)).ToList().Count());
                Console.WriteLine(PropertyUnity.GetProperties(typeof(Customers1)).ToList().Count());

                var order = db.Wrap(() => new Orders { CustomerID = "11" });
                value = order.GetValue(PropertyUnity.GetProperty(typeof(Orders), "CustomerID"));
                Assert.AreEqual(value, "11");
            }
        }

        [TestMethod]
        public void TestInitValue()
        {
            using (var db = new DbContext())
            {
                var customer = new Customers();
                customer.InitializeValue(PropertyUnity.GetProperty(typeof(Customers), "ContactName"), "ddd");
                Assert.AreEqual(customer.ContactName, "ddd");
                Assert.IsFalse(customer.IsModified("ContactName"));

                var order = db.New<Orders>();
                order.InitializeValue(PropertyUnity.GetProperty(typeof(Orders), "CustomerID"), "eee");
                Assert.AreEqual(order.CustomerID, "eee");
                Assert.IsFalse(order.IsModified("CustomerID"));
            }
        }

        [TestMethod]
        public void TestGetProperty()
        {
            using (var db = new DbContext())
            {
                var customer = db.New<Customers>();
                var property1 = PropertyUnity.GetProperty(customer.GetType(), "CustomerID");
                var property2 = PropertyUnity.GetProperty(typeof(Customers), "CustomerID");
                var property3 = PropertyUnity.GetProperty(typeof(Customers), "CustomerID1");
                Assert.IsNotNull(property1);
                Assert.IsNotNull(property2);
                Assert.IsNull(property3);
                Assert.AreEqual(property1, property2);
            }
        }

        [TestMethod]
        public void TestNormalize()
        {
            using (var db = new DbContext())
            {
                var customer = db.Wrap(() => new Customers
                {
                    ContactName = "fireasy"
                });

                customer = customer.Normalize("t1");
                Assert.AreEqual("t1", customer.CustomerID);
            }
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

        [TestMethod]
        public void TestDynamic()
        {
            var builder = new EntityTypeBuilder("test");
            builder.Properties.Add(new GeneralProperty() { Name = "aa", Type = typeof(string), Info = new PropertyMapInfo { FieldName = "aa", IsPrimaryKey = true, DefaultValue = 33, GenerateType = IdentityGenerateType.AutoIncrement } });
            builder.Properties.Add(new GeneralProperty() { Name = "bb", Type = typeof(string), Info = new PropertyMapInfo { FieldName = "bb", IsNullable = true } });
            builder.Properties.Add(new GeneralProperty() { Name = "cc", Type = typeof(int), Info = new PropertyMapInfo { FieldName = "cc", Scale = 2 } });
            builder.Properties.Add(new EntityProperty() { Name = "dd", Type = typeof(Products) });
            var a = builder.Create();

            var pp = PropertyUnity.GetPrimaryProperties(a).FirstOrDefault(s => s.Info.GenerateType == IdentityGenerateType.AutoIncrement);
        }

        [TestMethod]
        public void TestMSetValue()
        {
            var t1 = TimeWatcher.Watch(() =>
            {
                var order = new Orders();

                for (var i = 0; i < 100000; i++)
                {
                    order.ShipCity = "aaa" + i;
                }
            });

            var t2 = TimeWatcher.Watch(() =>
            {
                var p = PropertyUnity.GetProperty(typeof(Orders), "ShipCity");
                var order = new Orders();

                for (var i = 0; i < 100000; i++)
                {
                    order.InitializeValue(p, "aaa" + i);
                }
            });

            var t3 = TimeWatcher.Watch(() =>
            {
                var p = typeof(Orders).GetProperty("ShipCity");
                var order = new Orders();

                for (var i = 0; i < 100000; i++)
                {
                    p.SetValue(order, "aaa" + i);
                }
            });

            Console.WriteLine("直接:" + t1);
            Console.WriteLine("赋值:" + t2);
            Console.WriteLine("反射:" + t3);
        }

        [TestMethod]
        public void TestGetProperties()
        {
            var pp = PropertyUnity.GetProperties(typeof(PA2)).ToArray();
            Assert.AreEqual(2, pp.Length);
        }

        [TestMethod]
        public void TestGetProperties1()
        {
            var pp = PropertyUnity.GetProperties(typeof(PA1)).ToArray();
            Assert.AreEqual(1, pp.Length);
        }

        [TestMethod]
        public void TestGetProperty1()
        {
            var pp = PropertyUnity.GetProperty(typeof(PA1), "Name");
            Assert.IsNull(pp);
            pp = PropertyUnity.GetProperty(typeof(PA2), "Name");
            Assert.IsNotNull(pp);
        }

        [TestMethod]
        public void TestGetProperty2()
        {
            var pp = PropertyUnity.GetProperty(typeof(PA1), "Name");
            Assert.IsNull(pp);

            var p = (PropertyValue)1;
            PropertyValue e = (TestAA)(int)p;
            Console.WriteLine(e);

            var a = (PropertyValue)TestAA.BB;
            var b = (float)a;
            Console.WriteLine(b);
        }
    }

    public enum TestAA
    {
        AA,
        BB
    }

    [EntityMapping("pa1")]
    public class PA1 : LightEntity<PA1>
    {
        public virtual int Id { get; set; }
    }

    [EntityMapping("pa2")]
    public class PA2 : PA1
    {
        public virtual string Name { get; set; }
    }
}
