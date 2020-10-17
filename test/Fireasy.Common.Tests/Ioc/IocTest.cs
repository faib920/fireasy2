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
    public class Generic<T> : IGeneric<T>
    {
        public T Test()
        {
            throw new NotImplementedException();
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
        IDClass DClass { get; set; }
    }

    public interface IDClass
    {
    }

    public class CClass : ICClass, IDisposable
    {
        private Guid g = Guid.NewGuid();

        //[IgnoreInjectProperty]
        public IDClass DClass { get; set; }

        public void Dispose()
        {
            Console.WriteLine("CClass Dispose");
        }
    }

    public class CClass2 : ICClass, IDisposable
    {
        public IDClass DClass { get; set; }

        public void Dispose()
        {
            Console.WriteLine("CClass Dispose");
        }
    }

    public class DClass : IDClass, IDisposable
    {
        private Guid g = Guid.NewGuid();

        public void Dispose()
        {
            Console.WriteLine("DClass Dispose");
        }
    }

    public class DClass1 : IDClass, IDisposable
    {
        private Guid g = Guid.NewGuid();

        public DClass1()
        {

        }

        public IEnumerable<ICClass> Clss { get; set; }

        public void Dispose()
        {
            Console.WriteLine("DClass1 Dispose");
        }

    }

    [TestClass]
    public class IocTest
    {

        [TestMethod]
        public void Test()
        {
            var container = ContainerUnity.GetContainer();

            container
                .RegisterTransient<IMainService, MainService>()
                .RegisterTransient(typeof(IMainService), typeof(MainServiceSecond));

            var obj = container.Resolve<IMainService>();

            Console.WriteLine(obj.GetType().Name);
        }

        [TestMethod]
        public void TestSigleton()
        {
            var container = ContainerUnity.GetContainer();
            container.RegisterSingleton<IMainService>(r => new MainService());

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
            container.RegisterTransient<IMainService>(r => new MainService());

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
            container.RegisterTransient<IMainService>(r => new MainService());
            container.RegisterTransient<IMainService>(r => new MainService());
            container.RegisterTransient<IMainService>(r => new MainService());

            var list = container.Resolve<IEnumerable<IMainService>>();
            var obj = container.Resolve<IMainService>();

            Assert.IsTrue(list.Count() == 3);
        }

        [TestMethod]
        public void TestEnumerableInjection()
        {
            var container = ContainerUnity.GetContainer();
            container.RegisterTransient<ICClass, CClass>();
            container.RegisterTransient<ICClass, CClass>();
            container.RegisterTransient<IDClass, DClass1>();

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

            container.RegisterTransient<IMainService, MainService>();
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
            container.RegisterTransient<IGeneric<MainService>, Generic>();

            var obj = container.Resolve<IGeneric<MainService>>();

            Console.WriteLine(obj.Test());
        }

        [TestMethod]
        public void TestGeneric1()
        {
            var container = ContainerUnity.GetContainer();
            container.RegisterTransient(typeof(IGeneric<>), typeof(Generic<>));

            var obj = container.Resolve<IGeneric<MainService>>();

            Console.WriteLine(obj.Test());
        }

        [TestMethod]
        public void TestConstructorInjection()
        {
            var container = ContainerUnity.GetContainer();

            container.RegisterTransient<IAClass, AClass>();
            container.RegisterTransient<IBClass, BClass>();

            var a = container.Resolve<IAClass>();

            Assert.IsTrue(a.HasB);
        }

        [TestMethod]
        public void TestPropertyInjection()
        {
            var container = ContainerUnity.GetContainer();

            container.RegisterTransient<ICClass, CClass>();
            container.RegisterTransient<IDClass, DClass>();

            var c = container.Resolve<ICClass>();

            Assert.IsNotNull(c.DClass);
        }

        [TestMethod]
        public void TestIgnoreInjectProperty()
        {
            var container = ContainerUnity.GetContainer();

            container.RegisterTransient<ICClass, CClass>();
            container.RegisterTransient<IDClass, DClass>();

            var c = container.Resolve<ICClass>();

            Assert.IsNull(c.DClass);
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
                .RegisterTransient<IAopBase, AopImpl>()
                .RegisterTransient<IAClass, AClass>();
            //.Register<IBClass, BClass>();

            var obj = container.Resolve<IAopBase>();
            obj.Test();
        }

        [TestMethod]
        public void TestScope()
        {
            var container = ContainerUnity.GetContainer();

            container.Register<ICClass, CClass2>(Lifetime.Scoped);
            container.Register<IDClass, DClass>(Lifetime.Scoped);

            using (var scope = container.CreateScope())
            {
                var c1 = scope.Resolve<ICClass>();
                var c2 = scope.Resolve<ICClass>();
                var d1 = scope.Resolve<IDClass>();
                var d2 = scope.Resolve<IDClass>();
            }
        }

        [TestMethod]
        public void TestScope1()
        {
            var container = ContainerUnity.GetContainer();

            container.Register<ICClass, CClass2>(Lifetime.Transient);
            container.Register<IDClass, DClass>(Lifetime.Scoped);

            using (var scope = container.CreateScope())
            {
                var c1 = scope.Resolve<ICClass>();
                var c2 = scope.Resolve<ICClass>();
                var d1 = scope.Resolve<IDClass>();
                var d2 = scope.Resolve<IDClass>();
            }
        }

        [TestMethod]
        public void TestScope1_1()
        {
            var container = ContainerUnity.GetContainer();

            container.Register<ICClass, CClass2>(Lifetime.Scoped);
            container.Register<IDClass, DClass>(Lifetime.Transient);

            using (var scope = container.CreateScope())
            {
                var c1 = scope.Resolve<ICClass>();
                var c2 = scope.Resolve<ICClass>();
                var d1 = scope.Resolve<IDClass>();
                var d2 = scope.Resolve<IDClass>();
            }
        }

        [TestMethod]
        public void TestScope1_2()
        {
            var container = ContainerUnity.GetContainer();

            container.Register<ICClass>(p => new CClass2 { DClass = p.Resolve<IDClass>() }, Lifetime.Scoped);
            container.Register<IDClass>(p => new DClass(), Lifetime.Transient);

            using (var scope = container.CreateScope())
            {
                var c1 = scope.Resolve<ICClass>();
                var c2 = scope.Resolve<ICClass>();
                var d1 = scope.Resolve<IDClass>();
                var d2 = scope.Resolve<IDClass>();

                using (var scope1 = scope.CreateScope())
                {
                    var c11 = scope1.Resolve<ICClass>();
                    var c21 = scope1.Resolve<ICClass>();
                    var d11 = scope1.Resolve<IDClass>();
                    var d21 = scope1.Resolve<IDClass>();
                }
            }
        }

        [TestMethod]
        public void TestScope2()
        {
            var container = ContainerUnity.GetContainer();

            container.Register<ICClass, CClass>(Lifetime.Singleton);

            using (var scope = container.CreateScope())
            {
                var c1 = scope.Resolve<ICClass>();
                var c2 = scope.Resolve<ICClass>();
            }

            Console.WriteLine("最后释放");
            container.Dispose();
        }
    }
}
