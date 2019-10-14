// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Data.Batcher;
using Fireasy.Data.Entity.Linq;
using Fireasy.Data.Entity.Metadata;
using Fireasy.Data.Entity.Properties;
using Fireasy.Data.Syntax;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 缺省的仓储服务实现，使用 Linq to SQL。
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public sealed class DefaultRepositoryProvider<TEntity> : IQueryPolicyExecutor<TEntity>,
        IRepositoryProvider<TEntity> where TEntity : IEntity
    {
        private DefaultContextService context;

        /// <summary>
        /// 初始化 <see cref="DefaultRepositoryProvider"/> 类的新实例。
        /// </summary>
        /// <param name="service"></param>
        public DefaultRepositoryProvider(IContextService service)
        {
            context = (DefaultContextService)service;

            var entityQueryProvider = new EntityQueryProvider(this.context);
            context.As((Action<IEntityPersistentInstanceContainer>)(s => entityQueryProvider.InitializeInstanceName(s.InstanceName)));

            QueryProvider = new QueryProvider(entityQueryProvider);
            Queryable = new QuerySet<TEntity>(QueryProvider);
        }

        IRepository IRepositoryProvider.CreateRepository(EntityContextOptions options)
        {
            return new EntityRepository<TEntity>(this, options);
        }

        /// <summary>
        /// 获取 <see cref="IQueryable"/> 对象。
        /// </summary>
        public IQueryable Queryable { get; private set; }

        /// <summary>
        /// 获取 <see cref="IQueryProvider"/> 对象。
        /// </summary>
        public IQueryProvider QueryProvider { get; private set; }

        /// <summary>
        /// 将一个新的实体对象插入到库。
        /// </summary>
        /// <param name="entity">要创建的实体对象。</param>
        /// <returns>如果实体中有自增属性的主键，则返回主键值；否则返回影响的实体数。</returns>
        public async Task<int> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var trans = CheckRelationHasModified(entity);
            if (trans)
            {
                context.Database.BeginTransaction();
            }

            int result = 0;

            try
            {
                if ((result = await Queryable.CreateEntityAsync(entity, cancellationToken)) > 0)
                {
                    entity.As<IEntityPersistentEnvironment>(s => s.Environment = context.Environment);
                    entity.As<IEntityPersistentInstanceContainer>(s => s.InstanceName = context.InstanceName);

                    await HandleRelationPropertiesAsync(entity, cancellationToken);
                }

                if (trans)
                {
                    context.Database.CommitTransaction();
                }
            }
            catch (Exception exp)
            {
                if (trans)
                {
                    context.Database.RollbackTransaction();
                }

                throw exp;
            }

            return result;
        }

        /// <summary>
        /// 更新一个实体对象。
        /// </summary>
        /// <param name="entity">要更新的实体对象。</param>
        /// <returns>影响的实体数。</returns>
        public async Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var trans = CheckRelationHasModified(entity);
            if (trans)
            {
                context.Database.BeginTransaction();
            }

            int result;
            try
            {
                result = await Queryable.UpdateEntityAsync(entity, cancellationToken);

                await HandleRelationPropertiesAsync(entity, cancellationToken);

                if (trans)
                {
                    context.Database.CommitTransaction();
                }
            }
            catch (Exception exp)
            {
                if (trans)
                {
                    context.Database.RollbackTransaction();
                }

                throw exp;
            }

            return result;
        }

        /// <summary>
        /// 批量将一组实体对象插入到库中。
        /// </summary>
        /// <param name="entities">一组要插入实体对象。</param>
        /// <param name="batchSize">每一个批次插入的实体数量。默认为 1000。</param>
        /// <param name="completePercentage">已完成百分比的通知方法。</param>
        public async Task BatchInsertAsync(IEnumerable<TEntity> entities, int batchSize = 1000, Action<int> completePercentage = null, CancellationToken cancellationToken = default)
        {
            var batcher = context.Database.Provider.GetService<IBatcherProvider>();
            if (batcher == null)
            {
                throw new EntityPersistentException(SR.GetString(SRKind.NotSupportBatcher), null);
            }

            var syntax = context.Database.Provider.GetService<ISyntaxProvider>();
            var rootType = typeof(TEntity).GetRootEntityType();
            string tableName;

            if (context.Environment != null)
            {
                tableName = DbUtility.FormatByQuote(syntax, context.Environment.GetVariableTableName(rootType));
            }
            else
            {
                var metadata = EntityMetadataUnity.GetEntityMetadata(rootType);
                tableName = DbUtility.FormatByQuote(syntax, metadata.TableName);
            }

            await batcher.InsertAsync(context.Database, entities, tableName, batchSize, completePercentage, cancellationToken);
        }

        /// <summary>
        /// 将指定的实体对象从库中删除。
        /// </summary>
        /// <param name="entity">要移除的实体对象。</param>
        /// <param name="logicalDelete">是否为逻辑删除。</param>
        /// <returns>影响的实体数。</returns>
        public async Task<int> DeleteAsync(TEntity entity, bool logicalDelete = true, CancellationToken cancellationToken = default)
        {
            return await Queryable.RemoveEntityAsync(entity, logicalDelete, cancellationToken);
        }

        /// <summary>
        /// 根据主键值将对象从库中删除。
        /// </summary>
        /// <param name="primaryValues">一组主键值。</param>
        /// <param name="logicalDelete">是否为逻辑删除。</param>
        /// <returns></returns>
        public async Task<int> DeleteAsync(PropertyValue[] primaryValues, bool logicalDelete = true, CancellationToken cancellationToken = default)
        {
            if (primaryValues.Any(s => PropertyValue.IsEmpty(s)))
            {
                return 0;
            }

            return await Queryable.RemoveByPrimaryAsync(primaryValues, logicalDelete, cancellationToken);
        }

        /// <summary>
        /// 通过一组主键值返回一个实体对象。
        /// </summary>
        /// <param name="primaryValues">一组主键值。</param>
        /// <returns>影响的实体数。</returns>
        public async Task<TEntity> GetAsync(PropertyValue[] primaryValues, CancellationToken cancellationToken = default)
        {
            if (primaryValues.Any(s => PropertyValue.IsEmpty(s)))
            {
                return default(TEntity);
            }

            return await Queryable.GetByPrimaryAsync<TEntity>(primaryValues, cancellationToken);
        }

        /// <summary>
        /// 将满足条件的一组对象从库中移除。
        /// </summary>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="logicalDelete">是否为逻辑删除</param>
        /// <returns>影响的实体数。</returns>
        public async Task<int> DeleteAsync(Expression<Func<TEntity, bool>> predicate, bool logicalDelete = true, CancellationToken cancellationToken = default)
        {
            return await Queryable.RemoveWhereAsync(predicate, logicalDelete, cancellationToken);
        }

        /// <summary>
        /// 使用一个参照的实体对象更新满足条件的一序列对象。
        /// </summary>
        /// <param name="entity">更新的参考对象。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <returns>影响的实体数。</returns>
        public async Task<int> UpdateAsync(TEntity entity, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await Queryable.UpdateWhereAsync(entity, predicate, cancellationToken);
        }

        /// <summary>
        /// 使用一个累加器更新满足条件的一序列对象。
        /// </summary>
        /// <param name="calculator">一个计算器表达式。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <returns>影响的实体数。</returns>
        public async Task<int> UpdateAsync(Expression<Func<TEntity, TEntity>> calculator, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await Queryable.UpdateWhereByCalculatorAsync(calculator, predicate);
        }

        /// <summary>
        /// 对实体集合进行批量操作。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instances"></param>
        /// <param name="fnOperation"></param>
        /// <returns>影响的实体数。</returns>
        public async Task<int> BatchAsync(IEnumerable<TEntity> instances, Expression<Func<IRepository<TEntity>, TEntity, int>> fnOperation, CancellationToken cancellationToken = default)
        {
            return await Queryable.BatchOperateAsync(instances.Cast<IEntity>(), fnOperation, cancellationToken);
        }

        void IQueryPolicyExecutor<TEntity>.IncludeWith(Expression<Func<TEntity, object>> fnMember)
        {
            context.IncludeWith(fnMember);
        }

        void IQueryPolicyExecutor<TEntity>.AssociateWith(Expression<Func<TEntity, IEnumerable>> memberQuery)
        {
            context.AssociateWith(memberQuery);
        }

        /// <summary>
        /// 检查有没有关联属性被修改.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private bool CheckRelationHasModified(IEntity entity)
        {
            //判断实体类型有是不是编译的代理类型，如果不是，取非null的属性，否则使用IsModified判断
            var isNotCompiled = entity.GetType().IsNotCompiled();
            return PropertyUnity.GetRelatedProperties(entity.EntityType).Any(s => isNotCompiled ? !PropertyValue.IsEmpty(entity.GetValue(s)) : entity.IsModified(s.Name));
        }

        /// <summary>
        /// 检查实体的关联属性。
        /// </summary>
        /// <param name="entity"></param>
        private async Task HandleRelationPropertiesAsync(IEntity entity, CancellationToken cancellationToken = default)
        {
            //判断实体类型有是不是编译的代理类型，如果不是，取非null的属性，否则使用IsModified判断
            var isNotCompiled = entity.GetType().IsNotCompiled();
            var properties = PropertyUnity.GetRelatedProperties(entity.EntityType).Where(m => isNotCompiled ?
                    !PropertyValue.IsEmpty(entity.GetValue(m)) :
                    entity.IsModified(m.Name));

            await HandleRelationPropertiesAsync(entity, properties, cancellationToken);
        }

        /// <summary>
        /// 处理实体的关联的属性。
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="properties"></param>
        private async Task HandleRelationPropertiesAsync(IEntity entity, IEnumerable<IProperty> properties, CancellationToken cancellationToken = default)
        {
            foreach (RelationProperty property in properties)
            {
                var queryable = (IQueryable)context.GetDbSet(property.RelationalType);

                switch (property.RelationalPropertyType)
                {
                    case RelationPropertyType.Entity:
                        var refEntity = (IEntity)entity.GetValue(property).GetValue();
                        switch (refEntity.EntityState)
                        {
                            case EntityState.Modified:
                                await queryable.UpdateEntityAsync(refEntity, cancellationToken);
                                refEntity.SetState(EntityState.Unchanged);
                                break;
                        }

                        await HandleRelationPropertiesAsync(refEntity, cancellationToken);
                        break;
                    case RelationPropertyType.EntitySet:
                        var value = entity.GetValue(property);
                        if (PropertyValue.IsEmpty(value))
                        {
                            value = entity.GetOldValue(property);
                        }

                        if (!PropertyValue.IsEmpty(value))
                        {
                            await HandleEntitySetPropertyAsync(queryable, entity, value.GetValue() as IEntitySet, property, cancellationToken);
                        }

                        break;
                }
            }
        }

        /// <summary>
        /// 处理关联的实体集合。
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="entity"></param>
        /// <param name="entitySet"></param>
        /// <param name="property"></param>
        private async Task HandleEntitySetPropertyAsync(IQueryable queryable, IEntity entity, IEntitySet entitySet, IProperty property, CancellationToken cancellationToken = default)
        {
            var added = entitySet.GetAttachedList();
            var modified = entitySet.GetModifiedList();
            var deleted = entitySet.GetDetachedList();

            //处理删除的
            if (deleted.Count() > 0)
            {
                await queryable.BatchOperateAsync(deleted, queryable.CreateDeleteExpression(true), cancellationToken);
            }

            //处理修改的
            if (modified.Count() > 0)
            {
                if (entitySet.AllowBatchUpdate)
                {
                    await queryable.BatchOperateAsync(modified, queryable.CreateUpdateExpression(), cancellationToken);
                }
                else
                {
                    foreach (var e in modified)
                    {
                        await queryable.UpdateEntityAsync(e, cancellationToken);
                        e.SetState(EntityState.Unchanged);
                        await HandleRelationPropertiesAsync(e, cancellationToken);
                    }
                }
            }

            //处理新增的
            if (added.Count() > 0)
            {
                var relation = RelationshipUnity.GetRelationship(property);
                added.ForEach(e =>
                {
                    foreach (var key in relation.Keys)
                    {
                        var value = entity.GetValue(key.ThisProperty);
                        e.SetValue(key.OtherProperty, value);
                    }
                });

                if (entitySet.AllowBatchInsert)
                {
                    await queryable.BatchOperateAsync(added, queryable.CreateInsertExpression(), cancellationToken);
                }
                else
                {
                    foreach (var e in added)
                    {
                        await queryable.CreateEntityAsync(e, cancellationToken);
                        e.SetState(EntityState.Unchanged);
                        await HandleRelationPropertiesAsync(e, cancellationToken);
                    }
                }
            }

            entitySet.Reset();
        }
    }
}