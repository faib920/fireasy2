using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fireasy.Common;
using System.Collections.Generic;
using Fireasy.Common.Extensions;
using System.Linq.Expressions;

namespace Fireasy.Common.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var types = typeof(LambdaExpression).Assembly.GetTypes();
            var type = Type.GetType("System.Linq.Expressions.LambdaExpression, System.Linq.Expressions");
            foreach (var t in type.GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance))
            {
                Console.WriteLine(t);
            }
            var assert = new AssertFlag();
            Assert.IsTrue(assert.AssertTrue());
        }
        public class Data1
        {
            public string Name { get; set; }
        }

        public class Data2
        {
            public string Name { get; set; }

            public int Age { get; set; }

            public string Sex { get; set; }
        }

[TestMethod]
public void Test()
{
    Assert.AreEqual("bodies", "body".ToPlural());
    Assert.AreEqual("people", "people".ToPlural());
    Assert.AreEqual("girls", "girl".ToPlural());
    Console.WriteLine("----");
    Assert.AreEqual("body", "bodies".ToSingular());
    Assert.AreEqual("people", "people".ToSingular());
    Assert.AreEqual("girl", "girls".ToSingular());
}



    }
}
