// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using Fireasy.Data.Entity.Extensions;
using Fireasy.Data.Entity.Properties;

namespace Fireasy.Data.Entity.QueryBuilder
{
    internal static class EntityLazyloadQueryBuilder
    {
        internal static SqlCommand BuildEntityQuery(EntityQueryContext context, IEntity entity, RelationProperty relationProperty)
        {
            var relationKey = RelationshipUnity.GetRelationship(relationProperty);
            if (relationKey == null)
            {
                return null;
            }

            Type relationType;
            Func<RelationshipKey, IProperty> func1, func2;
            if (entity.EntityType == relationKey.ThisType)
            {
                relationType = relationKey.OtherType;
                func1 = key => key.OtherProperty;
                func2 = key => key.ThisProperty;
            }
            else
            {
                relationType = relationKey.ThisType;
                func1 = key => key.ThisProperty;
                func2 = key => key.OtherProperty;
            }

            var query = new EntityQueryBuilder(context, relationType)
                .Select().All().From();
            foreach (var key in relationKey.Keys)
            {
                var val = entity.InternalGetValue(func2(key));
                if (PropertyValue.IsNullOrEmpty(val) || !val.IsValid)
                {
                    return string.Empty;
                }
                query = query.And(func1(key), val);
            }
            return query.ToSqlCommand();
        }

        internal static SqlCommand BuildEntitySetQuery(EntityQueryContext context, IEntity entity, RelationProperty relationProperty)
        {
            var relationKey = RelationshipUnity.GetRelationship(relationProperty);
            if (relationKey == null || relationKey.Style != RelationshipStyle.One2Many)
            {
                return null;
            }

            var query = new EntityQueryBuilder(context, relationProperty.RelationType)
                .Select().All().From();

            var valid = true;
            foreach (var key in relationKey.Keys)
            {
                var val = entity.InternalGetValue(key.ThisProperty);
                if (PropertyValue.IsNullOrEmpty(val) || !val.IsValid)
                {
                    valid = false;
                    continue;
                }

                query = query.And(key.OtherProperty, entity.InternalGetValue(key.ThisProperty));
            }

            if (!valid)
            {
                return string.Empty;
            }

            return query.ToSqlCommand();
        }

        internal static SqlCommand BuildReferenceQuery(EntityQueryContext context, IEntity entity, ReferenceProperty referenceProperty)
        {
            var relationKey = RelationshipUnity.GetRelationship(referenceProperty);
            if (relationKey == null)
            {
                return null;
            }
            var query = new EntityQueryBuilder(context, referenceProperty.RelationType)
                .Select().Single(referenceProperty.Reference).From();
            foreach (var key in relationKey.Keys)
            {
                query = query.And(key.ThisProperty, entity.InternalGetValue(key.OtherProperty));
            }
            return query.ToSqlCommand();
        }
    }
}
