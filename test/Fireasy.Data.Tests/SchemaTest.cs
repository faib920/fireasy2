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
            using (var db = DatabaseFactory.CreateDatabase("mssql1"))
            {
                var schema = db.Provider.GetService<ISchemaProvider>();
                action(db, schema);
            }
        }

        [TestMethod]
        public void TestGetDatabases()
        {
            Invoke((db, schema) =>
            {
                foreach (var database in schema.GetSchemas<DataBase>(db))
                {
                    Console.WriteLine(database.Name);
                }
            });
        }

        [TestMethod]
        public void TestGetTables()
        {
            Invoke((db, schema) =>
            {
                var parameter = db.Provider.GetConnectionParameter(db.ConnectionString);
                foreach (var table in schema.GetSchemas<Table>(db, s => s.Schema == parameter.Schema))
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
                foreach (var column in schema.GetSchemas<Column>(db, s => s.TableName == "TB_CORP"))
                {
                    Console.WriteLine($"Name: {column.Name}\tIsPrimaryKey: {column.IsNullable}\tDescription: {column.Description}");
                }
            });
        }

        [TestMethod]
        public void TestForeignKeys()
        {
            Invoke((db, schema) =>
            {
                var parameter = db.Provider.GetConnectionParameter(db.ConnectionString);
                foreach (var fk in schema.GetSchemas<ForeignKey>(db, s => s.Schema == parameter.Schema))
                {
                    Console.WriteLine(fk.Name);
                }
            });
        }
    }
}
