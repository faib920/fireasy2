using System;
using System.ComponentModel.DataAnnotations;

//[assembly: Fireasy.Data.Entity.Relationship("dff1", typeof(Fireasy.Data.Entity.Tests.Models.Products), typeof(Fireasy.Data.Entity.Tests.Models.OrderDetails), "ProductID=>ProductID")]
//[assembly: Fireasy.Data.Entity.Relationship("dff2", typeof(Fireasy.Data.Entity.Tests.Models.Products), typeof(Fireasy.Data.Entity.Tests.Models.OrderDetails), "ProductID=>Quantity")]

namespace Fireasy.Data.Entity.Tests.Models
{
    [Serializable]
    [EntityMapping("order details", Description = "")]
    [MetadataType(typeof(OrderDetailsMetadata))]
    public partial class OrderDetails : LightEntity<OrderDetails>
    {
        /// <summary>
        /// 获取或设置。
        /// </summary>

        [PropertyMapping(ColumnName = "OrderID", Description = "", IsPrimaryKey = true, IsNullable = false)]
        public virtual long OrderID { get; set; }

        /// <summary>
        /// 获取或设置。
        /// </summary>

        [PropertyMapping(ColumnName = "ProductID", Description = "", IsPrimaryKey = true, IsNullable = false)]
        public virtual long Product1ID { get; set; }

        /// <summary>
        /// 获取或设置。
        /// </summary>

        [PropertyMapping(ColumnName = "UnitPrice", Description = "", IsNullable = false)]
        public virtual int? UnitPrice { get; set; }

        /// <summary>
        /// 获取或设置。
        /// </summary>

        [PropertyMapping(ColumnName = "Quantity", Description = "", IsNullable = false)]
        public virtual long Quantity { get; set; }

        /// <summary>
        /// 获取或设置。
        /// </summary>

        [PropertyMapping(ColumnName = "Discount", Description = "", IsNullable = false)]
        public virtual double Discount { get; set; }

        /// <summary>
        /// 获取或设置关联 <see cref="products"/> 对象。
        /// </summary>
        //[RelationshipAssign("ProductID", "ProductID")]
        //[RelationshipUse("ProductID")]
        public virtual Products Products { get; set; }

        /// <summary>
        /// 获取或设置关联 <see cref="products"/> 对象。
        /// </summary>
        [RelationshipAssign("ProductID", "Quantity")]
        //[RelationshipUse("Quantity")]
        public virtual Products Products1 { get; set; }

        /// <summary>
        /// 获取或设置关联 <see cref="orders"/> 对象。
        /// </summary>
        public virtual Orders Orders { get; set; }

        public string ReorderLevelName { get; set; }

    }

    public class OrderDetailsMetadata
    {
        /// <summary>
        /// 属性 OrderID 的验证特性。
        /// </summary>
        [Required]
        public object OrderID { get; set; }

        /// <summary>
        /// 属性 ProductID 的验证特性。
        /// </summary>
        [Required]
        public object ProductID { get; set; }

        /// <summary>
        /// 属性 UnitPrice 的验证特性。
        /// </summary>
        [Required]
        public object UnitPrice { get; set; }

        /// <summary>
        /// 属性 Quantity 的验证特性。
        /// </summary>
        [Required]
        public object Quantity { get; set; }

        /// <summary>
        /// 属性 Discount 的验证特性。
        /// </summary>
        [Required]
        public object Discount { get; set; }

    }
}