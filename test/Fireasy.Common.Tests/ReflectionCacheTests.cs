using Fireasy.Common.Extensions;
using Fireasy.Common.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fireasy.Common.Tests
{
    [TestClass]
    public class ReflectionCacheTests
    {
        public class TestClass
        {
            public TestClass()
            {

            }

            public TestClass(string bb)
            {

            }

            public TestClass(string bb, int age)
            {

            }

            public string Name { get; set; }

            public int? Age { get; set; }

            public string Call(string str)
            {
                return str;
            }

            public TestClass Clone()
            {
                return (TestClass)MemberwiseClone();
            }

            public static TestClass AA(object[] pars)
            {
                return new TestClass((string)pars[0], (int)pars[1]);
            }
        }

        [TestMethod]
        public void TestNew()
        {
            var t1 = TimeWatcher.Watch(() =>
            {
                for (var i = 0; i < 100000; i++)
                {
                    var tt = Activator.CreateInstance(typeof(TestClass));
                }
            });

            var t2 = TimeWatcher.Watch(() =>
            {
                for (var i = 0; i < 100000; i++)
                {
                    var tt = typeof(TestClass).New();
                }
            });

            var t3 = TimeWatcher.Watch(() =>
            {
                for (var i = 0; i < 100000; i++)
                {
                    var tt = new TestClass();
                }
            });

            Console.WriteLine("反射:" + t1);
            Console.WriteLine("缓存:" + t2);
            Console.WriteLine("直接:" + t3);
        }

        [TestMethod]
        public void TestNew1()
        {
            var t1 = TimeWatcher.Watch(() =>
            {
                for (var i = 0; i < 100000; i++)
                {
                    var tt = Activator.CreateInstance(typeof(TestClass), "aa");
                }
            });

            var t2 = TimeWatcher.Watch(() =>
            {
                for (var i = 0; i < 100000; i++)
                {
                    var tt = typeof(TestClass).New(null, 11);
                }
            });

            var t3 = TimeWatcher.Watch(() =>
            {
                for (var i = 0; i < 100000; i++)
                {
                    var tt = new TestClass("aa");
                }
            });

            Console.WriteLine("反射:" + t1);
            Console.WriteLine("缓存:" + t2);
            Console.WriteLine("直接:" + t3);
        }

        [TestMethod]
        public void TestSetValue()
        {
            var n = typeof(TestClass).GetProperty("Name");
            var a = typeof(TestClass).GetProperty("Age");
            var ins = new TestClass("bb");

            var t1 = TimeWatcher.Watch(() =>
            {
                for (var i = 0; i < 100000; i++)
                {
                    //n.SetValue(ins, "aaaaaa");
                    a.SetValue(ins, i);
                }
            });

            var t2 = TimeWatcher.Watch(() =>
            {
                for (var i = 0; i < 100000; i++)
                {
                    //n.FastSetValue(ins, "aaaaaa");
                    a.FastSetValue(ins, i);
                }
            });

            var t3 = TimeWatcher.Watch(() =>
            {
                for (var i = 0; i < 100000; i++)
                {
                    ins.Name = "aaaaaa";
                    //ins.Age = i;
                }
            });

            Console.WriteLine("反射:" + t1);
            Console.WriteLine("缓存:" + t2);
            Console.WriteLine("直接:" + t3);
        }

        [TestMethod]
        public void TestInvoke()
        {
            var n = typeof(TestClass).GetMethod("Call");
            var ins = new TestClass("bb");

            var t1 = TimeWatcher.Watch(() =>
            {
                for (var i = 0; i < 100000; i++)
                {
                    n.Invoke(ins, new[] { "aaaaaa" });
                }
            });

            var t2 = TimeWatcher.Watch(() =>
            {
                for (var i = 0; i < 100000; i++)
                {
                    n.FastInvoke(ins, "aaaaaa");
                }
            });

            var t3 = TimeWatcher.Watch(() =>
            {
                for (var i = 0; i < 100000; i++)
                {
                    ins.Call("aaaaaa");
                }
            });

            Console.WriteLine("反射:" + t1);
            Console.WriteLine("缓存:" + t2);
            Console.WriteLine("直接:" + t3);
        }

        [TestMethod]
        public void TestMakeGenericType()
        {
            var t1 = TimeWatcher.Watch(() =>
            {
                for (var i = 0; i < 100000; i++)
                {
                    var t = typeof(IEnumerable<>).MakeGenericType(typeof(Func<>).MakeGenericType(typeof(int)));
                }
            });

            var t2 = TimeWatcher.Watch(() =>
            {
                for (var i = 0; i < 100000; i++)
                {
                    var t = ReflectionCache.GetMember("a", typeof(int),
                        s => typeof(IEnumerable<>).MakeGenericType(typeof(Func<>).MakeGenericType(s)));
                }
            });

            Console.WriteLine("反射:" + t1);
            Console.WriteLine("缓存:" + t2);
        }

        [TestMethod]
        public void TestGetEnumerableElementType()
        {
            var t1 = TimeWatcher.Watch(() =>
            {
                for (var i = 0; i < 100000; i++)
                {
                    var t = typeof(IEnumerable<int>).GetEnumerableElementType();
                }
            });

            var t2 = TimeWatcher.Watch(() =>
            {
                for (var i = 0; i < 100000; i++)
                {
                    var t = ReflectionCache.GetMember("a", typeof(IEnumerable<int>),
                        s => s.GetEnumerableElementType());
                }
            });

            Console.WriteLine("反射:" + t1);
            Console.WriteLine("缓存:" + t2);
        }

        [TestMethod]
        public void TestMakeGenericMethod()
        {
            var t1 = TimeWatcher.Watch(() =>
            {
                for (var i = 0; i < 100000; i++)
                {
                    var t = this.GetType().GetMethod("Test").MakeGenericMethod(typeof(string));
                }
            });

            var t2 = TimeWatcher.Watch(() =>
            {
                for (var i = 0; i < 100000; i++)
                {
                    var t = ReflectionCache.GetMember("b", typeof(string),
                        s => this.GetType().GetMethod("Test").MakeGenericMethod(s));
                }
            });

            Console.WriteLine("反射:" + t1);
            Console.WriteLine("缓存:" + t2);
        }

    }
}
