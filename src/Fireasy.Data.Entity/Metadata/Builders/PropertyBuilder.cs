// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Reflection;

namespace Fireasy.Data.Entity.Metadata.Builders
{
    /// <summary>
    /// 属性映射构造器。
    /// </summary>
    /// <typeparam name="TProperty"></typeparam>
    public class PropertyBuilder<TProperty> : IMetadataBuilder
    {
        private readonly EntityMetadata _metadata;
        private readonly PropertyMapInfo _mapInfo;

        public PropertyBuilder(EntityMetadata metadata, PropertyInfo pinfo)
        {
            _metadata = metadata;

            if (pinfo != null)
            {
                _mapInfo = new PropertyMapInfo { ReflectionInfo = pinfo };
            }
        }

        /// <summary>
        /// 指定列名。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual PropertyBuilder<TProperty> HasColumnName(string name)
        {
            _mapInfo.ColumnName = name;
            return this;
        }

        /// <summary>
        /// 指定注释。
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        public virtual PropertyBuilder<TProperty> HasDescription(string description)
        {
            _mapInfo.Description = description;
            return this;
        }

        /// <summary>
        /// 指定该属性是主键。
        /// </summary>
        /// <returns></returns>
        public virtual PropertyBuilder<TProperty> IsPrimaryKey()
        {
            _mapInfo.IsPrimaryKey = true;
            return this;
        }

        /// <summary>
        /// 指定该属性是逻辑删除标记。
        /// </summary>
        /// <returns></returns>
        public virtual PropertyBuilder<TProperty> IsDeletedKey()
        {
            _mapInfo.IsDeletedKey = true;
            return this;
        }

        /// <summary>
        /// 指定并发控件标志。
        /// </summary>
        /// <returns></returns>
        public virtual PropertyBuilder<TProperty> IsConcurrencyToken()
        {
            _mapInfo.IsConcurrencyToken = true;
            return this;
        }

        /// <summary>
        /// 指定行版本标识。
        /// </summary>
        /// <returns></returns>
        public virtual PropertyBuilder<TProperty> IsRowVersion()
        {
            _mapInfo.IsRowVersion = true;
            return this;
        }

        /// <summary>
        /// 指定可空。
        /// </summary>
        /// <returns></returns>
        public virtual PropertyBuilder<TProperty> IsNullable()
        {
            _mapInfo.IsNullable = true;
            return this;
        }

        /// <summary>
        /// 指定字符串的长度。
        /// </summary>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public virtual PropertyBuilder<TProperty> HasMaxLength(int maxLength)
        {
            _mapInfo.Length = maxLength;
            return this;
        }

        /// <summary>
        /// 指定数值的小数位数。
        /// </summary>
        /// <param name="scale"></param>
        /// <returns></returns>
        public virtual PropertyBuilder<TProperty> HasScale(int scale)
        {
            _mapInfo.Scale = scale;
            return this;
        }

        /// <summary>
        /// 指定数值的精度。
        /// </summary>
        /// <param name="precision"></param>
        /// <returns></returns>
        public virtual PropertyBuilder<TProperty> HasPrecision(int precision)
        {
            _mapInfo.Precision = precision;
            return this;
        }

        /// <summary>
        /// 指定标记的生成方式。
        /// </summary>
        /// <param name="generateType"></param>
        /// <returns></returns>
        public virtual PropertyBuilder<TProperty> HasIdentity(IdentityGenerateType generateType)
        {
            _mapInfo.GenerateType = generateType;
            return this;
        }

        /// <summary>
        /// 指定默认值。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual PropertyBuilder<TProperty> HasDefaultValue(PropertyValue value)
        {
            _mapInfo.DefaultValue = value;
            return this;
        }

        /// <summary>
        /// 构造元数据。
        /// </summary>
        public virtual void Build()
        {
            var property = PropertyUnity.RegisterProperty(_mapInfo.ReflectionInfo.Name, _mapInfo.ReflectionInfo.PropertyType, _metadata.EntityType, _mapInfo);
            _metadata.InternalAddProperty(property);
        }

        internal class NullBuilder : PropertyBuilder<TProperty>
        {
            public NullBuilder()
                : base(null, null)
            {
            }

            public override PropertyBuilder<TProperty> HasColumnName(string name)
            {
                return this;
            }

            public override PropertyBuilder<TProperty> HasDescription(string description)
            {
                return this;
            }

            public override PropertyBuilder<TProperty> IsPrimaryKey()
            {
                return this;
            }

            public override PropertyBuilder<TProperty> IsDeletedKey()
            {
                return this;
            }

            public override PropertyBuilder<TProperty> IsConcurrencyToken()
            {
                return this;
            }

            public override PropertyBuilder<TProperty> IsRowVersion()
            {
                return base.IsRowVersion();
            }

            public override PropertyBuilder<TProperty> IsNullable()
            {
                return this;
            }

            public override PropertyBuilder<TProperty> HasMaxLength(int maxLength)
            {
                return this;
            }

            public override PropertyBuilder<TProperty> HasScale(int scale)
            {
                return this;
            }

            public override PropertyBuilder<TProperty> HasPrecision(int precision)
            {
                return this;
            }

            public override PropertyBuilder<TProperty> HasIdentity(IdentityGenerateType generateType)
            {
                return this;
            }

            public override PropertyBuilder<TProperty> HasDefaultValue(PropertyValue value)
            {
                return this;
            }

            public override void Build()
            {
            }
        }

    }
}
