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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Fireasy.Data.Entity.Metadata.Builders
{
    /// <summary>
    /// 实体映射构造器。
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class EntityBuilder<TEntity> : IMetadataBuilder
    {
        private readonly EntityMetadata _metadata;
        private readonly Dictionary<PropertyInfo, IMetadataBuilder> _propBuilders = null;
        private EntityTreeBuilder<TEntity> _treeBuilder = null;

        public EntityBuilder(EntityMetadata metadata)
        {
            _metadata = metadata;

            if (_metadata != null)
            {
                _propBuilders = new Dictionary<PropertyInfo, IMetadataBuilder>();
            }
        }

        /// <summary>
        /// 指定数据表名。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual EntityBuilder<TEntity> ToTable(string name)
        {
            _metadata.TableName = name;

            return this;
        }

        /// <summary>
        /// 指定注释。
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        public virtual EntityBuilder<TEntity> HasDescription(string description)
        {
            _metadata.Description = description;

            return this;
        }

        /// <summary>
        /// 映射树型结构。
        /// </summary>
        /// <returns></returns>
        public virtual EntityTreeBuilder<TEntity> HasTree()
        {
            return _treeBuilder ??= new EntityTreeBuilder<TEntity>(_metadata);
        }

        /// <summary>
        /// 映射指定的属性，返回一个 <see cref="PropertyBuilder{TProperty}"/> 实例。
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="propertyExp"></param>
        /// <param name="builderAction"></param>
        /// <returns></returns>
        public virtual PropertyBuilder<TProperty> Property<TProperty>(Expression<Func<TEntity, TProperty>> propertyExp, Action<PropertyBuilder<TProperty>> builderAction = null)
        {
            var property = MetadataHelper.FindProperty(propertyExp);
            return (PropertyBuilder<TProperty>)_propBuilders.TryGetValue(property,
                () =>
                {
                    var builder = new PropertyBuilder<TProperty>(_metadata, property);
                    builderAction?.Invoke(builder);
                    return builder;
                });
        }

        /// <summary>
        /// 指定一对一或一对多关系。
        /// </summary>
        /// <typeparam name="TRelatedEntity"></typeparam>
        /// <param name="propertyExp"></param>
        /// <returns></returns>
        public virtual EntityReferenceBuilder<TEntity, TRelatedEntity> HasOne<TRelatedEntity>(Expression<Func<TEntity, TRelatedEntity>> propertyExp)
        {
            var property = MetadataHelper.FindProperty(propertyExp);
            return (EntityReferenceBuilder<TEntity, TRelatedEntity>)_propBuilders.TryGetValue(property,
                () => new EntityReferenceBuilder<TEntity, TRelatedEntity>(_metadata, property));
        }

        /// <summary>
        /// 指定多对一或多对多关系。
        /// </summary>
        /// <typeparam name="TRelatedEntity"></typeparam>
        /// <param name="propertyExp"></param>
        /// <returns></returns>
        public virtual EntityCollectionBuilder<TEntity, TRelatedEntity> HasMany<TRelatedEntity>(Expression<Func<TEntity, IList<TRelatedEntity>>> propertyExp)
        {
            var property = MetadataHelper.FindProperty(propertyExp);
            return (EntityCollectionBuilder<TEntity, TRelatedEntity>)_propBuilders.TryGetValue(property,
                () => new EntityCollectionBuilder<TEntity, TRelatedEntity>(_metadata, property));
        }

        /// <summary>
        /// 构造元数据。
        /// </summary>
        public virtual void Build()
        {
            using (var scope = new MetadataInitializeScope(_metadata))
            {
                //没有显式映射的
                foreach (var p in EntityMetadataUnity.PropertyMetadataResolver.GetProperties(_metadata.EntityType)
                    .Where(s => !_propBuilders.Keys.Contains(s)))
                {
                    _propBuilders.Add(p, new PropertyBuilder(_metadata, p));
                }

                _propBuilders.ForEach(s => s.Value.Build());
                _treeBuilder?.Build();
            }

            EntityMetadataUnity.AddEntityMetadata(typeof(TEntity), _metadata);
        }

        internal class NullBuilder : EntityBuilder<TEntity>
        {
            public NullBuilder()
                : base(null)
            {
            }

            public override EntityBuilder<TEntity> ToTable(string name)
            {
                return this;
            }

            public override EntityBuilder<TEntity> HasDescription(string description)
            {
                return this;
            }

            public override PropertyBuilder<TProperty> Property<TProperty>(Expression<Func<TEntity, TProperty>> propertyExp, Action<PropertyBuilder<TProperty>> builderAction = null)
            {
                return new PropertyBuilder<TProperty>.NullBuilder();
            }

            public override EntityTreeBuilder<TEntity> HasTree()
            {
                return new EntityTreeBuilder<TEntity>.NullBuilder();
            }

            public override EntityReferenceBuilder<TEntity, TRelatedEntity> HasOne<TRelatedEntity>(Expression<Func<TEntity, TRelatedEntity>> propertyExp)
            {
                return new EntityReferenceBuilder<TEntity, TRelatedEntity>.NullBuilder();
            }

            public override EntityCollectionBuilder<TEntity, TRelatedEntity> HasMany<TRelatedEntity>(Expression<Func<TEntity, IList<TRelatedEntity>>> propertyExp)
            {
                return new EntityCollectionBuilder<TEntity, TRelatedEntity>.NullBuilder();
            }

            public override void Build()
            {
            }
        }
    }
}
