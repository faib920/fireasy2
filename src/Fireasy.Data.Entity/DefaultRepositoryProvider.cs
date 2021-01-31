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
using Fireasy.Data.Entity.Query;
using Fireasy.Data.Provider;
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
    public sealed class DefaultRepositoryProvider<TEntity> : IQueryPolicyExecutor,
        IRepositoryProvider<TEntity> where TEntity : IEntity
    {
        private IRepository _repository;
        private readonly DefaultContextService _contextService;
        private readonly IProvider _provider;
        private readonly IDatabase _database;

        /// <summary>
        /// 初始化 <see cref="DefaultRepositoryProvider{TEntity}"/> 类的新实例。
        /// </summary>
        /// <param name="contextService"></param>
        public DefaultRepositoryProvider(DefaultContextService contextService)
        {
            _contextService = contextService;
            _provider = contextService.Provider;
            _database = contextService.Database;

            QueryProvider = new QueryProvider(new EntityQueryProvider(contextService));
            Queryable = new QuerySet<TEntity>(QueryProvider);
        }

        IRepository IRepositoryProvider.CreateRepository(EntityContextOptions options)
        {
            return _repository ??= new EntityRepository<TEntity>(_contextService, this, options);
        }

        /// <summary>
        /// 获取 <see cref="IQueryable"/> 实例。
        /// </summary>
        public IQueryable Queryable { get; }

        /// <summary>
        /// 获取 <see cref="IQueryProvider"/> 实例。
        /// </summary>
        public IQueryProvider QueryProvider { get; }

        /// <summary>
        /// 将一个新的实体对象插入到库。
        /// </summary>
        /// <param name="entity">要创建的实体对象。</param>
        /// <param name="propertyFilter">属性过滤器。</param>
        /// <returns>如果实体中有自增属性的主键，则返回主键值；否则返回影响的实体数。</returns>
        public long Insert(TEntity entity, IPropertyFilter propertyFilter = null)
        {
            var exeContext = new EntityExecuteContext(_contextService.Provider, entity, propertyFilter);
            return InternalInsert(exeContext);
        }

        /// <summary>
        /// 异步的，将一个新的实体对象插入到库。
        /// </summary>
        /// <param name="entity">要创建的实体对象。</param>
        /// <param name="propertyFilter">属性过滤器。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>如果实体中有自增属性的主键，则返回主键值；否则返回影响的实体数。</returns>
        public async Task<long> InsertAsync(TEntity entity, IPropertyFilter propertyFilter = null, CancellationToken cancellationToken = default)
        {
            var exeContext = new EntityExecuteContext(_contextService.Provider, entity, propertyFilter);
            return await InternalInsertAsync(exeContext);
        }

        /// <summary>
        /// 更新一个实体对象。
        /// </summary>
        /// <param name="entity">要更新的实体对象。</param>
        /// <param name="propertyFilter">属性过滤器。</param>
        /// <returns>影响的实体数。</returns>
        public int Update(TEntity entity, IPropertyFilter propertyFilter = null)
        {
            var exeContext = new EntityExecuteContext(_contextService.Provider, entity, propertyFilter);

            return InternalUpdate(exeContext);
        }

        /// <summary>
        /// 异步的，更新一个实体对象。
        /// </summary>
        /// <param name="entity">要更新的实体对象。</param>
        /// <param name="propertyFilter">属性过滤器。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>影响的实体数。</returns>
        public async Task<int> UpdateAsync(TEntity entity, IPropertyFilter propertyFilter = null, CancellationToken cancellationToken = default)
        {
            var exeContext = new EntityExecuteContext(_contextService.Provider, entity, propertyFilter);

            return await InternalUpdateAsync(exeContext);
        }

        /// <summary>
        /// 批量将一组实体对象插入到库中。
        /// </summary>
        /// <param name="entities">一组要插入实体对象。</param>
        /// <param name="batchSize">每一个批次插入的实体数量。默认为 1000。</param>
        /// <param name="completePercentage">已完成百分比的通知方法。</param>
        public void BatchInsert(IEnumerable<TEntity> entities, int batchSize = 1000, Action<int> completePercentage = null)
        {
            var batcher = _provider.GetService<IBatcherProvider>();
            if (batcher == null)
            {
                throw new EntityPersistentException(SR.GetString(SRKind.NotSupportBatcher), null);
            }

            var syntax = _provider.GetService<ISyntaxProvider>();
            var rootType = typeof(TEntity).GetRootEntityType();
            string tableName;

            if (_contextService.Environment != null)
            {
                tableName = syntax.DelimitTable(_contextService.Environment.GetVariableTableName(rootType));
            }
            else
            {
                var metadata = EntityMetadataUnity.GetEntityMetadata(rootType);
                tableName = syntax.DelimitTable(metadata.TableName);
            }

            batcher.Insert(_database, entities, tableName, batchSize, completePercentage);
        }

        /// <summary>
        /// 异步的，批量将一组实体对象插入到库中。
        /// </summary>
        /// <param name="entities">一组要插入实体对象。</param>
        /// <param name="batchSize">每一个批次插入的实体数量。默认为 1000。</param>
        /// <param name="completePercentage">已完成百分比的通知方法。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        public async Task BatchInsertAsync(IEnumerable<TEntity> entities, int batchSize = 1000, Action<int> completePercentage = null, CancellationToken cancellationToken = default)
        {
            var batcher = _provider.GetService<IBatcherProvider>();
            if (batcher == null)
            {
                throw new EntityPersistentException(SR.GetString(SRKind.NotSupportBatcher), null);
            }

            var syntax = _provider.GetService<ISyntaxProvider>();
            var rootType = typeof(TEntity).GetRootEntityType();
            string tableName;

            if (_contextService.Environment != null)
            {
                tableName = syntax.DelimitTable(_contextService.Environment.GetVariableTableName(rootType));
            }
            else
            {
                var metadata = EntityMetadataUnity.GetEntityMetadata(rootType);
                tableName = syntax.DelimitTable(metadata.TableName);
            }

            await batcher.InsertAsync(_database, entities, tableName, batchSize, completePercentage, cancellationToken);
        }

        /// <summary>
        /// 将指定的实体对象从库中删除。
        /// </summary>
        /// <param name="entity">要移除的实体对象。</param>
        /// <param name="logicalDelete">是否为逻辑删除。</param>
        /// <returns>影响的实体数。</returns>
        public int Delete(TEntity entity, bool logicalDelete = true)
        {
            return Queryable.RemoveEntity(entity, logicalDelete);
        }

        /// <summary>
        /// 异步的，将指定的实体对象从库中删除。
        /// </summary>
        /// <param name="entity">要移除的实体对象。</param>
        /// <param name="logicalDelete">是否为逻辑删除。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
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
        public int Delete(PropertyValue[] primaryValues, bool logicalDelete = true)
        {
            if (primaryValues.Any(s => PropertyValue.IsEmpty(s)))
            {
                return 0;
            }

            return Queryable.RemoveByPrimary(primaryValues, logicalDelete);
        }

        /// <summary>
        /// 异步的，根据主键值将对象从库中删除。
        /// </summary>
        /// <param name="primaryValues">一组主键值。</param>
        /// <param name="logicalDelete">是否为逻辑删除。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
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
        /// <returns></returns>
        public TEntity Get(PropertyValue[] primaryValues)
        {
            if (primaryValues.Any(s => PropertyValue.IsEmpty(s)))
            {
                return default;
            }

            return Queryable.GetByPrimary<TEntity>(primaryValues);
        }

        /// <summary>
        /// 异步的，通过一组主键值返回一个实体对象。
        /// </summary>
        /// <param name="primaryValues">一组主键值。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        public async Task<TEntity> GetAsync(PropertyValue[] primaryValues, CancellationToken cancellationToken = default)
        {
            if (primaryValues.Any(s => PropertyValue.IsEmpty(s)))
            {
                return default;
            }

            return await Queryable.GetByPrimaryAsync<TEntity>(primaryValues, cancellationToken);
        }

        /// <summary>
        /// 将满足条件的一组对象从库中移除。
        /// </summary>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="logicalDelete">是否为逻辑删除</param>
        /// <returns>影响的实体数。</returns>
        public int Delete(Expression<Func<TEntity, bool>> predicate, bool logicalDelete = true)
        {
            return Queryable.RemoveWhere(predicate, logicalDelete);
        }

        /// <summary>
        /// 异步的，将满足条件的一组对象从库中移除。
        /// </summary>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="logicalDelete">是否为逻辑删除</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
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
        /// <param name="propertyFilter">属性过滤器。</param>
        /// <returns>影响的实体数。</returns>
        public int Update(TEntity entity, Expression<Func<TEntity, bool>> predicate, IPropertyFilter propertyFilter = null)
        {
            var exeContext = new EntityExecuteContext(_contextService.Provider, entity, propertyFilter);
            return Queryable.UpdateWhere(exeContext, predicate);
        }

        /// <summary>
        /// 异步的，使用一个参照的实体对象更新满足条件的一序列对象。
        /// </summary>
        /// <param name="entity">更新的参考对象。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="propertyFilter">属性过滤器。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>影响的实体数。</returns>
        public async Task<int> UpdateAsync(TEntity entity, Expression<Func<TEntity, bool>> predicate, IPropertyFilter propertyFilter = null, CancellationToken cancellationToken = default)
        {
            var exeContext = new EntityExecuteContext(_contextService.Provider, entity, propertyFilter);
            return await Queryable.UpdateWhereAsync(exeContext, predicate, cancellationToken);
        }

        /// <summary>
        /// 使用一个累加器更新满足条件的一序列对象。
        /// </summary>
        /// <param name="calculator">一个计算器表达式。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <returns>影响的实体数。</returns>
        public int Update(Expression<Func<TEntity, TEntity>> calculator, Expression<Func<TEntity, bool>> predicate)
        {
            return Queryable.UpdateWhereByCalculator(calculator, predicate);
        }

        /// <summary>
        /// 异步的，使用一个累加器更新满足条件的一序列对象。
        /// </summary>
        /// <param name="calculator">一个计算器表达式。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>影响的实体数。</returns>
        public async Task<int> UpdateAsync(Expression<Func<TEntity, TEntity>> calculator, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await Queryable.UpdateWhereByCalculatorAsync(calculator, predicate, cancellationToken);
        }

        /// <summary>
        /// 对实体集合进行批量操作。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instances"></param>
        /// <param name="fnOperation"></param>
        /// <returns>影响的实体数。</returns>
        public int Batch(IEnumerable<TEntity> instances, Expression<Action<IRepository<TEntity>, TEntity>> fnOperation, BatchOperateOptions batchOpt = null)
        {
            return Queryable.BatchOperate(instances.Cast<IEntity>(), fnOperation, batchOpt);
        }

        /// <summary>
        /// 异步的，对实体集合进行批量操作。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instances"></param>
        /// <param name="fnOperation"></param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>影响的实体数。</returns>
        public async Task<int> BatchAsync(IEnumerable<TEntity> instances, Expression<Action<IRepository<TEntity>, TEntity>> fnOperation, BatchOperateOptions batchOpt = null, CancellationToken cancellationToken = default)
        {
            return await Queryable.BatchOperateAsync(instances.Cast<IEntity>(), fnOperation, batchOpt, cancellationToken);
        }

        void IQueryPolicyExecutor.IncludeWith<TOEntity, TProperty>(Expression<Func<TOEntity, TProperty>> fnMember)
        {
            InvokeQueryPolicy(q => q.IncludeWith(fnMember));
        }

        void IQueryPolicyExecutor.AssociateWith<TOEntity>(Expression<Func<TOEntity, IEnumerable>> memberQuery)
        {
            InvokeQueryPolicy(q => q.AssociateWith(memberQuery));
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

        #region InternalInsert
        private long InternalInsert(EntityExecuteContext exeContext)
        {
            var trans = CheckRelationHasModified(exeContext.Entity);
            if (trans)
            {
                _database.BeginTransaction();
            }

            long result;

            try
            {
                result = Queryable.CreateEntity(exeContext);

                exeContext.Entity.InitializeEnvironment(_contextService.Environment).InitializeInstanceName(_contextService.InstanceName);

                if (exeContext.IsEfficient)
                {
                    HandleRelationProperties(exeContext.Entity);
                }

                if (trans)
                {
                    _database.CommitTransaction();
                }
            }
            catch (Exception exp)
            {
                if (trans)
                {
                    _database.RollbackTransaction();
                }

                throw exp;
            }

            return result;
        }

        private async Task<long> InternalInsertAsync(EntityExecuteContext exeContext)
        {
            var trans = CheckRelationHasModified(exeContext.Entity);
            if (trans)
            {
                _database.BeginTransaction();
            }

            long result;

            try
            {
                result = await Queryable.CreateEntityAsync(exeContext);

                exeContext.Entity.InitializeEnvironment(_contextService.Environment).InitializeInstanceName(_contextService.InstanceName);

                if (exeContext.IsEfficient)
                {
                    await HandleRelationPropertiesAsync(exeContext.Entity);
                }

                if (trans)
                {
                    _database.CommitTransaction();
                }
            }
            catch (Exception exp)
            {
                if (trans)
                {
                    _database.RollbackTransaction();
                }

                throw exp;
            }

            return result;
        }

        #endregion

        #region InternalUpdate
        private int InternalUpdate(EntityExecuteContext exeContext)
        {
            var trans = CheckRelationHasModified(exeContext.Entity);
            if (trans)
            {
                _database.BeginTransaction();
            }

            int result;
            try
            {
                result = Queryable.UpdateEntity(exeContext);

                if (exeContext.IsEfficient)
                {
                    HandleRelationProperties(exeContext.Entity);
                }

                if (trans)
                {
                    _database.CommitTransaction();
                }
            }
            catch (Exception exp)
            {
                if (trans)
                {
                    _database.RollbackTransaction();
                }

                throw exp;
            }

            return result;
        }

        private async Task<int> InternalUpdateAsync(EntityExecuteContext exeContext)
        {
            var trans = CheckRelationHasModified(exeContext.Entity);
            if (trans)
            {
                _database.BeginTransaction();
            }

            int result;
            try
            {
                result = await Queryable.UpdateEntityAsync(exeContext);

                if (exeContext.IsEfficient)
                {
                    await HandleRelationPropertiesAsync(exeContext.Entity);
                }

                if (trans)
                {
                    _database.CommitTransaction();
                }
            }
            catch (Exception exp)
            {
                if (trans)
                {
                    _database.RollbackTransaction();
                }

                throw exp;
            }

            return result;
        }
        #endregion

        #region Internal
        /// <summary>
        /// 检查实体的关联属性。
        /// </summary>
        /// <param name="entity"></param>
        private void HandleRelationProperties(IEntity entity)
        {
            //判断实体类型有是不是编译的代理类型，如果不是，取非null的属性，否则使用IsModified判断
            var isNotCompiled = entity.GetType().IsNotCompiled();
            var properties = PropertyUnity.GetRelatedProperties(entity.EntityType).Where(m => isNotCompiled ?
                    !PropertyValue.IsEmpty(entity.GetValue(m)) :
                    entity.IsModified(m.Name));

            HandleRelationProperties(entity, properties);
        }

        /// <summary>
        /// 检查实体的关联属性。
        /// </summary>
        /// <param name="entity"></param>
        private async Task HandleRelationPropertiesAsync(IEntity entity)
        {
            //判断实体类型有是不是编译的代理类型，如果不是，取非null的属性，否则使用IsModified判断
            var isNotCompiled = entity.GetType().IsNotCompiled();
            var properties = PropertyUnity.GetRelatedProperties(entity.EntityType).Where(m => isNotCompiled ?
                    !PropertyValue.IsEmpty(entity.GetValue(m)) :
                    entity.IsModified(m.Name));

            await HandleRelationPropertiesAsync(entity, properties);
        }

        /// <summary>
        /// 处理实体的关联的属性。
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="properties"></param>
        private void HandleRelationProperties(IEntity entity, IEnumerable<IProperty> properties)
        {
            foreach (RelationProperty property in properties)
            {
                var queryable = (IQueryable)_contextService.CreateRepository(property.RelationalType);

                switch (property.RelationalPropertyType)
                {
                    case RelationPropertyType.Entity:
                        var refEntity = (IEntity)entity.GetValue(property).GetValue();
                        switch (refEntity.EntityState)
                        {
                            case EntityState.Modified:
                                queryable.UpdateEntity(new EntityExecuteContext(_contextService.Provider, refEntity));
                                refEntity.SetState(EntityState.Unchanged);
                                break;
                        }

                        HandleRelationProperties(refEntity);
                        break;
                    case RelationPropertyType.EntitySet:
                        var value = entity.GetValue(property);
                        if (PropertyValue.IsEmpty(value))
                        {
                            value = entity.GetOldValue(property);
                        }

                        if (!PropertyValue.IsEmpty(value))
                        {
                            HandleEntitySetProperty(queryable, entity, value.GetValue() as IEntitySet, property);
                        }

                        break;
                }
            }
        }

        /// <summary>
        /// 处理实体的关联的属性。
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="properties"></param>
        private async Task HandleRelationPropertiesAsync(IEntity entity, IEnumerable<IProperty> properties)
        {
            foreach (RelationProperty property in properties)
            {
                var queryable = (IQueryable)_contextService.CreateRepository(property.RelationalType);

                switch (property.RelationalPropertyType)
                {
                    case RelationPropertyType.Entity:
                        var refEntity = (IEntity)entity.GetValue(property).GetValue();
                        switch (refEntity.EntityState)
                        {
                            case EntityState.Modified:
                                await Queryable.UpdateEntityAsync(new EntityExecuteContext(_contextService.Provider, refEntity));
                                refEntity.SetState(EntityState.Unchanged);
                                break;
                        }

                        await HandleRelationPropertiesAsync(refEntity);
                        break;
                    case RelationPropertyType.EntitySet:
                        var value = entity.GetValue(property);
                        if (PropertyValue.IsEmpty(value))
                        {
                            value = entity.GetOldValue(property);
                        }

                        if (!PropertyValue.IsEmpty(value))
                        {
                            await HandleEntitySetPropertyAsync(queryable, entity, value.GetValue() as IEntitySet, property);
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
        private void HandleEntitySetProperty(IQueryable queryable, IEntity entity, IEntitySet entitySet, IProperty property)
        {
            var added = entitySet.GetAttachedList();
            var modified = entitySet.GetModifiedList();
            var deleted = entitySet.GetDetachedList();

            //处理删除的
            if (deleted.Any())
            {
                queryable.BatchOperate(deleted, queryable.CreateDeleteExpression(true));
            }

            //处理修改的
            if (modified.Any())
            {
                if (entitySet.AllowBatchUpdate)
                {
                    queryable.BatchOperate(modified, queryable.CreateUpdateExpression());
                }
                else
                {
                    foreach (var e in modified)
                    {
                        queryable.UpdateEntity(new EntityExecuteContext(_contextService.Provider, e));
                        e.SetState(EntityState.Unchanged);
                        HandleRelationProperties(e);
                    }
                }
            }

            //处理新增的
            if (added.Any())
            {
                var relation = RelationshipUnity.GetRelationship(property);
                added.ForEach(e =>
                {
                    foreach (var key in relation.Keys)
                    {
                        var value = entity.GetValue(key.PrincipalProperty);
                        e.SetValue(key.DependentProperty, value);
                    }
                });

                if (entitySet.AllowBatchInsert)
                {
                    queryable.BatchOperate(added, queryable.CreateInsertExpression());
                }
                else
                {
                    foreach (var e in added)
                    {
                        queryable.CreateEntity(new EntityExecuteContext(_contextService.Provider, e));
                        e.SetState(EntityState.Unchanged);
                        HandleRelationProperties(e);
                    }
                }
            }

            entitySet.Reset();
        }

        /// <summary>
        /// 处理关联的实体集合。
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="entity"></param>
        /// <param name="entitySet"></param>
        /// <param name="property"></param>
        private async Task HandleEntitySetPropertyAsync(IQueryable queryable, IEntity entity, IEntitySet entitySet, IProperty property)
        {
            var added = entitySet.GetAttachedList();
            var modified = entitySet.GetModifiedList();
            var deleted = entitySet.GetDetachedList();

            //处理删除的
            if (deleted.Any())
            {
                await queryable.BatchOperateAsync(deleted, queryable.CreateDeleteExpression(true));
            }

            //处理修改的
            if (modified.Any())
            {
                if (entitySet.AllowBatchUpdate)
                {
                    await queryable.BatchOperateAsync(modified, queryable.CreateUpdateExpression());
                }
                else
                {
                    foreach (var e in modified)
                    {
                        await queryable.UpdateEntityAsync(new EntityExecuteContext(_contextService.Provider, e));
                        e.SetState(EntityState.Unchanged);
                        await HandleRelationPropertiesAsync(e);
                    }
                }
            }

            //处理新增的
            if (added.Any())
            {
                var relation = RelationshipUnity.GetRelationship(property);
                added.ForEach(e =>
                {
                    foreach (var key in relation.Keys)
                    {
                        var value = entity.GetValue(key.PrincipalProperty);
                        e.SetValue(key.DependentProperty, value);
                    }
                });

                if (entitySet.AllowBatchInsert)
                {
                    await queryable.BatchOperateAsync(added, queryable.CreateInsertExpression());
                }
                else
                {
                    foreach (var e in added)
                    {
                        await queryable.CreateEntityAsync(new EntityExecuteContext(_contextService.Provider, e));
                        e.SetState(EntityState.Unchanged);
                        await HandleRelationPropertiesAsync(e);
                    }
                }
            }

            entitySet.Reset();
        }
        #endregion

        private void InvokeQueryPolicy(Action<IQueryPolicy> action)
        {
            if (_contextService is IQueryPolicyAware aware && aware.QueryPolicy != null)
            {
                action(aware.QueryPolicy);
            }
        }
    }
}