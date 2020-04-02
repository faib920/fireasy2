using Fireasy.Data.Entity.Initializers;
using Fireasy.Data.Entity.Tests.Models;
using Fireasy.Data.Provider;
using Fireasy.MongoDB;
using System;

namespace Fireasy.Data.Entity.Tests
{
    public class DbContext : EntityContext
    {
        public DbContext()
        {

        }

        protected override void OnConfiguring(EntityContextOptionsBuilder builder)
        {
            //builder.Options.CacheParsing = false;
            builder.Options.CacheExecution = true;
            //builder.UseMongoDB("server=mongodb://localhost;database=test");
            base.OnConfiguring(builder);
        }

        protected override bool Dispose(bool disposing)
        {
            return base.Dispose(disposing);
        }

        public IRepository<Products> Products { get; set; }

        public EntityRepository<Categories> Categories { get; set; }

        public EntityRepository<Customers> Customers { get; set; }

        public EntityRepository<Orders> Orders { get; set; }

        public EntityRepository<OrderDetails> OrderDetails { get; set; }

        public EntityRepository<Depts> Depts { get; set; }

        public EntityRepository<OperateLog> OperateLog { get; set; }
    }

    public class MyDatabase : Database
    {
        public MyDatabase(ConnectionString c, IProvider p)
            : base(c, p)
        {
        }
    }
}
