using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fireasy.Data.Entity.Tests.Models;
using System.Linq;
using System.Collections.Generic;

namespace Fireasy.Data.Entity.Tests
{
    [TestClass]
    public class EntityTreeTest
    {
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
                var rep = db.CreateTreeRepository<Depts>();
                var sc = db.Depts.FirstOrDefault(s => s.DeptName == "四川");

                //var dept = new Depts { DeptName = "成都" };
                //rep.Insert(dept, sc, EntityTreePosition.Children);

                var dept1 = new Depts { DeptName = "贵州" };
                rep.Insert(dept1, null);
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
                rep.Insert(dept1, null, isolation: () => new Depts { DeptID = 1, DeptCode = dc });
            }
        }

    }
}
