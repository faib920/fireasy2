using Fireasy.Common.Emit;
using Fireasy.Common.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reflection;

namespace Fireasy.Common.Tests.Emit
{
    [TestClass]
    public class EmitTest
    {
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
                ilCoding: (e) => e.Emitter.ldstr("fireasy").call(typeof(Console).GetMethod("WriteLine", new[] { typeof(string) })).ret() );

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
        public void TestDefineGenericMethod()
        {
            var typeBuilder = CreateBuilder();

            // Helo<T1, T2>(string name, T1 any1, T2 any2)
            var methodBuilder = typeBuilder.DefineMethod("Hello", parameterTypes: new Type[] { typeof(string), null, null });
            methodBuilder.GenericArguments = new string[] { string.Empty, "T1", "T2" };
            methodBuilder.DefineParameter("name");
            methodBuilder.DefineParameter("any1");
            methodBuilder.DefineParameter("any2");

            var type = typeBuilder.CreateType();

            var method = type.GetMethod("Hello");
            Assert.IsNotNull(method);
            Assert.IsTrue(method.IsGenericMethod);
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
            var typeBuilder = CreateBuilder();

            typeBuilder.DefineConstructor(new Type[] { typeof(string), typeof(int) }).AppendCode(e => e.ret());
            var type = typeBuilder.CreateType();

            var constructor = type.GetConstructors()[0];
            Assert.IsNotNull(constructor);
            Assert.AreEqual(2, constructor.GetParameters().Length);
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
