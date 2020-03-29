using Fireasy.Common;
using Fireasy.Common.Extensions;
// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Entity.Linq;
using Fireasy.Data.Entity.Query;
using Fireasy.Data.Entity.Subscribes;
using Fireasy.Data.Entity.Validation;
using Fireasy.Data.Provider;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 表示在 <see cref="EntityContext"/> 实例中对实体 <typeparamref name="TEntity"/> 的仓储。它可以用于直接对实体进行创建、查询、修改和删除。
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class EntityRepository<TEntity> :
        IOrderedQueryable<TEntity>,
        IQueryProviderAware,
        IRepository<TEntity>,
        IListSource
        where TEntity : IEntity
    {
        private readonly IRepositoryProvider<TEntity> repositoryProxy;
        private readonly EntityContextOptions options;
        private readonly IProvider provider;
        private readonly InnerPersistentSubscribeManager subMgr;

        /// <summary>
        /// 初始化 <see cref="EntityRepository{TEntity}"/> 类的新实例。
        /// </summary>
        /// <param name="repositoryProxy"></param>
        /// <param name="options"></param>
        public EntityRepository(IRepositoryProvider<TEntity> repositoryProxy, EntityContextOptions options)
        {
            this.repositoryProxy = repositoryProxy;
            this.options = options;
            provider = options.Provider;
            EntityType = typeof(TEntity);

            Type contextType = null;
            if (options is IInstanceIdentifier identifier)
            {
                contextType = identifier.ContextType;
            }

            subMgr = EntityPersistentSubscribeManager.GetRequiredManager(contextType);
        }

        /// <summary>
        /// 获取关联的实体类型。
        /// </summary>
        public Type EntityType { get; private set; }

        /// <summary>
        /// 获取 <see cref="IQueryProvider"/> 对象。
        /// </summary>
        public IQueryProvider Provider
        {
            get { return repositoryProxy.QueryProvider; }
        }

        #region Get
        /// <summary>
        /// 通过一组主键值返回一个实体对象。
        /// </summary>
        /// <param name="primaryValues">一组主键值。</param>
        /// <returns></returns>
        public virtual TEntity Get(params PropertyValue[] primaryValues)
        {
            return repositoryProxy.Get(primaryValues);
        }

        /// <summary>
        /// 异步的，通过一组主键值返回一个实体对象。
        /// </summary>
        /// <param name="primaryValues">一组主键值。</param>
        /// <returns></returns>
        public virtual async Task<TEntity> GetAsync(params PropertyValue[] primaryValues)
        {
            return await repositoryProxy.GetAsync(primaryValues);
        }
        #endregion

        #region Insert
        /// <summary>
        /// 将一个新的实体对象创建到库。
        /// </summary>
        /// <param name="entity">要创建的实体对象。</param>
        /// <returns>如果主键是自增类型，则为主键值，否则为影响的实体数。</returns>
        public virtual int Insert(TEntity entity)
        {
            Guard.ArgumentNull(entity, nameof(entity));

            if (options.AllowDefaultValue)
            {
                SetDefaultValue(entity);
            }

            return subMgr.OnCreate<TEntity, int>(options.NotifyEvents, entity,
                () => repositoryProxy.Insert(HandleValidate(entity)));
        }

        /// <summary>
        /// 异步的，将一个新的实体对象创建到库。
        /// </summary>
        /// <param name="entity">要创建的实体对象。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>如果主键是自增类型，则为主键值，否则为影响的实体数。</returns>
        public async virtual Task<int> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            Guard.ArgumentNull(entity, nameof(entity));

            if (options.AllowDefaultValue)
            {
                SetDefaultValue(entity);
            }

            return await subMgr.OnCreateAsync<TEntity, int>(options.NotifyEvents, entity,
                () => repositoryProxy.InsertAsync(HandleValidate(entity), cancellationToken));
        }

        /// <summary>
        /// 使用一个 <see cref="MemberInitExpression"/> 表达式插入新的对象。
        /// </summary>
        /// <param name="creator">一个构造实例并成员绑定的表达式。</param>
        /// <returns>如果主键是自增类型，则为主键值，否则为影响的实体数。</returns>
        public virtual int Insert(Expression<Func<TEntity>> creator)
        {
            var entity = EntityProxyManager.GetType(provider.ProviderName, typeof(TEntity)).New<TEntity>();
            entity.InitByExpression(creator);

            return Insert(entity);
        }

        /// <summary>
        /// 异步的，使用一个 <see cref="MemberInitExpression"/> 表达式插入新的对象。
        /// </summary>
        /// <param name="creator">一个构造实例并成员绑定的表达式。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>如果主键是自增类型，则为主键值，否则为影响的实体数。</returns>
        public async virtual Task<int> InsertAsync(Expression<Func<TEntity>> creator, CancellationToken cancellationToken = default)
        {
            var entity = EntityProxyManager.GetType(provider.ProviderName, typeof(TEntity)).New<TEntity>();
            entity.InitByExpression(creator);

            return await InsertAsync(entity, cancellationToken);
        }


        /// <summary>
        /// 使用初始化函数将一个新的实体对象插入到库。
        /// </summary>
        /// <param name="initializer">一个初始化实体成员绑定的函数。</param>
        /// <returns>如果主键是自增类型，则为主键值，否则为影响的实体数。</returns>
        public virtual int Insert(Action<TEntity> initializer)
        {
            Guard.ArgumentNull(initializer, nameof(initializer));

            var entity = EntityProxyManager.GetType(provider.ProviderName, typeof(TEntity)).New<TEntity>();
            initializer(entity);

            return Insert(entity);
        }

        /// <summary>
        /// 异步的，使用初始化函数将一个新的实体对象插入到库。
        /// </summary>
        /// <param name="initializer">一个初始化实体成员绑定的函数。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>如果主键是自增类型，则为主键值，否则为影响的实体数。</returns>
        public async virtual Task<int> InsertAsync(Action<TEntity> initializer, CancellationToken cancellationToken = default)
        {
            Guard.ArgumentNull(initializer, nameof(initializer));

            var entity = EntityProxyManager.GetType(provider.ProviderName, typeof(TEntity)).New<TEntity>();
            initializer(entity);

            return await InsertAsync(entity, cancellationToken);
        }

        #endregion

        #region BatchInsert
        /// <summary>
        /// 批量将一组实体对象插入到库中。
        /// </summary>
        /// <param name="entities">一组要插入实体对象。</param>
        /// <param name="batchSize">每一个批次插入的实体数量。默认为 1000。</param>
        /// <param name="completePercentage">已完成百分比的通知方法。</param>
        public virtual void BatchInsert(IEnumerable<TEntity> entities, int batchSize = 1000, Action<int> completePercentage = null)
        {
            Guard.ArgumentNull(entities, nameof(entities));

            repositoryProxy.BatchInsert(entities, batchSize, completePercentage);
        }

        /// <summary>
        /// 异步的，批量将一组实体对象插入到库中。
        /// </summary>
        /// <param name="entities">一组要插入实体对象。</param>
        /// <param name="batchSize">每一个批次插入的实体数量。默认为 1000。</param>
        /// <param name="completePercentage">已完成百分比的通知方法。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        public async virtual Task BatchInsertAsync(IEnumerable<TEntity> entities, int batchSize = 1000, Action<int> completePercentage = null, CancellationToken cancellationToken = default)
        {
            Guard.ArgumentNull(entities, nameof(entities));

            await repositoryProxy.BatchInsertAsync(entities, batchSize, completePercentage, cancellationToken);
        }

        #endregion

        #region InsertOrUpdate
        /// <summary>
        /// 根据实体的状态，插入或更新实体对象。
        /// </summary>
        /// <param name="entity">要保存的实体对象。</param>
        /// <returns>影响的实体数。</returns>
        public virtual int InsertOrUpdate(TEntity entity)
        {
            Guard.ArgumentNull(entity, nameof(entity));

            var properties = PropertyUnity.GetPrimaryProperties(typeof(TEntity));
            var isNew = entity.EntityState == EntityState.Attached;

            if (isNew && properties.Any(s => !PropertyValue.IsEmptyOrDefault(entity.GetValue(s))))
            {
                var parExp = Expression.Parameter(typeof(TEntity), "s");
                var equalExp = properties.Select(s => Expression.Equal(Expression.MakeMemberAccess(parExp, s.Info.ReflectionInfo), Expression.Constant(entity.GetValue(s)))).Aggregate(Expression.And);
                var lambdaExp = Expression.Lambda<Func<TEntity, bool>>(equalExp, parExp);
                isNew = !this.Any(lambdaExp);
            }

            return isNew ? Insert(entity) : Update(entity);
        }

        /// <summary>
        /// 异步的，根据实体的状态，插入或更新实体对象。
        /// </summary>
        /// <param name="entity">要保存的实体对象。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>影响的实体数。</returns>
        public async virtual Task<int> InsertOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            Guard.ArgumentNull(entity, nameof(entity));

            var properties = PropertyUnity.GetPrimaryProperties(typeof(TEntity));
            var isNew = entity.EntityState == EntityState.Attached;

            if (isNew && properties.Any(s => !PropertyValue.IsEmptyOrDefault(entity.GetValue(s))))
            {
                var parExp = Expression.Parameter(typeof(TEntity), "s");
                var equalExp = properties.Select(s => Expression.Equal(Expression.MakeMemberAccess(parExp, s.Info.ReflectionInfo), Expression.Constant(entity.GetValue(s)))).Aggregate(Expression.And);
                var lambdaExp = Expression.Lambda<Func<TEntity, bool>>(equalExp, parExp);
                isNew = !this.Any(lambdaExp);
            }

            return isNew ? await InsertAsync(entity, cancellationToken) : await UpdateAsync(entity, cancellationToken);
        }
        #endregion

        #region Delete
        /// <summary>
        /// 将指定的实体对象从库中移除。
        /// </summary>
        /// <param name="entity">要移除的实体对象。</param>
        /// <param name="logicalDelete">是否为逻辑删除。</param>
        /// <returns>影响的实体数。</returns>
        public virtual int Delete(TEntity entity, bool logicalDelete = true)
        {
            Guard.ArgumentNull(entity, nameof(entity));

            return subMgr.OnRemove<TEntity, int>(options.NotifyEvents, entity,
                () => repositoryProxy.Delete(entity, logicalDelete));
        }

        /// <summary>
        /// 异步的，将指定的实体对象从库中移除。
        /// </summary>
        /// <param name="entity">要移除的实体对象。</param>
        /// <param name="logicalDelete">是否为逻辑删除。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>影响的实体数。</returns>
        public async virtual Task<int> DeleteAsync(TEntity entity, bool logicalDelete = true, CancellationToken cancellationToken = default)
        {
            Guard.ArgumentNull(entity, nameof(entity));

            return await subMgr.OnRemoveAsync<TEntity, int>(options.NotifyEvents, entity,
                () => repositoryProxy.DeleteAsync(entity, logicalDelete, cancellationToken));
        }

        /// <summary>
        /// 根据主键值将对象从库中移除。
        /// </summary>
        /// <param name="primaryValues">一组主键值。</param>
        /// <returns>影响的实体数。</returns>
        public virtual int Delete(params PropertyValue[] primaryValues)
        {
            var ret = repositoryProxy.Delete(primaryValues);
            if (ret > 0 && options.NotifyEvents)
            {
                subMgr.Publish<TEntity>(EntityPersistentEventType.AfterRemove);
            }

            return ret;
        }

        /// <summary>
        /// 异步的，根据主键值将对象从库中移除。
        /// </summary>
        /// <param name="primaryValues">一组主键值。</param>
        /// <returns>影响的实体数。</returns>
        public async virtual Task<int> DeleteAsync(params PropertyValue[] primaryValues)
        {
            var ret = await repositoryProxy.DeleteAsync(primaryValues, default);
            if (ret > 0 && options.NotifyEvents)
            {
                subMgr.Publish<TEntity>(EntityPersistentEventType.AfterRemove);
            }

            return ret;
        }

        /// <summary>
        /// 根据主键值将对象从库中移除。
        /// </summary>
        /// <param name="primaryValues">一组主键值。</param>
        /// <param name="logicalDelete">是否为逻辑删除。</param>
        /// <returns>影响的实体数。</returns>
        public virtual int Delete(PropertyValue[] primaryValues, bool logicalDelete = true)
        {
            var ret = repositoryProxy.Delete(primaryValues, logicalDelete);
            if (ret > 0 && options.NotifyEvents)
            {
                subMgr.Publish<TEntity>(EntityPersistentEventType.AfterRemove);
            }

            return ret;
        }

        /// <summary>
        /// 异步的，根据主键值将对象从库中移除。
        /// </summary>
        /// <param name="primaryValues">一组主键值。</param>
        /// <param name="logicalDelete">是否为逻辑删除。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>影响的实体数。</returns>
        public async virtual Task<int> DeleteAsync(PropertyValue[] primaryValues, bool logicalDelete = true, CancellationToken cancellationToken = default)
        {
            var ret = await repositoryProxy.DeleteAsync(primaryValues, logicalDelete, cancellationToken);
            if (ret > 0 && options.NotifyEvents)
            {
                subMgr.Publish<TEntity>(EntityPersistentEventType.AfterRemove);
            }

            return ret;
        }

        /// <summary>
        /// 将满足条件的一组对象从库中移除。
        /// </summary>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="logicalDelete">是否为逻辑删除</param>
        /// <returns>影响的实体数。</returns>
        public virtual int Delete(Expression<Func<TEntity, bool>> predicate, bool logicalDelete = true)
        {
            var ret = repositoryProxy.Delete(predicate, logicalDelete);
            if (ret > 0 && options.NotifyEvents)
            {
                subMgr.Publish<TEntity>(EntityPersistentEventType.AfterRemove);
            }

            return ret;
        }

        /// <summary>
        /// 异步的，将满足条件的一组对象从库中移除。
        /// </summary>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="logicalDelete">是否为逻辑删除</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>影响的实体数。</returns>
        public async virtual Task<int> DeleteAsync(Expression<Func<TEntity, bool>> predicate, bool logicalDelete = true, CancellationToken cancellationToken = default)
        {
            var ret = await repositoryProxy.DeleteAsync(predicate, logicalDelete);
            if (ret > 0 && options.NotifyEvents)
            {
                subMgr.Publish<TEntity>(EntityPersistentEventType.AfterRemove);
            }

            return ret;
        }

        #endregion

        #region Update
        /// <summary>
        /// 更新一个实体对象。
        /// </summary>
        /// <param name="entity">实体对象。</param>
        /// <returns>影响的实体数。</returns>
        public virtual int Update(TEntity entity)
        {
            Guard.ArgumentNull(entity, nameof(entity));

            return subMgr.OnUpdate<TEntity, int>(options.NotifyEvents, entity,
                () => repositoryProxy.Update(HandleValidate(entity)));
        }

        /// <summary>
        /// 异步的，更新一个实体对象。
        /// </summary>
        /// <param name="entity">实体对象。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>影响的实体数。</returns>
        public async virtual Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            Guard.ArgumentNull(entity, nameof(entity));

            return await subMgr.OnUpdateAsync<TEntity, int>(options.NotifyEvents, entity,
                () => repositoryProxy.UpdateAsync(HandleValidate(entity), cancellationToken));
        }

        /// <summary>
        /// 使用一个参照的实体对象更新满足条件的一序列对象。
        /// </summary>
        /// <param name="entity">更新的参考对象。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <returns>影响的实体数。</returns>
        public virtual int Update(TEntity entity, Expression<Func<TEntity, bool>> predicate)
        {
            var ret = repositoryProxy.Update(entity, predicate);
            if (ret > 0 && options.NotifyEvents)
            {
                subMgr.Publish<TEntity>(EntityPersistentEventType.AfterUpdate);
            }

            return ret;
        }

        /// <summary>
        /// 异步的，使用一个参照的实体对象更新满足条件的一序列对象。
        /// </summary>
        /// <param name="entity">更新的参考对象。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>影响的实体数。</returns>
        public async virtual Task<int> UpdateAsync(TEntity entity, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var ret = await repositoryProxy.UpdateAsync(entity, predicate);
            if (ret > 0 && options.NotifyEvents)
            {
                subMgr.Publish<TEntity>(EntityPersistentEventType.AfterUpdate);
            }

            return ret;
        }

        /// <summary>
        /// 使用一个 <see cref="MemberInitExpression"/> 表达式更新满足条件的一序列对象。
        /// </summary>
        /// <param name="creator">一个构造实例并成员绑定的表达式。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <returns>影响的实体数。</returns>
        public virtual int Update(Expression<Func<TEntity>> creator, Expression<Func<TEntity, bool>> predicate)
        {
            var entity = EntityProxyManager.GetType(provider.ProviderName, typeof(TEntity)).New<TEntity>();
            entity.InitByExpression(creator);

            return predicate == null ? Update(entity) : Update(entity, predicate);
        }

        /// <summary>
        /// 异步的，使用一个 <see cref="MemberInitExpression"/> 表达式更新满足条件的一序列对象。
        /// </summary>
        /// <param name="creator">一个构造实例并成员绑定的表达式。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>影响的实体数。</returns>
        public async virtual Task<int> UpdateAsync(Expression<Func<TEntity>> creator, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var entity = EntityProxyManager.GetType(provider.ProviderName, typeof(TEntity)).New<TEntity>();
            entity.InitByExpression(creator);

            return predicate == null ? await UpdateAsync(entity, cancellationToken) : await UpdateAsync(entity, predicate, cancellationToken);
        }

        /// <summary>
        /// 使用初始化函数更新满足条件的一序列对象。
        /// </summary>
        /// <param name="initializer">一个初始化实体成员绑定的函数。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <returns>影响的实体数。</returns>
        public virtual int Update(Action<TEntity> initializer, Expression<Func<TEntity, bool>> predicate)
        {
            Guard.ArgumentNull(initializer, nameof(initializer));

            var entity = EntityProxyManager.GetType(provider.ProviderName, typeof(TEntity)).New<TEntity>();

            initializer(entity);

            return predicate == null ? Update(entity) : Update(entity, predicate);
        }

        /// <summary>
        /// 异步的，使用初始化函数更新满足条件的一序列对象。
        /// </summary>
        /// <param name="initializer">一个初始化实体成员绑定的函数。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>影响的实体数。</returns>
        public async virtual Task<int> UpdateAsync(Action<TEntity> initializer, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            Guard.ArgumentNull(initializer, nameof(initializer));

            var entity = EntityProxyManager.GetType(provider.ProviderName, typeof(TEntity)).New<TEntity>();

            initializer(entity);

            return predicate == null ? await UpdateAsync(entity, cancellationToken) : await UpdateAsync(entity, predicate, cancellationToken);
        }

        /// <summary>
        /// 使用一个累加器更新满足条件的一序列对象。
        /// </summary>
        /// <param name="calculator">一个计算器表达式。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <returns>影响的实体数。</returns>
        public virtual int Update(Expression<Func<TEntity, TEntity>> calculator, Expression<Func<TEntity, bool>> predicate)
        {
            var ret = repositoryProxy.Update(calculator, predicate);
            if (ret > 0 && options.NotifyEvents)
            {
                subMgr.Publish<TEntity>(EntityPersistentEventType.AfterUpdate);
            }

            return ret;
        }

        /// <summary>
        /// 异步的，使用一个累加器更新满足条件的一序列对象。
        /// </summary>
        /// <param name="calculator">一个计算器表达式。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>影响的实体数。</returns>
        public async virtual Task<int> UpdateAsync(Expression<Func<TEntity, TEntity>> calculator, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var ret = await repositoryProxy.UpdateAsync(calculator, predicate, cancellationToken);
            if (ret > 0 && options.NotifyEvents)
            {
                subMgr.Publish<TEntity>(EntityPersistentEventType.AfterUpdate);
            }

            return ret;
        }

        #endregion

        #region Batch
        /// <summary>
        /// 对实体集合进行批量操作。
        /// </summary>
        /// <param name="instances">要操作的实体序列。</param>
        /// <param name="fnOperation">实体操作表达式，权提供 Insert、Update 和 Delete 操作。</param>
        /// <param name="batchOpt"></param>
        /// <returns>影响的实体数。</returns>
        public virtual int Batch(IEnumerable<TEntity> instances, Expression<Func<IRepository<TEntity>, TEntity, int>> fnOperation, BatchOperateOptions batchOpt = null)
        {
            if (instances.IsNullOrEmpty())
            {
                return -1;
            }

            var operateName = OperateFinder.Find(fnOperation);
            var eventType = GetEventType(operateName);

            return subMgr.OnBatch<TEntity, int>(options.NotifyEvents,
                instances.Cast<IEntity>(), eventType,
                () => repositoryProxy.Batch(instances, fnOperation, batchOpt));
        }

        /// <summary>
        /// 异步的，对实体集合进行批量操作。
        /// </summary>
        /// <param name="instances">要操作的实体序列。</param>
        /// <param name="fnOperation">实体操作表达式，权提供 Insert、Update 和 Delete 操作。</param>
        /// <param name="batchOpt"></param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>影响的实体数。</returns>
        public async virtual Task<int> BatchAsync(IEnumerable<TEntity> instances, Expression<Func<IRepository<TEntity>, TEntity, int>> fnOperation, BatchOperateOptions batchOpt = null, CancellationToken cancellationToken = default)
        {
            if (instances.IsNullOrEmpty())
            {
                return -1;
            }

            var operateName = OperateFinder.Find(fnOperation);
            var eventType = GetEventType(operateName);

            return await subMgr.OnBatchAsync<TEntity, int>(options.NotifyEvents,
                instances.Cast<IEntity>(), eventType,
                () => repositoryProxy.BatchAsync(instances, fnOperation, batchOpt, cancellationToken));
        }
        #endregion

        /// <summary>
        /// 指定要包括在查询结果中的关联对象。
        /// </summary>
        /// <param name="fnMember">要包含的属性的表达式。</param>
        /// <returns></returns>
        public virtual EntityRepository<TEntity> Include(Expression<Func<TEntity, object>> fnMember)
        {
            repositoryProxy.As<IQueryPolicyExecutor<TEntity>>(s => s.IncludeWith(fnMember));
            return this;
        }

        /// <summary>
        /// 对指定割开的查询始终附加指定的谓语。
        /// </summary>
        /// <param name="memberQuery"></param>
        /// <returns></returns>
        public virtual EntityRepository<TEntity> Associate(Expression<Func<TEntity, IEnumerable>> memberQuery)
        {
            repositoryProxy.As<IQueryPolicyExecutor<TEntity>>(s => s.AssociateWith(memberQuery));
            return this;
        }

        /// <summary>
        /// 配置参数。
        /// </summary>
        /// <param name="configuration">配置参数的方法。</param>
        /// <returns></returns>
        public EntityRepository<TEntity> ConfigOptions(Action<EntityContextOptions> configuration)
        {
            configuration?.Invoke(options);
            return this;
        }

        #region IRepository实现
        /// <summary>
        /// 通过一组主键值返回一个对象。
        /// </summary>
        /// <param name="primaryValues">一组主键值。</param>
        /// <returns></returns>
        IEntity IRepository.Get(params PropertyValue[] primaryValues)
        {
            return Get(primaryValues);
        }

        /// <summary>
        /// 通过一组主键值返回一个对象。
        /// </summary>
        /// <param name="primaryValues">一组主键值。</param>
        /// <returns></returns>
        async Task<IEntity> IRepository.GetAsync(params PropertyValue[] primaryValues)
        {
            return await GetAsync(primaryValues);
        }

        /// <summary>
        /// 通过一组主键值返回一个对象。
        /// </summary>
        /// <param name="primaryValues">一组主键值。</param>
        /// <returns></returns>
        async Task<TEntity> IRepository<TEntity>.GetAsync(params PropertyValue[] primaryValues)
        {
            return await GetAsync(primaryValues);
        }

        /// <summary>
        /// 将一个新的对象插入到库。
        /// </summary>
        /// <param name="entity">要创建的对象。</param>
        /// <returns>影响的实体数。</returns>
        int IRepository.Insert(IEntity entity)
        {
            return Insert((TEntity)entity);
        }

        /// <summary>
        /// 将一个新的对象插入到库。
        /// </summary>
        /// <param name="entity">要创建的对象。</param>
        /// <returns>影响的实体数。</returns>
        async Task<int> IRepository.InsertAsync(IEntity entity, CancellationToken cancellationToken)
        {
            return await InsertAsync((TEntity)entity, cancellationToken);
        }

        /// <summary>
        /// 更新一个对象。
        /// </summary>
        /// <param name="entity">要更新的对象。</param>
        /// <returns>影响的实体数。</returns>
        int IRepository.Update(IEntity entity)
        {
            return Update((TEntity)entity);
        }

        /// <summary>
        /// 更新一个对象。
        /// </summary>
        /// <param name="entity">要更新的对象。</param>
        /// <returns>影响的实体数。</returns>
        async Task<int> IRepository.UpdateAsync(IEntity entity, CancellationToken cancellationToken)
        {
            return await UpdateAsync((TEntity)entity, cancellationToken);
        }

        /// <summary>
        /// 更新一个对象。
        /// </summary>
        /// <param name="entity">更新的参考对象。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <returns>影响的实体数。</returns>
        int IRepository.Update(IEntity entity, Expression predicate)
        {
            return Update((TEntity)entity, (Expression<Func<TEntity, bool>>)predicate);
        }

        /// <summary>
        /// 更新一个对象。
        /// </summary>
        /// <param name="entity">更新的参考对象。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <returns>影响的实体数。</returns>
        async Task<int> IRepository.UpdateAsync(IEntity entity, Expression predicate, CancellationToken cancellationToken)
        {
            return await UpdateAsync((TEntity)entity, (Expression<Func<TEntity, bool>>)predicate, cancellationToken);
        }

        /// <summary>
        /// 将对象的改动保存到库。
        /// </summary>
        /// <param name="entity">要保存的对象。</param>
        /// <returns>影响的实体数。</returns>
        int IRepository.InsertOrUpdate(IEntity entity)
        {
            return InsertOrUpdate((TEntity)entity);
        }

        /// <summary>
        /// 将对象的改动保存到库。
        /// </summary>
        /// <param name="entity">要保存的对象。</param>
        /// <returns>影响的实体数。</returns>
        async Task<int> IRepository.InsertOrUpdateAsync(IEntity entity, CancellationToken cancellationToken)
        {
            return await InsertOrUpdateAsync((TEntity)entity, cancellationToken);
        }

        /// <summary>
        /// 将指定的对象从库中删除。
        /// </summary>
        /// <param name="entity">要移除的对象。</param>
        /// <param name="logicalDelete">是否为逻辑删除。</param>
        /// <returns>影响的实体数。</returns>
        int IRepository.Delete(IEntity entity, bool logicalDelete)
        {
            return Delete((TEntity)entity, logicalDelete);
        }

        /// <summary>
        /// 将满足条件的一组对象从库中移除。
        /// </summary>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="logicalDelete">是否为逻辑删除</param>
        /// <returns>影响的实体数。</returns>
        async Task<int> IRepository.DeleteAsync(Expression predicate, bool logicalDelete, CancellationToken cancellationToken)
        {
            return await DeleteAsync((Expression<Func<TEntity, bool>>)predicate, logicalDelete, cancellationToken);
        }

        /// <summary>
        /// 将满足条件的一组对象从库中移除。
        /// </summary>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="logicalDelete">是否为逻辑删除</param>
        /// <returns>影响的实体数。</returns>
        int IRepository.Delete(Expression predicate, bool logicalDelete)
        {
            return Delete((Expression<Func<TEntity, bool>>)predicate, logicalDelete);
        }

        /// <summary>
        /// 将指定的对象从库中删除。
        /// </summary>
        /// <param name="entity">要移除的对象。</param>
        /// <param name="logicalDelete">是否为逻辑删除。</param>
        /// <returns>影响的实体数。</returns>
        async Task<int> IRepository.DeleteAsync(IEntity entity, bool logicalDelete, CancellationToken cancellationToken)
        {
            return await DeleteAsync((TEntity)entity, logicalDelete, cancellationToken);
        }

        /// <summary>
        /// 指定要包括在查询结果中的关联对象。
        /// </summary>
        /// <param name="fnMember">要包含的属性的表达式。</param>
        /// <returns></returns>
        IRepository<TEntity> IRepository<TEntity>.Include(Expression<Func<TEntity, object>> fnMember)
        {
            return Include(fnMember);
        }

        /// <summary>
        /// 对指定割开的查询始终附加指定的谓语。
        /// </summary>
        /// <param name="memberQuery"></param>
        /// <returns></returns>
        IRepository<TEntity> IRepository<TEntity>.Associate(Expression<Func<TEntity, IEnumerable>> memberQuery)
        {
            return Associate(memberQuery);
        }

        #endregion

        /// <summary>
        /// 处理实体验证。
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected virtual TEntity HandleValidate(TEntity entity)
        {
            if (options.ValidateEntity)
            {
                ValidationUnity.Validate(entity);
            }

            return entity;
        }

        /// <summary>
        /// 设置默认值。
        /// </summary>
        /// <param name="entity"></param>
        protected virtual void SetDefaultValue(TEntity entity)
        {
            var isNotCompiled = entity.EntityType.IsNotCompiled();
            foreach (var property in PropertyUnity.GetPersistentProperties(EntityType))
            {
                var isModify = isNotCompiled ? !PropertyValue.IsEmpty(entity.GetValue(property)) : entity.IsModified(property.Name);
                if (!isModify && !PropertyValue.IsEmpty(property.Info.DefaultValue))
                {
                    entity.SetValue(property, property.Info.DefaultValue.TryAllotValue(property.Type, property.Info.DefaultValueFormatter));
                }
            }
        }

        /// <summary>
        /// 操作查找器。
        /// </summary>
        private class OperateFinder : Fireasy.Common.Linq.Expressions.ExpressionVisitor
        {
            private string operateName;

            /// <summary>
            /// 在表达式中查找操作的名称。
            /// </summary>
            /// <param name="expression"></param>
            /// <returns></returns>
            public static string Find(Expression expression)
            {
                var finder = new OperateFinder();
                finder.Visit(expression);
                return finder.operateName;
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.DeclaringType.IsGenericType &&
                    node.Method.DeclaringType.GetGenericTypeDefinition() == typeof(IRepository<>))
                {
                    switch (node.Method.Name)
                    {
                        case "Insert":
                        case "Update":
                        case "Delete":
                            operateName = node.Method.Name;
                            break;
                    }
                }

                return node;
            }
        }

        private EntityPersistentOperater GetEventType(string operateName)
        {
            return operateName switch
            {
                "Insert" => EntityPersistentOperater.Create,
                "Update" => EntityPersistentOperater.Update,
                "Delete" => EntityPersistentOperater.Remove,
                _ => throw new InvalidOperationException(),
            };
        }

        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator()
        {
            return (IEnumerator<TEntity>)repositoryProxy.Queryable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return repositoryProxy.Queryable.GetEnumerator();
        }

        Type IQueryable.ElementType
        {
            get { return repositoryProxy.Queryable.ElementType; }
        }

        Expression IQueryable.Expression
        {
            get { return repositoryProxy.Queryable.Expression; }
        }

        IList IListSource.GetList()
        {
            if (repositoryProxy.Queryable is IListSource source)
            {
                return source.GetList();
            }

            return null;
        }

        public bool ContainsListCollection => throw new NotImplementedException();
    }
}
