// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using System;
using System.Collections.Generic;

namespace Fireasy.Data.Entity.Metadata.Builders
{
    /// <summary>
    /// 实体模型构造器。
    /// </summary>
    public class ModelBuilder : IMetadataBuilder
    {
        private readonly Dictionary<Type, IMetadataBuilder> _builders = new Dictionary<Type, IMetadataBuilder>();

        public EntityBuilder<TEntity> Entity<TEntity>(Action<EntityBuilder<TEntity>> buildAction = null) where TEntity : IEntity
        {
            if (EntityMetadataUnity.IsMetadata(typeof(TEntity)))
            {
                return new EntityBuilder<TEntity>.NullBuilder();
            }

            return (EntityBuilder<TEntity>)_builders.TryGetValue(typeof(TEntity), () =>
            {
                var metadata = EntityMetadata.Build(typeof(TEntity));
                var builder = new EntityBuilder<TEntity>(metadata);
                buildAction?.Invoke(builder);
                return builder;
            });
        }

        /// <summary>
        /// 构造元数据。
        /// </summary>
        void IMetadataBuilder.Build()
        {
            _builders.ForEach(s => s.Value.Build());
        }
    }
}