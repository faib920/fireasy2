using Fireasy.Common.Dynamic;
using Fireasy.Common.Extensions;
using Fireasy.Common.Mapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Fireasy.Common.Extensions.GenericExtension;

namespace Fireasy.Common.Tests.Extensions
{
    [TestClass]
    public class ExtensionsTest
    {
        #region General
        [TestMethod]
        public void TestIsNullOrEmpty()
        {
            var list = new List<string>();
            object obj = null;
            int? i = null;

            Assert.IsTrue(list.IsNullOrEmpty());
            Assert.IsTrue(obj.IsNullOrEmpty());
            Assert.IsTrue(i.IsNullOrEmpty());
        }

        [TestMethod]
        public void TestAssertNotNull()
        {
            DateTime? date1 = null;
            DateTime? date2 = DateTime.Parse("2009-1-1");
            Assert.IsNull(date1.AssertNotNull(s => s.ToShortDateString()));
            Assert.AreEqual("2009-01-01", date2.AssertNotNull(s => s.ToString("yyyy-MM-dd")));
        }
        public interface IInvokable
        {
            void Invoke();
        }

        public class GenericInvoker : IInvokable
        {
            public void Invoke()
            {
                Console.WriteLine("hello world");
            }
        }

        public class NoGenericInvoker
        {
        }

        [TestMethod]
        public void TestAs()
        {
            var obj1 = new GenericInvoker();
            obj1.As<IInvokable>(s => s.Invoke());

            var obj2 = new NoGenericInvoker();
            obj2.As<IInvokable>(s => s.Invoke(), () => Console.WriteLine("obj2不是 IInvokable 类型"));
        }

        public class GenericData
        {
            public override string ToString()
            {
                return string.Empty;
            }
        }

        [TestMethod]
        public void TestToString()
        {
            GenericData data = null;

            //使用ToString()将抛出异常
            Assert.AreEqual(string.Empty, data.ToStringSafely());
        }

        [TestMethod]
        public void TestTo()
        {
            Assert.AreEqual(true, "true".To<bool>());
            Assert.AreEqual(false, "0".To<bool>());
            Assert.AreEqual(2332.4m, "2332.4".To<decimal>());
            Assert.AreEqual(34, "34d".To<int>(34));
        }

        public interface IPeople
        {
            string Name { get; set; }
        }

        [TestMethod]
        public void TestObjectTo()
        {
            var source = new List<object>();
            source.Add(new { Name = "huangxd" });
            source.Add(new { Name = "liming" });

            var dest = source.To<List<IPeople>>();
            Assert.AreEqual(2, dest.Count);
            Assert.AreEqual("huangxd", dest[0].Name);
        }

        [TestMethod]
        public void TestMapTo()
        {
            var mapper = new ConvertMapper<Data1, Data2>()
                .Map(s => s.Description, s => s.Name + " test")
                .Map(s => s.Other, s => "other");

            var data1 = new Data1 { Name = "fireasy" };
            var data2 = data1.To(mapper);
            Assert.AreEqual("fireasy test", data2.Description);
            Assert.AreEqual("other", data2.Other);
        }

        [TestMethod]
        public void TestExtend()
        {
            object obj = new { Name = "fireasy", Sex = 0 };
            dynamic obj1 = obj.Extend(new { Address = "kunming" });

            Assert.AreEqual("fireasy", obj1.Name);
            Assert.AreEqual("kunming", obj1.Address);
        }

        public class Data1
        {
            public string Name { get; set; }
        }
        public class Data2
        {
            public string Sex { get; set; }

            public int Age { get; set; }

            public string Description { get; set; }

            public string Other { get; set; }
        }

        [TestMethod]
        public void TestExtendAs()
        {
            var d1 = new Data1 { Name = "fireasy" };
            var d2 = d1.ExtendAs<Data2>(new { Age = 12 });
            var d3 = d2.ExtendAs<Data2>(new { Sex = "男" });
            Assert.AreEqual(12, d3.Age);
            Assert.AreEqual("男", d3.Sex);
        }

        [TestMethod]
        public void TestToDynamic()
        {
            dynamic obj = new { Name = "fireasy" }.ToDynamic();
            var dic = obj as IDictionary<string, object>;
            Assert.AreEqual("fireasy", obj.Name);
            Assert.AreEqual("fireasy", dic["Name"]);
        }

        [TestMethod]
        public void TestFromDynamic()
        {
            dynamic d = new DynamicExpandoObject();
            d.Name = "fireasy";

            var obj = (Data1)GenericExtension.To<Data1>(d);
            Assert.AreEqual("fireasy", obj.Name);
        }
        #endregion

        #region String
        [TestMethod]
        public void TestLeftRight()
        {
            Assert.AreEqual("放开", "放开那女孩".Left(2));
            Assert.AreEqual("女孩", "放开那女孩".Right(2));
        }

        [TestMethod]
        public void TestGetAnsiLength()
        {
            Assert.AreEqual(10, "放开那女孩".GetAnsiLength());
            Assert.AreEqual(20, "放开那女孩，good luck".GetAnsiLength());
            Assert.AreEqual(5, "放开那女孩".Length);
        }

        [TestMethod]
        public void TestGeLines()
        {
            var lines = @"中国
人".GetLines();
            Assert.AreEqual(2, lines);
        }

        [TestMethod]
        public void TestIsChinese()
        {
            Assert.IsTrue('中'.IsChinese());
            Assert.IsFalse('A'.IsChinese());
        }

        [TestMethod]
        public void TestGetAsciiCode()
        {
            Assert.AreEqual(-6984, '中'.GetAsciiCode());
            Assert.AreEqual(65, 'A'.GetAsciiCode());
        }


        [TestMethod]
        public void TestToHex()
        {
            var bytes = Encoding.UTF8.GetBytes("帆易动力");
            Assert.AreEqual("E5B886E69893E58AA8E58A9B", bytes.ToHex());
        }

        [TestMethod]
        public void TestFromHex()
        {
            var hex = "E5B886E69893E58AA8E58A9B";
            var bytes = hex.FromHex();
            Assert.AreEqual("帆易动力", Encoding.UTF8.GetString(bytes));
        }
        #endregion

        #region Math
        [TestMethod]
        public void TestRound()
        {
            Assert.AreEqual(855.58, 855.573.Round(2, RoundType.TowThree));
            Assert.AreEqual(855.57, 855.572.Round(2, RoundType.TowThree));
            Assert.AreEqual(855.58, 855.577.Round(2, RoundType.SixSeven));
            Assert.AreEqual(855.57, 855.576.Round(2, RoundType.SixSeven));
        }

        [TestMethod]
        public void TestVariance()
        {
            var data = new[] { 45, 55, 77, 28, 44, 60 };
            Assert.AreEqual(37.2223051408695, data.Variance());
            Assert.AreEqual(37.2223051408695, data.Variance((t, v) => t + v * 2));
        }

        [TestMethod]
        public void TestMedian()
        {
            var data = new[] { 45, 55, 77, 28, 44, 600 };
            Assert.AreEqual(141.5, data.Average()); //平均数
            Assert.AreEqual(50, data.Median()); //中位数
        }
        #endregion

        [TestMethod]
        public void TestDate()
        {
            var date = new DateTime(2009, 12, 22, 13, 22, 34);
            Console.WriteLine(date.ToTimeStamp());
            Console.WriteLine(1261459354L.ToDateTime());
        }
    }
}
