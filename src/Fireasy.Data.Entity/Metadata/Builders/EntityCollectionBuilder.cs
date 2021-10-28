// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Collections.Generic;
using Fireasy.Common.Extensions;
using System.Reflection;

namespace Fireasy.Data.Entity.Metadata.Builders
{
    /// <summary>
    /// 实体集映射构造器。
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TRelatedEntity"></typeparam>
    public class EntityCollectionBuilder<TEntity, TRelatedEntity> : IMetadataBuilder
    {
        private readonly EntityMetadata _metadata;
        private readonly PropertyInfo _propertyInfo;
        private readonly Dictionary<RelationshipStyle, IMetadataBuilder> _builders = null;

        public EntityCollectionBuilder(EntityMetadata metadata, PropertyInfo pinfo)
        {
            _metadata = metadata;
            _propertyInfo = pinfo;

            if (metadata != null)
            {
                _builders = new Dictionary<RelationshipStyle, IMetadataBuilder>();
            }
        }

        /// <summary>
        /// 标记多对一关系。
        /// </summary>
        public virtual RelationshipBuilder<TEntity, TRelatedEntity> WithOne()
        {
            return (RelationshipBuilder<TEntity, TRelatedEntity>)_builders.TryGetValue(RelationshipStyle.Many2One,
                () => new RelationshipBuilder<TEntity, TRelatedEntity>(_metadata, _propertyInfo, RelationshipStyle.Many2One));
        }

        /// <summary>
        /// 构造元数据。
        /// </summary>
        void IMetadataBuilder.Build()
        {
            InternalBuild();
        }

        protected virtual void InternalBuild()
        {
            var property = PropertyUnity.RegisterSupposedProperty(_propertyInfo.Name, _propertyInfo.PropertyType, _metadata.EntityType);
            _metadata.InternalAddProperty(property);

            _builders.ForEach(s => s.Value.Build());
        }

        internal class NullBuilder : EntityCollectionBuilder<TEntity, TRelatedEntity>
        {
            public NullBuilder()
                : base(null, null)
            {
            }

            public override RelationshipBuilder<TEntity, TRelatedEntity> WithOne()
            {
                return new RelationshipBuilder<TEntity, TRelatedEntity>.NullBuilder();
            }

            protected override void InternalBuild()
            {
            }
        }
    }
}
