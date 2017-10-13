using Fireasy.Data.Batcher;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fireasy.Data.Tests
{
    [TestClass]
    public class BatcherTest
    {
        [TestMethod]
        public void TestDataTable()
        {
            var tb = new DataTable("tt");
            tb.Columns.Add("guid", typeof(Guid));

            var row = tb.NewRow();
            row["guid"] = Guid.NewGuid();

            tb.Rows.Add(row);

            using (var db = DatabaseFactory.CreateDatabase("mysql"))
            {
                var batcher = db.Provider.GetService<IBatcherProvider>();
                batcher.Insert(db, tb);
            }
        }
    }
}
