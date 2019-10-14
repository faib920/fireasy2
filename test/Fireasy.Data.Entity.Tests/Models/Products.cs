using Fireasy.Common;
using System;
using System.ComponentModel.DataAnnotations;

namespace Fireasy.Data.Entity.Tests.Models
{
    [Serializable]
    [EntityMapping("products", Description = "产品类")]
    //[MetadataType(typeof(ProductsMetadata))]
    public partial class Products : LightEntity<Products>
    {
        [PropertyMapping(ColumnName = "ProductID", Description = "", IsPrimaryKey = true, GenerateType = IdentityGenerateType.AutoIncrement, IsNullable = false)]
        public virtual long ProductID { get; set; }

        [PropertyMapping(ColumnName = "ProductName", Description = "", Length = 40, IsNullable = false)]
        [Required]
        [StringLength(40)]
        public virtual string ProductName { get; set; }

        [PropertyMapping(ColumnName = "SupplierID", Description = "", IsNullable = true)]
        public virtual long? SupplierID { get; set; }

        [PropertyMapping(ColumnName = "CategoryID", Description = "", IsNullable = true)]
        public virtual long? CategoryID { get; set; }

        [PropertyMapping(ColumnName = "QuantityPerUnit", Description = "", Length = 20, IsNullable = true)]
        public virtual string QuantityPerUnit { get; set; }

        [PropertyMapping(ColumnName = "UnitPrice", Description = "", IsNullable = true)]
        public virtual decimal? UnitPrice { get; set; }

        [PropertyMapping(ColumnName = "UnitsInStock", Description = "", IsNullable = true, DefaultValue = 34)]
        public virtual long? UnitsInStock { get; set; }

        [PropertyMapping(ColumnName = "UnitsOnOrder", Description = "", IsNullable = true)]
        public virtual long? UnitsOnOrder { get; set; }

        [PropertyMapping(ColumnName = "ReorderLevel", Description = "", IsNullable = true)]
        public virtual RecorderLevel ReorderLevel { get; set; }

        [PropertyMapping(ColumnName = "Discontinued", Description = "", IsNullable = false)]
        public virtual long Discontinued { get; set; }

        [PropertyMapping(ColumnName = "Photo", Description = "", IsNullable = true, DataType = System.Data.DbType.Binary)]
        public virtual byte[] Photo { get; set; }

        public virtual Categories categories { get; set; }

        public virtual EntitySet<OrderDetails> OrderDetailses { get; set; }

        public string ReorderLevelName { get; set; }
    }

    public class ProductsMetadata
    {
        /// <summary>
        /// 属性 ProductID 的验证特性。
        /// </summary>
        [Required]
        public object ProductID { get; set; }

        /// <summary>
        /// 属性 ProductName 的验证特性。
        /// </summary>
        public object ProductName { get; set; }

        /// <summary>
        /// 属性 SupplierID 的验证特性。
        /// </summary>
        public object SupplierID { get; set; }

        /// <summary>
        /// 属性 CategoryID 的验证特性。
        /// </summary>
        public object CategoryID { get; set; }

        /// <summary>
        /// 属性 QuantityPerUnit 的验证特性。
        /// </summary>
        [StringLength(20)]
        public object QuantityPerUnit { get; set; }

        /// <summary>
        /// 属性 UnitPrice 的验证特性。
        /// </summary>
        public object UnitPrice { get; set; }

        /// <summary>
        /// 属性 UnitsInStock 的验证特性。
        /// </summary>
        public object UnitsInStock { get; set; }

        /// <summary>
        /// 属性 UnitsOnOrder 的验证特性。
        /// </summary>
        public object UnitsOnOrder { get; set; }

        /// <summary>
        /// 属性 ReorderLevel 的验证特性。
        /// </summary>
        public object ReorderLevel { get; set; }

        /// <summary>
        /// 属性 Discontinued 的验证特性。
        /// </summary>
        [Required]
        public object Discontinued { get; set; }

    }
}