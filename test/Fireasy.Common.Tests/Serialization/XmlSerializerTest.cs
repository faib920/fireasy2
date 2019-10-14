using Fireasy.Common.ComponentModel;
using Fireasy.Common.Extensions;
using Fireasy.Common.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Fireasy.Common.Tests.Serialization
{
    [TestClass]
    public class XmlSerializerTest
    {
        private DateTime DEFAULT_DATE = new DateTime(2012, 6, 7, 12, 45, 33);

        /// <summary>
        /// 测试构造器。
        /// </summary>
        [TestMethod()]
        public void Test()
        {
            var serializer = new XmlSerializer();

            Assert.IsNotNull(serializer);
        }

        /// <summary>
        /// 使用整型测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeInt()
        {
            var xxx = new System.Xml.Serialization.XmlSerializer(typeof(int));
            using (var str = new StringWriter())
            {
                xxx.Serialize(str, 11);
                Console.WriteLine(str.ToString());
            }

            var serializer = new XmlSerializer();

            var xml = serializer.Serialize(89);

            //Assert.AreEqual("89", xml);
            Console.WriteLine(xml);
        }

        /// <summary>
        /// 使用整型测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestNullArray()
        {
            var serializer = new XmlSerializer();

            object footer = null;
            var f = footer is IEnumerable ? footer : new[] { footer };
            var obj = new { total = 100, rows = new List<int> { 22 }, footer = f };
            var xml = serializer.Serialize(obj);

            Assert.AreEqual("89", xml);
            Console.WriteLine(xml);
        }

        /// <summary>
        /// 使用字符串测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeString()
        {
            var xxx = new System.Xml.Serialization.XmlSerializer(typeof(string));
            using (var str = new StringWriter())
            {
                xxx.Serialize(str, "fireasy");
                Console.WriteLine(str.ToString());
            }

            var serializer = new XmlSerializer();

            var xml = serializer.Serialize("fireasy");

            Console.WriteLine(xml);
        }

        /// <summary>
        /// 使用字符串测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeStringWithQuote()
        {
            var serializer = new XmlSerializer();

            var xml = serializer.Serialize("<span style=\"background-color:#ffffff\"></span>");

            Assert.AreEqual("\"<span style=\\\"background-color:#ffffff\\\"></span>\"", xml);
            Console.WriteLine(xml);
        }

        /// <summary>
        /// 使用字符串测试Deserialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializePath()
        {
            var serializer = new XmlSerializer();

            var s1 = "path/a.gif";
            var str = serializer.Serialize(s1);
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(s1));
            Console.WriteLine(str);
            s1 = "c:\\path\\a.gif";
            str = serializer.Serialize(s1);
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(s1));
            Console.WriteLine(str);
        }

        /// <summary>
        /// 使用字符串测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeStringWithEnter()
        {
            var serializer = new XmlSerializer();

            var xml = serializer.Serialize(@"fireasy
studio");

            Assert.AreEqual("\"fireasy\\r\\nstudio\"", xml);
            Console.WriteLine(xml);
        }

        /// <summary>
        /// 使用字符串测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeStringWithEnter1()
        {
            var serializer = new XmlSerializer();

            var xml = serializer.Serialize(@"yunnan\honghe");

            Assert.AreEqual(@"""yunnan\\honghe""", xml);
            Console.WriteLine(xml);
        }


        /// <summary>
        /// 使用字符串测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeStringEmpty()
        {
            var serializer = new XmlSerializer();

            var xml = serializer.Serialize<string>("");

            Assert.AreEqual(xml, "\"\"");
            Console.WriteLine(xml);
        }

        /// <summary>
        /// 使用字符串测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeStringNull()
        {
            var serializer = new XmlSerializer();

            var xml = serializer.Serialize<string>(null);

            Assert.AreEqual(xml, "null");
            Console.WriteLine(xml);
        }

        /// <summary>
        /// 使用Html字符串测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeHtmlString()
        {
            var serializer = new XmlSerializer();

            var xml = serializer.Serialize("<a>中国</a>");

            Assert.AreEqual(xml, "\"<a>中国</a>\"");
        }

        /// <summary>
        /// 使用Html字符串和ConverterS测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeHtmlStringWithConverter()
        {
            var option = new XmlSerializeOption { Converters = new ConverterList { new UnicodeStringJsonConverter() } };
            var serializer = new XmlSerializer();

            var xml = serializer.Serialize("<a>中国</a>");

            Assert.AreEqual(xml, "\"\u003ca\u003e中国\u003c/a\u003e\"");
        }

        /// <summary>
        /// 使用布尔测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeBoolean()
        {
            var xxx = new System.Xml.Serialization.XmlSerializer(typeof(bool));
            using (var str = new StringWriter())
            {
                xxx.Serialize(str, true);
                Console.WriteLine(str.ToString());
            }

            var serializer = new XmlSerializer();

            var xml = serializer.Serialize(false);

            Console.WriteLine(xml);
        }

        /// <summary>
        /// 使用日期测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeDateTime()
        {
            var xxx = new System.Xml.Serialization.XmlSerializer(typeof(DateTime));
            using (var str = new StringWriter())
            {
                xxx.Serialize(str, DEFAULT_DATE);
                Console.WriteLine(str.ToString());
            }


            var serializer = new XmlSerializer();

            var xml = serializer.Serialize(DEFAULT_DATE);

            Console.WriteLine(xml);
        }

        /// <summary>
        /// 使用日期及Object格式测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeDateTimeToObject()
        {
            var option = new XmlSerializeOption();
            var serializer = new XmlSerializer(option);

            var xml = serializer.Serialize(DEFAULT_DATE);

            Console.WriteLine(xml);
        }

        /// <summary>
        /// 使用日期及Converters测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeDateTimeWithConverter()
        {
            var option = new XmlSerializeOption();
            var serializer = new XmlSerializer(option);
            var obj = new JsonData
            {
                Age = 12,
                Name = "huangxd",
                Birthday = DateTime.Parse("1981-1-10 12:33:55"),
                Record = new WorkRecord { Company = "aa" },
                WorkRecords = new List<WorkRecord> { new WorkRecord { Company = "bb" } }
            };

            var xml = serializer.Serialize(obj);
            Console.WriteLine(xml);

            var xxx = new System.Xml.Serialization.XmlSerializer(typeof(JsonData));
            using (var str = new StringWriter())
            {
                xxx.Serialize(str, obj);
                Console.WriteLine(str.ToString());
            }
        }

        /// <summary>
        /// 使用object测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeObject()
        {
            var serializer = new XmlSerializer();

            var obj = new JsonData
            {
                Age = 12,
                Name = "huangxd",
                Birthday = DateTime.Parse("1982-9-20")
            };

            var xml = serializer.Serialize(obj);

            Console.WriteLine(xml);
        }

        /// <summary>
        /// 使用object及Camel命名选项测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeObjectWithCamel()
        {
            var option = new XmlSerializeOption { CamelNaming = true };
            var serializer = new XmlSerializer(option);

            var obj = new JsonData
            {
                Age = 12,
                Name = "huangxd",
                Birthday = DateTime.Parse("1982-9-20")
            };

            var xml = serializer.Serialize(obj);

            Console.WriteLine(xml);
        }

        /// <summary>
        /// 使用包含选项测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeObjectWithInclusiveNames()
        {
            var option = new XmlSerializeOption { InclusiveNames = new string[] { "Name" } };
            var serializer = new XmlSerializer(option);

            var obj = new JsonData
            {
                Age = 12,
                Name = "huangxd",
                Birthday = DateTime.Parse("1982-9-20")
            };

            var xml = serializer.Serialize(obj);

            Console.WriteLine(xml);
        }

        /// <summary>
        /// 使用排除选项测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeObjectWithExclusiveNames()
        {
            var option = new XmlSerializeOption { ExclusiveNames = new string[] { "Birthday" } };
            var serializer = new XmlSerializer(option);

            var obj = new JsonData
            {
                Age = 12,
                Name = "huangxd",
                Birthday = DateTime.Parse("1982-9-20")
            };

            var xml = serializer.Serialize(obj);

            Console.WriteLine(xml);
        }

        /// <summary>
        /// 使用排除选项测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeObjectWithExclusiveMembers()
        {
            var option = new XmlSerializeOption();
            option.Exclude<JsonData>(s => s.Birthday, s => s.Record.StartDate).Include<WorkRecord>(s => s.Company);
            var serializer = new XmlSerializer(option);

            var obj = new JsonData
            {
                Age = 12,
                Name = "huangxd",
                Birthday = DateTime.Parse("1982-9-20"),
                Record = new WorkRecord { Company = "fireasy", StartDate = DateTime.Now, EndDate = DateTime.Now }
            };

            var xml = serializer.Serialize(obj);

            Console.WriteLine(xml);
        }

        /// <summary>
        /// 使用复杂对象测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeObjectDetail()
        {
            var serializer = new XmlSerializer();

            var obj = new JsonData
            {
                Age = 12,
                Name = "huangxd",
                Birthday = DateTime.Parse("1982-9-20"),
                WorkRecords = new List<WorkRecord>
                        {
                            new WorkRecord {  Company = "company1", StartDate = DateTime.Parse("2009-3-4"), EndDate = DateTime.Parse("2012-3-4") },
                            new WorkRecord {  Company = "company2", StartDate = DateTime.Parse("2012-4-14") }
                        }
            };

            var xml = serializer.Serialize(obj);

            Console.WriteLine(xml);
        }

        /// <summary>
        /// 使用TextSerializeElement特性测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeObjectWithElement()
        {
            var serializer = new XmlSerializer();

            var obj = new ElementData
            {
                Name = "huangxd"
            };

            var xml = serializer.Serialize(obj);
            Console.WriteLine(xml);
        }

        /// <summary>
        /// 使用LazyManager特性测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeLazyManager()
        {
            var serializer = new XmlSerializer();

            var obj = new LazyLoadData { E1 = new ElementData { Name = "X1" }, E2 = new ElementData { Name = "X2" } };

            var xml = serializer.Serialize(obj);

            Console.WriteLine(xml);
        }

        /// <summary>
        /// 使用Type测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeTypeIgnore()
        {
            var serializer = new XmlSerializer();

            var obj = new JsonData
            {
                DataType = typeof(WorkRecord)
            };

            var xml = serializer.Serialize(obj);

            Console.WriteLine(xml);
        }

        [TestMethod]
        public void TestSerializeIgnoreNull()
        {
            var option = new XmlSerializeOption { IgnoreNull = true };
            var serializer = new XmlSerializer(option);

            var obj = new JsonData
            {
                Age = 12,
                Name = "huangxd",
                Birthday = DateTime.Parse("1982-9-20")
            };

            var xml = serializer.Serialize(obj);
            Console.WriteLine(xml);
        }

        [TestMethod]
        public void TestSerializeWithIndent()
        {
            var option = new XmlSerializeOption { Indent = true };
            var serializer = new XmlSerializer(option);

            var obj = new JsonData
            {
                Age = 12,
                Name = "huangxd",
                Birthday = DateTime.Parse("1982-9-20"),
                WorkRecords = new List<WorkRecord>
                        {
                            new WorkRecord {  Company = "company1", StartDate = DateTime.Parse("2009-3-4"), EndDate = DateTime.Parse("2012-3-4") },
                            new WorkRecord {  Company = "company2", StartDate = DateTime.Parse("2012-4-14") }
                        }
            };

            var xml = serializer.Serialize(obj);
            Console.WriteLine(xml);
        }

        /// <summary>
        /// 使用Type测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeType()
        {
            var serializer = new XmlSerializer(new XmlSerializeOption { IgnoreType = false });

            var obj = new JsonData
            {
                DataType = typeof(WorkRecord)
            };

            var xml = serializer.Serialize(obj);

            Console.WriteLine(xml);
        }

        /// <summary>
        /// 使用列表测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeList()
        {
            var list = new List<JsonData>();
            for (var i = 0; i < 10; i++)
            {
                list.Add(new JsonData
                {
                    Age = 12,
                    Name = "huangxd",
                    Birthday = DateTime.Parse("1982-9-20")
                });
            }

            Console.WriteLine(new XmlSerializer().Serialize(list));

            var xxx = new System.Xml.Serialization.XmlSerializer(typeof(List<JsonData>));
            using (var str = new StringWriter())
            {
                xxx.Serialize(str, list);
                Console.WriteLine(str.ToString());
            }
        }

        /// <summary>
        /// 使用字典测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeDictonary()
        {
            var serializer = new XmlSerializer();

            var dictionary = new Dictionary<string, JsonData>();

            for (var i = 0; i < 10; i++)
            {
                dictionary.Add("h" + i, new JsonData
                {
                    Age = 12,
                    Name = "huangxd",
                    Birthday = DateTime.Parse("1982-9-20")
                });
            }

            Console.WriteLine(serializer.Serialize(dictionary));
        }

        /// <summary>
        /// 使用数据集测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeDataSet()
        {
            var serializer = new XmlSerializer();

            var ds = new DataSet();

            var tb = new DataTable("table1");
            tb.Columns.Add("Name", typeof(string));
            tb.Columns.Add("Age", typeof(int));
            tb.Columns.Add("Birthday", typeof(DateTime));

            tb.Rows.Add("huangxd", 12, DateTime.Parse("1982-9-20"));
            tb.Rows.Add("liping", 22, DateTime.Parse("1972-9-20"));

            ds.Tables.Add(tb);

            var xml = serializer.Serialize(ds);

            Console.WriteLine(xml);
        }

        /// <summary>
        /// 使用数据表测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeDataTable()
        {
            var serializer = new XmlSerializer();

            var tb = new DataTable("table1");
            tb.Columns.Add("Name", typeof(string));
            tb.Columns.Add("Age", typeof(int));
            tb.Columns.Add("Birthday", typeof(DateTime));

            for (var i = 0; i < 10; i++)
            {
                tb.Rows.Add("huangxd", 12, DateTime.Parse("1982-9-20"));
            }

            Console.WriteLine("使用Fireasy序列化100000行的DataTable");

            var time1 = TimeWatcher.Watch(() =>
            {
                var xml = serializer.Serialize(tb);
                Console.WriteLine("长度: " + xml);
            });

            Console.WriteLine("耗时: {0} 毫秒", time1.Milliseconds);
        }

        /// <summary>
        /// 使用数组测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeArray()
        {
            var serializer = new XmlSerializer();

            var array = new JsonData[]
                {
                    new JsonData
                        {
                            Age = 12,
                            Name = "huangxd",
                            Birthday = DateTime.Parse("1982-9-20")
                        },
                    new JsonData
                        {
                            Age = 22,
                            Name = "liping",
                            Birthday = DateTime.Parse("1972-5-10")
                        }
                };

            var xml = serializer.Serialize(array);

            Console.WriteLine(xml);


            var xxx = new System.Xml.Serialization.XmlSerializer(typeof(JsonData[]));
            using (var str = new StringWriter())
            {
                xxx.Serialize(str, array);
                Console.WriteLine(str.ToString());
            }
        }

        [TestMethod]
        public void TestSerializeArrayList()
        {
            var array = new ArrayList();

            array.Add("test");
            array.Add(1222);
            array.Add(true);

            var serializer = new XmlSerializer();
            var xml = serializer.Serialize(array);

            Console.WriteLine(xml);


            var xxx = new System.Xml.Serialization.XmlSerializer(typeof(ArrayList));
            using (var str = new StringWriter())
            {
                xxx.Serialize(str, array);
                Console.WriteLine(str.ToString());
            }
        }

        [TestMethod()]
        public void TestSerializeColor()
        {
            var serializer = new XmlSerializer();

            var xml = serializer.Serialize(Color.Red);

            Console.WriteLine(xml);
        }

        [TestMethod()]
        public void TestSerializeExpression()
        {
            var age = 44;
            var l = new[] { 34m, 55m, 66m };
            var aaa = new { DD = new { Age = 34 } };
            Expression<Func<JsonData, bool>> expression = (s) => s.Age == age ||
                s.Age == aaa.DD.Age && s.Age == new { Age = 34 }.Age &&
                s.Name.Substring(1, 2) == "12" ||
                l.Contains((decimal)s.Age) ||
                s.Name == new JsonData("12") { Age = 12 }.Name;

            var option = new XmlSerializeOption();
            option.Converters.Add(new ExpressionJsonConverter());
            var serializer = new XmlSerializer(option);

            Console.WriteLine("使用Fireasy序列化表达式");
            var time = TimeWatcher.Watch(() =>
            {
                var xml = serializer.Serialize(expression);
                Console.WriteLine(xml);
            });

            Console.WriteLine("耗时: {0} 毫秒", time.Milliseconds);
        }

        [TestMethod()]
        public void TestSerializeExpandoObject()
        {
            dynamic obj = new ExpandoObject();
            obj.AA = new LazyClass { Address = "kunming" };
            obj.Name = "aaaa";
            obj.IsOld = true;
            obj.Age = 12.5;
            obj.Items = new[] { 34, 55, 66 };

            var dd = new { A = 43434 };

            var serializer = new XmlSerializer(new XmlSerializeOption { NodeStyle = XmlNodeStyle.Attribute });

            var newo = GenericExtension.Extend(obj, dd);
            var xml = serializer.Serialize(newo);
            Console.WriteLine(xml);
        }

        [TestMethod]
        public void TestSerializeLazyObject()
        {
            var obj = new LazyClass { Name = "fireasy", AA = new CycleClass { B = "Dfaf" }, Address = "kunming", Age = 30 };
            var serializer = new XmlSerializer(new XmlSerializeOption { NodeStyle = XmlNodeStyle.Attribute });
            Console.WriteLine(serializer.Serialize(obj));
        }

        [TestMethod]
        public void TestSerializeCycle()
        {
            try
            {
                var obj = new CycleClass();
                obj.Parent = obj;
                var serializer = new XmlSerializer();
                Console.WriteLine(serializer.Serialize(obj));
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        [TestMethod]
        public void TestSerializeWithCompositeConverter()
        {
            var data = new JsonData
            {
                Age = 12,
                Name = "huangxd",
                Birthday = DateTime.Parse("1982-9-20"),
                WorkTime = DateTime.Parse("2000-9-20"),
                Record = new WorkRecord { StartDate = DateTime.Now, Company = "dd" }
            };

            var converter = new CompositeJsonConverter<WorkRecord>();
            converter.AddConverter(s => s.StartDate, new DateTimeJsonConverter("yyyy-MM"));
            var option = new XmlSerializeOption() { Indent = true };
            option.Converters.Add(converter);
            var serializer = new XmlSerializer(option);

            var xml = serializer.Serialize(data);

            Console.WriteLine(xml);

        }

        [TestMethod]
        public void TestSerializeTuple()
        {
            var tuple = new Tuple<int, int, string>(23, 44, "fireasy");
            var serializer = new XmlSerializer(null);

            var xml = serializer.Serialize(tuple);
            Console.WriteLine(xml);
        }

        [TestMethod]
        public void TestSerializeTuple1()
        {
            var tuple = new Tuple<int, int, string, Tuple<int, int>>(23, 44, "fireasy", new Tuple<int, int>(55, 45));
            var serializer = new XmlSerializer(null);

            var xml = serializer.Serialize(tuple);
            Console.WriteLine(xml);
        }

        [TestMethod]
        public void TestSerializeWithConverterAttr()
        {
            var attr = new AttrTest { Name = "fireasy" };
            var serializer = new XmlSerializer(null);

            var xml = serializer.Serialize(attr);
            Console.WriteLine(xml);
        }

        public class LazyClass : ILazyManager
        {
            public CycleClass AA { get; set; }

            public string Name { get; set; }

            public string Address { get; set; }

            public int Age { get; set; }

            public bool IsValueCreated(string propertyName)
            {
                switch (propertyName)
                {
                    case "Name":
                    case "Address":
                    case "AA":
                        return true;
                    case "Age":
                        return true;
                    default:
                        return false;
                }
            }
        }

        public class CycleClass
        {
            public string B { get; set; }
            public CycleClass Parent { get; set; }
        }

        /// <summary>
        /// 测试Deserialize方法，返回整型。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeInt()
        {
            var serializer = new XmlSerializer();

            var i = serializer.Deserialize<int>("<?xml version=\"1.0\" encoding=\"utf-16\"?><int>89</int>");

            Assert.AreEqual(i, 89);
        }


        /// <summary>
        /// 使用字符串测试Deserialize方法。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeStringWithQuote()
        {
            var serializer = new XmlSerializer();

            var str = serializer.Deserialize<string>("\"<span style=\\\"background-color:#ffffff\\\">201/12/12</span>\"");
            Console.WriteLine(str);
            //Assert.AreEqual("<span style=\"background-color:#ffffff\"></span>", str);
        }

        /// <summary>
        /// 使用字符串测试Deserialize方法。
        /// </summary>
        [TestMethod()]
        public void TestDeserializePath()
        {
            var serializer = new XmlSerializer();

            var s1 = "\"e:\\\\doct\\\\test.doc\"";

            var str = serializer.Deserialize<string>(s1);
            Console.WriteLine(Newtonsoft.Json.JsonConvert.DeserializeObject<string>(s1));
            Console.WriteLine(str);
            s1 = "\"/doct/test.doc\"";
            str = serializer.Deserialize<string>(s1);
            Console.WriteLine(Newtonsoft.Json.JsonConvert.DeserializeObject<string>(s1));
            Console.WriteLine(str);
            s1 = "\"/doct/test.doc\"";
            str = serializer.Deserialize<string>(s1);
            Console.WriteLine(Newtonsoft.Json.JsonConvert.DeserializeObject<string>(s1));
            Console.WriteLine(str);
        }

        /// <summary>
        /// 使用字符串测试Deserialize方法。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeStringNull()
        {
            var serializer = new XmlSerializer();

            var str = serializer.Deserialize<string>("\"null\"");

            Assert.AreEqual(null, str);
        }

        /// <summary>
        /// 使用字符串测试Deserialize方法。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeStringUnicode()
        {
            var serializer = new XmlSerializer();

            var str = serializer.Deserialize<string>("\"\\user\"");

            Assert.AreEqual("\\user", str);

            str = serializer.Deserialize<string>("\"\\u4e2d\\u56fd\"");

            Assert.AreEqual("中国", str);
        }

        /// <summary>
        /// 测试Deserialize方法，返回布尔。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeBoolean()
        {
            var serializer = new XmlSerializer();

            var b = serializer.Deserialize<bool>(@"<?xml version=""1.0"" encoding=""utf-16""?>
<boolean>true</boolean>");

            Assert.AreEqual(b, true);
        }

        /// <summary>
        /// 测试Deserialize方法，返回日期。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeDateTime()
        {
            var serializer = new XmlSerializer();

            var d = serializer.Deserialize<DateTime>(@"<?xml version=""1.0"" encoding=""utf-16""?>
<dateTime>2012-6-7 12:45:33</dateTime>");

            Assert.AreEqual(d, new DateTime(2012, 6, 7, 12, 45, 33));
        }

        /// <summary>
        /// 测试Deserialize方法，返回日期。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeDateTimeNull()
        {
            var serializer = new XmlSerializer();

            var d1 = serializer.Deserialize<DateTime?>("\"\"");
            Assert.IsNull(d1);
            var d2 = serializer.Deserialize<DateTime?>("null");
            Assert.IsNull(d2);
            var d3 = serializer.Deserialize<DateTime?>("");
            Assert.IsNull(d3);
        }

        /// <summary>
        /// 测试Deserialize方法，返回日期。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeDateTimeWithConverter()
        {
            var option = new XmlSerializeOption { Converters = new ConverterList { new DateTimeJsonConverter("d-M-yyyy") } };
            var serializer = new XmlSerializer(option);

            var d = serializer.Deserialize<DateTime>("\"6-7-2012\"");

            Assert.AreEqual(d, new DateTime(2012, 7, 6));
        }

        /// <summary>
        /// 测试Deserialize方法，返回日期。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeDateTimeComplex()
        {
            var serializer = new XmlSerializer();

            var d = serializer.Deserialize<DateTime>(@"""\/Date(1339044333000+0800)\/""");

            Assert.AreEqual(d, new DateTime(2012, 6, 7, 12, 45, 33));
        }

        /// <summary>
        /// 测试Deserialize方法，返回Html文本。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeHtmlString()
        {
            var serializer = new XmlSerializer();

            var d = serializer.Deserialize<string>("\"\u003ca\u003e中国\u003c/a\u003e\"");

            Assert.AreEqual(d, "<a>中国</a>");
        }

        [TestMethod()]
        public void TestDeserializeEnum()
        {
            var serializer = new XmlSerializer();

            var obj = serializer.Deserialize<TestEnum?>("");

            Assert.AreEqual(obj, null);
        }

        /// <summary>
        /// 测试Deserialize方法，返回object。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeObject()
        {
            var serializer = new XmlSerializer();
            var xml = @"<JsonData>
 <Name>huangxd</Name>
 <Birthday>2019-1-1</Birthday>
 <Record>
   <Company>fireasy</Company>
 </Record>
 <WorkRecords>
  <Record>
    <Company>fireasy</Company>
  </Record>
  <Record>
    <Company>fireasy</Company>
  </Record>
 </WorkRecords>
</JsonData>
";

            var obj = serializer.Deserialize<JsonData>(xml);

            Assert.IsNotNull(obj);
        }

        /// <summary>
        /// 测试Deserialize方法，返回object。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeDynamicObject()
        {
            var option = new XmlSerializeOption();
            var serializer = new XmlSerializer(option);

            var obj = serializer.Deserialize<ComponentModel.Result<object>>(
@"<?xml version=""1.0"" encoding=""utf-16""?>
<Result>
  <succeed>true</succeed>
  <data>
    <Name>fireasy</Name>
    <Birthday>2018-12-1</Birthday>
    <Age>12</Age>
    <Record>
      <Company>fireasy</Company>
      <StartDate>2018-12-1</StartDate>
    </Record>
  </data>
</Result>");

            Assert.IsNotNull(obj);
            Console.WriteLine(serializer.Serialize(obj));
        }

        /// <summary>
        /// 测试Deserialize方法，返回object。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeObject2()
        {
            var serializer = new XmlSerializer();
            var xml = @"<JsonData Name=""fireasy"" Birthday=""2019-1-1"">
 <Record Company=""fireasy"" />
 <WorkRecords>
  <Record Company=""fireasy"" />
  <Record Company=""fireasy"" />
 </WorkRecords>
</JsonData>
";

            var obj = serializer.Deserialize<JsonData>(xml);

            Assert.IsNotNull(obj);
            Assert.AreEqual(obj.WorkRecords[0].Company, "fireasy");
        }

        /// <summary>
        /// 测试Deserialize方法，返回object。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeObjectByInterface()
        {
            var serializer = new XmlSerializer();

            var obj = serializer.Deserialize<IJsonData>(
                new JsonText(@"{'Name':'huangxd','Birthday':'\/Date(401299200000+0800)\/','Age':12,'WorkRecords':null}").ToString()
                );

            Assert.IsNotNull(obj);
            Console.WriteLine(obj.Name);
        }

        /// <summary>
        /// key不带引号测试Deserialize方法，返回object。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeObjectWithoutQuote()
        {
            var serializer = new XmlSerializer();

            var obj = serializer.Deserialize<JsonData>(
                new JsonText(@"{Name:'huangxd',Birthday:'\/Date(401299200000+0800)\/',Age:12,WorkRecords:null}").ToString()
                );

            Assert.IsNotNull(obj);
            Console.WriteLine(obj.Name);
        }

        /// <summary>
        /// 测试Deserialize方法，返回复杂的object。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeObjectDetail()
        {
            var serializer = new XmlSerializer();

            var obj = serializer.Deserialize<JsonData>(
                new JsonText(@"{'Name':'huangxd','Birthday':'\/Date(401299200000+0800)\/','Age':12,'WorkRecords':[{'Company':'company1','StartDate':'\/Date(401299200000+0800)\/','EndDate':'\/Date(401299200000+0800)\/'},{'Company':'company1','StartDate':'\/Date(401299200000+0800)\/','EndDate':null}]}").ToString()
                );

            Assert.IsNotNull(obj);
            Assert.AreEqual(2, obj.WorkRecords.Count);
        }

        /// <summary>
        /// 使用Camel选项测试Deserialize方法，返回object。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeObjectWithCamelNaming()
        {
            var option = new XmlSerializeOption { CamelNaming = true };
            var serializer = new XmlSerializer(option);

            var obj = serializer.Deserialize<JsonData>(
                new JsonText(@"{'name':'huangxd','birthday':'\/Date(401299200000+0800)\/','age':12,'workRecords':null}").ToString()
                );

            Assert.IsNotNull(obj);
            Console.WriteLine(obj.Name);
        }

        /// <summary>
        /// 使用匿名类型测试Deserialize方法，返回object。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeObjectWithAnonymousType()
        {
            var serializer = new XmlSerializer();

            var obj = serializer.Deserialize(
                new JsonText(@"{'Name':'huangxd','Birthday':'\/Date(401299200000+0800)\/','Age':12,'WorkRecords':null}").ToString(),
                new { Name = "", Birthday = DateTime.MinValue, Age = 0 }
                );

            Assert.IsNotNull(obj);
            Console.WriteLine(obj.Name);
        }

        /// <summary>
        /// 使用匿名类型，但是属性顺序不一致测试Deserialize方法，返回object。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeObjectWithAnonymousTypeDiscordant()
        {
            var serializer = new XmlSerializer();

            var obj = serializer.Deserialize(
                new JsonText(@"{'Name':'huangxd','n':null,'isdad':true,'bodDate':'\/Date(401299200000+0800)\/','Birthday':'\/Date(401299200000+0800)\/','Age':12,'WorkRecords':null}").ToString(),
                new { Name = "", Age = 0, Birthday = DateTime.MinValue }
                );

            Assert.IsNotNull(obj);
            Console.WriteLine(obj.Name);
        }

        /// <summary>
        /// 使用TextSerializeElement特性测试Deserialize方法。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeObjectWithElement()
        {
            var serializer = new XmlSerializer(new XmlSerializeOption { CamelNaming = true });

            var obj = serializer.Deserialize<ElementData>(
                "<ElementData><xm>huangxd</xm></ElementData>"
                );

            Assert.IsNotNull(obj);
            Assert.AreEqual(obj.Name, "huangxd");
        }

        [TestMethod()]
        public void TestDeserializeType()
        {
            var serializer = new XmlSerializer(new XmlSerializeOption { IgnoreType = false });
            var xml = new JsonText(@"{'Name':null,'WorkRecords':null,'DataType':'Fireasy.Common.Serialization.Test.XmlSerializerTests+WorkRecord, Fireasy.Common.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'}").ToString();
            var obj = serializer.Deserialize<JsonData>(xml);
            Assert.IsNotNull(obj);
            Console.WriteLine(obj.DataType);
        }

        [TestMethod()]
        public void TestDeserializeTypeIgnore()
        {
            var serializer = new XmlSerializer();
            var xml = new JsonText(@"{'Name':null,'WorkRecords':null,'DataType':'Fireasy.Common.Serialization.Test.XmlSerializerTests+WorkRecord, Fireasy.Common.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'}").ToString();
            var obj = serializer.Deserialize<JsonData>(xml);
            Assert.IsNotNull(obj);
            Console.WriteLine(obj.DataType);
        }

        /// <summary>
        /// 测试Deserialize方法，返回List。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeList()
        {
            var serializer = new XmlSerializer();

            var xml = @"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfJsonData>
  <JsonData>
    <Name>huangxd</Name>
    <Birthday>1982-09-20</Birthday>
    <WorkTime />
    <Age>12</Age>
  </JsonData>
  <JsonData>
    <Name>huangxd</Name>
    <Birthday>1982-09-20</Birthday>
    <WorkTime />
    <Age>12</Age>
  </JsonData>
  <JsonData>
    <Name>huangxd</Name>
    <Birthday>1982-09-20</Birthday>
    <WorkTime />
    <Age>12</Age>
  </JsonData>
  <JsonData>
    <Name>huangxd</Name>
    <Birthday>1982-09-20</Birthday>
    <WorkTime />
    <Age>12</Age>
  </JsonData>
  <JsonData>
    <Name>huangxd</Name>
    <Birthday>1982-09-20</Birthday>
    <WorkTime />
    <Age>12</Age>
  </JsonData>
  <JsonData>
    <Name>huangxd</Name>
    <Birthday>1982-09-20</Birthday>
    <WorkTime />
    <Age>12</Age>
  </JsonData>
  <JsonData>
    <Name>huangxd</Name>
    <Birthday>1982-09-20</Birthday>
    <WorkTime />
    <Age>12</Age>
  </JsonData>
  <JsonData>
    <Name>huangxd</Name>
    <Birthday>1982-09-20</Birthday>
    <WorkTime />
    <Age>12</Age>
  </JsonData>
  <JsonData>
    <Name>huangxd</Name>
    <Birthday>1982-09-20</Birthday>
    <WorkTime />
    <Age>12</Age>
  </JsonData>
  <JsonData>
    <Name>lilei</Name>
    <Birthday>1982-09-20</Birthday>
    <WorkTime />
    <Age />
  </JsonData>
</ArrayOfJsonData>";

            var list = serializer.Deserialize<List<JsonData>>(xml);
            Assert.AreEqual(list.Count, 10);
            Assert.AreEqual(list[9].Name, "lilei");
            Assert.AreEqual(list[9].Age, null);
        }

        /// <summary>
        /// 测试Deserialize方法，返回数组。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeArray()
        {
            var serializer = new XmlSerializer();

            var xml = @"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfJsonData>
  <JsonData>
    <Name>huangxd</Name>
    <Birthday>1982-09-20</Birthday>
    <WorkTime />
    <Age>12</Age>
  </JsonData>
  <JsonData>
    <Name>huangxd</Name>
    <Birthday>1982-09-20</Birthday>
    <WorkTime />
    <Age>12</Age>
  </JsonData>
  <JsonData>
    <Name>huangxd</Name>
    <Birthday>1982-09-20</Birthday>
    <WorkTime />
    <Age>12</Age>
  </JsonData>
  <JsonData>
    <Name>huangxd</Name>
    <Birthday>1982-09-20</Birthday>
    <WorkTime />
    <Age>12</Age>
  </JsonData>
  <JsonData>
    <Name>huangxd</Name>
    <Birthday>1982-09-20</Birthday>
    <WorkTime />
    <Age>12</Age>
  </JsonData>
  <JsonData>
    <Name>huangxd</Name>
    <Birthday>1982-09-20</Birthday>
    <WorkTime />
    <Age>12</Age>
  </JsonData>
  <JsonData>
    <Name>huangxd</Name>
    <Birthday>1982-09-20</Birthday>
    <WorkTime />
    <Age>12</Age>
  </JsonData>
  <JsonData>
    <Name>huangxd</Name>
    <Birthday>1982-09-20</Birthday>
    <WorkTime />
    <Age>12</Age>
  </JsonData>
  <JsonData>
    <Name>huangxd</Name>
    <Birthday>1982-09-20</Birthday>
    <WorkTime />
    <Age>12</Age>
  </JsonData>
  <JsonData>
    <Name>lilei</Name>
    <Birthday>1982-09-20</Birthday>
    <WorkTime />
    <Age />
    <Record><Company><![CDATA[ 
fireasy
total co.ltd
]]></Company></Record>
  </JsonData>
</ArrayOfJsonData>";

            var array = serializer.Deserialize<JsonData[]>(xml);

            Assert.IsNotNull(array);
            Assert.AreEqual(10, array.Length);
        }

        /// <summary>
        /// 测试Deserialize方法，返回数组。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeDynamicArray()
        {
            var serializer = new XmlSerializer();

            var xml = new JsonText(@"[12,33,44.55,'55']").ToString();

            var array = serializer.Deserialize<dynamic>(xml);

            Assert.IsNotNull(array);
            Assert.AreEqual(4, array.Count);
        }

        /// <summary>
        /// 测试Deserialize方法，返回ArrayList。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeArrayList()
        {
            var serializer = new XmlSerializer();

            var xml = @"<Array><string>a</string><string>b</string></Array>";

            var array = serializer.Deserialize<ArrayList>(xml);

            Assert.IsNotNull(array);
            Assert.AreEqual(2, array.Count);
        }

        /// <summary>
        /// 测试Deserialize方法，返回字典。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeDictionary()
        {
            var serializer = new XmlSerializer();

            var xml = @"<Dictionary>
  <h0>
    <Name>huangxd</Name>
    <Birthday>1982-09-20</Birthday>
    <WorkTime />
    <Age>12</Age>
  </h0>
  <h1>
    <Name>huangxd</Name>
    <Birthday>1982-09-20</Birthday>
    <WorkTime />
    <Age>12</Age>
  </h1>
  <h2>
    <Name>huangxd</Name>
    <Birthday>1982-09-20</Birthday>
    <WorkTime />
    <Age>12</Age>
  </h2>
  <h3>
    <Name>huangxd</Name>
    <Birthday>1982-09-20</Birthday>
    <WorkTime />
    <Age>12</Age>
  </h3>
  <h4>
    <Name>huangxd</Name>
    <Birthday>1982-09-20</Birthday>
    <WorkTime />
    <Age>12</Age>
  </h4>
  <h5>
    <Name>huangxd</Name>
    <Birthday>1982-09-20</Birthday>
    <WorkTime />
    <Age>12</Age>
  </h5>
  <h6>
    <Name>huangxd</Name>
    <Birthday>1982-09-20</Birthday>
    <WorkTime />
    <Age>12</Age>
  </h6>
  <h7>
    <Name>huangxd</Name>
    <Birthday>1982-09-20</Birthday>
    <WorkTime />
    <Age>12</Age>
  </h7>
  <h8>
    <Name>huangxd</Name>
    <Birthday>1982-09-20</Birthday>
    <WorkTime />
    <Age>12</Age>
  </h8>
  <h9>
    <Name>huangxd</Name>
    <Birthday>1982-09-20</Birthday>
    <WorkTime />
    <Age>12</Age>
  </h9>
</Dictionary>
";

            var dictionary = serializer.Deserialize<Dictionary<string, JsonData>>(xml);

            Assert.IsNotNull(dictionary);
            Assert.AreEqual(10, dictionary.Count);
        }

        /// <summary>
        /// 测试Deserialize方法，返回数据表。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeDataTable()
        {
            var serializer = new XmlSerializer();

            var xml = new JsonText(@"[{'Name':null,'Birthday':'\/Date(401299200000+0800)\/','Age':12},{'Name':'liping','Age':22,'Birthday':'\/Date(401299200000+0800)\/'}]").ToString();

            var table = serializer.Deserialize<DataTable>(xml);

            Assert.IsNotNull(table);
            Assert.AreEqual(2, table.Rows.Count);
        }

        /// <summary>
        /// 测试Deserialize方法，返回数据集。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeDataSet()
        {
            var serializer = new XmlSerializer();

            var xml = new JsonText(@"{'table1':[{'Name':'huangxd','Birthday':'\/Date(401299200000+0800)\/','Age':12},{'Name':'liping','Age':22,'Birthday':'\/Date(401299200000+0800)\/'}]}").ToString();

            var ds = serializer.Deserialize<DataSet>(xml);

            Assert.IsNotNull(ds);
            Assert.AreEqual(1, ds.Tables.Count);
        }

        [TestMethod()]
        public void TestDeserializeExpression()
        {
            var option = new XmlSerializeOption();
            option.Converters.Add(new ExpressionJsonConverter());
            var serializer = new XmlSerializer(option);

            var xml = new JsonText(@"{Lambda:{Parameters:[{Parameter:{Name:'s',Type:'Fireasy.Common.Tests.Serialization.XmlSerializerTest+JsonData, Fireasy.Common.Tests'}}],Body:{OrElse:{Left:{OrElse:{Left:{OrElse:{Left:{Equal:{Left:{MemberAccess:{Type:'Fireasy.Common.Tests.Serialization.XmlSerializerTest+JsonData, Fireasy.Common.Tests',Member:'Age',Expression:{Parameter:{Name:'s'}}}},Right:{Constant:{Type:'System.Nullable`1[[System.Decimal, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]',Value:'44'}}}},Right:{AndAlso:{Left:{AndAlso:{Left:{Equal:{Left:{MemberAccess:{Type:'Fireasy.Common.Tests.Serialization.XmlSerializerTest+JsonData, Fireasy.Common.Tests',Member:'Age',Expression:{Parameter:{Name:'s'}}}},Right:{Constant:{Type:'System.Nullable`1[[System.Decimal, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]',Value:'34'}}}},Right:{Equal:{Left:{MemberAccess:{Type:'Fireasy.Common.Tests.Serialization.XmlSerializerTest+JsonData, Fireasy.Common.Tests',Member:'Age',Expression:{Parameter:{Name:'s'}}}},Right:{Constant:{Type:'System.Nullable`1[[System.Decimal]]',Value:'34'}}}}}},Right:{Equal:{Left:{Call:{Type:'System.String',Method:'Substring',Object:{MemberAccess:{Type:'Fireasy.Common.Tests.Serialization.XmlSerializerTest+JsonData, Fireasy.Common.Tests',Member:'Name',Expression:{Parameter:{Name:'s'}}}},Arguments:[{Constant:{Type:'System.Int32',Value:'1'}},{Constant:{Type:'System.Int32',Value:'2'}}],GenericArgTypes:[],ParameterTypes:['System.Int32','System.Int32']}},Right:{Constant:{Type:'System.String',Value:'\'12\''}}}}}}}},Right:{Call:{Type:'System.Linq.Enumerable, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089',Method:'Contains',Object:null,Arguments:[{Constant:{Type:'System.Decimal[]',Value:'[34,55,66]'}},{Convert:{Type:'System.Decimal',Operand:{MemberAccess:{Type:'Fireasy.Common.Tests.Serialization.XmlSerializerTest+JsonData, Fireasy.Common.Tests',Member:'Age',Expression:{Parameter:{Name:'s'}}}}}}],GenericArgTypes:['System.Decimal'],ParameterTypes:['System.Collections.Generic.IEnumerable`1[[System.Decimal, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]','System.Decimal']}}}},Right:{Equal:{Left:{MemberAccess:{Type:'Fireasy.Common.Tests.Serialization.XmlSerializerTest+JsonData, Fireasy.Common.Tests',Member:'Name',Expression:{Parameter:{Name:'s'}}}},Right:{Constant:{Type:'System.String',Value:'\'12\''}}}}}}}}").ToString();

            var expression = serializer.Deserialize<Expression>(xml);

            Assert.IsNotNull(expression);
        }

        [TestMethod()]
        public void TestDeserializeExpandoObject()
        {
            var serializer = new XmlSerializer();

            dynamic obj = serializer.Deserialize<dynamic>(
                new JsonText(@"{'xm':'huangxd'}").ToString()
                );

            Assert.IsNotNull(obj);
            Assert.AreEqual("huangxd", obj.xm);
        }

        [TestMethod()]
        public void TestDeserializeDynamicObject1()
        {
            var serializer = new XmlSerializer();

            var obj = serializer.Deserialize<dynamic>(
                new JsonText(@"{'xm':'huangxd','age':33, parent: { 'xm': 'jiming', 'age': 55 }}").ToString()
                );

            Assert.IsNotNull(obj);
            Assert.AreEqual("jiming", obj.parent.xm);
        }

        [TestMethod()]
        public void TestDeserializeDynamicObject2()
        {
            var serializer = new XmlSerializer();

            var obj = serializer.Deserialize<dynamic>(
                new JsonText(@"{'xm':'huangxd','age':33, clidren: [{ 'xm': 'jiming', 'age': 55 }], point: [12,33,44]}").ToString()
                );

            Assert.IsNotNull(obj);
            Assert.AreEqual(1, obj.clidren.Count);
            Assert.AreEqual("jiming", obj.clidren[0].xm);
            Assert.AreEqual(12, obj.point[0]);
        }

        [TestMethod]
        public void TestDeserializeComplex()
        {
        }

        [TestMethod]
        public void TestDeserializeTuple()
        {
            var ss = new JsonText(@"{'Item2':23,'Item1':44,'Item3':'fireasy'}");
            var serializer = new XmlSerializer();
            var tuple = serializer.Deserialize<Tuple<int, int, string>>(ss.ToString());
        }

        [TestMethod]
        public void TestDeserializeTuple1()
        {
            var ss = new JsonText(@"{'Item2':23,'Item1':44,'Item3':'fireasy', 'Item4':{'Item1':55,'Item2':45}}");
            var serializer = new XmlSerializer();
            var tuple = serializer.Deserialize<Tuple<int, int, string, Tuple<int, int>>>(ss.ToString());
        }

        [TestMethod]
        public void TestDeserializeWithConverterAttr()
        {
            var ss = "\"test\"";
            var serializer = new XmlSerializer(null);

            var attr = serializer.Deserialize<AttrTest>(ss);
            Console.WriteLine(attr.Name);
        }

        [TestMethod]
        public void TestDeserializeArrayConverter()
        {
            var ss = @"<ArrayData>
  <Address>a,b,c,d,e,f,g</Address>
</ArrayData>
";

            var serializer = new XmlSerializer();

            var attr = serializer.Deserialize<ArrayData>(ss);
            Assert.AreEqual(7, attr.Address.Count);
        }

        public class Result<T>
        {
            public List<T> data { get; set; }

            public bool success { get; set; }

            public string msg { get; set; }
        }

        [TestMethod()]
        public void TestDeserializeDynamicObject3()
        {
            var serializer = new XmlSerializer();

            var obj = serializer.Deserialize<dynamic>(
                new JsonText(@"{'xm':'huangxd','age':33, clidren: [], mm: 23}").ToString()
                );

            Assert.IsNotNull(obj);
            Assert.AreEqual(0, obj.clidren.Count);
        }

        public interface IJsonData
        {
            decimal? Age { get; set; }
            DateTime Birthday { get; set; }
            Type DataType { get; set; }
            string Name { get; set; }
            List<WorkRecord> WorkRecords { get; set; }
        }

        [TextConverter(typeof(AttrTestConverter))]
        public class AttrTest
        {
            public string Name { get; set; }
        }

        public class AttrTestConverter : XmlConverter<AttrTest>
        {
            public override void WriteXml(XmlSerializer serializer, XmlWriter writer, object obj)
            {
                writer.WriteRaw($"\"{((AttrTest)obj).Name}\"");
            }

            public override object ReadXml(XmlSerializer serializer, XmlReader reader, Type dataType)
            {
                var xml = reader.ReadInnerXml();
                return new AttrTest { Name = xml };
            }
        }

        /// <summary>
        /// 测试数据的结构。
        /// </summary>
        public class JsonData : IJsonData
        {
            public JsonData()
            {

            }

            public JsonData(string name)
            {
                this.Name = name;
            }

            public string Name { get; set; }

            [TextFormatter("yy-MM-dd")]
            public DateTime Birthday { get; set; }

            public DateTime WorkTime { get; set; }

            public decimal? Age { get; set; }

            public List<WorkRecord> WorkRecords { get; set; }

            public Type DataType { get; set; }

            public WorkRecord Record { get; set; }
        }

        public class WorkRecord
        {
            public string Company { get; set; }

            [DefaultValue("2019-3-3")]
            public DateTime StartDate { get; set; }

            public DateTime? EndDate { get; set; }
        }

        public class ElementData
        {
            [TextSerializeElement("xm")]
            public string Name { get; set; }
        }

        public class LazyLoadData : ILazyManager
        {

            public ElementData E1 { get; set; }

            public ElementData E2 { get; set; }

            public bool IsValueCreated(string propertyName)
            {
                if (propertyName == "E1")
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Json转换类，由于双引号比较麻烦，因此可以使用单引号来替代，在ToString时再转换为双引号。
        /// </summary>
        private class JsonText
        {
            public JsonText(string xml)
            {
                Content = xml.Replace("'", "\"");
            }

            public string Content { get; set; }

            public override string ToString()
            {
                return Content;
            }
        }

        private enum TestEnum
        {
            A,
            B,
            C
        }

        [TestMethod]
        public void TestNew()
        {
            var s = "\"42342424/ffadfafd\"";

            var ad = new XmlSerializer().Deserialize<string>(s);
            Console.WriteLine(ad);
        }

        [TestMethod]
        public void TestMethod1()
        {
            var ss = "{ Name: \"aa\", Sex: \"M\" }";
            var sss = new XmlSerializer().Deserialize<People>(ss);
            Console.WriteLine(sss);
        }

        [TestMethod]
        public void TestMethod2()
        {
            var ss = "{ Name: \"aa\", Address: \"M\", C: [] }";
            var sss = new XmlSerializer().Deserialize<IB>(ss);
            Console.WriteLine(sss);
        }

        [TestMethod]
        public void TestMethod3()
        {
            var ss = "{ Name: \"aa\", Sex: \"M\", Farther: {} }";
            var sss = new XmlSerializer().Deserialize<People>(ss);
            Console.WriteLine(sss);
        }

        [TestMethod]
        public void TestSerializeUseFilterConverter()
        {
            var people = new People { Name = "fireasy", Sex = Sex.M, Address = "kunming" };
            var option = new XmlSerializeOption();
            option.Converters.Add(new JsonFilterConverter<People>(s => s.Name, s => s.Sex));
            var serializer = new XmlSerializer(option);
            Console.WriteLine(serializer.Serialize(people));
        }

        public class People
        {
            public string Name { get; set; }

            public Sex Sex { get; set; }

            public string Address { get; set; }

            public People Farther { get; set; }
        }

        public enum Sex
        {
            M,
            F
        }

        public interface IA
        {
            string Name { get; }
        }

        public interface IB : IA
        {
            string Address { get; }

            IList<string> C { get; set; }
        }

        public class ArrayData
        {
            public List<string> Address { get; set; }
        }
    }
}
