using Fireasy.Data.Entity;
using Fireasy.Data.Entity.Tests.Models;

namespace Fireasy.Data.Entity.Tests
{
    public class DbContext : EntityContext
    {
        public EntityRepository<Products> Products { get; set; }

        public EntityRepository<Categories> Categories { get; set; }

        public EntityRepository<Customers> Customers { get; set; }

        public EntityRepository<Orders> Orders { get; set; }

        public EntityRepository<OrderDetails> OrderDetails { get; set; }

        public EntityRepository<Depts> Depts { get; set; }
    }
}
