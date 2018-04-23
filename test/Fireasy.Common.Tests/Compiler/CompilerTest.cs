using Fireasy.Common.Compiler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Fireasy.Common.Tests.Compiler
{
    [TestClass]
    public class CompilerTest
    {
        [TestMethod]
        public void TestClass()
        {
            var code = "public class A {  public int test() { return 1; }  }";

            var type = new CodeCompiler().CompileType(code);
            Assert.AreEqual("A", type.Name);
        }

        [TestMethod]
        public void TestDelegate()
        {
            var code = "public class A {  public int test() { return 1; }  }";

            var func = new CodeCompiler().CompileDelegate<Func<int>>(code);
            Assert.AreEqual(1, func());
        }

        [TestMethod]
        public void TestDelegateIn()
        {
            var code = "public class A {  public int test(int r) { return r * 100; }  }";

            var func = new CodeCompiler().CompileDelegate<Func<int, int>>(code);
            Assert.AreEqual(10000, func(100));
        }
    }
}
