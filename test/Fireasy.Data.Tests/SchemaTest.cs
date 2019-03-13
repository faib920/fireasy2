using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fireasy.Data.Schema;

namespace Fireasy.Data.Tests
{
    [TestClass]
    public class SchemaTest
    {
        public SchemaTest()
        {
            InitConfig.Init();
        }

        private void Invoke(Action<IDatabase, ISchemaProvider> action)
        {
            using (var db = DatabaseFactory.CreateDatabase("pqsql"))
            {
                var schema = db.Provider.GetService<ISchemaProvider>();
                action(db, schema);
            }
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
                    Console.WriteLine($"Name: {column.Name}\tIsPrimaryKey: {column.IsPrimaryKey}\tDescription: {column.Description}");
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
                foreach (var column in schema.GetSchemas<Column>(db, s => s.TableName == "orders" && s.Name == "OrderID"))
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
    }
}
