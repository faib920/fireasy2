using Fireasy.Data.Entity.Dynamic;
using Fireasy.Data.Entity.Properties;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Fireasy.Common.Extensions;
using Fireasy.Common.Emit;
using Fireasy.Data.Entity.Validation;

namespace Fireasy.Data.Entity.Tests
{
    [TestClass]
    public class DynamicTypeBuilderTests
    {
        [TestMethod]
        public void TestBuild()
        {
#if !NETCOREAPP2_0
            var assBuilder = new DynamicAssemblyBuilder("test_dll", "e:\\test.dll");
            var typeBuilder = new EntityTypeBuilder("TestEntity", assBuilder);
#else
            var typeBuilder = new EntityTypeBuilder("testclass");
#endif
            var pName = new GeneralProperty() { Name = "Name", Info = new PropertyMapInfo { FieldName = "name" }, Type = typeof(string) };
            typeBuilder.Properties.Add(pName);
            typeBuilder.Properties.Add(new GeneralProperty() { Name = "Age", Info = new PropertyMapInfo { FieldName = "age" }, Type = typeof(int?) });
            typeBuilder.Properties.Add(new GeneralProperty() { Name = "Sex", Info = new PropertyMapInfo { FieldName = "sex" }, Type = typeof(Sex) });

            typeBuilder.DefineValidateRule(pName, () => new System.ComponentModel.DataAnnotations.MaxLengthAttribute(15));

            var type = typeBuilder.Create();

#if !NETCOREAPP2_0
            assBuilder.Save();
#endif

            var e = type.New<IEntity>();
            e.SetValue("Name", "fireasy");
            e.SetValue("Age", 12);
            e.SetValue("Sex", Sex.M);

            Assert.AreEqual(e.GetValue("Name"), "fireasy");
            Assert.AreEqual(e.GetValue("Age"), 12);
            Assert.AreEqual(e.GetValue("Sex"), Sex.M);

            ValidationUnity.Validate(e);

            var property = PropertyUnity.GetProperty(type, "Name");
            Assert.IsNotNull(property);

            Assert.AreEqual(type.GetProperty("Name").GetValue(e), "fireasy");
        }
    }

    public enum Sex
    {
        M,
        F
    }
}
