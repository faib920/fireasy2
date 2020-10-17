// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Metadata.Builders
{
    /// <summary>
    /// 树型实体映射构造器。
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class EntityTreeBuilder<TEntity> : IMetadataBuilder
    {
        private readonly EntityMetadata _metadata;
        private readonly EntityTreeMappingAttribute _attribute;

        public EntityTreeBuilder(EntityMetadata metadata)
        {
            _metadata = metadata;

            if (metadata != null)
            {
                _attribute = new EntityTreeMappingAttribute();
            }
        }

        /// <summary>
        /// 指定 InnerSign 属性。
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="propertyExp"></param>
        /// <returns></returns>
        public virtual EntityTreeBuilder<TEntity> HasInnerSign<TProperty>(Expression<Func<TEntity, TProperty>> propertyExp)
        {
            _attribute.InnerSign = MetadataHelper.FindProperty(propertyExp).Name;

            return this;
        }

        /// <summary>
        /// 指定 Name 属性。
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="propertyExp"></param>
        public virtual EntityTreeBuilder<TEntity> HasIName<TProperty>(Expression<Func<TEntity, TProperty>> propertyExp)
        {
            _attribute.Name = MetadataHelper.FindProperty(propertyExp).Name;

            return this;
        }

        /// <summary>
        /// 指定 FullName 属性。
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="propertyExp"></param>
        public virtual EntityTreeBuilder<TEntity> HasFullName<TProperty>(Expression<Func<TEntity, TProperty>> propertyExp)
        {
            _attribute.FullName = MetadataHelper.FindProperty(propertyExp).Name;

            return this;
        }

        /// <summary>
        /// 指定 Level 属性。
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="propertyExp"></param>
        public virtual EntityTreeBuilder<TEntity> HasLevel<TProperty>(Expression<Func<TEntity, TProperty>> propertyExp)
        {
            _attribute.Level = MetadataHelper.FindProperty(propertyExp).Name;

            return this;
        }

        /// <summary>
        /// 指定 Order 属性。
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="propertyExp"></param>
        public virtual EntityTreeBuilder<TEntity> HasOrder<TProperty>(Expression<Func<TEntity, TProperty>> propertyExp)
        {
            _attribute.Order = MetadataHelper.FindProperty(propertyExp).Name;

            return this;
        }

        /// <summary>
        /// 指定 InnerSign 的长度。
        /// </summary>
        /// <param name="length"></param>
        public virtual EntityTreeBuilder<TEntity> HasSignLength(int length)
        {
            _attribute.SignLength = length;

            return this;
        }

        /// <summary>
        /// 构造元数据。
        /// </summary>
        public virtual void Build()
        {
            var treeMetdata = new EntityTreeMetadata(_metadata, _attribute);

            IProperty property;

            if (!string.IsNullOrEmpty(_attribute.InnerSign) && (property = _metadata.GetProperty(_attribute.InnerSign)) != null)
            {
                treeMetdata.InitTreeMetadata(property);
            }

            if (!string.IsNullOrEmpty(_attribute.Name) && (property = _metadata.GetProperty(_attribute.Name)) != null)
            {
                treeMetdata.InitTreeMetadata(property);
            }

            if (!string.IsNullOrEmpty(_attribute.FullName) && (property = _metadata.GetProperty(_attribute.FullName)) != null)
            {
                treeMetdata.InitTreeMetadata(property);
            }

            if (!string.IsNullOrEmpty(_attribute.Level) && (property = _metadata.GetProperty(_attribute.Level)) != null)
            {
                treeMetdata.InitTreeMetadata(property);
            }

            if (!string.IsNullOrEmpty(_attribute.Order) && (property = _metadata.GetProperty(_attribute.Order)) != null)
            {
                treeMetdata.InitTreeMetadata(property);
            }

            _metadata.InternalSetTreeMapping(treeMetdata);
        }

        internal class NullBuilder : EntityTreeBuilder<TEntity>
        {
            public NullBuilder()
                : base(null)
            {
            }

            public override EntityTreeBuilder<TEntity> HasInnerSign<TProperty>(Expression<Func<TEntity, TProperty>> propertyExp)
            {
                return this;
            }

            public override EntityTreeBuilder<TEntity> HasIName<TProperty>(Expression<Func<TEntity, TProperty>> propertyExp)
            {
                return this;
            }

            public override EntityTreeBuilder<TEntity> HasFullName<TProperty>(Expression<Func<TEntity, TProperty>> propertyExp)
            {
                return this;
            }

            public override EntityTreeBuilder<TEntity> HasLevel<TProperty>(Expression<Func<TEntity, TProperty>> propertyExp)
            {
                return this;
            }

            public override EntityTreeBuilder<TEntity> HasOrder<TProperty>(Expression<Func<TEntity, TProperty>> propertyExp)
            {
                return this;
            }

            public override EntityTreeBuilder<TEntity> HasSignLength(int length)
            {
                return this;
            }

            public override void Build()
            {
            }
        }
    }
}
