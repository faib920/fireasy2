using Fireasy.Common.ComponentModel;
using System;
using System.ComponentModel.DataAnnotations;

namespace Fireasy.Data.Entity.Tests.Models
{
    /// <summary>
    ///  实体类。
    /// </summary>
    [Serializable]
    [EntityMapping("Depts", Description = "")]
    [EntityTreeMapping(InnerSign = nameof(DeptCode), Name = nameof(DeptName), FullName = nameof(FullName))]
    [MetadataType(typeof(CategoriesMetadata))]
    public partial class Depts : LightEntity<Depts>, ITreeNode<Depts>
    {
        /// <summary>
        /// 获取或设置。
        /// </summary>

        [PropertyMapping(ColumnName = "DeptID", Description = "", IsPrimaryKey = true, GenerateType = IdentityGenerateType.AutoIncrement, IsNullable = false)]
        public virtual long DeptID { get; set; }

        /// <summary>
        /// 获取或设置。
        /// </summary>

        [PropertyMapping(ColumnName = "DeptName", Description = "", Length = 15, IsNullable = false)]
        public virtual string DeptName { get; set; }

        /// <summary>
        /// 获取或设置。
        /// </summary>

        [PropertyMapping(ColumnName = "FullName", Description = "", Length = 50, IsNullable = false)]
        public virtual string FullName { get; set; }

        /// <summary>
        /// 获取或设置。
        /// </summary>

        [PropertyMapping(ColumnName = "Attributes", Description = "", Length = 15, DataType = System.Data.DbType.String, IsNullable = false)]
        public virtual int[] Attributes { get; set; }

        /// <summary>
        /// 获取或设置。
        /// </summary>

        [PropertyMapping(ColumnName = "Image", Description = "", Length = 15, IsNullable = false)]
        public virtual byte[] Image { get; set; }

        /// <summary>
        /// 获取或设置。
        /// </summary>

        [PropertyMapping(ColumnName = "DeptCode", Description = "", Length = 2147483647, IsNullable = true)]
        public virtual string DeptCode { get; set; }

        public System.Collections.Generic.List<Depts> Children { get; set; }

        System.Collections.IList ITreeNode.Children
        {
            get
            {
                return Children;
            }
            set
            {
                Children = (System.Collections.Generic.List<Depts>)value;
            }
        }

        public bool HasChildren { get; set; }

        public object Id
        {
            get
            {
                return DeptID;
            }
            set
            {
                DeptID = (long)value;
            }
        }

        public bool IsLoaded { get; set; }

    }
}