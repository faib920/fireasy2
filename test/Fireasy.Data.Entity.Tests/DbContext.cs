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
            builder.Entity<Orders>(e =>
                {
                    e.Property(p => p.OrderID)
                        .IsPrimaryKey()
                        .HasIdentity(IdentityGenerateType.AutoIncrement);

                    e.Property(p => p.CustomerID).HasColumnName("customerid");
                    e.Property(p => p.EmployeeID);
                    e.Property(p => p.OrderDate);

                    e.HasMany(r => r.OrderDetailses)
                        .WithOne().HasPrimaryKey(t => t.OrderID)
                        .HasForeignKey(t => t.OrderID);

                    e.HasOne(r => r.Customers)
                        .WithMany()
                        .HasPrimaryKey(t => t.CompanyName)
                        .HasForeignKey(t => t.CustomerID);
                })
                .ToTable("orders");

            builder.Entity<OrderDetails>(s =>
                {
                    s.Property(p => p.OrderID);
                    s.Property(p => p.Product1ID);
                })
                .ToTable("order details");

            builder.Entity<Customers>(s =>
                {
                    s.Property(p => p.CustomerID).IsPrimaryKey();
                    s.Property(p => p.CompanyName);
                })
                .ToTable("customers");


            base.OnModelCreating(builder);
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
