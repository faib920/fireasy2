// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Data;

namespace Fireasy.Data.Entity
{

    /// <summary>
    /// 一个标识实体如何与数据表进行映射的特性。无法继承此类。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PropertyMappingAttribute : Attribute
    {
        private DbType dataType;
        private int length;
        private int precision;
        private int scale;
        private bool isPrimaryKey;
        private bool isNullable;
        private bool isDeletedKey;
        private LoadBehavior loadBehavior;
        private SetMark flags = SetMark.None;

        /// <summary>
        /// 初始化类 <see cref="PropertyMappingAttribute"/> 的新实例。
        /// </summary>
        public PropertyMappingAttribute()
        {
        }

        /// <summary>
        /// 初始化类 <see cref="PropertyMappingAttribute"/> 的新实例。
        /// </summary>
        /// <param name="columnName"></param>
        public PropertyMappingAttribute(string columnName)
        {
            ColumnName = columnName;
        }

        /// <summary>
        /// 获取或设置列名称。
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// 获取或设置列的注释。
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 获取或设置列的数据类型。
        /// </summary>
        public DbType DataType
        {
            get { return dataType; }
            set
            {
                dataType = value;
                SetFlag(SetMark.DataType);
            }
        }

        /// <summary>
        /// 获取或设置默认值。
        /// </summary>
        public object DefaultValue { get; set; }

        /// <summary>
        /// 获取或设置列的最大长度。
        /// </summary>
        public int Length
        {
            get { return length; }
            set
            {
                length = value;
                SetFlag(SetMark.Length);
            }
        }

        /// <summary>
        /// 获取或设置数值型列的精度。
        /// </summary>
        public int Precision
        {
            get { return precision; }
            set
            {
                precision = value;
                SetFlag(SetMark.Precision);
            }
        }

        /// <summary>
        /// 获取或设置数值型列的小数位数。
        /// </summary>
        public int Scale
        {
            get { return scale; }
            set
            {
                scale = value;
                SetFlag(SetMark.Scale);
            }
        }

        /// <summary>
        /// 获取或设置列的自动生成类型。
        /// </summary>
        public IdentityGenerateType GenerateType { get; set; }

        /// <summary>
        /// 获取或设置是否为主键。
        /// </summary>
        public bool IsPrimaryKey
        {
            get { return isPrimaryKey; }
            set
            {
                isPrimaryKey = value;
                SetFlag(SetMark.IsPrimaryKey);
            }
        }

        /// <summary>
        /// 获取或设置是否可为空。
        /// </summary>
        public bool IsNullable
        {
            get { return isNullable; }
            set
            {
                isNullable = value;
                SetFlag(SetMark.IsNullable);
            }
        }

        /// <summary>
        /// 获取或设置是否为假删除标识。
        /// </summary>
        public bool IsDeletedKey
        {
            get { return isDeletedKey; }
            set
            {
                isDeletedKey = value;
                SetFlag(SetMark.IsDeletedKey);
            }
        }

        /// <summary>
        /// 获取或设置关系型属性的加载行为。
        /// </summary>
        public LoadBehavior LoadBehavior
        {
            get { return loadBehavior; }
            set
            {
                loadBehavior = value;
                SetFlag(SetMark.LoadBehavior);
            }
        }

        private void SetFlag(SetMark p)
        {
            flags = (flags | p);
        }

        internal bool GetFlag(SetMark p)
        {
            return flags.HasFlag(p);
        }

        [Flags]
        internal enum SetMark
        {
            None = 0,
            DataType = 1,
            Length = 2,
            Precision = 4,
            Scale = 8,
            IsPrimaryKey = 16,
            IsNullable = 32,
            IsDeletedKey = 64,
            LoadBehavior = 128
        }
    }
}
