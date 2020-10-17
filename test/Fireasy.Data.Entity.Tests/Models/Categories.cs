using System;
using System.ComponentModel.DataAnnotations;

namespace Fireasy.Data.Entity.Tests.Models
{
    /// <summary>
    ///  ʵ���ࡣ
    /// </summary>
    [Serializable]
    [EntityMapping("categories", Description = "", IsReadonly = true)]
    [MetadataType(typeof(CategoriesMetadata))]
    public partial class Categories : LightEntity<Categories>
    {
        /// <summary>
        /// ��ȡ�����á�
        /// </summary>

        [PropertyMapping(ColumnName = "CategoryID", Description = "", IsPrimaryKey = true, IsNullable = false)]
        public virtual long CategoryID { get; set; }

        /// <summary>
        /// ��ȡ�����á�
        /// </summary>

        [PropertyMapping(ColumnName = "CategoryName", Description = "", Length = 15, IsNullable = false)]
        public virtual string CategoryName { get; set; }

        /// <summary>
        /// ��ȡ�����á�
        /// </summary>

        [PropertyMapping(ColumnName = "Description", Description = "", Length = 2147483647, IsNullable = true)]
        public virtual string Description { get; set; }

        /// <summary>
        /// ��ȡ�����á�
        /// </summary>

        [PropertyMapping(ColumnName = "Picture", Description = "", IsNullable = true)]
        public virtual byte[] Picture { get; set; }

        /// <summary>
        /// ��ȡ������ <see cref="products"/> ����ʵ�弯��
        /// </summary>
        public virtual EntitySet<Products> productses { get; set; }

    }

    public class CategoriesMetadata
    {
        /// <summary>
        /// ���� CategoryID ����֤���ԡ�
        /// </summary>
        [Required]
        public object CategoryID { get; set; }

        /// <summary>
        /// ���� CategoryName ����֤���ԡ�
        /// </summary>
        [Required]
        [StringLength(15)]
        public object CategoryName { get; set; }

        /// <summary>
        /// ���� Description ����֤���ԡ�
        /// </summary>
        [StringLength(2147483647)]
        public object Description { get; set; }

        /// <summary>
        /// ���� Picture ����֤���ԡ�
        /// </summary>
        public object Picture { get; set; }

    }
}