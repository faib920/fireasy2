// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using Fireasy.Common.Extensions;
using Fireasy.Data.Entity.Extensions;
using Fireasy.Data.Entity.Metadata;
using Fireasy.Data.Extensions;
using Fireasy.Data.Identity;

namespace Fireasy.Data.Entity.QueryBuilder
{
    internal static class EntityPersistentQueryBuilder
    {
        internal static SqlCommand BuidCreateQuery(EntityQueryContext context, IEntity entity)
        {
            var properties = GetInsertProperties(entity);
            if (properties.Count == 0)
            {
                return string.Empty;
            }

            var values = new QueryValue[properties.Count];
            for (var i = 0; i < values.Length; i++)
            {
                values[i] = new QueryValue { Property = properties[i], Value = GetInsertValue(context, entity, properties[i]) };
            }

            var query = new EntityQueryBuilder(context, entity.EntityType)
                .Insert().Set(values);
            return query.ToSqlCommand();
        }

        internal static SqlCommand BuildUpdateQuery(EntityQueryContext context, IEntity entity)
        {
            var properties = GetUpdateProperties(entity);
            if (properties.Count == 0)
            {
                return string.Empty;
            }

            var values = new QueryValue[properties.Count];
            for (var i = 0; i < values.Length; i++)
            {
                values[i] = new QueryValue { Property = properties[i], Value = GetInsertValue(context, entity, properties[i]) };
            }

            var query = new EntityQueryBuilder(context, entity.EntityType)
                .Update().Set(values);
            query = BuildPrimaryKeyCondition(query, entity);
            return query.ToSqlCommand();
        }

        internal static SqlCommand BuildDeleteQuery(EntityQueryContext context, IEntity entity)
        {
            var query = new EntityQueryBuilder(context, entity.EntityType)
                .Delete();
            query = BuildPrimaryKeyCondition(query, entity);
            return query.ToSqlCommand();
        }

        internal static SqlCommand BuildDeleteQuery(EntityQueryContext context, Type entityType, object[] primaryValues)
        {
            var query = new EntityQueryBuilder(context, entityType)
                .Delete();
            query = BuildPrimaryKeyCondition(query, entityType, primaryValues);
            return query.ToSqlCommand();
        }

        internal static SqlCommand BuildGetFirstQuery(EntityQueryContext context, Type entityType, object[] primaryValues)
        {
            var query = new EntityQueryBuilder(context, entityType)
                .Select().All().From();
            query = BuildPrimaryKeyCondition(query, entityType, primaryValues);
            return query.ToSqlCommand();
        }

        internal static SqlCommand BuildUpdateFakeDeleteQuery(EntityQueryContext context, IEntity entity, IProperty fakeProperty)
        {
            var query = new EntityQueryBuilder(context, entity.EntityType)
                .Update().Set(new QueryValue { Property = fakeProperty, Value = 1 });
            query = BuildPrimaryKeyCondition(query, entity);
            return query.ToSqlCommand();
        }

        internal static SqlCommand BuildUpdateFakeDeleteQuery(EntityQueryContext context, IProperty fakeProperty, object[] primaryValues)
        {
            var query = new EntityQueryBuilder(context, fakeProperty.EntityType)
                .Update().Set(new QueryValue { Property = fakeProperty, Value = 1 });
            query = BuildPrimaryKeyCondition(query, fakeProperty.EntityType, primaryValues);
            return query.ToSqlCommand();
        }

        private static EntityQueryBuilder BuildPrimaryKeyCondition(EntityQueryBuilder query, IEntity entity)
        {
            var primaryKeys = PropertyUnity.GetPrimaryProperties(entity.EntityType.GetRootType());
            if (primaryKeys.IsNullOrEmpty())
            {
                throw new Exception(SR.GetString(SRKind.NotDefinedPrimaryKey));
            }

            foreach (var property in primaryKeys)
            {
                query = query.And(property, entity.InternalGetValue(property));
            }

            return query;
        }

        private static EntityQueryBuilder BuildPrimaryKeyCondition(EntityQueryBuilder query, Type entityType, object[] primaryValues)
        {
            var primaryKeys = PropertyUnity.GetPrimaryProperties(entityType.GetRootType());
            if (primaryKeys.IsNullOrEmpty())
            {
                throw new Exception(SR.GetString(SRKind.NotDefinedPrimaryKey));
            }

            var i = 0;
            foreach (var property in primaryKeys)
            {
                query = query.And(property, primaryValues[i++]);
            }

            return query;
        }

        private static PropertyValue GetInsertValue(EntityQueryContext context, IEntity entity, IProperty property)
        {
            var value = entity.InternalGetValue(property);

            switch (property.Info.GenerateType)
            {
                case IdentityGenerateType.Generator:
                    if (property.Info.DataType != null &&
                        property.Info.DataType.Value.IsStringDbType() &&
                        value.IsNullOrEmpty())
                    {
                        value = Guid.NewGuid().ToString();
                        entity.InternalSetValue(property, value);
                    }
                    else
                    {
                        var generator = context.Database.Provider.GetService<IGeneratorProvider>();
                        if (generator != null)
                        {
                            var metadata = EntityMetadataUnity.GetEntityMetadata(entity.EntityType);
                            var inc = generator.GenerateValue(context.Database, context.Environment == null ? metadata.TableName : context.Environment.GetVariableTableName(metadata), property.Info.FieldName);
                            entity.InternalSetValue(property, inc);
                            return inc;
                        }
                    }

                    break;
            }

            if (value.IsNullOrEmpty() && 
                !property.Info.DefaultValue.IsNullOrEmpty())
            {
                return property.Info.DefaultValue;
            }

            return value;
        }

        private static List<IProperty> GetInsertProperties(IEntity entity)
        {
            var properties = PropertyUnity.GetProperties(entity.EntityType, true);
            var entityEx = entity as IEntityStatefulExtension;

            return (from property in properties
                    where property is ISavedProperty
                    where (entityEx != null && entityEx.IsModified(property.Name)) || 
                    !property.Info.DefaultValue.IsNullOrEmpty() ||
                    property.Info.GenerateType == IdentityGenerateType.Generator
                    select property).ToList();
        }

        private static List<IProperty> GetUpdateProperties(IEntity entity)
        {
            var properties = PropertyUnity.GetProperties(entity.EntityType, true);
            var entityEx = entity as IEntityStatefulExtension;

            return (from property in properties
                    where (entityEx != null && entityEx.IsModified(property.Name))
                    && property is ISavedProperty
                    select property).ToList();
        }
    }
}
