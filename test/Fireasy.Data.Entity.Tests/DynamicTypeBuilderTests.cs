using Fireasy.Data.Entity.Dynamic;
using Fireasy.Data.Entity.Properties;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Fireasy.Common.Extensions;
using Fireasy.Common.Emit;
using Fireasy.Data.Entity.Validation;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System.Linq.Expressions;
using System.Collections;

namespace Fireasy.Data.Entity.Tests
{
    [TestClass]
    public class DynamicTypeBuilderTests
    {
        public DynamicTypeBuilderTests()
        {
            InitConfig.Init();
        }
        [TestMethod]
        public void TestBuild()
        {
            var assemblyBuilder = new DynamicAssemblyBuilder("TestAssembly");

            var empBuilder = new EntityTypeBuilder("Employee", assemblyBuilder) { Mapping = new EntityMappingAttribute("employee") };
            empBuilder.SetCustomAttribute(() => new EntityMappingAttribute("employee"));
            var empId = new GeneralProperty() { Name = "EmployeeId", Info = new PropertyMapInfo { FieldName = "emp_id", IsPrimaryKey = true, GenerateType = IdentityGenerateType.AutoIncrement }, Type = typeof(int) };
            var empName = new GeneralProperty() { Name = "Name", Info = new PropertyMapInfo { FieldName = "name" }, Type = typeof(string) };
            var empAge = new GeneralProperty() { Name = "Age", Info = new PropertyMapInfo { FieldName = "age" }, Type = typeof(int?) };
            var empSex = new GeneralProperty() { Name = "Sex", Info = new PropertyMapInfo { FieldName = "sex" }, Type = typeof(Sex) };


            empBuilder.Properties.Add(empName);
            empBuilder.Properties.Add(empAge);
            empBuilder.Properties.Add(empSex);

            empBuilder.DefineValidateRule(empName, () => new RequiredAttribute());
            empBuilder.DefineValidateRule(empName, () => new MaxLengthAttribute(15));

            var ordBuilder = new EntityTypeBuilder("Orders", assemblyBuilder);
            var ordId = new GeneralProperty() { Name = "Id", Info = new PropertyMapInfo { FieldName = "id", IsPrimaryKey = true, GenerateType = IdentityGenerateType.AutoIncrement }, Type = typeof(int) };
            var ordOrderDate = new GeneralProperty() { Name = "OrderDate", Info = new PropertyMapInfo { FieldName = "order_date" }, Type = typeof(DateTime) };
            var ordEmpId = new GeneralProperty() { Name = "EmployeeId", Info = new PropertyMapInfo { FieldName = "emp_id" }, Type = typeof(int?) };

            //Employee的主键是Id，而Orders中外键为EmployeeId，两者不一致，不能自动映射关系
            var pext = new PropertyExtension(ordEmpId);
            pext.SetCustomAttribute(() => new RelationshipAssignAttribute("Id", "EmployeeId"));

            var ordEmp = new EntityProperty() { Name = "Employee", Type = empBuilder.EntityType, RelationalType = empBuilder.EntityType };

            ordBuilder.Properties.Add(ordId);
            ordBuilder.Properties.Add(ordOrderDate);
            ordBuilder.Properties.Add(ordEmpId);
            ordBuilder.Properties.Add(ordEmp);

            assemblyBuilder.Create();

            var assembly = assemblyBuilder.AssemblyBuilder;

            Assert.AreEqual("Employee", assembly.GetType("Employee").Name);
            Assert.AreEqual("Orders", assembly.GetType("Orders").Name);

            using (var db = new DbContext())
            {
                var entityType = assembly.GetType("Employee");
                var parExp = Expression.Parameter(entityType, "s");
                var memExp = Expression.MakeMemberAccess(parExp, entityType.GetProperty("Sex"));
                var valExp = Expression.Constant(Sex.M);
                var binExp = Expression.Equal(memExp, valExp);
                var lambdaExp = Expression.Lambda(binExp, parExp);

                var enumerable = (IEnumerable)db.Set(entityType).Where(lambdaExp).Select("Sex", "Name");
                foreach (dynamic item in enumerable)
                {
                    Console.WriteLine(item.Name);
                }
            }
        }
    }

    public enum Sex
    {
        M,
        F
    }
}
