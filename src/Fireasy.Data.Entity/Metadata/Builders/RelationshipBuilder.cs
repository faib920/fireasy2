// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Fireasy.Data.Entity.Metadata.Builders
{
    /// <summary>
    /// 关系映射构造器。
    /// </summary>
    /// <typeparam name="TPrincipalEntity"></typeparam>
    /// <typeparam name="TDependentEntity"></typeparam>
    public class RelationshipBuilder<TPrincipalEntity, TDependentEntity> : IMetadataBuilder
    {
        private readonly List<PropertyInfo> _primaryKeys;
        private readonly List<PropertyInfo> _foreignKeys;
        private readonly PropertyInfo _propertyInfo;
        private readonly RelationshipStyle _style;

        public RelationshipBuilder(EntityMetadata metadata, PropertyInfo pinfo, RelationshipStyle style)
        {
            if (metadata != null)
            {
                _primaryKeys = new List<PropertyInfo>();
                _foreignKeys = new List<PropertyInfo>();
                _propertyInfo = pinfo;
                _style = style;
            }
        }

        /// <summary>
        /// 指定主键属性。
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="propertyExp"></param>
        /// <returns></returns>
        public virtual RelationshipBuilder<TPrincipalEntity, TDependentEntity> HasPrimaryKey<TProperty>(Expression<Func<TPrincipalEntity, TProperty>> propertyExp)
        {
            var property = MetadataHelper.FindProperty(propertyExp);
            if (property != null)
            {
                _primaryKeys.Add(property);
            }

            return this;
        }

        /// <summary>
        /// 指定外键属性。
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="propertyExp"></param>
        /// <returns></returns>
        public virtual RelationshipBuilder<TPrincipalEntity, TDependentEntity> HasForeignKey<TProperty>(Expression<Func<TDependentEntity, TProperty>> propertyExp)
        {
            var property = MetadataHelper.FindProperty(propertyExp);
            if (property != null)
            {
                _foreignKeys.Add(property);
            }

            return this;
        }

        /// <summary>
        /// 构造元数据。
        /// </summary>
        public virtual void Build()
        {
            if (_primaryKeys.Count != _foreignKeys.Count)
            {
                throw new ArgumentException();
            }

            var keys = new RelationshipKey[_primaryKeys.Count];

            for (var i = 0; i < _primaryKeys.Count; i++)
            {
                keys[i] = new RelationshipKey { PrincipalKey = _primaryKeys[i].Name, DependentKey = _foreignKeys[i].Name };
            }

            var ship = new RelationshipMetadata(typeof(TPrincipalEntity), typeof(TDependentEntity), _style, RelationshipSource.MetadataBuild, keys);

            RelationshipUnity.InternalAddMetadata(_propertyInfo.Name, ship);
        }

        internal class NullBuilder : RelationshipBuilder<TPrincipalEntity, TDependentEntity>
        {
            public NullBuilder()
                : base (null, null, RelationshipStyle.One2One)
            {
            }

            public override RelationshipBuilder<TPrincipalEntity, TDependentEntity> HasPrimaryKey<TProperty>(Expression<Func<TPrincipalEntity, TProperty>> propertyExp)
            {
                return this;
            }

            public override RelationshipBuilder<TPrincipalEntity, TDependentEntity> HasForeignKey<TProperty>(Expression<Func<TDependentEntity, TProperty>> propertyExp)
            {
                return this;
            }

            public override void Build()
            {
            }
        }
    }
}
