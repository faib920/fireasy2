using System;
using System.ComponentModel.DataAnnotations;

namespace Fireasy.Data.Entity.Tests.Models
{
    /// <summary>
    ///  ʵ���ࡣ
    /// </summary>
    [Serializable]
    [EntityMapping("customers", Description = "")]
    [MetadataType(typeof(CustomersMetadata))]
    public partial class Customers : LightEntity<Customers>
    {
        public Customers()
        {

        }
        public Customers(string cid)
        {
            CustomerID = cid;
        }
        /// <summary>
        /// ��ȡ�����á�
        /// </summary>

        [PropertyMapping(ColumnName = "CustomerID", Description = "", IsPrimaryKey = true, Length = 5, IsNullable = false)]
        public virtual string CustomerID { get; set; }

        /// <summary>
        /// ��ȡ�����á�
        /// </summary>

        [PropertyMapping(ColumnName = "CompanyName", Description = "", Length = 40, IsNullable = false)]
        public virtual string CompanyName { get; set; }

        /// <summary>
        /// ��ȡ�����á�
        /// </summary>

        [PropertyMapping(ColumnName = "ContactName", Description = "", Length = 30, IsNullable = true)]
        public virtual string ContactName { get; set; }

        /// <summary>
        /// ��ȡ�����á�
        /// </summary>

        [PropertyMapping(ColumnName = "ContactTitle", Description = "", Length = 30, IsNullable = true)]
        public virtual string ContactTitle { get; set; }

        /// <summary>
        /// ��ȡ�����á�
        /// </summary>

        [PropertyMapping(ColumnName = "Address", Description = "", Length = 60, IsNullable = true)]
        public virtual string Address { get; set; }

        /// <summary>
        /// ��ȡ�����á�
        /// </summary>

        [PropertyMapping(ColumnName = "City", Description = "", Length = 15, IsNullable = true)]
        public virtual string City { get; set; }

        /// <summary>
        /// ��ȡ�����á�
        /// </summary>

        [PropertyMapping(ColumnName = "Region", Description = "", Length = 15, IsNullable = true)]
        public virtual string Region { get; set; }

        /// <summary>
        /// ��ȡ�����á�
        /// </summary>

        [PropertyMapping(ColumnName = "PostalCode", Description = "", Length = 10, IsNullable = true)]
        public virtual string PostalCode { get; set; }

        /// <summary>
        /// ��ȡ�����á�
        /// </summary>

        [PropertyMapping(ColumnName = "Country", Description = "", Length = 15, IsNullable = true)]
        public virtual string Country { get; set; }

        /// <summary>
        /// ��ȡ�����á�
        /// </summary>

        [PropertyMapping(ColumnName = "Phone", Description = "", Length = 24, IsNullable = true)]
        public virtual string Phone { get; set; }

        /// <summary>
        /// ��ȡ�����á�
        /// </summary>

        [PropertyMapping(ColumnName = "Fax", Description = "", Length = 24, IsNullable = true)]
        public virtual string Fax { get; set; }

        [PropertyMapping(ColumnName = "Test1", Description = "", Length = 24, IsNullable = true)]
        public virtual string Test1 { get; set; }

        /// <summary>
        /// ��ȡ������ <see cref="orders"/> ����ʵ�弯��
        /// </summary>
        public virtual EntitySet<Orders> Orderses { get; set; }

    }
    [Serializable]
    [EntityMapping("customers", Description = "")]
    [MetadataType(typeof(CustomersMetadata))]
    public partial class Customers1 : LightEntity<Customers1>
    {
        public Customers1(string cid)
        {
            CustomerID = cid;
        }
        /// <summary>
        /// ��ȡ�����á�
        /// </summary>

        [PropertyMapping(ColumnName = "CustomerID", Description = "", IsPrimaryKey = true, Length = 5, IsNullable = false)]
        public virtual string CustomerID { get; set; }

        /// <summary>
        /// ��ȡ�����á�
        /// </summary>

        [PropertyMapping(ColumnName = "CompanyName", Description = "", Length = 40, IsNullable = false)]
        public virtual string CompanyName { get; set; }

        /// <summary>
        /// ��ȡ�����á�
        /// </summary>

        [PropertyMapping(ColumnName = "ContactName", Description = "", Length = 30, IsNullable = true)]
        public virtual string ContactName { get; set; }

        /// <summary>
        /// ��ȡ�����á�
        /// </summary>

        [PropertyMapping(ColumnName = "ContactTitle", Description = "", Length = 30, IsNullable = true)]
        public virtual string ContactTitle { get; set; }

        /// <summary>
        /// ��ȡ�����á�
        /// </summary>

        [PropertyMapping(ColumnName = "Address", Description = "", Length = 60, IsNullable = true)]
        public virtual string Address { get; set; }

        /// <summary>
        /// ��ȡ�����á�
        /// </summary>

        [PropertyMapping(ColumnName = "City", Description = "", Length = 15, IsNullable = true)]
        public virtual string City { get; set; }

        /// <summary>
        /// ��ȡ�����á�
        /// </summary>

        [PropertyMapping(ColumnName = "Region", Description = "", Length = 15, IsNullable = true)]
        public virtual string Region { get; set; }

        /// <summary>
        /// ��ȡ�����á�
        /// </summary>

        [PropertyMapping(ColumnName = "PostalCode", Description = "", Length = 10, IsNullable = true)]
        public virtual string PostalCode { get; set; }

        /// <summary>
        /// ��ȡ�����á�
        /// </summary>

        [PropertyMapping(ColumnName = "Country", Description = "", Length = 15, IsNullable = true)]
        public virtual string Country { get; set; }

        /// <summary>
        /// ��ȡ�����á�
        /// </summary>

        [PropertyMapping(ColumnName = "Phone", Description = "", Length = 24, IsNullable = true)]
        public virtual string Phone { get; set; }

        /// <summary>
        /// ��ȡ�����á�
        /// </summary>

        [PropertyMapping(ColumnName = "Fax", Description = "", Length = 24, IsNullable = true)]
        public virtual string Fax { get; set; }

        [PropertyMapping(ColumnName = "Test1", Description = "", Length = 24, IsNullable = true)]
        public virtual string Test1 { get; set; }

        /// <summary>
        /// ��ȡ������ <see cref="orders"/> ����ʵ�弯��
        /// </summary>
        public virtual EntitySet<Orders> Orderses { get; set; }

    }

    public class CustomersMetadata
    {
        /// <summary>
        /// ���� CustomerID ����֤���ԡ�
        /// </summary>
        [Required]
        [StringLength(5)]
        public object CustomerID { get; set; }

        /// <summary>
        /// ���� CompanyName ����֤���ԡ�
        /// </summary>
        [Required]
        [StringLength(40)]
        public object CompanyName { get; set; }

        /// <summary>
        /// ���� ContactName ����֤���ԡ�
        /// </summary>
        [StringLength(30)]
        public object ContactName { get; set; }

        /// <summary>
        /// ���� ContactTitle ����֤���ԡ�
        /// </summary>
        [StringLength(30)]
        public object ContactTitle { get; set; }

        /// <summary>
        /// ���� Address ����֤���ԡ�
        /// </summary>
        [StringLength(60)]
        public object Address { get; set; }

        /// <summary>
        /// ���� City ����֤���ԡ�
        /// </summary>
        [StringLength(15)]
        public object City { get; set; }

        /// <summary>
        /// ���� Region ����֤���ԡ�
        /// </summary>
        [StringLength(15)]
        public object Region { get; set; }

        /// <summary>
        /// ���� PostalCode ����֤���ԡ�
        /// </summary>
        [StringLength(10)]
        public object PostalCode { get; set; }

        /// <summary>
        /// ���� Country ����֤���ԡ�
        /// </summary>
        [StringLength(15)]
        public object Country { get; set; }

        /// <summary>
        /// ���� Phone ����֤���ԡ�
        /// </summary>
        [StringLength(24)]
        public object Phone { get; set; }

        /// <summary>
        /// ���� Fax ����֤���ԡ�
        /// </summary>
        [StringLength(24)]
        public object Fax { get; set; }

    }
}