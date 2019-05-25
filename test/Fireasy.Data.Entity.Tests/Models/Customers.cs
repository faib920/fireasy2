using System;
using System.ComponentModel.DataAnnotations;

namespace Fireasy.Data.Entity.Tests.Models
{
    /// <summary>
    ///  实体类。
    /// </summary>
    [Serializable]
    [EntityMapping("customers", Description = "")]
    [MetadataType(typeof(CustomersMetadata))]
    public partial class Customers : LightEntity<Customers>
    {
        /// <summary>
        /// 获取或设置。
        /// </summary>

        [PropertyMapping(ColumnName = "CustomerID", Description = "", IsPrimaryKey = true, Length = 5, IsNullable = false)]
        public virtual string CustomerID { get; set; }

        /// <summary>
        /// 获取或设置。
        /// </summary>

        [PropertyMapping(ColumnName = "CompanyName", Description = "", Length = 40, IsNullable = false)]
        public virtual string CompanyName { get; set; }

        /// <summary>
        /// 获取或设置。
        /// </summary>

        [PropertyMapping(ColumnName = "ContactName", Description = "", Length = 30, IsNullable = true)]
        public virtual string ContactName { get; set; }

        /// <summary>
        /// 获取或设置。
        /// </summary>

        [PropertyMapping(ColumnName = "ContactTitle", Description = "", Length = 30, IsNullable = true)]
        public virtual string ContactTitle { get; set; }

        /// <summary>
        /// 获取或设置。
        /// </summary>

        [PropertyMapping(ColumnName = "Address", Description = "", Length = 60, IsNullable = true)]
        public virtual string Address { get; set; }

        /// <summary>
        /// 获取或设置。
        /// </summary>

        [PropertyMapping(ColumnName = "City", Description = "", Length = 15, IsNullable = true)]
        public virtual string City { get; set; }

        /// <summary>
        /// 获取或设置。
        /// </summary>

        [PropertyMapping(ColumnName = "Region", Description = "", Length = 15, IsNullable = true)]
        public virtual string Region { get; set; }

        /// <summary>
        /// 获取或设置。
        /// </summary>

        [PropertyMapping(ColumnName = "PostalCode", Description = "", Length = 10, IsNullable = true)]
        public virtual string PostalCode { get; set; }

        /// <summary>
        /// 获取或设置。
        /// </summary>

        [PropertyMapping(ColumnName = "Country", Description = "", Length = 15, IsNullable = true)]
        public virtual string Country { get; set; }

        /// <summary>
        /// 获取或设置。
        /// </summary>

        [PropertyMapping(ColumnName = "Phone", Description = "", Length = 24, IsNullable = true)]
        public virtual string Phone { get; set; }

        /// <summary>
        /// 获取或设置。
        /// </summary>

        [PropertyMapping(ColumnName = "Fax", Description = "", Length = 24, IsNullable = true)]
        public virtual string Fax { get; set; }

        [PropertyMapping(ColumnName = "Test1", Description = "", Length = 24, IsNullable = true)]
        public virtual string Test1 { get; set; }

        /// <summary>
        /// 获取或设置 <see cref="orders"/> 的子实体集。
        /// </summary>
        public virtual EntitySet<Orders> Orderses { get; set; }

    }

    public class CustomersMetadata
    {
        /// <summary>
        /// 属性 CustomerID 的验证特性。
        /// </summary>
        [Required]
        [StringLength(5)]
        public object CustomerID { get; set; }

        /// <summary>
        /// 属性 CompanyName 的验证特性。
        /// </summary>
        [Required]
        [StringLength(40)]
        public object CompanyName { get; set; }

        /// <summary>
        /// 属性 ContactName 的验证特性。
        /// </summary>
        [StringLength(30)]
        public object ContactName { get; set; }

        /// <summary>
        /// 属性 ContactTitle 的验证特性。
        /// </summary>
        [StringLength(30)]
        public object ContactTitle { get; set; }

        /// <summary>
        /// 属性 Address 的验证特性。
        /// </summary>
        [StringLength(60)]
        public object Address { get; set; }

        /// <summary>
        /// 属性 City 的验证特性。
        /// </summary>
        [StringLength(15)]
        public object City { get; set; }

        /// <summary>
        /// 属性 Region 的验证特性。
        /// </summary>
        [StringLength(15)]
        public object Region { get; set; }

        /// <summary>
        /// 属性 PostalCode 的验证特性。
        /// </summary>
        [StringLength(10)]
        public object PostalCode { get; set; }

        /// <summary>
        /// 属性 Country 的验证特性。
        /// </summary>
        [StringLength(15)]
        public object Country { get; set; }

        /// <summary>
        /// 属性 Phone 的验证特性。
        /// </summary>
        [StringLength(24)]
        public object Phone { get; set; }

        /// <summary>
        /// 属性 Fax 的验证特性。
        /// </summary>
        [StringLength(24)]
        public object Fax { get; set; }

    }
}