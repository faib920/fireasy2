using Fireasy.Common.Aop;
using Fireasy.Common.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fireasy.Common.Tests
{
    [TestClass]
    public class AopTest
    {
        /// <summary>
        /// 简单的测试。
        /// </summary>
        [TestMethod()]
        public void TestSampleCall()
        {
            var proxy = AspectFactory.BuildProxy<AspectTester>();

            proxy.SampleCall();
        }

        /// <summary>
        /// 简单的测试。
        /// </summary>
        [TestMethod()]
        public void TestSampleCallByParameter()
        {
            var proxy = AspectFactory.BuildProxy<AspectTester>("fireasy");

            proxy.SampleCall();
        }

        /// <summary>
        /// 测试属性值的设置和获取。
        /// </summary>
        [TestMethod()]
        public void TestPropertyValue()
        {
            var proxy = AspectFactory.BuildProxy<AspectTester>();

            proxy.Name = "fireasy";
            var name = proxy.Name;
        }

        /// <summary>
        /// 测试带参数的方法。
        /// </summary>
        [TestMethod()]
        public void TestWithParameters()
        {
            var proxy = AspectFactory.BuildProxy<AspectTester>();

            proxy.WithParameters("fireasy", 22, null);
        }

        /// <summary>
        /// 测试输出参数的方法。
        /// </summary>
        [TestMethod()]
        public void TestPrintParameters()
        {
            var proxy = AspectFactory.BuildProxy<AspectTester>();

            proxy.PrintParameters("fireasy", 22, null);
        }

        /// <summary>
        /// 测试带输出参数的方法。
        /// </summary>
        [TestMethod()]
        public void TestWithOutParameters()
        {
            var proxy = AspectFactory.BuildProxy<AspectTester>();

            var age = 19;
            DateTime birthday;
            object any = null;
            proxy.WithOutParameters("fireasy", ref age, out birthday, ref any);

            Assert.AreEqual(age, 21);
            Assert.AreEqual(birthday, DateTime.Parse("2009-2-2"));
            Assert.IsNotNull(any);
        }

        /// <summary>
        /// 测试带混合型输出参数的方法。
        /// </summary>
        [TestMethod()]
        public void TestWithOutParametersComplex()
        {
            var proxy = AspectFactory.BuildProxy<AspectTester>();

            int age = 19, age1;
            string sex = null, sex1;
            DateTime date = DateTime.Now, date1;
            decimal? money = 0, money1;
            object any = null, any1;
            proxy.WithOutParametersComplex("fireasy", ref age, out age1, ref sex, out sex1, ref date, out date1, ref money, out money1, ref any, out any1);

            Assert.AreEqual(age1, 21);
            Assert.AreEqual(sex1, "男");
            Assert.AreEqual(date1, DateTime.Parse("2009-1-1"));
            Assert.AreEqual(money1, 334.4m);
            Assert.IsNotNull(any1);
        }

        /// <summary>
        /// 测试泛型方法。
        /// </summary>
        [TestMethod()]
        public void TestGenericCall()
        {
            var proxy = AspectFactory.BuildProxy<AspectTester>();

            proxy.GenericCall("fireasy", 22);
        }

        /// <summary>
        /// 测试抛出异常的方法。
        /// </summary>
        [TestMethod()]
        public void TestThrowException()
        {
            var proxy = AspectFactory.BuildProxy<AspectTester>();

            proxy.ThrowException();
        }

        /// <summary>
        /// 测试忽略异常的方法。
        /// </summary>
        [TestMethod()]
        public void TestIgnoreException()
        {
            var proxy = AspectFactory.BuildProxy<AspectTester>();

            //拦截器没有设置返回值，取返回值类型为非可空类型的值类型时，则会引发新的异常
            proxy.IgnoreException();
        }

        /// <summary>
        /// 测试忽略异常的方法。
        /// </summary>
        [TestMethod()]
        public void TestIgnoreAnotherException()
        {
            var proxy = AspectFactory.BuildProxy<AspectTester>();

            var tb = proxy.IgnoreAnotherException();

            Assert.IsNull(tb);
        }

        /// <summary>
        /// 测试忽略异常的方法，并且有返回值。
        /// </summary>
        [TestMethod()]
        public void TestIgnoreExceptionHasReturnValue()
        {
            var proxy = AspectFactory.BuildProxy<AspectTester>();

            var result = proxy.IgnoreExceptionHasReturnValue();

            Assert.AreEqual(99, result);
        }

        /// <summary>
        /// 测试自定义拦截特性。
        /// </summary>
        [TestMethod]
        public void TestWithCustomInterceptAttribute()
        {
            var proxy = AspectFactory.BuildProxy<AspectTester>();

            proxy.WithCustomInterceptAttribute();
        }

        /// <summary>
        /// 测试初始化中Target对象。
        /// </summary>
        [TestMethod]
        public void TestContextTarget()
        {
            var proxy = AspectFactory.BuildProxy<AspectTester>();

            proxy.ContextTarget();
        }

        /// <summary>
        /// 测试多个拦截器应用。
        /// </summary>
        [TestMethod]
        public void TestMultiCall()
        {
            var proxy = AspectFactory.BuildProxy<AspectTester>();

            proxy.MultiCall("fireasy");
        }

        [TestMethod]
        public void TestGlobalIntercept()
        {
            var proxy = AspectFactory.BuildProxy<AspectTesterEx>();

            Console.WriteLine(proxy.Name);
            Console.WriteLine(proxy.None);
            Console.WriteLine(proxy.TestMethod("huangxd"));
        }

        [TestMethod]
        public void TestInterfaceAop()
        {
            var proxy = typeof(IAopTester).New<IAopTester>();

            Console.WriteLine(proxy.Name);
            Console.WriteLine(proxy.TestMethod("huangxd"));
        }

        [TestMethod]
        public async Task TestAsyncMethod()
        {
            var proxy = AspectFactory.BuildProxy<AspectTester>();

            var str = await proxy.TestAsync("fireasy");
            Assert.AreEqual("hello fireasy", str);
        }

        [Intercept(typeof(SampleInterceptor))]
        public interface IAopTester : IAopSupport
        {
            string Name { get; set; }

            string TestMethod(string name);
        }

        public class AspectTester
        {
            public AspectTester(string name = null)
            {
            }

            [Intercept(typeof(SampleInterceptor))]
            public virtual string Name { get; set; }

            [Intercept(typeof(SampleInterceptor))]
            public virtual void SampleCall()
            {
                Console.WriteLine("正在调用 SampleCall 当前方法");
            }

            [Intercept(typeof(SampleInterceptor))]
            public virtual void WithParameters(string name, int age, DateTime? birthday)
            {
                Console.WriteLine("{0}正在调用 WithParameters 当前方法", name);
            }

            [Intercept(typeof(PrintParameterInterceptor))]
            public virtual void PrintParameters(string name, int age, DateTime? birthday)
            {
                Console.WriteLine("{0}正在调用 PrintParameters 当前方法", name);
            }

            [Intercept(typeof(SampleInterceptor))]
            public virtual void WithOutParameters(string name, ref int age, out DateTime birthday, ref object any)
            {
                age = 21;
                birthday = DateTime.Parse("2009-2-2");
                any = new object();
            }

            [Intercept(typeof(ComplexInterceptor))]
            public virtual void WithOutParametersComplex(string name, ref int age, out int age1, ref string sex, out string sex1, ref DateTime date, out DateTime date1, ref decimal? money, out decimal? money1, ref object any, out object any1, object any2 = null)
            {
                age = 21;
                age1 = 21;
                sex1 = "男";
                date1 = DateTime.Parse("2009-1-1");
                money1 = 334.4m;
                any1 = new List<string>();

                var args = new object[] { name, age, age1, sex, sex1, date, date1, money, money1, any, any1 };
                Console.WriteLine(args);
            }

            [Intercept(typeof(SampleInterceptor))]
            public virtual int ThrowException()
            {
                return int.Parse("abc");
            }

            [Intercept(typeof(SampleInterceptor), false)]
            public virtual int IgnoreException()
            {
                return int.Parse("abc");
            }

            [Intercept(typeof(SampleInterceptor), false)]
            public virtual object IgnoreAnotherException()
            {
                throw new NotImplementedException();
            }

            [Intercept(typeof(ExceptionInterceptor), false)]
            public virtual int IgnoreExceptionHasReturnValue()
            {
                return int.Parse("abc");
            }

            [Intercept(typeof(SampleInterceptor))]
            public virtual void GenericCall<T1, T2>(T1 t1, T2 t2)
            {
                Console.WriteLine("参数1: {0}，参数2: {1}", t1, t2);
            }

            [CustomInterceptAttribute(typeof(CustomInterceptAttributeInterceptor), true, Message = "这是一个自定义的特性")]
            public virtual void WithCustomInterceptAttribute()
            {
            }

            [Intercept(typeof(ContextTargetInterceptor))]
            public virtual void ContextTarget()
            {
            }

            [CustomInterceptAttribute(typeof(CustomInterceptAttributeInterceptor), true, Message = "这是一个自定义的特性")]
            [Intercept(typeof(ContextTargetInterceptor))]
            [Intercept(typeof(PrintParameterInterceptor))]
            public virtual void MultiCall(string name)
            {
            }

            [Intercept(typeof(AsyncInterceptor))]
            public virtual async Task<string> TestAsync(string str)
            {
                Console.WriteLine("tt");
                return str;
            }
        }

        [Intercept(typeof(SampleInterceptor))]
        public class AspectTesterEx
        {
            public virtual string Name { get; set; }

            public string None { get; set; }

            public virtual string TestMethod(string name)
            {
                return name;
            }
        }

        /// <summary>
        /// 简单的拦截器。
        /// </summary>
        public class SampleInterceptor : IInterceptor
        {
            /// <summary>
            /// 拦截器初始化。
            /// </summary>
            /// <param name="context"></param>
            public virtual void Initialize(InterceptContext context)
            {
            }

            /// <summary>
            /// 拦截到的信息。
            /// </summary>
            /// <param name="info"></param>
            public virtual void Intercept(InterceptCallInfo info)
            {
                switch (info.InterceptType)
                {
                    case InterceptType.BeforeMethodCall:
                        Console.WriteLine("调用 {0} 方法之前", info.Member.Name);
                        break;
                    case InterceptType.AfterMethodCall:
                        Console.WriteLine("调用 {0} 方法之后", info.Member.Name);
                        break;
                    case InterceptType.BeforeGetValue:
                        Console.WriteLine("获取 {0} 属性之前", info.Member.Name);
                        break;
                    case InterceptType.AfterGetValue:
                        Console.WriteLine("获取 {0} 属性之后", info.Member.Name);
                        break;
                    case InterceptType.BeforeSetValue:
                        Console.WriteLine("设置 {0} 属性之前", info.Member.Name);
                        break;
                    case InterceptType.AfterSetValue:
                        Console.WriteLine("设置 {0} 属性之后", info.Member.Name);
                        break;
                    case InterceptType.Catching:
                        Console.WriteLine("{0} 发生了异常", info.Member.Name);
                        break;
                    case InterceptType.Finally:
                        Console.WriteLine("{0} Finally", info.Member.Name);
                        break;
                }
            }
        }

        /// <summary>
        /// 输出参数的拦截器。
        /// </summary>
        public class PrintParameterInterceptor : SampleInterceptor
        {
            /// <summary>
            /// 重写拦截方法。
            /// </summary>
            /// <param name="info"></param>
            public override void Intercept(InterceptCallInfo info)
            {
                base.Intercept(info);

                if (info.Arguments != null)
                {
                    for (var i = 0; i < info.Arguments.Length; i++)
                    {
                        Console.WriteLine("参数{0}: {1}", i, info.Arguments[i]);
                    }
                }
            }
        }

        /// <summary>
        /// 异常的拦截器。
        /// </summary>
        public class ExceptionInterceptor : SampleInterceptor
        {
            /// <summary>
            /// 重写拦截方法。
            /// </summary>
            /// <param name="info"></param>
            public override void Intercept(InterceptCallInfo info)
            {
                if (info.InterceptType == InterceptType.Catching)
                {
                    info.ReturnValue = 99;
                }

                base.Intercept(info);
            }
        }

        /// <summary>
        /// 复合的拦截器，用于在拦截前后输出参数。
        /// </summary>
        public class ComplexInterceptor : SampleInterceptor
        {
            /// <summary>
            /// 重写拦截方法，输出被拦截方法的参数。
            /// </summary>
            /// <param name="info"></param>
            public override void Intercept(InterceptCallInfo info)
            {
                base.Intercept(info);

                switch (info.InterceptType)
                {
                    case InterceptType.BeforeMethodCall:
                    case InterceptType.AfterMethodCall:
                        PrintParameters(info);
                        break;
                }
            }

            /// <summary>
            /// 打印方法的参数。
            /// </summary>
            /// <param name="info"></param>
            private void PrintParameters(InterceptCallInfo info)
            {
                if (info.Arguments != null)
                {
                    for (var i = 0; i < info.Arguments.Length; i++)
                    {
                        Console.WriteLine("参数{0}: {1}", i, info.Arguments[i]);
                    }
                }
            }
        }

        /// <summary>
        /// 与 <see cref="CustomInterceptAttribute"/> 匹配的拦截器。
        /// </summary>
        public class CustomInterceptAttributeInterceptor : SampleInterceptor
        {
            private CustomInterceptAttribute attribute;

            /// <summary>
            /// 重写初始化方法，获得 Attribute 对象。
            /// </summary>
            /// <param name="context"></param>
            public override void Initialize(InterceptContext context)
            {
                attribute = context.Attribute as CustomInterceptAttribute;
                Assert.IsNotNull(attribute);

                base.Initialize(context);
            }

            /// <summary>
            /// 重写拦截方法，输出 Attribute 中的信息。
            /// </summary>
            /// <param name="info"></param>
            public override void Intercept(InterceptCallInfo info)
            {
                if (attribute != null)
                {
                    Assert.AreEqual("这是一个自定义的特性", attribute.Message);
                }
                base.Intercept(info);
            }
        }

        /// <summary>
        /// 用于测试 Initialize 方法中的 Target 属性。
        /// </summary>
        public class ContextTargetInterceptor : SampleInterceptor
        {
            /// <summary>
            /// 重写初始化方法。
            /// </summary>
            /// <param name="context"></param>
            public override void Initialize(InterceptContext context)
            {
                var target = context.Target as AspectTester;
                Assert.IsNotNull(target);

                base.Initialize(context);
            }
        }

        public class AsyncInterceptor : SampleInterceptor
        {
            public override void Intercept(InterceptCallInfo info)
            {
                if (info.InterceptType == InterceptType.AfterMethodCall)
                {
                    info.ReturnValue = Task.FromResult("hello " + (info.ReturnValue as Task<string>).Result);
                }

                base.Intercept(info);
            }
        }

        /// <summary>
        /// 自定义拦截特性，用于测试扩展的属性。
        /// </summary>
        public class CustomInterceptAttribute : InterceptAttribute
        {
            /// <summary>
            /// 初始化 <see cref="CustomInterceptAttribute"/> 类的新实例。
            /// </summary>
            /// <param name="interceptorType"></param>
            /// <param name="allowThrowException"></param>
            public CustomInterceptAttribute(Type interceptorType, bool allowThrowException = true)
                : base(interceptorType, allowThrowException)
            {
            }

            /// <summary>
            /// 获取或设置附加信息。
            /// </summary>
            public string Message { get; set; }
        }
    }
}
