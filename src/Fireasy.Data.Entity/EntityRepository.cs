using Fireasy.Common;
// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Entity.Linq;
using Fireasy.Data.Entity.Subscribes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Fireasy.Data.Entity.Validation;
using System.Threading.Tasks;
using Fireasy.Common.Extensions;
using System.Threading;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 表示在 <see cref="EntityContext"/> 实例中对实体 <typeparamref name="TEntity"/> 的仓储。它可以用于直接对实体进行创建、查询、修改和删除。
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public sealed class EntityRepository<TEntity> : IOrderedQueryable<TEntity>, IQueryProviderAware, IRepository<TEntity> where TEntity : IEntity
    {
        private IRepositoryProvider<TEntity> repositoryProxy;
        private EntityContextOptions options;

        /// <summary>
        /// 初始化 <see cref="EntityRepository{TEntity}"/> 类的新实例。
        /// </summary>
        /// <param name="repositoryProxy"></param>
        /// <param name="options"></param>
        public EntityRepository(IRepositoryProvider<TEntity> repositoryProxy, EntityContextOptions options)
        {
            this.repositoryProxy = repositoryProxy;
            this.options = options;
            EntityType = typeof(TEntity);
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
        public TEntity Get(params PropertyValue[] primaryValues)
        {
            return GetAsync(primaryValues).Result;
        }

        /// <summary>
        /// 异步的，通过一组主键值返回一个实体对象。
        /// </summary>
        /// <param name="primaryValues">一组主键值。</param>
        /// <returns></returns>
        public async Task<TEntity> GetAsync(params PropertyValue[] primaryValues)
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
        public int Insert(TEntity entity)
        {
            return InsertAsync(entity).Result;
        }

        /// <summary>
        /// 异步的，将一个新的实体对象创建到库。
        /// </summary>
        /// <param name="entity">要创建的实体对象。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>如果主键是自增类型，则为主键值，否则为影响的实体数。</returns>
        public async Task<int> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            Guard.ArgumentNull(entity, nameof(entity));

            return await EntityPersistentSubscribeManager.OnCreateAsync(options.NotifyEvents, entity,
                () => repositoryProxy.InsertAsync(HandleValidate(entity), cancellationToken));
        }

        /// <summary>
        /// 使用一个 <see cref="MemberInitExpression"/> 表达式插入新的对象。
        /// </summary>
        /// <param name="factory">一个构造实例并成员绑定的表达式。</param>
        /// <returns>如果主键是自增类型，则为主键值，否则为影响的实体数。</returns>
        public int Insert(Expression<Func<TEntity>> factory)
        {
            return InsertAsync(factory).Result;
        }

        /// <summary>
        /// 异步的，使用一个 <see cref="MemberInitExpression"/> 表达式插入新的对象。
        /// </summary>
        /// <param name="factory">一个构造实例并成员绑定的表达式。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>如果主键是自增类型，则为主键值，否则为影响的实体数。</returns>
        public async Task<int> InsertAsync(Expression<Func<TEntity>> factory, CancellationToken cancellationToken = default)
        {
            var entity = EntityProxyManager.GetType(typeof(TEntity)).New<TEntity>();
            entity.InitByExpression(factory);

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
        public void BatchInsert(IEnumerable<TEntity> entities, int batchSize = 1000, Action<int> completePercentage = null)
        {
            BatchInsertAsync(entities, batchSize, completePercentage);
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
        public int InsertOrUpdate(TEntity entity)
        {
            return InsertOrUpdateAsync(entity).Result;
        }

        /// <summary>
        /// 异步的，根据实体的状态，插入或更新实体对象。
        /// </summary>
        /// <param name="entity">要保存的实体对象。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>影响的实体数。</returns>
        public async Task<int> InsertOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
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

            return await (isNew ? InsertAsync(entity, cancellationToken) : UpdateAsync(entity, cancellationToken));
        }
        #endregion

        #region Delete
        /// <summary>
        /// 将指定的实体对象从库中移除。
        /// </summary>
        /// <param name="entity">要移除的实体对象。</param>
        /// <param name="logicalDelete">是否为逻辑删除。</param>
        /// <returns>影响的实体数。</returns>
        public int Delete(TEntity entity, bool logicalDelete = true)
        {
            Guard.ArgumentNull(entity, nameof(entity));

            return EntityPersistentSubscribeManager.OnRemove(options.NotifyEvents, entity,
                () => repositoryProxy.DeleteAsync(entity, logicalDelete).Result);
        }

        /// <summary>
        /// 异步的，将指定的实体对象从库中移除。
        /// </summary>
        /// <param name="entity">要移除的实体对象。</param>
        /// <param name="logicalDelete">是否为逻辑删除。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>影响的实体数。</returns>
        public async Task<int> DeleteAsync(TEntity entity, bool logicalDelete = true, CancellationToken cancellationToken = default)
        {
            Guard.ArgumentNull(entity, nameof(entity));

            return await EntityPersistentSubscribeManager.OnRemoveAsync(options.NotifyEvents, entity,
                () => repositoryProxy.DeleteAsync(entity, logicalDelete, cancellationToken));
        }

        /// <summary>
        /// 根据主键值将对象从库中移除。
        /// </summary>
        /// <param name="primaryValues">一组主键值。</param>
        /// <returns>影响的实体数。</returns>
        public int Delete(params PropertyValue[] primaryValues)
        {
            return DeleteAsync(primaryValues).Result;
        }

        /// <summary>
        /// 异步的，根据主键值将对象从库中移除。
        /// </summary>
        /// <param name="primaryValues">一组主键值。</param>
        /// <returns>影响的实体数。</returns>
        public async Task<int> DeleteAsync(params PropertyValue[] primaryValues)
        {
            var ret = await repositoryProxy.DeleteAsync(primaryValues, default);
            if (ret > 0 && options.NotifyEvents)
            {
                EntityPersistentSubscribeManager.Publish<TEntity>(EntityPersistentEventType.AfterRemove);
            }

            return ret;
        }

        /// <summary>
        /// 根据主键值将对象从库中移除。
        /// </summary>
        /// <param name="primaryValues">一组主键值。</param>
        /// <param name="logicalDelete">是否为逻辑删除。</param>
        /// <returns>影响的实体数。</returns>
        public int Delete(PropertyValue[] primaryValues, bool logicalDelete = true)
        {
            return DeleteAsync(primaryValues, logicalDelete).Result;
        }

        /// <summary>
        /// 异步的，根据主键值将对象从库中移除。
        /// </summary>
        /// <param name="primaryValues">一组主键值。</param>
        /// <param name="logicalDelete">是否为逻辑删除。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>影响的实体数。</returns>
        public async Task<int> DeleteAsync(PropertyValue[] primaryValues, bool logicalDelete = true, CancellationToken cancellationToken = default)
        {
            var ret = await repositoryProxy.DeleteAsync(primaryValues, logicalDelete);
            if (ret > 0 && options.NotifyEvents)
            {
                EntityPersistentSubscribeManager.Publish<TEntity>(EntityPersistentEventType.AfterRemove);
            }

            return ret;
        }

        /// <summary>
        /// 将满足条件的一组对象从库中移除。
        /// </summary>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="logicalDelete">是否为逻辑删除</param>
        /// <returns>影响的实体数。</returns>
        public int Delete(Expression<Func<TEntity, bool>> predicate, bool logicalDelete = true)
        {
            return DeleteAsync(predicate, logicalDelete).Result;
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
            var ret = await repositoryProxy.DeleteAsync(predicate, logicalDelete);
            if (ret > 0 && options.NotifyEvents)
            {
                EntityPersistentSubscribeManager.Publish<TEntity>(EntityPersistentEventType.AfterRemove);
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
        public int Update(TEntity entity)
        {
            return UpdateAsync(entity).Result;
        }

        /// <summary>
        /// 异步的，更新一个实体对象。
        /// </summary>
        /// <param name="entity">实体对象。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>影响的实体数。</returns>
        public async Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            Guard.ArgumentNull(entity, nameof(entity));

            return await EntityPersistentSubscribeManager.OnUpdateAsync(options.NotifyEvents, entity,
                () => repositoryProxy.UpdateAsync(HandleValidate(entity), cancellationToken));
        }

        /// <summary>
        /// 使用一个参照的实体对象更新满足条件的一序列对象。
        /// </summary>
        /// <param name="entity">更新的参考对象。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <returns>影响的实体数。</returns>
        public int Update(TEntity entity, Expression<Func<TEntity, bool>> predicate)
        {
            return UpdateAsync(entity, predicate).Result;
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
            var ret = await repositoryProxy.UpdateAsync(entity, predicate);
            if (ret > 0 && options.NotifyEvents)
            {
                EntityPersistentSubscribeManager.Publish<TEntity>(EntityPersistentEventType.AfterUpdate);
            }

            return ret;
        }

        /// <summary>
        /// 使用一个 <see cref="MemberInitExpression"/> 表达式更新满足条件的一序列对象。
        /// </summary>
        /// <param name="factory">一个构造实例并成员绑定的表达式。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <returns>影响的实体数。</returns>
        public int Update(Expression<Func<TEntity>> factory, Expression<Func<TEntity, bool>> predicate)
        {
            return UpdateAsync(factory, predicate).Result;
        }

        /// <summary>
        /// 异步的，使用一个 <see cref="MemberInitExpression"/> 表达式更新满足条件的一序列对象。
        /// </summary>
        /// <param name="factory">一个构造实例并成员绑定的表达式。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>影响的实体数。</returns>
        public async Task<int> UpdateAsync(Expression<Func<TEntity>> factory, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var entity = EntityProxyManager.GetType(typeof(TEntity)).New<TEntity>();
            entity.InitByExpression(factory);

            return predicate == null ? await UpdateAsync(entity, cancellationToken) : await UpdateAsync(entity, predicate, cancellationToken);
        }

        /// <summary>
        /// 使用一个累加器更新满足条件的一序列对象。
        /// </summary>
        /// <param name="calculator">一个计算器表达式。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <returns>影响的实体数。</returns>
        public int Update(Expression<Func<TEntity, TEntity>> calculator, Expression<Func<TEntity, bool>> predicate)
        {
            return UpdateAsync(calculator, predicate).Result;
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
            var ret = await repositoryProxy.UpdateAsync(calculator, predicate, cancellationToken);
            if (ret > 0 && options.NotifyEvents)
            {
                EntityPersistentSubscribeManager.Publish<TEntity>(EntityPersistentEventType.AfterUpdate);
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
        /// <returns>影响的实体数。</returns>
        public int Batch(IEnumerable<TEntity> instances, Expression<Func<IRepository<TEntity>, TEntity, int>> fnOperation)
        {
            return BatchAsync(instances, fnOperation).Result;
        }

        /// <summary>
        /// 异步的，对实体集合进行批量操作。
        /// </summary>
        /// <param name="instances">要操作的实体序列。</param>
        /// <param name="fnOperation">实体操作表达式，权提供 Insert、Update 和 Delete 操作。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>影响的实体数。</returns>
        public async Task<int> BatchAsync(IEnumerable<TEntity> instances, Expression<Func<IRepository<TEntity>, TEntity, int>> fnOperation, CancellationToken cancellationToken = default)
        {
            if (instances.IsNullOrEmpty())
            {
                return -1;
            }

            var operateName = OperateFinder.Find(fnOperation);
            var eventType = GetBeforeEventType(operateName);

            return await EntityPersistentSubscribeManager.OnBatchAsync(options.NotifyEvents, instances.Cast<IEntity>(), eventType, () => repositoryProxy.BatchAsync(instances, fnOperation, cancellationToken));

        }
        #endregion

        /// <summary>
        /// 指定要包括在查询结果中的关联对象。
        /// </summary>
        /// <param name="fnMember">要包含的属性的表达式。</param>
        /// <returns></returns>
        public EntityRepository<TEntity> Include(Expression<Func<TEntity, object>> fnMember)
        {
            repositoryProxy.As<IQueryPolicyExecutor<TEntity>>(s => s.IncludeWith(fnMember));
            return this;
        }

        /// <summary>
        /// 对指定割开的查询始终附加指定的谓语。
        /// </summary>
        /// <param name="memberQuery"></param>
        /// <returns></returns>
        public EntityRepository<TEntity> Associate(Expression<Func<TEntity, IEnumerable>> memberQuery)
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
        /// 将一个新的对象插入到库。
        /// </summary>
        /// <param name="entity">要创建的对象。</param>
        /// <returns>影响的实体数。</returns>
        int IRepository.Insert(IEntity entity)
        {
            return Insert((TEntity)entity);
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
        /// 将对象的改动保存到库。
        /// </summary>
        /// <param name="entity">要保存的对象。</param>
        /// <returns>影响的实体数。</returns>
        int IRepository.InsertOrUpdate(IEntity entity)
        {
            return InsertOrUpdate((TEntity)entity);
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
        int IRepository.Delete(Expression predicate, bool logicalDelete)
        {
            return Delete((Expression<Func<TEntity, bool>>)predicate, logicalDelete);
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
        async Task<int> IRepository.InsertAsync(IEntity entity, CancellationToken cancellationToken)
        {
            return await InsertAsync((TEntity)entity, cancellationToken);
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
        /// 将对象的改动保存到库。
        /// </summary>
        /// <param name="entity">要保存的对象。</param>
        /// <returns>影响的实体数。</returns>
        async Task<int> IRepository.InsertOrUpdateAsync(IEntity entity, CancellationToken cancellationToken)
        {
            return await InsertOrUpdateAsync((TEntity)entity, cancellationToken);
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
        /// 将指定的对象从库中删除。
        /// </summary>
        /// <param name="entity">要移除的对象。</param>
        /// <param name="logicalDelete">是否为逻辑删除。</param>
        /// <returns>影响的实体数。</returns>
        async Task<int> IRepository.DeleteAsync(IEntity entity, bool logicalDelete, CancellationToken cancellationToken)
        {
            return await DeleteAsync((TEntity)entity, logicalDelete, cancellationToken);
        }
        #endregion

        private IEntity GetCloneEntity(IEntity entity)
        {
            var kp = entity as IKeepStateCloneable;
            if (kp != null)
            {
                return (IEntity)kp.Clone();
            }

            return entity;
        }

        private TEntity HandleValidate(TEntity entity)
        {
            if (options.ValidateEntity)
            {
                ValidationUnity.Validate(entity);
            }

            return entity;
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

        private EntityPersistentOperater GetBeforeEventType(string operateName)
        {
            switch (operateName)
            {
                case "Insert":
                    return EntityPersistentOperater.Create;
                case "Update":
                    return EntityPersistentOperater.Update;
                case "Delete":
                    return EntityPersistentOperater.Remove;
                default:
                    throw new InvalidOperationException();
            }
        }

        private EntityPersistentEventType GetAfterEventType(string operateName)
        {
            switch (operateName)
            {
                case "Insert":
                    return EntityPersistentEventType.AfterCreate;
                case "Update":
                    return EntityPersistentEventType.AfterUpdate;
                case "Delete":
                    return EntityPersistentEventType.AfterRemove;
                default:
                    throw new InvalidOperationException();
            }
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
    }
}
