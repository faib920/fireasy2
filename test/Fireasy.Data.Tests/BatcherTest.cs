using Fireasy.Data.Batcher;
using Fireasy.Data.Provider;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Data.Tests
{
    [TestClass]
    public class BatcherTest
    {
        [TestMethod]
        public void TestDataTable()
        {
            var tb = new DataTable("batch");
            tb.Columns.Add("id", typeof(int));
            tb.Columns.Add("a1", typeof(string));
            tb.Columns.Add("a2", typeof(string));

            for (var i = 0; i < 600; i++)
            {
                var row = tb.NewRow();
                row["id"] = i + 1;
                row["a1"] = "aaaaa" + i;
                row["a2"] = "bbbbb" + i;

                tb.Rows.Add(row);
            }

            using (var db = DatabaseFactory.CreateDatabase())
            {
                var batcher = db.Provider.GetService<IBatcherProvider>();
                batcher.Insert(db, tb);
            }
        }

        [TestMethod]
        public void TestList()
        {
            var list = new List<object>();

            for (var i = 0; i < 800; i++)
            {
                list.Add(new
                {
                    id = i + 1,
                    a1 = "aaaaa" + i,
                    a2 = "bbbbb" + i
                });
            }

            using (var db = DatabaseFactory.CreateDatabase())
            {
                var batcher = db.Provider.GetService<IBatcherProvider>();
                Console.WriteLine(DateTime.Now);
                batcher.Insert(db, list, "batch");
                Console.WriteLine(DateTime.Now);

                Console.WriteLine("后续代码");
            }
        }


        [TestMethod]
        public async Task TestListAsync()
        {
            var list = new List<object>();

            for (var i = 0; i < 800; i++)
            {
                list.Add(new
                {
                    id = i + 1,
                    a1 = "aaaaa" + i,
                    a2 = "bbbbb" + i
                });
            }

            using (var db = DatabaseFactory.CreateDatabase())
            {
                var batcher = db.Provider.GetService<IBatcherProvider>();
                Console.WriteLine(DateTime.Now);
                await batcher.InsertAsync(db, list, "batch");
                Console.WriteLine(DateTime.Now);

                Console.WriteLine("后续代码");
            }
        }

    }
}
