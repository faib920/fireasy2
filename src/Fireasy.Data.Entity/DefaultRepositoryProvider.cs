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
    public sealed class DefaultRepositoryProvider<TEntity> : IQueryPolicyExecutor<TEntity>,
        IRepositoryProvider<TEntity> where TEntity : IEntity
    {
        private IRepository repository;
        private readonly DefaultContextService contextService;
        private readonly IProvider provider;
        private readonly IDatabase database;

        /// <summary>
        /// 初始化 <see cref="DefaultRepositoryProvider{TEntity}"/> 类的新实例。
        /// </summary>
        /// <param name="contextService"></param>
        public DefaultRepositoryProvider(DefaultContextService contextService)
        {
            this.contextService = contextService;
            provider = contextService.Provider;
            database = contextService.Database;

            QueryProvider = new QueryProvider(new EntityQueryProvider(contextService));
            Queryable = new QuerySet<TEntity>(QueryProvider);
        }

        IRepository IRepositoryProvider.CreateRepository(EntityContextOptions options)
        {
            return repository ?? (repository = new EntityRepository<TEntity>(contextService, this, options));
        }

        /// <summary>
        /// 获取 <see cref="IQueryable"/> 实例。
        /// </summary>
        public IQueryable Queryable { get; private set; }

        /// <summary>
        /// 获取 <see cref="IQueryProvider"/> 实例。
        /// </summary>
        public IQueryProvider QueryProvider { get; private set; }

        /// <summary>
        /// 将一个新的实体对象插入到库。
        /// </summary>
        /// <param name="entity">要创建的实体对象。</param>
        /// <returns>如果实体中有自增属性的主键，则返回主键值；否则返回影响的实体数。</returns>
        public int Insert(TEntity entity)
        {
            var opContext = new OperationContext<int>(
                (q, e) => q.CreateEntity(e),
                (q, e) => q.UpdateEntity(e),
                (q, e) => q.RemoveEntity(e, false),
                (q, es, lambda) => q.BatchOperate(es, lambda));

            return InternalInsert(entity, opContext);
        }

        /// <summary>
        /// 异步的，将一个新的实体对象插入到库。
        /// </summary>
        /// <param name="entity">要创建的实体对象。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>如果实体中有自增属性的主键，则返回主键值；否则返回影响的实体数。</returns>
        public async Task<int> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var opContext = new OperationContext<Task<int>>(
                async (q, e) => await q.CreateEntityAsync(e),
                async (q, e) => await q.UpdateEntityAsync(e),
                async (q, e) => await q.RemoveEntityAsync(e, false),
                async (q, es, lambda) => await q.BatchOperateAsync(es, lambda));

            return await InternalInsert(entity, opContext);
        }

        /// <summary>
        /// 更新一个实体对象。
        /// </summary>
        /// <param name="entity">要更新的实体对象。</param>
        /// <returns>影响的实体数。</returns>
        public int Update(TEntity entity)
        {
            var opContext = new OperationContext<int>(
                (q, e) => q.CreateEntity(e),
                (q, e) => q.UpdateEntity(e),
                (q, e) => q.RemoveEntity(e, false),
                (q, es, lambda) => q.BatchOperate(es, lambda));

            return InternalUpdate(entity, opContext);
        }

        /// <summary>
        /// 异步的，更新一个实体对象。
        /// </summary>
        /// <param name="entity">要更新的实体对象。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>影响的实体数。</returns>
        public async Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var opContext = new OperationContext<Task<int>>(
                async (q, e) => await q.CreateEntityAsync(e),
                async (q, e) => await q.UpdateEntityAsync(e),
                async (q, e) => await q.RemoveEntityAsync(e, false),
                async (q, es, lambda) => await q.BatchOperateAsync(es, lambda));

            return await InternalUpdate(entity, opContext);
        }

        /// <summary>
        /// 批量将一组实体对象插入到库中。
        /// </summary>
        /// <param name="entities">一组要插入实体对象。</param>
        /// <param name="batchSize">每一个批次插入的实体数量。默认为 1000。</param>
        /// <param name="completePercentage">已完成百分比的通知方法。</param>
        public void BatchInsert(IEnumerable<TEntity> entities, int batchSize = 1000, Action<int> completePercentage = null)
        {
            var batcher = provider.GetService<IBatcherProvider>();
            if (batcher == null)
            {
                throw new EntityPersistentException(SR.GetString(SRKind.NotSupportBatcher), null);
            }

            var syntax = provider.GetService<ISyntaxProvider>();
            var rootType = typeof(TEntity).GetRootEntityType();
            string tableName;

            if (contextService.Environment != null)
            {
                tableName = DbUtility.FormatByQuote(syntax, contextService.Environment.GetVariableTableName(rootType));
            }
            else
            {
                var metadata = EntityMetadataUnity.GetEntityMetadata(rootType);
                tableName = DbUtility.FormatByQuote(syntax, metadata.TableName);
            }

            batcher.Insert(database, entities, tableName, batchSize, completePercentage);
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
            var batcher = provider.GetService<IBatcherProvider>();
            if (batcher == null)
            {
                throw new EntityPersistentException(SR.GetString(SRKind.NotSupportBatcher), null);
            }

            var syntax = provider.GetService<ISyntaxProvider>();
            var rootType = typeof(TEntity).GetRootEntityType();
            string tableName;

            if (contextService.Environment != null)
            {
                tableName = DbUtility.FormatByQuote(syntax, contextService.Environment.GetVariableTableName(rootType));
            }
            else
            {
                var metadata = EntityMetadataUnity.GetEntityMetadata(rootType);
                tableName = DbUtility.FormatByQuote(syntax, metadata.TableName);
            }

            await batcher.InsertAsync(database, entities, tableName, batchSize, completePercentage, cancellationToken);
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
        /// <returns>影响的实体数。</returns>
        public int Update(TEntity entity, Expression<Func<TEntity, bool>> predicate)
        {
            return Queryable.UpdateWhere(entity, predicate);
        }

        /// <summary>
        /// 异步的，使用一个参照的实体对象更新满足条件的一序列对象。
        /// </summary>
        /// <param name="entity">更新的参考对象。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
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
        public int Batch(IEnumerable<TEntity> instances, Expression<Func<IRepository<TEntity>, TEntity, int>> fnOperation, BatchOperateOptions batchOpt = null)
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
        public async Task<int> BatchAsync(IEnumerable<TEntity> instances, Expression<Func<IRepository<TEntity>, TEntity, int>> fnOperation, BatchOperateOptions batchOpt = null, CancellationToken cancellationToken = default)
        {
            return await Queryable.BatchOperateAsync(instances.Cast<IEntity>(), fnOperation, batchOpt, cancellationToken);
        }

        void IQueryPolicyExecutor<TEntity>.IncludeWith(Expression<Func<TEntity, object>> fnMember)
        {
            InvokeQueryPolicy(q => q.IncludeWith(fnMember));
        }

        void IQueryPolicyExecutor<TEntity>.AssociateWith(Expression<Func<TEntity, IEnumerable>> memberQuery)
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

        private T InternalInsert<T>(TEntity entity, OperationContext<T> opContext)
        {
            var trans = CheckRelationHasModified(entity);
            if (trans)
            {
                database.BeginTransaction();
            }

            var result = default(T);

            try
            {
                result = opContext.CreateFunc.Await(Queryable, entity);

                var ret = result is Task<int> ? (result as Task<int>).Result : result.To<int>();
                if (ret > 0)
                {
                    entity.InitializeEnvironment(contextService.Environment).InitializeInstanceName(contextService.InstanceName);

                    HandleRelationProperties(entity, opContext);
                }

                if (trans)
                {
                    database.CommitTransaction();
                }
            }
            catch (Exception exp)
            {
                if (trans)
                {
                    database.RollbackTransaction();
                }

                throw exp;
            }

            return result;
        }

        private T InternalUpdate<T>(TEntity entity, OperationContext<T> opContext)
        {
            var trans = CheckRelationHasModified(entity);
            if (trans)
            {
                database.BeginTransaction();
            }

            T result;
            try
            {
                result = opContext.UpdateFunc.Await(Queryable, entity);

                HandleRelationProperties(entity, opContext);

                if (trans)
                {
                    database.CommitTransaction();
                }
            }
            catch (Exception exp)
            {
                if (trans)
                {
                    database.RollbackTransaction();
                }

                throw exp;
            }

            return result;
        }

        /// <summary>
        /// 检查实体的关联属性。
        /// </summary>
        /// <param name="entity"></param>
        private T HandleRelationProperties<T>(IEntity entity, OperationContext<T> opContext)
        {
            //判断实体类型有是不是编译的代理类型，如果不是，取非null的属性，否则使用IsModified判断
            var isNotCompiled = entity.GetType().IsNotCompiled();
            var properties = PropertyUnity.GetRelatedProperties(entity.EntityType).Where(m => isNotCompiled ?
                    !PropertyValue.IsEmpty(entity.GetValue(m)) :
                    entity.IsModified(m.Name));

            return HandleRelationProperties(entity, properties, opContext);
        }

        /// <summary>
        /// 处理实体的关联的属性。
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="properties"></param>
        private T HandleRelationProperties<T>(IEntity entity, IEnumerable<IProperty> properties, OperationContext<T> opContext)
        {
            foreach (RelationProperty property in properties)
            {
                var queryable = (IQueryable)contextService.CreateRepository(property.RelationalType);

                switch (property.RelationalPropertyType)
                {
                    case RelationPropertyType.Entity:
                        var refEntity = (IEntity)entity.GetValue(property).GetValue();
                        switch (refEntity.EntityState)
                        {
                            case EntityState.Modified:
                                opContext.UpdateFunc.Await(queryable, refEntity);
                                refEntity.SetState(EntityState.Unchanged);
                                break;
                        }

                        HandleRelationProperties(refEntity, opContext);
                        break;
                    case RelationPropertyType.EntitySet:
                        var value = entity.GetValue(property);
                        if (PropertyValue.IsEmpty(value))
                        {
                            value = entity.GetOldValue(property);
                        }

                        if (!PropertyValue.IsEmpty(value))
                        {
                            HandleEntitySetProperty(queryable, entity, value.GetValue() as IEntitySet, property, opContext);
                        }

                        break;
                }
            }

            return default;
        }

        /// <summary>
        /// 处理关联的实体集合。
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="entity"></param>
        /// <param name="entitySet"></param>
        /// <param name="property"></param>
        private T HandleEntitySetProperty<T>(IQueryable queryable, IEntity entity, IEntitySet entitySet, IProperty property, OperationContext<T> opContext)
        {
            var added = entitySet.GetAttachedList();
            var modified = entitySet.GetModifiedList();
            var deleted = entitySet.GetDetachedList();

            //处理删除的
            if (deleted.Any())
            {
                opContext.BatchFunc.Await(queryable, deleted, queryable.CreateDeleteExpression(true));
            }

            //处理修改的
            if (modified.Any())
            {
                if (entitySet.AllowBatchUpdate)
                {
                    opContext.BatchFunc.Await(queryable, modified, queryable.CreateUpdateExpression());
                }
                else
                {
                    foreach (var e in modified)
                    {
                        opContext.UpdateFunc.Await(queryable, e);
                        e.SetState(EntityState.Unchanged);
                        HandleRelationProperties(e, opContext);
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
                        var value = entity.GetValue(key.ThisProperty);
                        e.SetValue(key.OtherProperty, value);
                    }
                });

                if (entitySet.AllowBatchInsert)
                {
                    opContext.BatchFunc.Await(queryable, added, queryable.CreateInsertExpression());
                }
                else
                {
                    foreach (var e in added)
                    {
                        opContext.CreateFunc.Await(queryable, e);
                        e.SetState(EntityState.Unchanged);
                        HandleRelationProperties(e, opContext);
                    }
                }
            }

            entitySet.Reset();
            return default;
        }

        private void InvokeQueryPolicy(Action<IQueryPolicy> action)
        {
            if (contextService is IQueryPolicyAware aware && aware.QueryPolicy != null)
            {
                action(aware.QueryPolicy);
            }
        }

        private class OperationContext<T>
        {
            public OperationContext(Func<IQueryable, IEntity, T> createFunc, Func<IQueryable, IEntity, T> updateFunc, Func<IQueryable, IEntity, T> deleteFunc, Func<IQueryable, IEnumerable<IEntity>, LambdaExpression, T> batchFunc)
            {
                CreateFunc = createFunc;
                UpdateFunc = updateFunc;
                DeleteFunc = deleteFunc;
                BatchFunc = batchFunc;
            }

            internal Func<IQueryable, IEntity, T> CreateFunc { get; set; }

            internal Func<IQueryable, IEntity, T> UpdateFunc { get; set; }

            internal Func<IQueryable, IEntity, T> DeleteFunc { get; set; }

            internal Func<IQueryable, IEnumerable<IEntity>, LambdaExpression, T> BatchFunc { get; set; }
        }
    }
}