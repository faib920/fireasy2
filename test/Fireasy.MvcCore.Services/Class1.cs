using Fireasy.Data.Entity;
using System;
using System.Linq;

namespace Fireasy.MvcCore.Services
{
    public class Service : IService
    {
        private TestContext context;

        public Service(TestContext context)
        {
            this.context = context;
        }

        public void Update()
        {
            Console.WriteLine(context.g);
            Console.WriteLine(DateTime.Now + " " + context.Orders.Count());
            context.Orders.Update(() => new Orders { OrderDate = DateTime.Now }, s => true);
        }
    }

    public interface IService
    {
        void Update();
    }


    public class TestContext : EntityContext
    {
        public Guid g = Guid.NewGuid();

        public TestContext(EntityContextOptions options)
            : base(options)
        {
        }

        public EntityRepository<Orders> Orders { get; set; }
    }

    /// <summary>
    ///  实体类。
    /// </summary>
    [Serializable]
    [EntityMapping("orders", Description = "")]
    public partial class Orders : LightEntity<Orders>
    {
        /// <summary>
        /// 获取或设置。
        /// </summary>

        [PropertyMapping(ColumnName = "OrderID", Description = "", IsPrimaryKey = true, GenerateType = IdentityGenerateType.AutoIncrement, IsNullable = false)]
        public virtual long OrderID { get; set; }

        /// <summary>
        /// 获取或设置。
        /// </summary>

        [PropertyMapping(ColumnName = "CustomerID", Description = "", Length = 5, IsNullable = true)]
        public virtual string CustomerID { get; set; }

        /// <summary>
        /// 获取或设置。
        /// </summary>

        [PropertyMapping(ColumnName = "EmployeeID", Description = "", IsNullable = true)]
        public virtual long? EmployeeID { get; set; }

        /// <summary>
        /// 获取或设置。
        /// </summary>

        [PropertyMapping(ColumnName = "OrderDate", Description = "", Length = 2147483647, IsNullable = true)]
        public virtual DateTime? OrderDate { get; set; }

        /// <summary>
        /// 获取或设置。
        /// </summary>

        [PropertyMapping(ColumnName = "RequiredDate", Description = "", Length = 2147483647, IsNullable = true)]
        public virtual DateTime? RequiredDate { get; set; }

        /// <summary>
        /// 获取或设置。
        /// </summary>

        [PropertyMapping(ColumnName = "ShippedDate", Description = "", Length = 2147483647, IsNullable = true)]
        public virtual string ShippedDate { get; set; }

        /// <summary>
        /// 获取或设置。
        /// </summary>

        [PropertyMapping(ColumnName = "ShipVia", Description = "", IsNullable = true)]
        public virtual long? ShipVia { get; set; }

        /// <summary>
        /// 获取或设置。
        /// </summary>

        [PropertyMapping(ColumnName = "Freight", Description = "", IsNullable = true)]
        public virtual long? Freight { get; set; }

        /// <summary>
        /// 获取或设置。
        /// </summary>

        [PropertyMapping(ColumnName = "ShipName", Description = "", Length = 40, IsNullable = true)]
        public virtual string ShipName { get; set; }

        /// <summary>
        /// 获取或设置。
        /// </summary>

        [PropertyMapping(ColumnName = "ShipAddress", Description = "", Length = 60, IsNullable = true)]
        public virtual string ShipAddress { get; set; }

        /// <summary>
        /// 获取或设置。
        /// </summary>

        [PropertyMapping(ColumnName = "ShipCity", Description = "", Length = 15, IsNullable = true)]
        public virtual string ShipCity { get; set; }

        /// <summary>
        /// 获取或设置。
        /// </summary>

        [PropertyMapping(ColumnName = "ShipRegion", Description = "", Length = 15, IsNullable = true)]
        public virtual string ShipRegion { get; set; }

        /// <summary>
        /// 获取或设置。
        /// </summary>

        [PropertyMapping(ColumnName = "ShipPostalCode", Description = "", Length = 10, IsNullable = true)]
        public virtual string ShipPostalCode { get; set; }

        /// <summary>
        /// 获取或设置。
        /// </summary>

        [PropertyMapping(ColumnName = "ShipCountry", Description = "", Length = 15, IsNullable = true)]
        public virtual string ShipCountry { get; set; }
    }

}
