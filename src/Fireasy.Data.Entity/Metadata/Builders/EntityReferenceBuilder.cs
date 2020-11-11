// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using System.Collections.Generic;
using System.Reflection;

namespace Fireasy.Data.Entity.Metadata.Builders
{
    /// <summary>
    /// 实体关联映射的构造器。
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TRelatedEntity"></typeparam>
    public class EntityReferenceBuilder<TEntity, TRelatedEntity> : IMetadataBuilder
    {
        private readonly EntityMetadata _metadata;
        private readonly PropertyInfo _propertyInfo;
        private RelationOptions _options;
        private readonly Dictionary<RelationshipStyle, IMetadataBuilder> _builders = null;

        public EntityReferenceBuilder(EntityMetadata metadata, PropertyInfo pinfo)
        {
            _metadata = metadata;
            _propertyInfo = pinfo;

            if (metadata != null)
            {
                _builders = new Dictionary<RelationshipStyle, IMetadataBuilder>();
            }
        }

        /// <summary>
        /// 标记一对一关系。
        /// </summary>
        /// <returns></returns>
        public virtual RelationshipBuilder<TEntity, TRelatedEntity> WithOne()
        {
            return (RelationshipBuilder<TEntity, TRelatedEntity>)_builders.TryGetValue(RelationshipStyle.One2One,
                () => new RelationshipBuilder<TEntity, TRelatedEntity>(_metadata, _propertyInfo, RelationshipStyle.One2One));
        }

        /// <summary>
        /// 标记一对多关系。
        /// </summary>
        /// <returns></returns>
        public virtual RelationshipBuilder<TRelatedEntity, TEntity> WithMany()
        {
            return (RelationshipBuilder<TRelatedEntity, TEntity>)_builders.TryGetValue(RelationshipStyle.One2Many,
               () => new RelationshipBuilder<TRelatedEntity, TEntity>(_metadata, _propertyInfo, RelationshipStyle.One2Many));
        }

        /// <summary>
        /// 指定关联选项。
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public virtual EntityReferenceBuilder<TEntity, TRelatedEntity> HasOptions(RelationOptions options)
        {
            _options = options;
            return this;
        }

        /// <summary>
        /// 构造元数据。
        /// </summary>
        public virtual void Build()
        {
            var property = PropertyUnity.RegisterSupposedProperty(_propertyInfo.Name, _propertyInfo.PropertyType, _metadata.EntityType, null, _options);
            _metadata.InternalAddProperty(property);

            _builders.ForEach(s => s.Value.Build());
        }

        internal class NullBuilder : EntityReferenceBuilder<TEntity, TRelatedEntity>
        {
            public NullBuilder()
                : base(null, null)
            {
            }

            public override RelationshipBuilder<TEntity, TRelatedEntity> WithOne()
            {
                return new RelationshipBuilder<TEntity, TRelatedEntity>.NullBuilder();
            }

            public override RelationshipBuilder<TRelatedEntity, TEntity> WithMany()
            {
                return new RelationshipBuilder<TRelatedEntity, TEntity>.NullBuilder();
            }

            public override EntityReferenceBuilder<TEntity, TRelatedEntity> HasOptions(RelationOptions options)
            {
                return this;
            }

            public override void Build()
            {
            }
        }
    }
}
