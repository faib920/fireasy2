using System;
using System.ComponentModel.DataAnnotations;

namespace Fireasy.Data.Entity.Tests.Models
{
    /// <summary>
    ///  实体类。
    /// </summary>
    [Serializable]
    [EntityMapping("categories", Description = "", IsReadonly = true)]
    [MetadataType(typeof(CategoriesMetadata))]
    public partial class Categories : LightEntity<Categories>
    {
        /// <summary>
        /// 获取或设置。
        /// </summary>

        [PropertyMapping(ColumnName = "CategoryID", Description = "", IsPrimaryKey = true, IsNullable = false)]
        public virtual long CategoryID { get; set; }

        /// <summary>
        /// 获取或设置。
        /// </summary>

        [PropertyMapping(ColumnName = "CategoryName", Description = "", Length = 15, IsNullable = false)]
        public virtual string CategoryName { get; set; }

        /// <summary>
        /// 获取或设置。
        /// </summary>

        [PropertyMapping(ColumnName = "Description", Description = "", Length = 2147483647, IsNullable = true)]
        public virtual string Description { get; set; }

        /// <summary>
        /// 获取或设置。
        /// </summary>

        [PropertyMapping(ColumnName = "Picture", Description = "", IsNullable = true)]
        public virtual byte[] Picture { get; set; }

        /// <summary>
        /// 获取或设置 <see cref="products"/> 的子实体集。
        /// </summary>
        public virtual EntitySet<Products> productses { get; set; }

    }

    public class CategoriesMetadata
    {
        /// <summary>
        /// 属性 CategoryID 的验证特性。
        /// </summary>
        [Required]
        public object CategoryID { get; set; }

        /// <summary>
        /// 属性 CategoryName 的验证特性。
        /// </summary>
        [Required]
        [StringLength(15)]
        public object CategoryName { get; set; }

        /// <summary>
        /// 属性 Description 的验证特性。
        /// </summary>
        [StringLength(2147483647)]
        public object Description { get; set; }

        /// <summary>
        /// 属性 Picture 的验证特性。
        /// </summary>
        public object Picture { get; set; }

    }
}