using Fireasy.Common.Emit;
using Fireasy.Common.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Fireasy.Common.Tests.Emit
{
    [TestClass]
    public class EmitTest
    {
        [TestMethod]
        public void TestEmitAssert()
        {
            var callMethod = true;
            var method = new DynamicMethod("DynamicMethod1", typeof(bool), null);
            var emiter = new EmitHelper(method.GetILGenerator())
                .Assert(callMethod,
                    e => e.ldstr("fireasy").call(typeof(Helper).GetMethod(nameof(Helper.GetHello), new[] { typeof(string) })),
                    e => e.ldstr("hello fireasy"))
                .call(typeof(Console).GetMethod("WriteLine", new[] { typeof(string) }))
                .ldc_i4_1.ret();

            var func = method.CreateDelegate(typeof(Func<bool>));

            Assert.IsTrue((bool)func.DynamicInvoke(null));
        }

        public bool Test()
        {
            Console.WriteLine(Helper.GetHello("fireasy"));
            return true;
        }

        [TestMethod]
        public void TestEmitEach()
        {
            var keys = new[] { "kunming", "chengdu", "guangzhou" };
            var method = new DynamicMethod("DynamicMethod", typeof(int), new[] { typeof(List<string>) });
            var emiter = new EmitHelper(method.GetILGenerator())
                .Each(keys, (e, str, i) =>
                    {
                        e.ldarg_0.ldstr(str)
                        .callvirt(typeof(List<string>).GetMethod("Add"));
                    })
                .ldarg_0.callvirt(typeof(List<string>).GetMethod("get_Count")).ret();

            var items = new List<string>();
            var func = method.CreateDelegate(typeof(Func<List<string>, int>));

            Assert.AreEqual(3, (int)func.DynamicInvoke(items));
        }

        [TestMethod]
        public void TestBuildAssembly()
        {
            var assemblyBuilder = new DynamicAssemblyBuilder("MyAssembly");

            var interfaceBuilder = assemblyBuilder.DefineInterface("MyInterface");
            var typeBuilder = assemblyBuilder.DefineType("MyClass");
            var enumBuilder = assemblyBuilder.DefineEnum("MyEnum");

            Assert.AreEqual("MyClass", typeBuilder.TypeBuilder.Name);
        }

        [TestMethod]
        public void TestBuildAssemblyWithDecoration()
        {
            var assemblyBuilder = new DynamicAssemblyBuilder("MyAssembly");
            var typeBuilder1 = assemblyBuilder.DefineType("MyPrivateClass", VisualDecoration.Private);
            var typeBuilder2 = assemblyBuilder.DefineType("MyAbstractClass", calling: CallingDecoration.Abstract);

            Assert.IsFalse(typeBuilder1.TypeBuilder.IsPublic);
            Assert.IsTrue(typeBuilder2.TypeBuilder.IsAbstract);
        }

        [TestMethod]
        public void TestEmitFor()
        {
            var method = new DynamicMethod("DynamicMethod", typeof(void), new[] { typeof(string), typeof(string), typeof(int) });

            var parameters = method.GetParameters();
            var emiter = new EmitHelper(method.GetILGenerator())
                .ldc_i4(parameters.Length)
                .newarr(typeof(object))
                .For(0, parameters.Length, (e, i) =>
                    {
                        e.dup.ldc_i4(i).ldarg(i)
                            .Assert(parameters[i].ParameterType.IsValueType,
                            e1 => e1.box(parameters[i].ParameterType))
                            .stelem_ref.end();
                    })
                .call(typeof(string).GetMethod("Concat", new[] { typeof(object[]) }))
                .call(typeof(Console).GetMethod("WriteLine", new[] { typeof(string) })).ret();

            var action = method.CreateDelegate(typeof(Action<string, string, int>));
            action.DynamicInvoke("fireasy", "kunming", 33);
        }

        public class Helper
        {
            public static string GetHello(string name)
            {
                return "hello " + name;
            }

            public static int Add(List<int> dd)
            {
                dd.Add(1);
                dd.Add(2);
                dd.Add(3);

                return dd.Count;
            }

            public void Write(string name, string city, int age)
            {
                Console.WriteLine(string.Concat(new object[] { name, city, age }));
            }
        }

        [TestMethod]
        public void TestAssemblyBuilder()
        {
            var assemblyBuilder = new DynamicAssemblyBuilder("DynamicAssembly1");

            var interfaceBuilder = assemblyBuilder.DefineInterface("IInterface");
            var typeBuilder = assemblyBuilder.DefineType("Class1");
            var enumBuilder = assemblyBuilder.DefineEnum("Enum1");
#if NETAPPCORE
            var assembly = assemblyBuilder.Save();
            Assert.AreEqual("Class1", typeBuilder.TypeBuilder.Name);
            Assert.AreEqual("DynamicAssembly1", assembly.GetExportedTypes().Length);
#endif
        }

        [TestMethod()]
        public void TestDynamicAssemblyBuilder()
        {
            var assemblyBuilder = new DynamicAssemblyBuilder("dynamicAssembly1");

            Assert.IsNotNull(assemblyBuilder);
        }

        [TestMethod()]
        public void TestDefineType()
        {
            var assemblyBuilder = new DynamicAssemblyBuilder("dynamicAssembly1");

            assemblyBuilder.DefineType("class1");
        }
        public interface IMyInterface
        {
            void HelloWorld();

            string Title { get; set; }
        }

        public class MyBaseClass
        {
            public virtual string GetName(string name)
            {
                return name;
            }
        }

        [TestMethod]
        public void TestTypeBuilder()
        {
            var assemblyBuilder = new DynamicAssemblyBuilder("MyAssembly");
            var typeBuilder = assemblyBuilder.DefineType("MyClass");

            typeBuilder.BaseType = typeof(MyBaseClass);

            typeBuilder.ImplementInterface(typeof(IMyInterface));

            var methodBuilder = typeBuilder.DefineMethod("HelloWorld");
            var propertyBuilder = typeBuilder.DefineProperty("Title", typeof(string)).DefineGetSetMethods();

            methodBuilder = typeBuilder.DefineMethod("WriteName", typeof(string), new[] { typeof(string) });

            var type = typeBuilder.CreateType();

            Assert.IsTrue(typeof(IMyInterface).IsAssignableFrom(type));
        }

        [TestMethod()]
        public void TestDefineInterface()
        {
            var assemblyBuilder = new DynamicAssemblyBuilder("dynamicAssembly1");

            assemblyBuilder.DefineInterface("interface1");
        }

        [TestMethod()]
        public void TestDefineEnum()
        {
            var assemblyBuilder = new DynamicAssemblyBuilder("dynamicAssembly1");

            assemblyBuilder.DefineEnum("enum1");
        }

        [TestMethod()]
        public void TestSave()
        {
            var fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dynamicAssembly1.dll");
            var assemblyBuilder = new DynamicAssemblyBuilder("dynamicAssembly1", fileName);

#if NETAPPCORE
            assemblyBuilder.Save();
#endif

            Assert.IsTrue(File.Exists(fileName));
        }

        [TestMethod()]
        public void TestSaveAll()
        {
            var assemblyBuilder = new DynamicAssemblyBuilder("dynamicAssembly1");
            var type1 = assemblyBuilder.DefineType("class1");
            var type2 = assemblyBuilder.DefineType("class2");

#if NETAPPCORE
            assemblyBuilder.Save();
#endif

            Assert.AreEqual(2, assemblyBuilder.AssemblyBuilder.GetTypes().Length);
        }

        private DynamicTypeBuilder CreateBuilder()
        {
            var assemblyBuilder = new DynamicAssemblyBuilder("assemblyTests");
            return assemblyBuilder.DefineType("testClass");
        }

        /// <summary>
        /// 测试CreateType方法。
        /// </summary>
        [TestMethod()]
        public void CreateType()
        {
            var typeBuilder = CreateBuilder();
            var type = typeBuilder.CreateType();

            Assert.IsNotNull(type);
        }

        /// <summary>
        /// 测试BaseType属性。
        /// </summary>
        [TestMethod()]
        public void BaseType()
        {
            var typeBuilder = CreateBuilder();
            typeBuilder.BaseType = typeof(DynamicBuilderBase);
            var type = typeBuilder.CreateType();

            Assert.AreEqual(typeof(DynamicBuilderBase), type.BaseType);
        }

        /// <summary>
        /// 测试ImplementInterface方法。
        /// </summary>
        [TestMethod()]
        public void ImplementInterface()
        {
            var typeBuilder = CreateBuilder();
            typeBuilder.ImplementInterface(typeof(IDynamicInterface));
            var type = typeBuilder.CreateType();

            Assert.IsTrue(typeof(IDynamicInterface).IsAssignableFrom(type));
        }

        /// <summary>
        /// 使用接口成员测试ImplementInterface方法。
        /// </summary>
        [TestMethod()]
        public void ImplementInterfaceWithMember()
        {
            var typeBuilder = CreateBuilder();
            typeBuilder.ImplementInterface(typeof(IDynamicPropertyInterface));
            typeBuilder.DefineProperty("Name", typeof(string)).DefineGetSetMethods();

            var type = typeBuilder.CreateType();

            Assert.IsTrue(typeof(IDynamicPropertyInterface).IsAssignableFrom(type));
            Assert.IsNotNull(type.GetProperty("Name"));
        }

        /// <summary>
        /// 使用接口成员显式实现测试ImplementInterface方法。
        /// </summary>
        [TestMethod()]
        public void ImplementInterfaceWithExplicitMember()
        {
            var typeBuilder = CreateBuilder();

            typeBuilder.ImplementInterface(typeof(IDynamicMethodInterface));
            var methodBuilder = typeBuilder.DefineMethod("Test",
                parameterTypes: new[] { typeof(int) },
                calling: CallingDecoration.ExplicitImpl,
                ilCoding: (e) => e.Emitter.ldstr("fireasy").call(typeof(Console).GetMethod("WriteLine", new[] { typeof(string) })).ret());

            methodBuilder.DefineParameter("s");

            var type = typeBuilder.CreateType();

            var obj = type.New<IDynamicMethodInterface>();
            obj.Test(111);

            Assert.IsTrue(typeof(IDynamicMethodInterface).IsAssignableFrom(type));
        }

        /// <summary>
        /// 测试DefineProperty方法。
        /// </summary>
        [TestMethod()]
        public void DefineProperty()
        {
            var typeBuilder = CreateBuilder();

            typeBuilder.DefineProperty("Name", typeof(string)).DefineGetSetMethods();
            var type = typeBuilder.CreateType();

            Assert.IsNotNull(type.GetProperty("Name"));
        }

        /// <summary>
        /// 测试DefineMethod方法。
        /// </summary>
        [TestMethod()]
        public void TestDefineMethod()
        {
            var typeBuilder = CreateBuilder();

            typeBuilder.DefineMethod("HelloWorld");
            var type = typeBuilder.CreateType();

            Assert.IsNotNull(type.GetMethod("HelloWorld"));
        }

        /// <summary>
        /// 使用泛型参数测试DefineMethod方法。
        /// </summary>
        [TestMethod()]
        [Description("dfdsf")]
        public void TestDefineGenericMethod()
        {
            var typeBuilder = CreateBuilder();

            // Helo<T1, T2>(string name, T1 any1, T2 any2)
            var methodBuilder = typeBuilder.DefineMethod("Hello", parameterTypes: new Type[] { typeof(string), null, null });
            methodBuilder.GenericArguments = new string[] { "", "T1", "T2" };
            methodBuilder.DefineParameter("name");
            methodBuilder.DefineParameter("any1");
            methodBuilder.DefineParameter("any2");

            var paraCount = methodBuilder.ParameterTypes.Length;

            methodBuilder.OverwriteCode(e =>
            {
                e.ldc_i4(paraCount)
                .newarr(typeof(object))
                .dup.ldc_i4_0.ldarg_1.stelem_ref
                .For(1, paraCount, (e1, i) =>
                {
                    e1.dup.ldc_i4(i).ldarg(i + 1).box(methodBuilder.ParameterTypes[i]).stelem_ref.end();
                })
                .call(typeof(string).GetMethod("Concat", new[] { typeof(object[]) }))
                .call(typeof(Console).GetMethod("WriteLine", new[] { typeof(string) }))
                .ret();
            });

            var type = typeBuilder.CreateType();

            var method = type.GetMethod("Hello");
            Assert.IsNotNull(method);
            Assert.IsTrue(method.IsGenericMethod);

            var obj = type.New();

            method = method.MakeGenericMethod(typeof(int), typeof(decimal));

            method.Invoke(obj, new object[] { "fireasy", 22, 45m });
        }

        public void Tess<T1, T2>(T1 t1, T2 t2)
        {
            Console.WriteLine(string.Concat(new object[] { t1, t2 }));
        }

        /// <summary>
        /// 使用重写抽象类方法测试DefineMethod方法。
        /// </summary>
        [TestMethod()]
        public void TestDefineOverrideMethod()
        {
            var typeBuilder = CreateBuilder();
            typeBuilder.BaseType = typeof(DynamicBuilderBase);

            typeBuilder.DefineMethod("Hello", parameterTypes: new Type[] { typeof(string) }, ilCoding: (context) =>
            {
                context.Emitter
                    .ldarg_0
                    .ldarg_1
                    .call(typeBuilder.BaseType.GetMethod("Hello"))
                    .ret();
            });
            var type = typeBuilder.CreateType();

            var method = type.GetMethod("Hello");
            Assert.IsNotNull(method);
            method.Invoke(type.New(), new object[] { "fireasy" });
        }

        /// <summary>
        /// 测试DefineConstructor方法。
        /// </summary>
        [TestMethod()]
        public void TestDefineConstructor()
        {
            var t = new ssss("dfdfd");
            var typeBuilder = CreateBuilder();

            var c = typeBuilder.DefineConstructor(new Type[] { typeof(string), typeof(string) });

            c.DefineParameter("name").DefineParameter("tt", "bbb");

            c.OverwriteCode(e =>
                e.ldarg_1.ldarg_2
                    .call(typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) }))
                    .call(typeof(Console).GetMethod("WriteLine", new[] { typeof(string) }))
                    .ret()
            );

            var type = typeBuilder.CreateType();


            type.GetConstructors().FirstOrDefault().Invoke(new[] { "fireasy", null });
        }

        public class ssss
        {
            public ssss(string a, string b = "bbb")
            {
                Console.WriteLine(a, b);
            }

            public static ssss dd()
            {
                return new ssss("tt");
            }
        }

        /// <summary>
        /// 测试DefineField方法。
        /// </summary>
        [TestMethod()]
        public void TestDefineField()
        {
            var typeBuilder = CreateBuilder();

            typeBuilder.DefineField("Name", typeof(string));
            var type = typeBuilder.CreateType();

            Assert.IsNotNull(type.GetField("Name", BindingFlags.NonPublic | BindingFlags.Instance));
        }

        /// <summary>
        /// 使用公有特性测试DefineField方法。
        /// </summary>
        [TestMethod()]
        public void TestDefineFieldWithPublic()
        {
            var typeBuilder = CreateBuilder();

            typeBuilder.DefineField("Name", typeof(string), "fireasy", VisualDecoration.Public);
            var type = typeBuilder.CreateType();

            Assert.IsNotNull(type.GetField("Name"));
        }

        /// <summary>
        /// 测试DefineNestedType方法。
        /// </summary>
        [TestMethod()]
        public void TestDefineNestedType()
        {
            var typeBuilder = CreateBuilder();

            var nestedType = typeBuilder.DefineNestedType("nestedClass");
            var type = typeBuilder.CreateType();

            Assert.IsNotNull(type.GetNestedType("nestedClass", BindingFlags.NonPublic));
        }

        [TestMethod()]
        public void TestDefineParameter()
        {
            // Hello(string name);
            var typeBuilder = CreateBuilder();
            var methodBuilder = typeBuilder.DefineMethod("Hello", parameterTypes: new Type[] { typeof(string) });

            methodBuilder.DefineParameter("name");
            typeBuilder.CreateType();

            Assert.AreEqual("name", methodBuilder.MethodBuilder.GetParameters()[0].Name);
        }

        [TestMethod()]
        public void TestDefineParameterRef()
        {
            // Hello(ref string name);
            var typeBuilder = CreateBuilder();
            var methodBuilder = typeBuilder.DefineMethod("Hello", parameterTypes: new Type[] { typeof(string) });

            methodBuilder.DefineParameter("name", true);
            typeBuilder.CreateType();

            Assert.IsTrue(methodBuilder.MethodBuilder.GetParameters()[0].IsOut);
        }

        [TestMethod()]
        public void TestDefineParameterOptional()
        {
            // Hello(string name = "fireasy");
            var typeBuilder = CreateBuilder();
            var methodBuilder = typeBuilder.DefineMethod("Hello", parameterTypes: new Type[] { typeof(string) });

            methodBuilder.DefineParameter("name", defaultValue: "fireasy");
            typeBuilder.CreateType();

            Assert.AreEqual("fireasy", methodBuilder.MethodBuilder.GetParameters()[0].DefaultValue);
        }

        [TestMethod()]
        public void TestAppendCode()
        {
            var typeBuilder = CreateBuilder();
            var writeLineMethod = typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) });
            var methodBuilder = typeBuilder.DefineMethod("Hello", calling: CallingDecoration.Static, ilCoding: context => { });

            methodBuilder.AppendCode(e => e.ldstr("Hello fireasy!").call(writeLineMethod));
            methodBuilder.AppendCode(e => e.ldstr("Hello world!").call(writeLineMethod).ret());

            var type = typeBuilder.CreateType();

            type.GetMethod("Hello").Invoke(null, null);
        }

        [TestMethod()]
        public void TestOverwriteCode()
        {
            var typeBuilder = CreateBuilder();
            var writeLineMethod = typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) });
            var methodBuilder = typeBuilder.DefineMethod("Hello", calling: CallingDecoration.Static, ilCoding: context =>
                context.Emitter.ldstr("Hello fireasy").call(writeLineMethod).ret());

            methodBuilder.OverwriteCode(e => e.ldstr("Hello world!").call(writeLineMethod).ret());

            var type = typeBuilder.CreateType();

            type.GetMethod("Hello").Invoke(null, null);
        }

        [TestMethod()]
        public void TestDefineGetSetMethods()
        {
            var typeBuilder = CreateBuilder();
            var propertyBuilder = typeBuilder.DefineProperty("Name", typeof(string));
            propertyBuilder.DefineGetSetMethods();

            Assert.IsTrue(propertyBuilder.PropertyBuilder.CanRead);
            Assert.IsTrue(propertyBuilder.PropertyBuilder.CanWrite);
        }

        [TestMethod()]
        public void TestDefineGetMethod()
        {
            var typeBuilder = CreateBuilder();
            var propertyBuilder = typeBuilder.DefineProperty("Name", typeof(string));
            propertyBuilder.DefineGetMethod();

            Assert.IsTrue(propertyBuilder.PropertyBuilder.CanRead);
            Assert.IsFalse(propertyBuilder.PropertyBuilder.CanWrite);
        }

        [TestMethod()]
        public void TestDefineGetMethodByField()
        {
            var typeBuilder = CreateBuilder();
            var propertyBuilder = typeBuilder.DefineProperty("Name", typeof(string));

            propertyBuilder.DefineGetMethodByField();

            Assert.IsTrue(propertyBuilder.PropertyBuilder.CanRead);
            Assert.IsFalse(propertyBuilder.PropertyBuilder.CanWrite);
        }

        [TestMethod()]
        public void TestDefineSetMethod()
        {
            var typeBuilder = CreateBuilder();
            var propertyBuilder = typeBuilder.DefineProperty("Name", typeof(string));
            propertyBuilder.DefineSetMethod();

            Assert.IsFalse(propertyBuilder.PropertyBuilder.CanRead);
            Assert.IsTrue(propertyBuilder.PropertyBuilder.CanWrite);
        }

        [TestMethod()]
        public void TestDefineSetMethodByField()
        {
            var typeBuilder = CreateBuilder();
            var propertyBuilder = typeBuilder.DefineProperty("Name", typeof(string));

            propertyBuilder.DefineSetMethodByField();

            Assert.IsFalse(propertyBuilder.PropertyBuilder.CanRead);
            Assert.IsTrue(propertyBuilder.PropertyBuilder.CanWrite);
        }
    }

    public interface IDynamicInterface
    {
    }

    public interface IDynamicPropertyInterface
    {
        string Name { get; set; }
    }

    public interface IDynamicMethodInterface
    {
        void Test(int s);
    }

    public class DynamicBuilderBase
    {
        public virtual void Hello(string name)
        {
            Console.WriteLine("Hello " + name);
        }
    }

}
