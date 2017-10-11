// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections;
using System.Data.Common;
using System.Linq;
using Fireasy.Common;
using Fireasy.Common.Extensions;
using Fireasy.Data.Entity.Extensions;
using Fireasy.Data.Entity.Metadata;
using Fireasy.Data.Entity.Properties;
using Fireasy.Data.Entity.QueryBuilder;
using Fireasy.Data.Extensions;
using Fireasy.Data.Identity;
using Fireasy.Data.RecordWrapper;
using Fireasy.Data.Syntax;
using Fireasy.Data.Batcher;
using Fireasy.Data.Entity.Validation;
using Fireasy.Common.Subscribe;
using Fireasy.Data.Entity.Subscribes;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 实体持久化的辅助类。
    /// </summary>
    public sealed class EntityPersisterHelper
    {
        private readonly IDatabase database;
        private readonly EntityPersistentEnvironment environment;
        private readonly Type entityType;

        /// <summary>
        /// 初始化 <see cref="EntityPersisterHelper"/> 类的新实例。
        /// </summary>
        /// <param name="database">提供给持久化操作的 <see cref="Database"/> 对象。</param>
        /// <param name="entityType">实体的类型。</param>
        /// <param name="environment">实体持久化环境对象。</param>
        public EntityPersisterHelper(IDatabase database, Type entityType, EntityPersistentEnvironment environment)
        {
            this.database = database;
            this.entityType = entityType;
            this.environment = environment;
        }

        /// <summary>
        /// 使用数据库事务进行批处理。
        /// </summary>
        /// <param name="action">批量执行的方法。</param>
        public void Batch(Action action)
        {
            if (action != null)
            {
                database.WithTransaction(db => action());
            }
        }

        /// <summary>
        /// 将一个新的实体对象创建到库。
        /// </summary>
        /// <param name="entity">要创建的实体对象。</param>
        public void Create(IEntity entity)
        {
            ValidationUnity.Validate(entity);
            EntityPersistentSubscribePublisher.OnBeforeCreate(entity);

            try
            {
                Batch(() =>
                    {
                        var isSucc = false;
                        entity.TryLockModifing(() =>
                            {
                                isSucc = InternalCreate(entity);
                                UpdateRelations(entity);
                            });
                        if (isSucc)
                        {
                            EntityPersistentSubscribePublisher.OnAfterCreate(entity);
                            entity.SetState(EntityState.Unchanged);
                        }
                    });
            }
            catch (DbException exp)
            {
                throw new EntityPersistentException(SR.GetString(SRKind.FailInEntityCreate), exp);
            }
        }

        /// <summary>
        /// 更新实体对象的修改。
        /// </summary>
        /// <param name="entity">要更新的实体对象。</param>
        public void Update(IEntity entity)
        {
            ValidationUnity.Validate(entity);
            EntityPersistentSubscribePublisher.OnBeforeUpdate(entity);

            try
            {
                Batch(() =>
                    {
                        var parameters = new ParameterCollection();
                        var context = CreateContext(parameters);
                        var sql = EntityPersistentQueryBuilder.BuildUpdateQuery(context, entity);

                        var isSucc = false;
                        entity.TryLockModifing(() =>
                            {
                                if (!sql.IsNullOrEmpty())
                                {
                                    isSucc = database.ExecuteNonQuery(sql, parameters) > 0;
                                }

                                UpdateRelations(entity);
                            });

                        if (isSucc)
                        {
                            EntityPersistentSubscribePublisher.OnAfterUpdate(entity);
                            entity.SetState(EntityState.Unchanged);
                        }
                    });
            }
            catch (Exception exp)
            {
                throw new EntityPersistentException(SR.GetString(SRKind.FailInEntityUpdate), exp);
            }
        }

        /// <summary>
        /// 将指定的实体对象从库中移除。
        /// </summary>
        /// <param name="entity">要移除的实体对象。</param>
        /// <param name="fakeProperty">标识假删除的属性。</param>
        /// <param name="fake">如果具有 IsDeletedKey 属性，则提供对数据假删除的支持。</param>
        /// <exception cref="NullReferenceException">对象内部的 Database 为 null。</exception>
        /// <exception cref="ArgumentNullException"><paramref name="entity"/> 参数为 null。</exception>
        public void Remove(IEntity entity, IProperty fakeProperty, bool fake = true)
        {
            EntityPersistentSubscribePublisher.OnBeforeRemove(entity);
            try
            {
                Batch(() =>
                    {
                        var parameters = new ParameterCollection();
                        var context = CreateContext(parameters);

                        var canFake = fakeProperty != null && fake;
                        var sql = canFake ? EntityPersistentQueryBuilder.BuildUpdateFakeDeleteQuery(context, entity, fakeProperty)
                            : EntityPersistentQueryBuilder.BuildDeleteQuery(context, entity);

                        var isSucc = false;
                        entity.TryLockModifing(() =>
                            {
                                if (!sql.IsNullOrEmpty())
                                {
                                    isSucc = database.ExecuteNonQuery(sql, parameters) > 0;
                                }

                                UpdateRelations(entity);
                            });
                        if (isSucc)
                        {
                            EntityPersistentSubscribePublisher.OnAfterRemove(entity);
                            entity.SetState(EntityState.Unchanged);
                        }
                    });
            }
            catch (DbException exp)
            {
                throw new EntityPersistentException(SR.GetString(SRKind.FailInEntityRemove), exp);
            }
        }
        
        /// <summary>
        /// 根据主键值将对象从库中移除。
        /// </summary>
        /// <param name="primaryValues">主键的值。数组的长度必须与实体所定义的主键相匹配。</param>
        /// <param name="fakeProperty">标识假删除的属性。</param>
        /// <param name="fake">如果具有 IsDeletedKey 属性，则提供对对象假删除的支持。</param>
        /// <exception cref="NullReferenceException">对象内部的 Database 或 EntityType 为 null。</exception>
        /// <exception cref="ArgumentNullException"><paramref name="primaryValues"/> 参数为 null。</exception>
        public void Remove(object[] primaryValues, IProperty fakeProperty, bool fake = true)
        {
            Guard.NullReference(database);
            Guard.NullReference(entityType);
            Guard.ArgumentNull(primaryValues, "primaryValues");

            try
            {
                var parameters = new ParameterCollection();
                var context = CreateContext(parameters);

                var canFake = fakeProperty != null && fake;
                var sql = canFake ? EntityPersistentQueryBuilder.BuildUpdateFakeDeleteQuery(context, fakeProperty, primaryValues)
                              : EntityPersistentQueryBuilder.BuildDeleteQuery(context, entityType, primaryValues);

                if (!sql.IsNullOrEmpty())
                {
                    database.ExecuteNonQuery(sql, parameters);
                }
            }
            catch (DbException exp)
            {
                throw new EntityPersistentException(SR.GetString(SRKind.FailInEntityRemove), exp);
            }
        }

        /// <summary>
        /// 使用主键值查询返回一个实体。
        /// </summary>
        /// <param name="primaryValues">主键的值。数组的长度必须与实体所定义的主键相匹配。</param>
        /// <param name="func">对实体对象进行修饰的方法。</param>
        /// <returns>一个实体对象。</returns>
        public IEntity First(object[] primaryValues, Func<object, object> func)
        {
            if (primaryValues == null || primaryValues.Length == 0)
            {
                return null;
            }

            var parameters = new ParameterCollection();
            var context = CreateContext(parameters);
            var sql = EntityPersistentQueryBuilder.BuildGetFirstQuery(context, entityType, primaryValues);
            if (sql.IsNullOrEmpty())
            {
                return null;
            }

            var mapper = RowMapperFactory.CreateMapper(entityType);
            mapper.RecordWrapper = database.Provider.GetService<IRecordWrapper>();
            using (var reader = database.ExecuteReader(sql, parameters: parameters))
            {
                if (reader.Read())
                {
                    return func(mapper.Map(reader).InitializeEnvironment(environment)) as IEntity;
                }
            }

            return null;
        }

        /// <summary>
        /// 将一组实体对象的更改保存到库。不会更新实体的其他引用属性。
        /// </summary>
        /// <param name="entities">要保存的实体序列。</param>
        public void Save(IEnumerable entities)
        {
            if (entities == null)
            {
                return;
            }

            var entitySet = entities.As<IEntitySetInternalExtension>();
            if (entitySet != null)
            {
                Save(entitySet);
            }
            else
            {
                foreach (IEntity entity in entities)
                {
                    switch (entity.EntityState)
                    {
                        case EntityState.Modified:
                            Update(entity);
                            break;
                        case EntityState.Attached:
                            Create(entity);
                            break;
                    }
                }
            }
        }

        private void Save(IEntitySetInternalExtension entitySet)
        {
            foreach (IEntity entity in entitySet.GetAttachedList())
            {
                Create(entity);
            }

            foreach (IEntity entity in entitySet.GetModifiedList())
            {
                Update(entity);
            }

            var fakePro = EntityMetadataUnity.GetEntityMetadata(entitySet.EntityType).DeleteProperty;
            foreach (IEntity entity in entitySet.GetDetachedList())
            {
                Remove(entity, fakePro);
            }

            entitySet.Reset();
        }

        private void UpdateRelations(IEntity entity)
        {
            var entityEx = entity.As<IEntityStatefulExtension>();
            if (entityEx == null)
            {
                return;
            }

            var relProperties = PropertyUnity.GetRelatedProperties(entity.EntityType);
            foreach (var property in relProperties)
            {
                var relationPro = property as RelationProperty;
                var value = entityEx.GetDirectValue(property);
                if (value == null)
                {
                    continue;
                }

                switch (relationPro.RelationPropertyType)
                {
                    case RelationPropertyType.EntitySet:
                        var list = PropertyValueHelper.GetValue<IEnumerable>(value);
                        EntityUtility.AttachPrimaryKeyValues(entity, relationPro, list);
                        Save(list);
                        break;
                    case RelationPropertyType.Entity:
                        SaveRelationEntity(PropertyValueHelper.GetValue<IEntity>(value));
                        break;
                }
            }
        }

        private void SaveRelationEntity(IEntity refEntity)
        {
            switch (refEntity.EntityState)
            {
                case EntityState.Modified:
                    Update(refEntity);
                    break;
                case EntityState.Attached:
                    Create(refEntity);
                    break;
                case EntityState.Detached:
                    var fakePro = EntityMetadataUnity.GetEntityMetadata(refEntity.EntityType).DeleteProperty;
                    Remove(refEntity, fakePro);
                    break;
            }
        }

        private EntityQueryContext CreateContext(ParameterCollection parameters)
        {
            return new EntityQueryContext
                {
                    Database = database,
                    Syntax = database.Provider.GetService<ISyntaxProvider>(),
                    Environment = environment,
                    Parameters = parameters
                };
        }

        private bool InternalCreate(IEntity entity)
        {
            var parameters = new ParameterCollection();
            var context = CreateContext(parameters);
            var sql = EntityPersistentQueryBuilder.BuidCreateQuery(context, entity);

            if (!sql.IsNullOrEmpty())
            {
                var rootType = entity.EntityType.GetRootType();

                //找出自增长序列的属性
                var incProperty = PropertyUnity.GetPersistentProperties(rootType).FirstOrDefault(s => s.Info.GenerateType == IdentityGenerateType.AutoIncrement);

                if (incProperty != null)
                {
                    if (!string.IsNullOrEmpty(context.Syntax.IdentitySelect))
                    {
                        //获得当前插入的自增长序列值
                        var identitySelect = context.Syntax.IdentitySelect;
                        if (!identitySelect.StartsWith(";"))
                        {
                            identitySelect = ";" + identitySelect;
                        }

                        var incValue = PropertyValueHelper.NewValue(context.Database.ExecuteScalar<int>(sql + identitySelect, context.Parameters));
                        entity.InternalSetValue(incProperty, incValue);
                        return !incValue.IsNullOrEmpty();
                    }
                    else
                    {
                        //使用生成器生成值
                        var generator = context.Database.Provider.GetService<IGeneratorProvider>();
                        if (generator != null)
                        {
                            var metadata = EntityMetadataUnity.GetEntityMetadata(entityType);
                            var inc = generator.GenerateValue(context.Database, context.Environment == null ? metadata.TableName : context.Environment.GetVariableTableName(metadata), incProperty.Info.FieldName);
                            entity.InternalSetValue(incProperty, inc);

                            parameters.Clear();
                            sql = EntityPersistentQueryBuilder.BuidCreateQuery(context, entity);

                            return context.Database.ExecuteNonQuery(sql, context.Parameters) > 0;
                        }
                    }
                }
                
                return database.ExecuteNonQuery(sql, parameters) > 0;
            }

            return false;
        }

        private IEntity GetCloneEntity(IEntity entity)
        {
            var kp = entity as IKeepStateCloneable;
            if (kp != null)
            {
                return (IEntity)kp.Clone();
            }

            return entity;
        }
    }
}
