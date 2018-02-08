using System;
using Fireasy.Data.Entity;
using Fireasy.Data.Entity.Tests.Models;

namespace Fireasy.Data.Entity.Tests
{
    public class DbContext : EntityContext
    {
        public DbContext()
            : base (new EntityContextOptions { AutoCreateTables = true })
        {
        }

        public EntityRepository<Products> Products { get; set; }

        public EntityRepository<Categories> Categories { get; set; }

        public EntityRepository<Customers> Customers { get; set; }

        public EntityRepository<Orders> Orders { get; set; }

        public EntityRepository<OrderDetails> OrderDetails { get; set; }

        public EntityRepository<Depts> Depts { get; set; }

        protected override void OnRespositoryCreated(Type entityType)
        {
            if (entityType == typeof(Products))
            {
                Products.Insert(new Models.Products { ProductName = "aa" });
            }
        }

        protected override void OnRespositoryCreateFailed(Type entityType, Exception exception)
        {
            base.OnRespositoryCreateFailed(entityType, exception);
        }
    }
}
