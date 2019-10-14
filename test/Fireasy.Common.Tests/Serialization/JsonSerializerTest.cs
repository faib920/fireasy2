using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fireasy.Common.Serialization;
using System.Collections;
using Fireasy.Common.ComponentModel;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Data;
using System.Dynamic;
using System.Linq;
using Fireasy.Common.Extensions;
using System.Drawing;
using System.IO;
using Swifter.Json;

namespace Fireasy.Common.Tests.Serialization
{
    /// <summary>
    /// JsonSerializerTest 的摘要说明
    /// </summary>
    [TestClass]
    public class JsonSerializerTest
    {
        private DateTime DEFAULT_DATE = new DateTime(2012, 6, 7, 12, 45, 33);

        /// <summary>
        /// 测试构造器。
        /// </summary>
        [TestMethod()]
        public void Test()
        {
            var serializer = new JsonSerializer();

            Assert.IsNotNull(serializer);
        }

        /// <summary>
        /// 使用整型测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeInt()
        {
            var serializer = new JsonSerializer();

            var json = serializer.Serialize(89);

            Assert.AreEqual("89", json);
            Console.WriteLine(json);
        }

        /// <summary>
        /// 使用整型测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestNullArray()
        {
            var serializer = new JsonSerializer();

            object footer = null;
            var f = footer is IEnumerable ? footer : new [] { footer };
            var obj = new { total = 100, rows = new List<int> { 22 }, footer = f };
            var json = serializer.Serialize(obj);

            Assert.AreEqual("89", json);
            Console.WriteLine(json);
        }

        /// <summary>
        /// 使用字符串测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeString()
        {
            var serializer = new JsonSerializer();

            var json = serializer.Serialize("fireasy");

            Assert.AreEqual("\"fireasy\"", json);
            Console.WriteLine(json);
        }

        /// <summary>
        /// 使用字符串测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeStringWithQuote()
        {
            var serializer = new JsonSerializer();

            var json = serializer.Serialize("<span style=\"background-color:#ffffff\"></span>");

            Assert.AreEqual("\"<span style=\\\"background-color:#ffffff\\\"></span>\"", json);
            Console.WriteLine(json);
        }

        /// <summary>
        /// 使用字符串测试Deserialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializePath()
        {
            var serializer = new JsonSerializer();

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
            var serializer = new JsonSerializer();

            var json = serializer.Serialize(@"fireasy
studio");

            Assert.AreEqual("\"fireasy\\r\\nstudio\"", json);
            Console.WriteLine(json);
        }

        /// <summary>
        /// 使用字符串测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeStringWithEnter1()
        {
            var serializer = new JsonSerializer();

            var json = serializer.Serialize(@"yunnan\honghe");

            Assert.AreEqual(@"""yunnan\\honghe""", json);
            Console.WriteLine(json);
        }


        /// <summary>
        /// 使用字符串测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeStringEmpty()
        {
            var serializer = new JsonSerializer();

            var json = serializer.Serialize<string>("");

            Assert.AreEqual(json, "\"\"");
            Console.WriteLine(json);
        }

        /// <summary>
        /// 使用字符串测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeStringNull()
        {
            var serializer = new JsonSerializer();

            var json = serializer.Serialize<string>(null);

            Assert.AreEqual(json, "null");
            Console.WriteLine(json);
        }

        /// <summary>
        /// 使用Html字符串测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeHtmlString()
        {
            var serializer = new JsonSerializer();

            var json = serializer.Serialize("<a>中国</a>");

            Assert.AreEqual(json, "\"<a>中国</a>\"");
        }

        /// <summary>
        /// 使用Html字符串和ConverterS测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeHtmlStringWithConverter()
        {
            var option = new JsonSerializeOption { Converters = new ConverterList { new UnicodeStringJsonConverter() } };
            var serializer = new JsonSerializer();

            var json = serializer.Serialize("<a>中国</a>");

            Assert.AreEqual(json, "\"\u003ca\u003e中国\u003c/a\u003e\"");
        }

        /// <summary>
        /// 使用布尔测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeBoolean()
        {
            var serializer = new JsonSerializer();

            var json = serializer.Serialize(false);

            Console.WriteLine(json);
        }

        /// <summary>
        /// 使用日期测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeDateTime()
        {
            var option = new JsonSerializeOption();
            option.DateFormatHandling = DateFormatHandling.JsonDateFormat;
            var serializer = new JsonSerializer(option);

            var json = serializer.Serialize(DEFAULT_DATE);

            Console.WriteLine(json);

            var s = new Newtonsoft.Json.JsonSerializer();
            s.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.MicrosoftDateFormat;
            s.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
            using (var t = new StringWriter())
            {
                s.Serialize(t, DEFAULT_DATE);
                Console.WriteLine(t.ToString());
            }
        }

        /// <summary>
        /// 使用日期及Object格式测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeDateTimeToObject()
        {
            var option = new JsonSerializeOption { Format = JsonFormat.Object };
            var serializer = new JsonSerializer(option);

            var json = serializer.Serialize(DEFAULT_DATE);

            Console.WriteLine(json);
        }

        /// <summary>
        /// 使用日期及Converters测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeDateTimeWithConverter()
        {
            var option = new JsonSerializeOption();
            option.Converters.Add(new FullDateTimeJsonConverter());
            var serializer = new JsonSerializer(option);
            var obj = new JsonData
            {
                Age = 12,
                Name = "huangxd",
                Birthday = DateTime.Parse("1981-1-10")
            };

            var json = serializer.Serialize(obj);
            Console.WriteLine(json);

        }

        /// <summary>
        /// 使用object测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeObject()
        {
            var serializer = new JsonSerializer();

            var obj = new JsonData
            {
                Age = 12,
                Name = "huangxd",
                Birthday = DateTime.Parse("1982-9-20")
            };

            var json = serializer.Serialize(obj);

            Console.WriteLine(json);
        }

        /// <summary>
        /// 使用object及Camel命名选项测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeObjectWithCamel()
        {
            var option = new JsonSerializeOption { CamelNaming = true };
            var serializer = new JsonSerializer(option);

            var obj = new JsonData
            {
                Age = 12,
                Name = "huangxd",
                Birthday = DateTime.Parse("1982-9-20")
            };

            var json = serializer.Serialize(obj);

            Console.WriteLine(json);
        }

        /// <summary>
        /// 使用包含选项测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeObjectWithInclusiveNames()
        {
            var option = new JsonSerializeOption { InclusiveNames = new string[] { "Name" } };
            var serializer = new JsonSerializer(option);

            var obj = new JsonData
            {
                Age = 12,
                Name = "huangxd",
                Birthday = DateTime.Parse("1982-9-20")
            };

            var json = serializer.Serialize(obj);

            Console.WriteLine(json);
        }

        /// <summary>
        /// 使用排除选项测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeObjectWithExclusiveNames()
        {
            var option = new JsonSerializeOption { ExclusiveNames = new string[] { "Birthday" } };
            var serializer = new JsonSerializer(option);

            var obj = new JsonData
            {
                Age = 12,
                Name = "huangxd",
                Birthday = DateTime.Parse("1982-9-20")
            };

            var json = serializer.Serialize(obj);

            Console.WriteLine(json);
        }

        /// <summary>
        /// 使用排除选项测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeObjectWithExclusiveMembers()
        {
            var option = new JsonSerializeOption();
            option.Exclude<JsonData>(s => s.Birthday, s => s.Record.StartDate).Include<WorkRecord>(s => s.Company);
            var serializer = new JsonSerializer(option);

            var obj = new JsonData
            {
                Age = 12,
                Name = "huangxd",
                Birthday = DateTime.Parse("1982-9-20"),
                Record = new WorkRecord {  Company = "fireasy", StartDate = DateTime.Now, EndDate = DateTime.Now }
            };

            var json = serializer.Serialize(obj);

            Console.WriteLine(json);
        }

        /// <summary>
        /// 使用复杂对象测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeObjectDetail()
        {
            var serializer = new JsonSerializer();

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

            var json = serializer.Serialize(obj);

            Console.WriteLine(json);
        }

        /// <summary>
        /// 使用TextSerializeElement特性测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeObjectWithElement()
        {
            var serializer = new JsonSerializer();

            var obj = new ElementData
            {
                Name = "huangxd"
            };

            var json = serializer.Serialize(obj);
            Console.WriteLine(json);
        }

        /// <summary>
        /// 使用LazyManager特性测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeLazyManager()
        {
            var serializer = new JsonSerializer();

            var obj = new LazyLoadData { E1 = new ElementData { Name = "X1" }, E2 = new ElementData { Name = "X2" } };

            var json = serializer.Serialize(obj);

            Console.WriteLine(json);
        }

        /// <summary>
        /// 使用Type测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeTypeIgnore()
        {
            var serializer = new JsonSerializer();

            var obj = new JsonData
            {
                DataType = typeof(WorkRecord)
            };

            var json = serializer.Serialize(obj);

            Console.WriteLine(json);
        }

        [TestMethod]
        public void TestSerializeIgnoreNull()
        {
            var option = new JsonSerializeOption { IgnoreNull = true };
            var serializer = new JsonSerializer(option);

            var obj = new JsonData
            {
                Age = 12,
                Name = "huangxd",
                Birthday = DateTime.Parse("1982-9-20")
            };

            var json = serializer.Serialize(obj);
            Console.WriteLine(json);
        }

        [TestMethod]
        public void TestSerializeWithIndent()
        {
            var option = new JsonSerializeOption { Indent = true };
            var serializer = new JsonSerializer(option);

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

            var json = serializer.Serialize(obj);
            Console.WriteLine(json);
        }

        /// <summary>
        /// 使用Type测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeType()
        {
            var serializer = new JsonSerializer(new JsonSerializeOption { IgnoreType = false });

            var obj = new JsonData
            {
                DataType = typeof(WorkRecord)
            };

            var json = serializer.Serialize(obj);

            Console.WriteLine(json);
        }

        /// <summary>
        /// 使用列表测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeList()
        {
            var list = new List<JsonData>();
            for (var i = 0; i < 5000; i++)
            {
                list.Add(new JsonData
                {
                    Age = 12,
                    Name = "huangxd",
                    Birthday = DateTime.Parse("1982-9-20")
                });
            }

            Console.WriteLine("使用Fireasy序列化 50000 个对象");

            var time1 = TimeWatcher.Watch(() =>
            {
                new JsonSerializer().Serialize(list);
            });

            Console.WriteLine("耗时: {0} 毫秒", time1.Milliseconds);
            Console.WriteLine("使用Json.Net序列化 50000 个对象");

            var time2 = TimeWatcher.Watch(() =>
            {
                Newtonsoft.Json.JsonConvert.SerializeObject(list);
            });

            Console.WriteLine("耗时: {0} 毫秒", time2.Milliseconds);

            Console.WriteLine("使用Swifter.Json序列化 50000 个对象");

            var time3 = TimeWatcher.Watch(() =>
            {
                var json = new JsonFormatter().Serialize(list);
                //Console.WriteLine("长度: " + json);
            });
            Console.WriteLine("耗时: {0} 毫秒", time3.Milliseconds);

            Console.WriteLine("使用fastJson序列化 50000 个对象");

            var time4 = TimeWatcher.Watch(() =>
            {
                var json = fastJSON.JSON.ToJSON(list);
                //Console.WriteLine("长度: " + json);
            });
            Console.WriteLine("耗时: {0} 毫秒", time4.Milliseconds);
        }

        /// <summary>
        /// 使用字典测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public async Task TestSerializeDictonary()
        {
            var serializer = new JsonSerializer();

            var dictionary = new Dictionary<string, JsonData>();

            for (var i = 0; i < 50000; i++)
            {
                dictionary.Add("h" + i, new JsonData
                {
                    Age = 12,
                    Name = "huangxd",
                    Birthday = DateTime.Parse("1982-9-20")
                });
            }

            Console.WriteLine("使用Fireasy序列化 50000 行的字典");
            var time1 = await TimeWatcher.WatchAsync(async () =>
            {
                var json = await serializer.SerializeAsync(dictionary);
                //Console.WriteLine("长度: " + json);
            });

            Console.WriteLine("耗时: {0} 毫秒", time1.Milliseconds);

            Console.WriteLine("使用Json.Net序列化 50000 行的字典");

            var time2 = TimeWatcher.Watch(() =>
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(dictionary);
                //Console.WriteLine("长度: " + json);
            });
            Console.WriteLine("耗时: {0} 毫秒", time2.Milliseconds);

            Console.WriteLine("使用Swifter.Json序列化 50000 行的字典");

            var time3 = TimeWatcher.Watch(() =>
            {
                var json = new JsonFormatter().Serialize(dictionary);
                //Console.WriteLine("长度: " + json);
            });
            Console.WriteLine("耗时: {0} 毫秒", time3.Milliseconds);

            Console.WriteLine("使用fastJson序列化 50000 行的字典");

            var time4 = TimeWatcher.Watch(() =>
            {
                var json = fastJSON.JSON.ToJSON(dictionary);
                //Console.WriteLine("长度: " + json);
            });
            Console.WriteLine("耗时: {0} 毫秒", time4.Milliseconds);
        }

        /// <summary>
        /// 使用数据集测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeDataSet()
        {
            var serializer = new JsonSerializer();

            var ds = new DataSet();

            var tb = new DataTable("table1");
            tb.Columns.Add("Name", typeof(string));
            tb.Columns.Add("Age", typeof(int));
            tb.Columns.Add("Birthday", typeof(DateTime));

            tb.Rows.Add("huangxd", 12, DateTime.Parse("1982-9-20"));
            tb.Rows.Add("liping", 22, DateTime.Parse("1972-9-20"));

            ds.Tables.Add(tb);

            var json = serializer.Serialize(ds);

            Console.WriteLine(json);
        }

        /// <summary>
        /// 使用数据表测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeDataTable()
        {
            var serializer = new JsonSerializer();

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
                var json = serializer.Serialize(tb);
                Console.WriteLine("长度: " + json);
            });

            Console.WriteLine("耗时: {0} 毫秒", time1.Milliseconds);

            Console.WriteLine("使用Json.Net序列化100000行的DataTable");

            var time2 = TimeWatcher.Watch(() =>
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(tb);
                Console.WriteLine("长度: " + json);
            });
            Console.WriteLine("耗时: {0} 毫秒", time2.Milliseconds);
        }

        /// <summary>
        /// 使用数组测试Serialize方法。
        /// </summary>
        [TestMethod()]
        public void TestSerializeArray()
        {
            var o = new JsonSerializeOption { IgnoreNull = false };
            var serializer = new JsonSerializer();

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

            var json = serializer.Serialize(array);

            Console.WriteLine(json);
        }

        [TestMethod]
        public void TestSerializeArrayList()
        {
            var array = new ArrayList();

            array.Add("test");
            array.Add(1222);
            array.Add(true);

            var serializer = new JsonSerializer();
            var json = serializer.Serialize(array);

            Console.WriteLine(json);
        }

        [TestMethod()]
        public void TestSerializeColor()
        {
            var serializer = new JsonSerializer();

            var json = serializer.Serialize(Color.Red);

            Console.WriteLine(json);
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

            var option = new JsonSerializeOption();
            option.Converters.Add(new ExpressionJsonConverter());
            var serializer = new JsonSerializer(option);

            Console.WriteLine("使用Fireasy序列化表达式");
            var time = TimeWatcher.Watch(() =>
            {
                var json = serializer.Serialize(expression);
                Console.WriteLine(json);
            });

            Console.WriteLine("耗时: {0} 毫秒", time.Milliseconds);
            Console.WriteLine("使用Json.Net序列化表达式");
            time = TimeWatcher.Watch(() =>
            {
                //Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(expression));
            });
            Console.WriteLine("耗时: {0} 毫秒", time.Milliseconds);
        }

        [TestMethod()]
        public void TestSerializeExpandoObject()
        {
            dynamic obj = new ExpandoObject();
            obj.Name = "aaaa";
            obj.IsOld = true;
            obj.Age = 12.5;
            obj.Items = new[] { 34, 55, 66 };

            var dd = new { A = 43434 };

            var serializer = new JsonSerializer();

            var newo = GenericExtension.Extend(obj, dd);
            Console.WriteLine("使用Fireasy序列化动态对象");
            var time = TimeWatcher.Watch(() =>
            {
                var json = serializer.Serialize(newo);
                Console.WriteLine(json);
            });

            Console.WriteLine("耗时: {0} 毫秒", time.Milliseconds);
            Console.WriteLine("使用Json.Net序列化动态对象");

            time = TimeWatcher.Watch(() =>
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(newo);
                Console.WriteLine(json);
            });
            Console.WriteLine("耗时: {0} 毫秒", time.Milliseconds);
        }

        [TestMethod]
        public void TestSerializeLazyObject()
        {
            var obj = new LazyClass { Name = "fireasy", Address = "kunming", Age = 30 };
            var serializer = new JsonSerializer();
            Console.WriteLine(serializer.Serialize(obj));
        }

        [TestMethod]
        public void TestSerializeCycle()
        {
            try
            {
                var obj = new CycleClass();
                obj.Parent = obj;
                var serializer = new JsonSerializer();
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
                Record = new WorkRecord {  StartDate = DateTime.Now, Company = "dd" }
            };

            var converter = new CompositeJsonConverter<WorkRecord>();
            converter.AddConverter(s => s.StartDate, new DateTimeJsonConverter("yyyy-MM"));
            var option = new JsonSerializeOption() { Indent = true };
            option.Converters.Add(converter);
            var serializer = new JsonSerializer(option);

            var json = serializer.Serialize(data);

            Console.WriteLine(json);

        }

        [TestMethod]
        public void TestSerializeTuple()
        {
            var tuple = new Tuple<int, int, string>(23, 44, "fireasy");
            var serializer = new JsonSerializer(null);

            var json = serializer.Serialize(tuple);
            Console.WriteLine(json);
        }

        [TestMethod]
        public void TestSerializeTuple1()
        {
            var tuple = new Tuple<int, int, string, Tuple<int, int>>(23, 44, "fireasy", new Tuple<int, int>(55, 45));
            var serializer = new JsonSerializer(null);

            var json = serializer.Serialize(tuple);
            Console.WriteLine(json);
        }

        [TestMethod]
        public void TestSerializeWithConverterAttr()
        {
            var attr = new AttrTest { Name = "fireasy" };
            var serializer = new JsonSerializer(null);

            var json = serializer.Serialize(attr);
            Console.WriteLine(json);
        }

        public class LazyClass : ILazyManager
        {
            public string Name { get; set; }

            public string Address { get; set; }

            public int Age { get; set; }

            public bool IsValueCreated(string propertyName)
            {
                switch (propertyName)
                {
                    case "Name":
                    case "Address":
                        return true;
                    case "Age":
                    default:
                        return false;
                }
            }
        }

        public class CycleClass
        {
            public CycleClass Parent { get; set; }
        }

        /// <summary>
        /// 测试Deserialize方法，返回整型。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeInt()
        {
            var serializer = new JsonSerializer();

            var i = serializer.Deserialize<int>("89");

            Assert.AreEqual(i, 89);
        }


        /// <summary>
        /// 使用字符串测试Deserialize方法。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeStringWithQuote()
        {
            var serializer = new JsonSerializer();

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
            var serializer = new JsonSerializer();

            var s1 = "\"C:\\\\Users\\\\faib\\\\Desktop\"";

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
            var serializer = new JsonSerializer();

            var str = serializer.Deserialize<string>("\"null\"");

            Assert.AreEqual(null, str);
        }

        /// <summary>
        /// 使用字符串测试Deserialize方法。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeUnicodeString()
        {
            var serializer = new JsonSerializer();

            var str = serializer.Deserialize<string>("\"\\\\u4e2d\\\\u56fd\"");

            Assert.AreEqual("中国", str);
        }

        /// <summary>
        /// 使用字符串测试Deserialize方法。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeStringUnicode()
        {
            var serializer = new JsonSerializer();

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
            var serializer = new JsonSerializer();

            var b = serializer.Deserialize<bool>("true");

            Assert.AreEqual(b, true);
        }

        /// <summary>
        /// 测试Deserialize方法，返回日期。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeDateTime()
        {
            var serializer = new JsonSerializer();

            var d = serializer.Deserialize<DateTime>("\"2012-6-7 12:45:33\"");

            Assert.AreEqual(d, new DateTime(2012, 6, 7, 12, 45, 33));
        }

        /// <summary>
        /// 测试Deserialize方法，返回日期。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeDateTimeNull()
        {
            var serializer = new JsonSerializer();

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
            var option = new JsonSerializeOption { Converters = new ConverterList { new DateTimeJsonConverter("d-M-yyyy") } };
            var serializer = new JsonSerializer(option);

            var d = serializer.Deserialize<DateTime>("\"6-7-2012\"");

            Assert.AreEqual(d, new DateTime(2012, 7, 6));
        }

        /// <summary>
        /// 测试Deserialize方法，返回日期。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeDateTimeComplex()
        {
            var serializer = new JsonSerializer();

            var d = serializer.Deserialize<DateTime>(@"""\/Date(1339044333000+0800)\/""");

            Assert.AreEqual(d, new DateTime(2012, 6, 7, 12, 45, 33));
        }

        /// <summary>
        /// 测试Deserialize方法，返回Html文本。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeHtmlString()
        {
            var serializer = new JsonSerializer();

            var d = serializer.Deserialize<string>("\"\u003ca\u003e中国\u003c/a\u003e\"");

            Assert.AreEqual(d, "<a>中国</a>");
        }

        [TestMethod()]
        public void TestDeserializeEnum()
        {
            var serializer = new JsonSerializer();

            var obj = serializer.Deserialize<TestEnum?>("");

            Assert.AreEqual(obj, null);
        }

        /// <summary>
        /// 测试Deserialize方法，返回object。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeObject()
        {
            var serializer = new JsonSerializer();

            var obj = serializer.Deserialize<JsonData>(
                new JsonText(@"{'Name':null,'Birthday':'\/Date(401299200000+0800)\/','Age':12,'WorkRecords':null}").ToString()
                );

            Assert.IsNotNull(obj);
            Console.WriteLine(obj.Name);
        }

        /// <summary>
        /// 测试Deserialize方法，返回object。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeObject1()
        {
            var serializer = new JsonSerializer();

            var obj = serializer.Deserialize<ComponentModel.Result<object>>(
                new JsonText(@"{'succeed':true,'data':{'Name':'fireasy','Address':'kunming'}}").ToString()
                );

            Assert.IsNotNull(obj);
            Console.WriteLine(serializer.Serialize(obj));
        }

        /// <summary>
        /// 测试Deserialize方法，返回object。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeObject2()
        {
            var serializer = new JsonSerializer();

            var obj = serializer.Deserialize<object>(
                new JsonText(@"[{'Name':null,'Birthday':'\/Date(401299200000+0800)\/','Age':12,'WorkRecords':null}]").ToString()
                );

            Assert.IsNotNull(obj);
        }

        /// <summary>
        /// 测试Deserialize方法，返回object。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeObject3()
        {
            var serializer = new JsonSerializer();

            var obj = serializer.Deserialize<JsonData>(
                new JsonText(@"{}").ToString()
                );

            Assert.IsNotNull(obj);
        }

        /// <summary>
        /// 测试Deserialize方法，返回object。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeObjectByInterface()
        {
            var serializer = new JsonSerializer();

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
            var serializer = new JsonSerializer();

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
            var serializer = new JsonSerializer();

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
            var option = new JsonSerializeOption { CamelNaming = true };
            var serializer = new JsonSerializer(option);

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
            var serializer = new JsonSerializer();

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
            var serializer = new JsonSerializer();

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
            var serializer = new JsonSerializer(new JsonSerializeOption { CamelNaming = true });

            var obj = serializer.Deserialize<ElementData>(
                new JsonText(@"{'xm':'huangxd'}").ToString()
                );

            Assert.IsNotNull(obj);
            Console.WriteLine(obj.Name);
        }

        [TestMethod()]
        public void TestDeserializeType()
        {
            var serializer = new JsonSerializer(new JsonSerializeOption { IgnoreType = false });
            var json = new JsonText(@"{'Name':null,'WorkRecords':null,'DataType':'Fireasy.Common.Serialization.Test.JsonSerializerTests+WorkRecord, Fireasy.Common.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'}").ToString();
            var obj = serializer.Deserialize<JsonData>(json);
            Assert.IsNotNull(obj);
            Console.WriteLine(obj.DataType);
        }

        [TestMethod()]
        public void TestDeserializeTypeIgnore()
        {
            var serializer = new JsonSerializer();
            var json = new JsonText(@"{'Name':null,'WorkRecords':null,'DataType':'Fireasy.Common.Serialization.Test.JsonSerializerTests+WorkRecord, Fireasy.Common.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'}").ToString();
            var obj = serializer.Deserialize<JsonData>(json);
            Assert.IsNotNull(obj);
            Console.WriteLine(obj.DataType);
        }

        /// <summary>
        /// 测试Deserialize方法，返回List。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeList()
        {
            var serializer = new JsonSerializer();

            var sb = new StringBuilder();
            for (var i = 0; i < 100000; i++)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }

                sb.Append(@"{'Name':'huangxd','Birthday':'\/Date(401299200000+0800)\/','Age':12,'WorkRecords':null}");
            }

            var json = new JsonText("[" + sb.ToString() + "]").ToString();

            Console.WriteLine("使用Fireasy反序列化100000行");
            var time1 = TimeWatcher.Watch(() =>
            {
                var list = serializer.Deserialize<List<JsonData>>(json);
            });
            Console.WriteLine("耗时: {0}", time1.Milliseconds);

            Console.WriteLine("使用Json.Net反序列化100000行");
            var time2 = TimeWatcher.Watch(() =>
            {
                var list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<JsonData>>(json);
            });
            Console.WriteLine("耗时: {0}", time2.Milliseconds);
            //Assert.IsNotNull(list);
            //Assert.AreEqual(10000, list.Count);
        }

        /// <summary>
        /// 测试Deserialize方法，返回数组。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeArray()
        {
            var serializer = new JsonSerializer();

            var json = new JsonText(@"[{'Name':'huangxd','Birthday':'\/Date(401299200000+0800)\/','Age':12,'WorkRecords':null},{'Name':'liping','Birthday':'\/Date(401299200000+0800)\/','Age':22,'WorkRecords':null}]").ToString();

            var array = serializer.Deserialize<JsonData[]>(json);

            Assert.IsNotNull(array);
            Assert.AreEqual(2, array.Length);
        }

        /// <summary>
        /// 测试Deserialize方法，返回数组。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeArray1()
        {
            var serializer = new JsonSerializer();

            var json = new JsonText(@"[]").ToString();

            var array = serializer.Deserialize<JsonData[]>(json);

            Assert.IsNotNull(array);
            Assert.AreEqual(2, array.Length);
        }

        /// <summary>
        /// 测试Deserialize方法，返回数组。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeDynamicArray()
        {
            var serializer = new JsonSerializer();

            var json = new JsonText(@"[12,33,44.55,'55']").ToString();

            var array = serializer.Deserialize<dynamic>(json);

            Assert.IsNotNull(array);
            Assert.AreEqual(4, array.Count);
        }

        /// <summary>
        /// 测试Deserialize方法，返回数组。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeDynamicArray1()
        {
            var serializer = new JsonSerializer();

            var json = new JsonText(@"[{a:12,b:33,c:44.55,d:'55',e:null}]").ToString();

            var array = serializer.Deserialize<object[]>(json);

            Assert.IsNotNull(array);
            Assert.AreEqual(1, array.Length);
        }

        /// <summary>
        /// 测试Deserialize方法，返回ArrayList。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeArrayList()
        {
            var serializer = new JsonSerializer();

            var json = new JsonText(@"[12,33,44.55,'55',{'Name':'fireasy'}]").ToString();

            var array = serializer.Deserialize<ArrayList>(json);

            Assert.IsNotNull(array);
            Assert.AreEqual(5, array.Count);
        }

        /// <summary>
        /// 测试Deserialize方法，返回字典。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeDictionary()
        {
            var serializer = new JsonSerializer();

            var json = new JsonText(@"{'huangxd':{'Name':'huangxd','Birthday':'\/Date(401299200000+0800)\/','Age':12,'WorkRecords':null},'liping':{'Name':'liping','Birthday':'\/Date(401299200000+0800)\/','Age':22,'WorkRecords':null}}").ToString();

            var dictionary = serializer.Deserialize<Dictionary<string, JsonData>>(json);

            Assert.IsNotNull(dictionary);
            Assert.AreEqual(2, dictionary.Count);
        }

        /// <summary>
        /// 测试Deserialize方法，返回字典。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeEnumDictionary()
        {
            var serializer = new JsonSerializer();

            var json = new JsonText(@"{'M':3434,'F':55}").ToString();

            var dictionary = serializer.Deserialize<Dictionary<Sex, int>>(json);

            Assert.IsNotNull(dictionary);
            Assert.AreEqual(2, dictionary.Count);
        }

        /// <summary>
        /// 测试Deserialize方法，返回数据表。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeDataTable()
        {
            var serializer = new JsonSerializer();

            var json = new JsonText(@"[{'Name':null,'Birthday':'\/Date(401299200000+0800)\/','Age':12},{'Name':'liping','Age':22,'Birthday':'\/Date(401299200000+0800)\/'}]").ToString();

            var table = serializer.Deserialize<DataTable>(json);

            Assert.IsNotNull(table);
            Assert.AreEqual(2, table.Rows.Count);
        }

        /// <summary>
        /// 测试Deserialize方法，返回数据集。
        /// </summary>
        [TestMethod()]
        public void TestDeserializeDataSet()
        {
            var serializer = new JsonSerializer();

            var json = new JsonText(@"{'table1':[{'Name':'huangxd','Birthday':'\/Date(401299200000+0800)\/','Age':12},{'Name':'liping','Age':22,'Birthday':'\/Date(401299200000+0800)\/'}]}").ToString();

            var ds = serializer.Deserialize<DataSet>(json);

            Assert.IsNotNull(ds);
            Assert.AreEqual(1, ds.Tables.Count);
        }

        [TestMethod()]
        public void TestDeserializeExpression()
        {
            var option = new JsonSerializeOption();
            option.Converters.Add(new ExpressionJsonConverter());
            var serializer = new JsonSerializer(option);

            var json = new JsonText(@"{Lambda:{Parameters:[{Parameter:{Name:'s',Type:'Fireasy.Common.Tests.Serialization.JsonSerializerTest+JsonData, Fireasy.Common.Tests'}}],Body:{OrElse:{Left:{OrElse:{Left:{OrElse:{Left:{Equal:{Left:{MemberAccess:{Type:'Fireasy.Common.Tests.Serialization.JsonSerializerTest+JsonData, Fireasy.Common.Tests',Member:'Age',Expression:{Parameter:{Name:'s'}}}},Right:{Constant:{Type:'System.Nullable`1[[System.Decimal, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]',Value:'44'}}}},Right:{AndAlso:{Left:{AndAlso:{Left:{Equal:{Left:{MemberAccess:{Type:'Fireasy.Common.Tests.Serialization.JsonSerializerTest+JsonData, Fireasy.Common.Tests',Member:'Age',Expression:{Parameter:{Name:'s'}}}},Right:{Constant:{Type:'System.Nullable`1[[System.Decimal, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]',Value:'34'}}}},Right:{Equal:{Left:{MemberAccess:{Type:'Fireasy.Common.Tests.Serialization.JsonSerializerTest+JsonData, Fireasy.Common.Tests',Member:'Age',Expression:{Parameter:{Name:'s'}}}},Right:{Constant:{Type:'System.Nullable`1[[System.Decimal]]',Value:'34'}}}}}},Right:{Equal:{Left:{Call:{Type:'System.String',Method:'Substring',Object:{MemberAccess:{Type:'Fireasy.Common.Tests.Serialization.JsonSerializerTest+JsonData, Fireasy.Common.Tests',Member:'Name',Expression:{Parameter:{Name:'s'}}}},Arguments:[{Constant:{Type:'System.Int32',Value:'1'}},{Constant:{Type:'System.Int32',Value:'2'}}],GenericArgTypes:[],ParameterTypes:['System.Int32','System.Int32']}},Right:{Constant:{Type:'System.String',Value:'\'12\''}}}}}}}},Right:{Call:{Type:'System.Linq.Enumerable, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089',Method:'Contains',Object:null,Arguments:[{Constant:{Type:'System.Decimal[]',Value:'[34,55,66]'}},{Convert:{Type:'System.Decimal',Operand:{MemberAccess:{Type:'Fireasy.Common.Tests.Serialization.JsonSerializerTest+JsonData, Fireasy.Common.Tests',Member:'Age',Expression:{Parameter:{Name:'s'}}}}}}],GenericArgTypes:['System.Decimal'],ParameterTypes:['System.Collections.Generic.IEnumerable`1[[System.Decimal, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]','System.Decimal']}}}},Right:{Equal:{Left:{MemberAccess:{Type:'Fireasy.Common.Tests.Serialization.JsonSerializerTest+JsonData, Fireasy.Common.Tests',Member:'Name',Expression:{Parameter:{Name:'s'}}}},Right:{Constant:{Type:'System.String',Value:'\'12\''}}}}}}}}").ToString();

            var expression = serializer.Deserialize<Expression>(json);

            Assert.IsNotNull(expression);
        }

        [TestMethod()]
        public void TestDeserializeExpandoObject()
        {
            var serializer = new JsonSerializer();

            dynamic obj = serializer.Deserialize<dynamic>(
                new JsonText(@"{'xm':'huangxd'}").ToString()
                );

            Assert.IsNotNull(obj);
            Assert.AreEqual("huangxd", obj.xm);
        }

        [TestMethod()]
        public void TestDeserializeDynamicObject()
        {
            var serializer = new JsonSerializer();

            var obj = serializer.Deserialize<dynamic>(
                new JsonText(@"{'xm':'huangxd','age':33}").ToString()
                );

            Assert.IsNotNull(obj);
            Assert.AreEqual("huangxd", obj.xm);
        }

        [TestMethod()]
        public void TestDeserializeDynamicObject1()
        {
            var serializer = new JsonSerializer();

            var obj = serializer.Deserialize<dynamic>(
                new JsonText(@"{'xm':'huangxd','age':33, parent: { 'xm': 'jiming', 'age': 55 }}").ToString()
                );

            Assert.IsNotNull(obj);
            Assert.AreEqual("jiming", obj.parent.xm);
        }

        [TestMethod()]
        public void TestDeserializeDynamicObject2()
        {
            var serializer = new JsonSerializer();

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
            var ret = DD().Result;
            Console.WriteLine(ret);
        }

        [TestMethod]
        public void TestDeserializeTuple()
        {
            var ss = new JsonText(@"{'Item2':23,'Item1':44,'Item3':'fireasy'}");
            var serializer = new JsonSerializer();
            var tuple = serializer.Deserialize<Tuple<int, int, string>>(ss.ToString());
        }

        [TestMethod]
        public void TestDeserializeTuple1()
        {
            var ss = new JsonText(@"{'Item2':23,'Item1':44,'Item3':'fireasy', 'Item4':{'Item1':55,'Item2':45}}");
            var serializer = new JsonSerializer();
            var tuple = serializer.Deserialize<Tuple<int, int, string, Tuple<int, int>>>(ss.ToString());
        }

        [TestMethod]
        public void TestDeserializeWithConverterAttr()
        {
            var ss = "\"test\"";
            var serializer = new JsonSerializer(null);

            var attr = serializer.Deserialize<AttrTest>(ss);
            Console.WriteLine(attr.Name);
        }

        private async Task<Result<MedInpatientsViewModel>> DD()
        {
            var s = new JsonText(@"{'data':[{'MedId':25437,'InhCode':'0001410726','InfoId':467,'MedName':'周伟','MedOutdate':'2015-10-16','MedVisit':null,'DeptName':'糖尿病科','DeptId':1076,'WeChatSign':'','TelStatus':null,'InHospCount':'1','HasMedTel':'','HasMedPlan':''},{'MedId':25282,'InhCode':'0001410501','InfoId':312,'MedName':'陶寿明','MedOutdate':'2015-10-16','MedVisit':null,'DeptName':'糖尿病科','DeptId':1076,'WeChatSign':'','TelStatus':null,'InHospCount':'1','HasMedTel':'','HasMedPlan':''},{'MedId':25322,'InhCode':'0001410525','InfoId':352,'MedName':'左凤云','MedOutdate':'2015-10-16','MedVisit':null,'DeptName':'糖尿病科','DeptId':1076,'WeChatSign':'','TelStatus':null,'InHospCount':'1','HasMedTel':'','HasMedPlan':''},{'MedId':25439,'InhCode':'0001410731','InfoId':469,'MedName':'叶应珠','MedOutdate':'2015-10-16','MedVisit':null,'DeptName':'糖尿病科','DeptId':1076,'WeChatSign':'','TelStatus':null,'InHospCount':'1','HasMedTel':'','HasMedPlan':''},{'MedId':25390,'InhCode':'0001410606','InfoId':420,'MedName':'徐远达','MedOutdate':'2015-10-16','MedVisit':null,'DeptName':'糖尿病科','DeptId':1076,'WeChatSign':'','TelStatus':null,'InHospCount':'1','HasMedTel':'','HasMedPlan':''},{'MedId':25405,'InhCode':'0001410664','InfoId':435,'MedName':'罗朝兰','MedOutdate':'2015-10-16','MedVisit':null,'DeptName':'糖尿病科','DeptId':1076,'WeChatSign':'','TelStatus':null,'InHospCount':'1','HasMedTel':'','HasMedPlan':''},{'MedId':25349,'InhCode':'0001410553','InfoId':379,'MedName':'黄新芝','MedOutdate':'2015-10-15','MedVisit':null,'DeptName':'糖尿病科','DeptId':1076,'WeChatSign':'','TelStatus':null,'InHospCount':'1','HasMedTel':'','HasMedPlan':''},{'MedId':25297,'InhCode':'0001410497','InfoId':327,'MedName':'余焕珍','MedOutdate':'2015-10-14','MedVisit':null,'DeptName':'糖尿病科','DeptId':1076,'WeChatSign':'','TelStatus':null,'InHospCount':'1','HasMedTel':'','HasMedPlan':''},{'MedId':25333,'InhCode':'0001410547','InfoId':363,'MedName':'全顺祥','MedOutdate':'2015-10-14','MedVisit':null,'DeptName':'糖尿病科','DeptId':1076,'WeChatSign':'','TelStatus':null,'InHospCount':'1','HasMedTel':'','HasMedPlan':''},{'MedId':25408,'InhCode':'0001410667','InfoId':438,'MedName':'柳卫民','MedOutdate':'2015-10-12','MedVisit':null,'DeptName':'糖尿病科','DeptId':1076,'WeChatSign':'','TelStatus':null,'InHospCount':'1','HasMedTel':'','HasMedPlan':''},{'MedId':25072,'InhCode':'0001410187','InfoId':102,'MedName':'张可栋','MedOutdate':'2015-10-12','MedVisit':null,'DeptName':'糖尿病科','DeptId':1076,'WeChatSign':'','TelStatus':null,'InHospCount':'1','HasMedTel':'','HasMedPlan':''},{'MedId':25248,'InhCode':'0001410431','InfoId':278,'MedName':'杨云芬','MedOutdate':'2015-10-11','MedVisit':null,'DeptName':'糖尿病科','DeptId':1076,'WeChatSign':'','TelStatus':null,'InHospCount':'1','HasMedTel':'','HasMedPlan':''},{'MedId':25263,'InhCode':'0001410456','InfoId':293,'MedName':'王正平','MedOutdate':'2015-10-10','MedVisit':null,'DeptName':'糖尿病科','DeptId':1076,'WeChatSign':'','TelStatus':null,'InHospCount':'1','HasMedTel':'','HasMedPlan':''},{'MedId':25125,'InhCode':'0001410289','InfoId':155,'MedName':'张仕追','MedOutdate':'2015-10-10','MedVisit':null,'DeptName':'糖尿病科','DeptId':1076,'WeChatSign':'','TelStatus':null,'InHospCount':'1','HasMedTel':'','HasMedPlan':''},{'MedId':25063,'InhCode':'0001410157','InfoId':93,'MedName':'张顺德','MedOutdate':'2015-10-10','MedVisit':null,'DeptName':'糖尿病科','DeptId':1076,'WeChatSign':'','TelStatus':null,'InHospCount':'1','HasMedTel':'','HasMedPlan':''},{'MedId':24998,'InhCode':'0001409857','InfoId':28,'MedName':'李丽华','MedOutdate':'2015-10-09','MedVisit':null,'DeptName':'糖尿病科','DeptId':1076,'WeChatSign':'','TelStatus':null,'InHospCount':'1','HasMedTel':'','HasMedPlan':''},{'MedId':25008,'InhCode':'0001409938','InfoId':38,'MedName':'陈志珍','MedOutdate':'2015-10-09','MedVisit':null,'DeptName':'糖尿病科','DeptId':1076,'WeChatSign':'','TelStatus':null,'InHospCount':'1','HasMedTel':'','HasMedPlan':''},{'MedId':25037,'InhCode':'0001410084','InfoId':67,'MedName':'何世凤','MedOutdate':'2015-10-09','MedVisit':null,'DeptName':'糖尿病科','DeptId':1076,'WeChatSign':'','TelStatus':null,'InHospCount':'1','HasMedTel':'','HasMedPlan':''},{'MedId':24972,'InhCode':'0001408306','InfoId':2,'MedName':'秦计明','MedOutdate':'2015-10-09','MedVisit':null,'DeptName':'糖尿病科','DeptId':1076,'WeChatSign':'W','TelStatus':null,'InHospCount':'1','HasMedTel':'','HasMedPlan':''},{'MedId':25021,'InhCode':'0001410033','InfoId':51,'MedName':'彭豫斌','MedOutdate':'2015-10-08','MedVisit':null,'DeptName':'糖尿病科','DeptId':1076,'WeChatSign':'','TelStatus':null,'InHospCount':'1','HasMedTel':'','HasMedPlan':''},{'MedId':25005,'InhCode':'0001409941','InfoId':35,'MedName':'胡斌','MedOutdate':'2015-10-06','MedVisit':null,'DeptName':'糖尿病科','DeptId':1076,'WeChatSign':'','TelStatus':null,'InHospCount':'1','HasMedTel':'','HasMedPlan':''},{'MedId':25262,'InhCode':'0001410454','InfoId':292,'MedName':'杨冬梅','MedOutdate':'2015-10-06','MedVisit':null,'DeptName':'糖尿病科','DeptId':1076,'WeChatSign':'','TelStatus':null,'InHospCount':'1','HasMedTel':'','HasMedPlan':''},{'MedId':25025,'InhCode':'0001410056','InfoId':55,'MedName':'王秀华','MedOutdate':'2015-10-04','MedVisit':null,'DeptName':'糖尿病科','DeptId':1076,'WeChatSign':'','TelStatus':null,'InHospCount':'1','HasMedTel':'','HasMedPlan':''},{'MedId':25089,'InhCode':'0001410218','InfoId':119,'MedName':'杨琦','MedOutdate':'2015-10-02','MedVisit':null,'DeptName':'糖尿病科','DeptId':1076,'WeChatSign':'','TelStatus':null,'InHospCount':'1','HasMedTel':'','HasMedPlan':''},{'MedId':25143,'InhCode':'0001410304','InfoId':173,'MedName':'汤凤新','MedOutdate':'2015-10-02','MedVisit':null,'DeptName':'糖尿病科','DeptId':1076,'WeChatSign':'','TelStatus':null,'InHospCount':'1','HasMedTel':'','HasMedPlan':''},{'MedId':25075,'InhCode':'0001410186','InfoId':105,'MedName':'周晓东','MedOutdate':'2015-10-01','MedVisit':null,'DeptName':'糖尿病科','DeptId':1076,'WeChatSign':'','TelStatus':null,'InHospCount':'1','HasMedTel':'','HasMedPlan':''},{'MedId':25116,'InhCode':'0001410273','InfoId':146,'MedName':'三保','MedOutdate':'2015-10-01','MedVisit':null,'DeptName':'糖尿病科','DeptId':1076,'WeChatSign':'','TelStatus':null,'InHospCount':'1','HasMedTel':'','HasMedPlan':''},{'MedId':25118,'InhCode':'0001410271','InfoId':148,'MedName':'李长凤','MedOutdate':'2015-10-01','MedVisit':null,'DeptName':'糖尿病科','DeptId':1076,'WeChatSign':'','TelStatus':null,'InHospCount':'1','HasMedTel':'','HasMedPlan':''}],'succeed':true,'msg':''}");

            var serializer = new JsonSerializer();
            var ret = serializer.Deserialize<Result<MedInpatientsViewModel>>(s.ToString());
            return await Task.FromResult(ret);
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
            var serializer = new JsonSerializer();

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

        public class AttrTestConverter : JsonConverter<AttrTest>
        {
            public override void WriteJson(JsonSerializer serializer, JsonWriter writer, object obj)
            {
                writer.WriteRaw($"\"{((AttrTest)obj).Name}\"");
            }

            public override object ReadJson(JsonSerializer serializer, JsonReader reader, Type dataType)
            {
                var json = reader.ReadRaw();
                return new AttrTest { Name = json };
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

            [TextPropertyConverter(typeof(FullDateTimeXmlConverter))]
            public DateTime Birthday { get; set; }

            public DateTime WorkTime { get; set; }

            public int? A { get; set; }

            public decimal? Age { get; set; }

            public List<WorkRecord> WorkRecords { get; set; }

            public Type DataType { get; set; }

            public WorkRecord Record { get; set; }
        }

        public class WorkRecord
        {
            public string Company { get; set; }

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
            public JsonText(string json)
            {
                Content = json.Replace("'", "\"");
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

        public class MedInpatientsViewModel
        {
            /// <summary>
            /// 基本信息ID
            /// </summary>
            public int MedId { get; set; }

            /// <summary>
            /// 住院号
            /// </summary>
            public string InhCode { get; set; }

            /// <summary>
            /// 住院病历ID
            /// </summary>
            public int InfoId { get; set; }

            /// <summary>
            /// 患者姓名
            /// </summary>
            public string MedName { get; set; }

            /// <summary>
            /// 出院日期
            /// </summary>
            public string MedOutdate { get; set; }

            /// <summary>
            /// 计划随访状态
            /// </summary>
            public string MedVisit { get; set; }

            /// <summary>
            /// 科室名称
            /// </summary>
            public string DeptName { get; set; }

            /// <summary>
            /// 科室ID
            /// </summary>
            public int? DeptId { get; set; }

            /// <summary>
            /// 是否是微信用户
            /// </summary>
            public string WeChatSign { get; set; }

            /// <summary>
            /// 电话随访状态
            /// </summary>
            public string TelStatus { get; set; }

            /// <summary>
            /// 住院次数
            /// </summary>
            public string InHospCount { get; set; }

            /// <summary>
            /// 之前住院是否有电话随访
            /// </summary>
            public string HasMedTel { get; set; }

            /// <summary>
            /// 之前住院是否有计划随访
            /// </summary>
            public string HasMedPlan { get; set; }

            /// <summary>
            /// 云医平台患者ID。
            /// </summary>
            public int? UserId { get; set; }

            /// <summary>
            /// 云医平台患者姓名。
            /// </summary>
            public string UserName { get; set; }
        }

        [TestMethod]
        public void TestNew()
        {
            var s = "\"42342424/ffadfafd\"";

           var ad = new JsonSerializer().Deserialize<string>(s);
            Console.WriteLine(ad);
        }

        [TestMethod]
        public void TestMethod1()
        {
            var ss = "{ Name: \"aa\", Sex: \"M\" }";
            var sss = new JsonSerializer().Deserialize<People>(ss);
            Console.WriteLine(sss);
        }

        [TestMethod]
        public void TestMethod2()
        {
            var ss = "{ Name: \"aa\", Address: \"M\", C: [] }";
            var sss = new JsonSerializer().Deserialize<IB>(ss);
            Console.WriteLine(sss);
        }

        [TestMethod]
        public void TestMethod3()
        {
            var ss = "{ Name: \"aa\", Sex: \"M\", Farther: {} }";
            var sss = new JsonSerializer().Deserialize<People>(ss);
            Console.WriteLine(sss);
        }

        [TestMethod]
        public void TestSerializeUseFilterConverter()
        {
            var people = new People { Name = "fireasy", Sex = Sex.M, Address = "kunming" };
            var option = new JsonSerializeOption();
            option.Converters.Add(new JsonFilterConverter<People>(s => s.Name, s => s.Sex));
            var serializer = new JsonSerializer(option);
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
    }
}
