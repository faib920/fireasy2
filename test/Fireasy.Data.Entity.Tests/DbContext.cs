using Fireasy.Data.Entity.Tests.Models;
using Fireasy.Data.Provider;
using System;

namespace Fireasy.Data.Entity.Tests
{
    public class DbContext : EntityContext
    {
        public DbContext()
            : base(new EntityContextOptions { AutoCreateTables = true, NotifyEvents = true })
        {
            System.Console.WriteLine(Guid.NewGuid());
        }

        protected override void Dispose(bool disposing)
        {
            System.Console.WriteLine("Dispose");
            base.Dispose(disposing);
        }

        public EntityRepository<Products> Products { get; set; }

        public EntityRepository<Categories> Categories { get; set; }

        public EntityRepository<Customers> Customers { get; set; }

        public EntityRepository<Orders> Orders { get; set; }

        public EntityRepository<OrderDetails> OrderDetails { get; set; }

        public EntityRepository<Depts> Depts { get; set; }

        protected override void OnRespositoryChanged(RespositoryChangedEventArgs args)
        {
            if (args.Succeed && args.EntityType == typeof(Products) && args.EventType == RespositoryChangeEventType.CreateTable)
            {
                Products.Insert(new Models.Products { ProductName = "aa" });
            }
        }
    }

    public class MyDatabase : Database
    {
        public MyDatabase(ConnectionString c, IProvider p)
            : base(c, p)
        {
        }
    }
}
