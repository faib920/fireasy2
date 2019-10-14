using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fireasy.Data.Entity.Tests.Models;
using System.Linq;
using System.Collections.Generic;
using Fireasy.Data.Entity.Linq;
using System.Threading.Tasks;

namespace Fireasy.Data.Entity.Tests
{
    [TestClass]
    public class EntityTreeTest
    {
        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            InitConfig.Init();
        }

        [TestMethod]
        public void TestHasChildren()
        {
            using (var db = new DbContext())
            {
                var rep = db.CreateTreeRepository<Depts>();
                var yn = db.Depts.FirstOrDefault(s => s.DeptName == "云南");
                var has = rep.HasChildren(yn);
                Assert.IsTrue(has);

                var gz = db.Depts.FirstOrDefault(s => s.DeptName == "贵州");
                has = rep.HasChildren(gz);
                Assert.IsFalse(has);
            }
        }

        [TestMethod]
        public async Task TestHasChildrenAsync()
        {
            using (var db = new DbContext())
            {
                var rep = db.CreateTreeRepository<Depts>();
                var yn = db.Depts.FirstOrDefault(s => s.DeptName == "云南");
                var has = await rep.HasChildrenAsync(yn);
                Assert.IsTrue(has);

                var gz = db.Depts.FirstOrDefault(s => s.DeptName == "贵州");
                has = await rep.HasChildrenAsync(gz);
                Assert.IsFalse(has);
            }
        }

        [TestMethod]
        public void TestQueryChildren()
        {
            using (var db = new DbContext())
            {
                var rep = db.CreateTreeRepository<Depts>();
                var yn = db.Depts.FirstOrDefault(s => s.DeptName == "云南");
                var list = rep.QueryChildren(yn).ToList();
                Assert.AreEqual(list[0].DeptName, "昆明");
            }
        }

        [TestMethod]
        public void TestQueryChildrenExtend()
        {
            using (var db = new DbContext())
            {
                var rep = db.CreateTreeRepository<Depts>();
                var yn = db.Depts.FirstOrDefault(s => s.DeptName == "云南");
                var list = rep.QueryChildren(yn).Select(s => s.ExtendAs<Depts>(() => new Depts { HasChildren = rep.HasChildren(s, t => true) })).ToList();
                Assert.AreEqual(list[0].DeptName, "昆明");
            }
        }

        [TestMethod]
        public void TestRecurrenceParent()
        {
            using (var db = new DbContext())
            {
                var rep = db.CreateTreeRepository<Depts>();
                var wh = db.Depts.FirstOrDefault(s => s.DeptName == "云南");
                var parents = rep.RecurrenceParent(wh).ToList();
                Assert.AreEqual(parents[0].DeptName, "昆明");
                Assert.AreEqual(parents[1].DeptName, "云南");
            }
        }

        [TestMethod]
        public void TestInsert()
        {
            using (var db = new DbContext())
            {
                var tree = db.CreateTreeRepository<Depts>();

                //参照对象
                var refer = db.Depts.FirstOrDefault(s => s.DeptName == "四川");

                var dept = new Depts { DeptName = "成都" };

                tree.Insert(dept, refer);
            }
        }

        [TestMethod]
        public async Task TestInsertAsync()
        {
            using (var db = new DbContext())
            {
                var tree = db.CreateTreeRepository<Depts>();

                //参照对象
                var refer = db.Depts.FirstOrDefault(s => s.DeptName == "四川");

                var dept = new Depts { DeptName = "成都11" };

                await tree.InsertAsync(dept, refer);
            }
        }

        [TestMethod]
        public void TestBatchInsert()
        {
            using (var db = new DbContext())
            {
                var rep = db.CreateTreeRepository<Depts>();
                var sc = db.Depts.FirstOrDefault(s => s.DeptName == "四川");

                var list = new List<Depts>();
                for (var i = 0; i < 20; i++)
                {
                    list.Add(new Depts { DeptName = "四川" + i });
                }

                rep.BatchInsert(list, sc);
            }
        }

        [TestMethod]
        public void TestInsertByIsolation()
        {
            var dc = "dd";

            using (var db = new DbContext())
            {
                var rep = db.CreateTreeRepository<Depts>();
                var sc = db.Depts.FirstOrDefault(s => s.DeptName == "四川");

                //var dept = new Depts { DeptName = "成都" };
                //rep.Insert(dept, sc, EntityTreePosition.Children);

                var dept1 = new Depts { DeptName = "贵州" };
                //rep.Insert(dept1, null, isolation: () => new Depts { DeptType = DeptType.Org });
            }
        }

        [TestMethod]
        public void TestMove()
        {
            using (var db = new DbContext())
            {
                var rep = db.CreateTreeRepository<Depts>();
                var dept = db.Depts.FirstOrDefault(s => s.DeptName == "昆明");
                var parent = db.Depts.FirstOrDefault(s => s.DeptName == "四川");

                rep.Move(dept, parent);

                db.Depts.Update(dept);
            }
        }

        [TestMethod]
        public void TestMoveToRoot()
        {
            using (var db = new DbContext())
            {
                var rep = db.CreateTreeRepository<Depts>();
                var dept = db.Depts.FirstOrDefault(s => s.DeptName == "昆明");

                dept.DeptName = "昆明1";

                rep.Move(dept, null);
            }
        }

        [TestMethod]
        public void TestModifyFullName()
        {
            using (var db = new DbContext())
            {
                var rep = db.CreateTreeRepository<Depts>();
                var dept = db.Depts.FirstOrDefault(s => s.DeptName == "昆明");
                var parent = db.Depts.FirstOrDefault(s => s.DeptName == "云南");

                dept.DeptName = "昆明1";

                rep.Move(dept, parent);
            }
        }
    }
}
