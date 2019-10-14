using Fireasy.Common.Aop;
using Fireasy.Common.Ioc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace Fireasy.Common.Tests.Ioc
{
    public interface IMainService
    {
        string Name { get; set; }
    }

    public class MainService : IMainService
    {
        public string Name { get; set; }
    }

    public class MainServiceSecond : IMainService
    {
        public string Name { get; set; }
    }

    public interface IGeneric<T>
    {
        T Test();
    }

    public class Generic : IGeneric<MainService>
    {
        public MainService Test()
        {
            return new MainService();
        }
    }

    public interface IAClass
    {
        bool HasB { get; }
    }

    public interface IBClass
    {
    }

    public class AClass : IAClass
    {
        public AClass(IBClass b)
        {
            this.B = b;
        }

        /// <summary>
        /// 判断是否包含 <see cref="IBClass"/> 对象。
        /// </summary>
        public bool HasB
        {
            get
            {
                return B != null;
            }
        }

        public IBClass B { get; set; }
    }

    public class BClass : IBClass
    {
    }
    public interface ICClass
    {
        IDClass D { get; set; }
    }

    public interface IDClass
    {
    }

    public class CClass : ICClass
    {
        //[IgnoreInjectProperty]
        public IDClass D { get; set; }
    }

    public class CClass2 : ICClass
    {
        public IDClass D { get; set; }
    }

    public class DClass : IDClass
    {
    }

    public class DClass1 : IDClass
    {
        public DClass1()
        {

        }

        public IEnumerable<ICClass> Clss { get; set; }
    }

    [TestClass]
    public class IocTest
    {

        [TestMethod]
        public void Test()
        {
            var container = ContainerUnity.GetContainer();

            container
                .Register<IMainService, MainService>()
                .Register(typeof(IMainService), typeof(MainServiceSecond));

            var obj = container.Resolve<IMainService>();

            Console.WriteLine(obj.GetType().Name);
        }

        [TestMethod]
        public void TestSigleton()
        {
            var container = ContainerUnity.GetContainer();
            container.RegisterSingleton<IMainService>(() => new MainService());

            //第一次反转
            var obj1 = container.Resolve<IMainService>();

            //再次反转，还是之前的对象
            var obj2 = container.Resolve<IMainService>();

            Assert.IsTrue(obj1 == obj2);
        }

        [TestMethod]
        public void TestSigletonInjection()
        {
            var container = ContainerUnity.GetContainer();
            container.RegisterSingleton<IAClass, AClass>();
            container.RegisterSingleton<IBClass, BClass>();

            var a = container.Resolve<IAClass>();
            var b = container.Resolve<IBClass>();

            Assert.IsTrue((a as AClass).B == b);
        }

        [TestMethod]
        public void TestInstance()
        {
            var container = ContainerUnity.GetContainer();
            container.Register<IMainService>(() => new MainService());

            //第一次反转
            var obj1 = container.Resolve<IMainService>();

            //再次反转，得到新的对象
            var obj2 = container.Resolve<IMainService>();

            Assert.IsTrue(obj1 != obj2);
        }

        [TestMethod]
        public void TestEnumerable()
        {
            var container = ContainerUnity.GetContainer();
            container.Register<IMainService>(() => new MainService());
            container.Register<IMainService>(() => new MainService());
            container.Register<IMainService>(() => new MainService());

            var list = container.Resolve<IEnumerable<IMainService>>();
            var obj = container.Resolve<IMainService>();

            Assert.IsTrue(list.Count() == 3);
        }

        [TestMethod]
        public void TestEnumerableInjection()
        {
            var container = ContainerUnity.GetContainer();
            container.Register<ICClass, CClass>();
            container.Register<ICClass, CClass>();
            container.Register<IDClass, DClass1>();

            var obj = container.Resolve<IDClass>();

            Assert.IsTrue(obj != null);
        }

        [TestMethod]
        public void TestEnumerableInjectionSingleton()
        {
            var container = ContainerUnity.GetContainer();
            container.RegisterSingleton<ICClass, CClass>();
            container.RegisterSingleton<ICClass, CClass2>();
            container.RegisterSingleton<IDClass, DClass1>();

            var obj = container.Resolve<IDClass>();

            Assert.IsTrue(obj != null);
        }

        [TestMethod]
        public void TestInitializer()
        {
            var container = ContainerUnity.GetContainer();

            container.Register<IMainService, MainService>();
            container.RegisterInitializer<IMainService>(s => s.Name = "fireasy");

            var obj = container.Resolve<IMainService>();
            Assert.AreEqual("fireasy", obj.Name);
        }

        [TestMethod]
        public void TestAssembly()
        {
            var container = ContainerUnity.GetContainer();
            container.RegisterAssembly(typeof(IocTest).Assembly);

            var obj = container.Resolve<IMainService>();

            Console.WriteLine(obj.GetType().Name);
        }

        [TestMethod]
        public void TestAssemblyName()
        {
            var container = ContainerUnity.GetContainer();
            container.RegisterAssembly("Fireasy.Common.Tests");

            var obj = container.Resolve<IMainService>();

            Console.WriteLine(obj.GetType().Name);
        }

        [TestMethod]
        public void TestGeneric()
        {
            var container = ContainerUnity.GetContainer();
            container.Register<IGeneric<MainService>, Generic>();

            var obj = container.Resolve<IGeneric<MainService>>();

            Console.WriteLine(obj.Test());
        }

        [TestMethod]
        public void TestConstructorInjection()
        {
            var container = ContainerUnity.GetContainer();

            container.Register<IAClass, AClass>();
            container.Register<IBClass, BClass>();

            var a = container.Resolve<IAClass>();

            Assert.IsTrue(a.HasB);
        }

        [TestMethod]
        public void TestPropertyInjection()
        {
            var container = ContainerUnity.GetContainer();

            container.Register<ICClass, CClass>();
            container.Register<IDClass, DClass>();

            var c = container.Resolve<ICClass>();

            Assert.IsNotNull(c.D);
        }

        [TestMethod]
        public void TestIgnoreInjectProperty()
        {
            var container = ContainerUnity.GetContainer();

            container.Register<ICClass, CClass>();
            container.Register<IDClass, DClass>();

            var c = container.Resolve<ICClass>();

            Assert.IsNull(c.D);
        }

        [TestMethod]
        public void TestSelf()
        {
            var container = ContainerUnity.GetContainer();
            var obj = container.Resolve<BClass>();
            Assert.IsNotNull(obj);
        }

        [TestMethod]
        public void TestLoadConfig()
        {
            var container = ContainerUnity.GetContainer();

            //查找config目录下的 *.ioc.config 文件
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config");
            container.Config(path, "*.ioc.xml");

            //如果是加载指定的文件
            //container.Config(Path.Combine(path, "sample.ioc.xml"));

            var service = container.Resolve<IMainService>();
            Assert.IsNotNull(service);
        }

        [TestMethod]
        public void TestContainers()
        {
            var c1 = ContainerUnity.GetContainer("c1");
            var c2 = ContainerUnity.GetContainer("c2");

            var b1 = c1.Resolve<IBClass>();
            var b2 = c2.Resolve<IBClass>();
            var b3 = c1.Resolve<IBClass>();

            Assert.IsNotNull(b1);
            Assert.IsNotNull(b2);

            Assert.IsTrue(b1 == b3); //同一容器反转的两个对象
            Assert.IsFalse(b1 == b2); //不同容器的两个对象
        }

        [IgnoreRegister]
        public interface IAopBase
        {
            void Test();
        }

        [Intercept(typeof(AopInterceptor))]
        public class AopImpl : IAopBase, IAopSupport
        {
            public AopImpl(IAClass a)
            {
                Console.WriteLine(a);
            }

            public virtual void Test()
            {
            }
        }

        public class AopInterceptor : Aop.IInterceptor
        {
            public void Initialize(InterceptContext context)
            {
            }

            public void Intercept(InterceptCallInfo info)
            {
                Console.WriteLine(info.InterceptType);
            }
        }

        [TestMethod]
        public void TestAop()
        {
            var container = ContainerUnity.GetContainer();

            container
                .Register<IAopBase, AopImpl>()
                .Register<IAClass, AClass>();
            //.Register<IBClass, BClass>();

            var obj = container.Resolve<IAopBase>();
            obj.Test();
        }
    }
}
