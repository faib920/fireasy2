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
        /// ��ȡ�����á�
        /// </summary>

        [PropertyMapping(ColumnName = "OrderID", Description = "", IsPrimaryKey = true, IsNullable = false)]
        public virtual long OrderID { get; set; }

        /// <summary>
        /// ��ȡ�����á�
        /// </summary>

        [PropertyMapping(ColumnName = "ProductID", Description = "", IsPrimaryKey = true, IsNullable = false)]
        public virtual long Product1ID { get; set; }

        /// <summary>
        /// ��ȡ�����á�
        /// </summary>

        [PropertyMapping(ColumnName = "UnitPrice", Description = "", IsNullable = false)]
        public virtual int? UnitPrice { get; set; }

        /// <summary>
        /// ��ȡ�����á�
        /// </summary>

        [PropertyMapping(ColumnName = "Quantity", Description = "", IsNullable = false)]
        public virtual long Quantity { get; set; }

        /// <summary>
        /// ��ȡ�����á�
        /// </summary>

        [PropertyMapping(ColumnName = "Discount", Description = "", IsNullable = false)]
        public virtual double Discount { get; set; }

        /// <summary>
        /// ��ȡ�����ù��� <see cref="products"/> ����
        /// </summary>
        //[RelationshipAssign("ProductID", "ProductID")]
        //[RelationshipUse("ProductID")]
        public virtual Products Products { get; set; }

        /// <summary>
        /// ��ȡ�����ù��� <see cref="products"/> ����
        /// </summary>
        [RelationshipAssign("ProductID", "Quantity")]
        //[RelationshipUse("Quantity")]
        public virtual Products Products1 { get; set; }

        /// <summary>
        /// ��ȡ�����ù��� <see cref="orders"/> ����
        /// </summary>
        public virtual Orders Orders { get; set; }

        public string ReorderLevelName { get; set; }

    }

    public class OrderDetailsMetadata
    {
        /// <summary>
        /// ���� OrderID ����֤���ԡ�
        /// </summary>
        [Required]
        public object OrderID { get; set; }

        /// <summary>
        /// ���� ProductID ����֤���ԡ�
        /// </summary>
        [Required]
        public object ProductID { get; set; }

        /// <summary>
        /// ���� UnitPrice ����֤���ԡ�
        /// </summary>
        [Required]
        public object UnitPrice { get; set; }

        /// <summary>
        /// ���� Quantity ����֤���ԡ�
        /// </summary>
        [Required]
        public object Quantity { get; set; }

        /// <summary>
        /// ���� Discount ����֤���ԡ�
        /// </summary>
        [Required]
        public object Discount { get; set; }

    }
}