using System;
using Fireasy.Data.Entity.Tests.Models;
using Fireasy.Data.Provider;

namespace Fireasy.Data.Entity.Tests
{
    public class DbContext : EntityContext
    {
        public DbContext()
        {
        }

        public EntityRepository<Products> Products { get; set; }

        public EntityRepository<Categories> Categories { get; set; }

        public EntityRepository<Customers> Customers { get; set; }

        public EntityRepository<Orders> Orders { get; set; }

        public EntityRepository<OrderDetails> OrderDetails { get; set; }

        public EntityRepository<Depts> Depts { get; set; }

        protected override void OnRespositoryCreated(RespositoryCreatedEventArgs args)
        {
            if (args.Succeed && args.EntityType == typeof(Products))
            {
                Products.Insert(new Models.Products { ProductName = "aa" });
            }
        }
    }

    public class MyDatabase : Database
    {
        public MyDatabase(ConnectionString c, IProvider p)
            : base (c, p)
        {
        }
    }
}
