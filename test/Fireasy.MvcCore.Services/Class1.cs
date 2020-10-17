using Fireasy.Common.Ioc;
using Fireasy.Common.Logging;
using Fireasy.Data.Entity;
using Fireasy.Data.Entity.Linq;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fireasy.MvcCore.Services
{
    public class Service : IService, ITransientService
    {
        private TestContext _context;

        public Service(TestContext context)
        {
            this._context = context;
        }

        public async Task<int> GetCountAsync()
        {
            return await _context.Orders.CountAsync();
        }
    }

    public class Service1 : IService1, ITransientService
    {
        private TestContext context;

        public Service1(TestContext context)
        {
            this.context = context;
        }

        public async Task Update()
        {
            var c = await context.Orders.CountAsync();
            Console.WriteLine(DateTime.Now + " " + c);
            await context.Orders.UpdateAsync(() => new Orders { OrderDate = DateTime.Now }, s => true);

            using (var cc = new TestContext())
            {
                Console.WriteLine("ddd");
            }
        }
    }

    public interface IService
    {
        Task<int> GetCountAsync();
    }

    public interface IService1
    {
        Task Update();
    }

    public class EntityOption
    {
        public string Name { get; set; }
    }

    public class TestContext : EntityContext
    {
        private static IServiceProvider sp;

        public TestContext()
        {
        }

        public TestContext(IServiceProvider serviceProvider, EntityContextOptions<TestContext> options, ILogger logger)
            : base(serviceProvider, options)
        {
            if (sp == null)
            {
                sp = serviceProvider;
            }
        }

        public EntityRepository<Orders> Orders { get; set; }

        protected override bool Dispose(bool disposing)
        {
            return base.Dispose(disposing);
        }
    }

    public class TestContext1 : EntityContext
    {
        public TestContext1(IServiceProvider serviceProvider, EntityContextOptions<TestContext1> options)
            : base(serviceProvider, options)
        {
        }

        public EntityRepository<Orders> Orders { get; set; }

        protected override bool Dispose(bool disposing)
        {
            return base.Dispose(disposing);
        }

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
