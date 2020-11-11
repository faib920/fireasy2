using Fireasy.Data.Entity.Initializers;
using Fireasy.Data.Entity.Metadata.Builders;
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
            builder.Options.CacheExecution = false;
            //builder.UseMongoDB("server=mongodb://localhost;database=test");
            base.OnConfiguring(builder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Orders>(s =>
            {
                s.Property(t => t.OrderID).IsPrimaryKey();

                s.HasMany(t => t.OrderDetailses);
                s.HasOne(t => t.Customers);

            }).ToTable("orders");

            builder.Entity<Products>(s =>
            {
                s.Property(t => t.Id).HasColumnName("productid").IsPrimaryKey();

            }).ToTable("products");

            builder.Entity<Customers>(s =>
            {
                s.Property(t => t.CustomerID).IsPrimaryKey();

                s.HasMany(t => t.Orderses);

            }).ToTable("customers");
        }

        protected override bool Dispose(bool disposing)
        {
            return base.Dispose(disposing);
        }

        public IRepository<Products> Products { get; set; }

        public EntityRepository<Categories> Categories { get; set; }

        public EntityRepository<Customers> Customers { get; set; }
        public EntityRepository<Customers1> Customer1s { get; set; }

        public EntityRepository<Orders> Orders { get; set; }

        public EntityRepository<OrderDetails> OrderDetails { get; set; }

        public EntityRepository<Depts> Depts { get; set; }

        public EntityRepository<OperateLog> OperateLog { get; set; }
    }
    public class DbContext1 : EntityContext
    {
        public DbContext1()
            : base("mysql")
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
