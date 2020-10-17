using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fireasy.Data.Schema;

namespace Fireasy.Data.Tests
{
    [TestClass]
    public class SchemaTest
    {
        private const string instanceName = "access";

        public SchemaTest()
        {
            InitConfig.Init();
        }

        private void Invoke(Action<IDatabase, ISchemaProvider> action)
        {
            using (var db = DatabaseFactory.CreateDatabase(instanceName))
            {
                var schema = db.Provider.GetService<ISchemaProvider>();
                action(db, schema);
            }
        }
        [TestMethod]
        public void TestGetConnectionStringProperty()
        {
            ConnectionString connectionString = "Data Source=localhost;database=northwind;User Id=root;password=faib;";
            var userId = connectionString.Properties.TryGetValue("userid", "user id", "user");
            Assert.AreEqual("root", userId);
        }
        [TestMethod]
        public void TestGeUsers()
        {
            Invoke((db, schema) =>
            {
                foreach (var user in schema.GetSchemas<User>(db))
                {
                    Console.WriteLine(user.Name);
                }
            });
        }

        [TestMethod]
        public void TestGetDatabases()
        {
            Invoke((db, schema) =>
            {
                foreach (var database in schema.GetSchemas<Schema.Database>(db))
                {
                    Console.WriteLine(database.Name);
                }
            });
        }

        [TestMethod]
        public void TestGetDataTypes()
        {
            Invoke((db, schema) =>
            {
                foreach (var dt in schema.GetSchemas<DataType>(db))
                {
                    Console.WriteLine(dt.Name + " " + dt.DbType + " " + dt.SystemType);
                }
            });
        }

        [TestMethod]
        public void TestGetTables()
        {
            Invoke((db, schema) =>
            {
                foreach (var table in schema.GetSchemas<Table>(db))
                {
                    Console.WriteLine(table.Name + "," + table.Type + "," + table.Description);
                }
            });
        }

        [TestMethod]
        public void TestGetTables1()
        {
            Invoke((db, schema) =>
            {
                foreach (var table in schema.GetSchemas<Table>(db, s => s.Name == "products"))
                {
                    Console.WriteLine(table.Name + "," + table.Description);
                }
            });
        }

        [TestMethod]
        public void TestGetColumns()
        {
            Invoke((db, schema) =>
            {
                foreach (var column in schema.GetSchemas<Column>(db))
                {
                    Console.WriteLine($"Name: {column.Name}\tIsPrimaryKey: {column.IsPrimaryKey}\tDescription: {column.Description} {column.DataType}");
                }
            });
        }

        [TestMethod]
        public void TestGetColumns1()
        {
            Invoke((db, schema) =>
            {
                foreach (var column in schema.GetSchemas<Column>(db, s => s.TableName == "orders"))
                {
                    Console.WriteLine($"Name: {column.Name}\tIsPrimaryKey: {column.IsPrimaryKey}\tDescription: {column.Description}");
                }
            });
        }

        [TestMethod]
        public void TestGetColumns2()
        {
            Invoke((db, schema) =>
            {
                foreach (var column in schema.GetSchemas<Column>(db, s => s.TableName == "products$" && s.Name == "ProductID"))
                {
                    Console.WriteLine($"Name: {column.Name}\tIsPrimaryKey: {column.IsPrimaryKey}\tDescription: {column.Description}");
                }
            });
        }

        [TestMethod]
        public void TestForeignKeys()
        {
            Invoke((db, schema) =>
            {
                foreach (var fk in schema.GetSchemas<ForeignKey>(db))
                {
                    Console.WriteLine(fk.PKTable + " " + fk.PKColumn + " " + fk.TableName + " " + fk.ColumnName);
                }
            });
        }

        [TestMethod]
        public void TestForeignKeys1()
        {
            Invoke((db, schema) =>
            {
                foreach (var fk in schema.GetSchemas<ForeignKey>(db, s => s.TableName == "orders"))
                {
                    Console.WriteLine(fk.PKTable + " " + fk.PKColumn + " " + fk.TableName + " " + fk.ColumnName);
                }
            });
        }

        [TestMethod]
        public void TestGetViews()
        {
            Invoke((db, schema) =>
            {
                foreach (var view in schema.GetSchemas<View>(db))
                {
                    Console.WriteLine(view.Name + "," + view.Description);
                }
            });
        }

        [TestMethod]
        public void TestGetViewColumns()
        {
            Invoke((db, schema) =>
            {
                foreach (var column in schema.GetSchemas<ViewColumn>(db))
                {
                    Console.WriteLine(column.ViewName + "," + column.Name);
                }
            });
        }

        [TestMethod]
        public void TestGetProcedures()
        {
            Invoke((db, schema) =>
            {
                foreach (var pro in schema.GetSchemas<Procedure>(db))
                {
                    Console.WriteLine(pro.Name);
                }
            });
        }

        [TestMethod]
        public void TestGetProcedures1()
        {
            Invoke((db, schema) =>
            {
                foreach (var pro in schema.GetSchemas<Procedure>(db, s => s.Name == "procProcessCountRice"))
                {
                    Console.WriteLine(pro.Name);
                }
            });
        }

        [TestMethod]
        public void TestGetProcedureParameters()
        {
            Invoke((db, schema) =>
            {
                foreach (var pro in schema.GetSchemas<ProcedureParameter>(db))
                {
                    Console.WriteLine(pro.Name);
                }
            });
        }

        [TestMethod]
        public void TestGetProcedureParameters1()
        {
            Invoke((db, schema) =>
            {
                foreach (var pro in schema.GetSchemas<ProcedureParameter>(db, s => s.ProcedureName == "procProcessCountRice"))
                {
                    Console.WriteLine(pro.Name);
                }
            });
        }

        [TestMethod]
        public void TestGetProcedureParameters2()
        {
            Invoke((db, schema) =>
            {
                foreach (var pro in schema.GetSchemas<ProcedureParameter>(db, s => s.ProcedureName == "procProcessCountRice" && s.Name == "@endDate"))
                {
                    Console.WriteLine(pro.Name);
                }
            });
        }

        [TestMethod]
        public void TestGetIndexs()
        {
            Invoke((db, schema) =>
            {
                foreach (var index in schema.GetSchemas<Schema.Index>(db))
                {
                    Console.WriteLine(index.TableName + " " + index.Name);
                }
            });
        }

        [TestMethod]
        public void TestGetIndexs1()
        {
            Invoke((db, schema) =>
            {
                foreach (var index in schema.GetSchemas<Schema.Index>(db, s => s.TableName == "ORDERS"))
                {
                    Console.WriteLine(index.TableName + " " + index.Name);
                }
            });
        }

        [TestMethod]
        public void TestGetIndexColumns()
        {
            Invoke((db, schema) =>
            {
                foreach (var column in schema.GetSchemas<IndexColumn>(db))
                {
                    Console.WriteLine(column.TableName + " " + column.IndexName + " " + column.ColumnName);
                }
            });
        }

        [TestMethod]
        public void TestGetIndexColumns1()
        {
            Invoke((db, schema) =>
            {
                foreach (var column in schema.GetSchemas<IndexColumn>(db, s => s.TableName == "ORDERS"))
                {
                    Console.WriteLine(column.TableName + " " + column.IndexName + " " + column.ColumnName);
                }
            });
        }
    }
}
