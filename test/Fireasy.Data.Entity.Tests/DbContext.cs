using Fireasy.Data.Entity.Initializers;
using Fireasy.Data.Entity.Tests.Models;
using Fireasy.Data.Provider;
using System;

namespace Fireasy.Data.Entity.Tests
{
    public class DbContext : EntityContext
    {
        protected override void OnConfiguring(EntityContextOptionsBuilder builder)
        {
            builder.Options.NotifyEvents = true;
            //builder.UseOracleTrigger<Orders>().UseOracleTrigger<Products>().UseCodeFirst();
            //builder.UseEnvironment(s => s.AddVariable("Year", "2009")).UseCodeFirst();
            //builder.UseCodeFirst();
            base.OnConfiguring(builder);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        public EntityRepository<Products> Products { get; set; }

        public EntityRepository<Categories> Categories { get; set; }

        public EntityRepository<Customers> Customers { get; set; }

        public EntityRepository<Orders> Orders { get; set; }

        public EntityRepository<OrderDetails> OrderDetails { get; set; }

        public EntityRepository<Depts> Depts { get; set; }
    }

    public class MyDatabase : Database
    {
        public MyDatabase(ConnectionString c, IProvider p)
            : base(c, p)
        {
        }
    }
}
