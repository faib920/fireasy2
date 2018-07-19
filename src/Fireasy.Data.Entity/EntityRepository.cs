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

#if !NET40 && !NET35
using System.Threading.Tasks;
#endif
using Fireasy.Common.Extensions;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 表示在 <see cref="EntityContext"/> 实例中对实体 <typeparamref name="TEntity"/> 的仓储。它可以用于直接对实体进行创建、查询、修改和删除。
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public sealed class EntityRepository<TEntity> : IOrderedQueryable<TEntity>, IQueryProviderAware, IRepository<TEntity> where TEntity : IEntity
    {
        private InternalContext context;
        private IRepositoryProvider<TEntity> repositoryProxy;

        /// <summary>
        /// 初始化 <see cref="EntityRepository{TEntity}"/> 类的新实例。
        /// </summary>
        /// <param name="context"></param>
        public EntityRepository(InternalContext context)
        {
            this.context = context;
            repositoryProxy = context.CreateRepositoryProvider<TEntity>();
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

        /// <summary>
        /// 通过一组主键值返回一个实体对象。
        /// </summary>
        /// <param name="primaryValues">一组主键值。</param>
        /// <returns></returns>
        public TEntity Get(params PropertyValue[] primaryValues)
        {
            return repositoryProxy.Get(primaryValues);
        }

        /// <summary>
        /// 将一个新的实体对象创建到库。
        /// </summary>
        /// <param name="entity">要创建的实体对象。</param>
        /// <returns>影响的实体数。</returns>
        public int Insert(TEntity entity)
        {
            Guard.ArgumentNull(entity, nameof(entity));

            if (context.Options.ValidateEntity)
            {
                ValidationUnity.Validate(entity);
            }

            int func() => repositoryProxy.Insert(entity);

            return context.Options.NotifyEvents ?
                EntityPersistentSubscribePublisher.OnCreate(entity, func) : func();
        }

        /// <summary>
        /// 使用一个 <see cref="MemberInitExpression"/> 表达式插入新的对象。
        /// </summary>
        /// <param name="factory">一个构造实例并成员绑定的表达式。</param>
        /// <returns></returns>
        public int Insert(Expression<Func<TEntity>> factory)
        {
            var entity = EntityProxyManager.GetType(typeof(TEntity)).New<TEntity>();
            entity.InitByExpression(factory);

            return Insert(entity);
        }

        /// <summary>
        /// 批量将一组实体对象插入到库中。
        /// </summary>
        /// <param name="entities">一组要插入实体对象。</param>
        /// <param name="batchSize">每一个批次插入的实体数量。默认为 1000。</param>
        /// <param name="completePercentage">已完成百分比的通知方法。</param>
        public void BatchInsert(IEnumerable<TEntity> entities, int batchSize = 1000, Action<int> completePercentage = null)
        {
            Guard.ArgumentNull(entities, nameof(entities));

            repositoryProxy.BatchInsert(entities, batchSize, completePercentage);
        }

        /// <summary>
        /// 根据实体的状态，插入或更新实体对象。
        /// </summary>
        /// <param name="entity">要保存的实体对象。</param>
        /// <returns>影响的实体数。</returns>
        public int InsertOrUpdate(TEntity entity)
        {
            Guard.ArgumentNull(entity, nameof(entity));

            if (context.Options.NotifyEvents)
            {
                if (entity.EntityState == EntityState.Attached)
                {
                    return EntityPersistentSubscribePublisher.OnCreate(entity, () => repositoryProxy.Insert(entity));
                }
                else
                {
                    return EntityPersistentSubscribePublisher.OnUpdate(entity, () => repositoryProxy.Update(entity));
                }
            }

            return entity.EntityState == EntityState.Attached ? repositoryProxy.Insert(entity) : repositoryProxy.Update(entity);
        }

        /// <summary>
        /// 将指定的实体对象从库中移除。
        /// </summary>
        /// <param name="entity">要移除的实体对象。</param>
        /// <param name="logicalDelete">是否为逻辑删除。</param>
        /// <returns>影响的实体数。</returns>
        public int Delete(TEntity entity, bool logicalDelete = true)
        {
            Guard.ArgumentNull(entity, nameof(entity));

            int func() => repositoryProxy.Delete(entity, logicalDelete);

            return context.Options.NotifyEvents ?
                EntityPersistentSubscribePublisher.OnRemove(entity, func) : func();
        }

        /// <summary>
        /// 根据主键值将对象从库中移除。
        /// </summary>
        /// <param name="primaryValues">一组主键值。</param>
        /// <returns>影响的实体数。</returns>
        public int Delete(params PropertyValue[] primaryValues)
        {
            var ret = repositoryProxy.Delete(primaryValues, true);
            if (ret > 0 && context.Options.NotifyEvents)
            {
                EntityPersistentSubscribePublisher.RaiseEvent<TEntity>(EntityPersistentEventType.AfterRemove);
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
            var ret = repositoryProxy.Delete(primaryValues, logicalDelete);
            if (ret > 0 && context.Options.NotifyEvents)
            {
                EntityPersistentSubscribePublisher.RaiseEvent<TEntity>(EntityPersistentEventType.AfterRemove);
            }

            return ret;
        }

        /// <summary>
        /// 根据主键值将对象从库中移除。
        /// </summary>
        /// <param name="primaryValues">一组主键值。</param>
        /// <param name="logicalDelete">是否为逻辑删除。</param>
        /// <returns>影响的实体数。</returns>
        public int Delete(object[] primaryValues, bool logicalDelete = true)
        {
            var ret = repositoryProxy.Delete(primaryValues, logicalDelete);
            if (ret > 0 && context.Options.NotifyEvents)
            {
                EntityPersistentSubscribePublisher.RaiseEvent<TEntity>(EntityPersistentEventType.AfterRemove);
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
            var ret = repositoryProxy.Delete(predicate, logicalDelete);
            if (ret > 0 && context.Options.NotifyEvents)
            {
                EntityPersistentSubscribePublisher.RaiseEvent<TEntity>(EntityPersistentEventType.AfterRemove);
            }

            return ret;
        }

        /// <summary>
        /// 更新一个实体对象。
        /// </summary>
        /// <param name="entity">实体对象。</param>
        /// <returns>影响的实体数。</returns>
        public int Update(TEntity entity)
        {
            Guard.ArgumentNull(entity, nameof(entity));

            if (context.Options.ValidateEntity)
            {
                ValidationUnity.Validate(entity);
            }

            int func() => repositoryProxy.Update(entity);

            return context.Options.NotifyEvents ?
                EntityPersistentSubscribePublisher.OnUpdate(entity, func) : func();
        }

        /// <summary>
        /// 使用一个参照的实体对象更新满足条件的一序列对象。
        /// </summary>
        /// <param name="entity">更新的参考对象。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <returns>影响的实体数。</returns>
        public int Update(TEntity entity, Expression<Func<TEntity, bool>> predicate)
        {
            var ret = repositoryProxy.Update(entity, predicate);
            if (ret > 0 && context.Options.NotifyEvents)
            {
                EntityPersistentSubscribePublisher.RaiseEvent<TEntity>(EntityPersistentEventType.AfterUpdate);
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
            var entity = EntityProxyManager.GetType(typeof(TEntity)).New<TEntity>();
            entity.InitByExpression(factory);

            return predicate == null ? Update(entity) : Update(entity, predicate);
        }

        /// <summary>
        /// 使用一个累加器更新满足条件的一序列对象。
        /// </summary>
        /// <param name="calculator">一个计算器表达式。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <returns>影响的实体数。</returns>
        public int Update(Expression<Func<TEntity, TEntity>> calculator, Expression<Func<TEntity, bool>> predicate)
        {
            var ret = repositoryProxy.Update(calculator, predicate);
            if (ret > 0 && context.Options.NotifyEvents)
            {
                EntityPersistentSubscribePublisher.RaiseEvent<TEntity>(EntityPersistentEventType.AfterUpdate);
            }

            return ret;
        }

        /// <summary>
        /// 对实体集合进行批量操作。
        /// </summary>
        /// <param name="instances">要操作的实体序列。</param>
        /// <param name="fnOperation">实体操作表达式，权提供 Insert、Update 和 Delete 操作。</param>
        /// <returns>影响的实体数。</returns>
        public int Batch(IEnumerable<TEntity> instances, Expression<Func<IRepository<TEntity>, TEntity, int>> fnOperation)
        {
            if (instances.IsNullOrEmpty())
            {
                return -1;
            }

            var operateName = OperateFinder.Find(fnOperation);
            var eventType = GetBeforeEventType(operateName);

            int func() => repositoryProxy.Batch(instances, fnOperation);

            return context.Options.NotifyEvents ?
                EntityPersistentSubscribePublisher.OnBatch(instances.Cast<IEntity>(), eventType, func) :
                func();
        }

#if !NET40 && !NET35
        /// <summary>
        /// 将一个新的实体对象创建到库。
        /// </summary>
        /// <param name="entity">要创建的实体对象。</param>
        /// <returns>影响的实体数。</returns>
        public async Task<int> InsertAsync(TEntity entity)
        {
            return await Task.Run(() => Insert(entity));
        }

        /// <summary>
        /// 使用一个 <see cref="MemberInitExpression"/> 表达式插入新的对象。
        /// </summary>
        /// <param name="factory">一个构造实例并成员绑定的表达式。</param>
        /// <returns></returns>
        public async Task<int> InsertAsync(Expression<Func<TEntity>> factory)
        {
            return await Task.Run(() => Insert(factory));
        }

        /// <summary>
        /// 批量将一组实体对象插入到库中。
        /// </summary>
        /// <param name="entities">一组要插入实体对象。</param>
        /// <param name="batchSize">每一个批次插入的实体数量。默认为 1000。</param>
        /// <param name="completePercentage">已完成百分比的通知方法。</param>
        public async Task BatchInsertAsync(IEnumerable<TEntity> entities, int batchSize = 1000, Action<int> completePercentage = null)
        {
            await Task.Run(() => BatchInsert(entities, batchSize, completePercentage));
        }

        /// <summary>
        /// 根据实体的状态，插入或更新实体对象。
        /// </summary>
        /// <param name="entity">要保存的实体对象。</param>
        /// <returns>影响的实体数。</returns>
        public async Task<int> InsertOrUpdateAsync(TEntity entity)
        {
            return await Task.Run(() => InsertOrUpdate(entity));
        }

        /// <summary>
        /// 将指定的实体对象从库中移除。
        /// </summary>
        /// <param name="entity">要移除的实体对象。</param>
        /// <param name="logicalDelete">是否为逻辑删除。</param>
        /// <returns>影响的实体数。</returns>
        public async Task<int> DeleteAsync(TEntity entity, bool logicalDelete = true)
        {
            return await Task.Run(() => Delete(entity, logicalDelete));
        }

        /// <summary>
        /// 根据主键值将对象从库中移除。
        /// </summary>
        /// <param name="primaryValues">一组主键值。</param>
        /// <returns>影响的实体数。</returns>
        public async Task<int> DeleteAsync(params PropertyValue[] primaryValues)
        {
            return await Task.Run(() => Delete(primaryValues, true));
        }

        /// <summary>
        /// 根据主键值将对象从库中移除。
        /// </summary>
        /// <param name="primaryValues">一组主键值。</param>
        /// <param name="logicalDelete">是否为逻辑删除。</param>
        /// <returns>影响的实体数。</returns>
        public async Task<int> DeleteAsync(PropertyValue[] primaryValues, bool logicalDelete = true)
        {
            return await Task.Run(() => Delete(primaryValues, logicalDelete));
        }

        /// <summary>
        /// 根据主键值将对象从库中移除。
        /// </summary>
        /// <param name="primaryValues">一组主键值。</param>
        /// <param name="logicalDelete">是否为逻辑删除。</param>
        /// <returns>影响的实体数。</returns>
        public async Task<int> DeleteAsync(object[] primaryValues, bool logicalDelete = true)
        {
            return await Task.Run(() => Delete(primaryValues, logicalDelete));
        }

        /// <summary>
        /// 将满足条件的一组对象从库中移除。
        /// </summary>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="logicalDelete">是否为逻辑删除</param>
        /// <returns>影响的实体数。</returns>
        public async Task<int> DeleteAsync(Expression<Func<TEntity, bool>> predicate, bool logicalDelete = true)
        {
            return await Task.Run(() => Delete(predicate, logicalDelete));
        }

        /// <summary>
        /// 更新一个实体对象。
        /// </summary>
        /// <param name="entity">实体对象。</param>
        /// <returns>影响的实体数。</returns>
        public async Task<int> UpdateAsync(TEntity entity)
        {
            return await Task.Run(() => Update(entity));
        }

        /// <summary>
        /// 使用一个参照的实体对象更新满足条件的一序列对象。
        /// </summary>
        /// <param name="entity">更新的参考对象。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <returns>影响的实体数。</returns>
        public async Task<int> UpdateAsync(TEntity entity, Expression<Func<TEntity, bool>> predicate)
        {
            return await Task.Run(() => Update(entity, predicate));
        }

        /// <summary>
        /// 使用一个 <see cref="MemberInitExpression"/> 表达式更新满足条件的一序列对象。
        /// </summary>
        /// <param name="factory">一个构造实例并成员绑定的表达式。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <returns>影响的实体数。</returns>
        public async Task<int> UpdateAsync(Expression<Func<TEntity>> factory, Expression<Func<TEntity, bool>> predicate)
        {
            return await Task.Run(() => Update(factory, predicate));
        }

        /// <summary>
        /// 使用一个累加器更新满足条件的一序列对象。
        /// </summary>
        /// <param name="calculator">一个计算器表达式。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <returns>影响的实体数。</returns>
        public async Task<int> UpdateAsync(Expression<Func<TEntity, TEntity>> calculator, Expression<Func<TEntity, bool>> predicate)
        {
            return await Task.Run(() => Update(calculator, predicate));
        }

        /// <summary>
        /// 对实体集合进行批量操作。
        /// </summary>
        /// <param name="instances">要操作的实体序列。</param>
        /// <param name="fnOperation">实体操作表达式，权提供 Insert、Update 和 Delete 操作。</param>
        /// <returns>影响的实体数。</returns>
        public async Task<int> BatchAsync(IEnumerable<TEntity> instances, Expression<Func<IRepository<TEntity>, TEntity, int>> fnOperation)
        {
            return await Task.Run(() => Batch(instances, fnOperation));
        }
#endif

        /// <summary>
        /// 指定要包括在查询结果中的关联对象。
        /// </summary>
        /// <param name="fnMember">要包含的属性的表达式。</param>
        /// <returns></returns>
        public EntityRepository<TEntity> Include(Expression<Func<TEntity, object>> fnMember)
        {
            context.IncludeWith(fnMember);
            return this;
        }

        /// <summary>
        /// 对指定割开的查询始终附加指定的谓语。
        /// </summary>
        /// <param name="memberQuery"></param>
        /// <returns></returns>
        public EntityRepository<TEntity> Associate(Expression<Func<TEntity, IEnumerable>> memberQuery)
        {
            context.AssociateWith(memberQuery);
            return this;
        }

        /// <summary>
        /// 配置参数。
        /// </summary>
        /// <param name="configuration">配置参数的方法。</param>
        /// <returns></returns>
        public EntityRepository<TEntity> ConfigOptions(Action<EntityContextOptions> configuration)
        {
            configuration?.Invoke(context.Options);
            return this;
        }

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

#if !NET40 && !NET35
        /// <summary>
        /// 将一个新的对象插入到库。
        /// </summary>
        /// <param name="entity">要创建的对象。</param>
        /// <returns>影响的实体数。</returns>
        async Task<int> IRepository.InsertAsync(IEntity entity)
        {
            return await InsertAsync((TEntity)entity);
        }

        /// <summary>
        /// 更新一个对象。
        /// </summary>
        /// <param name="entity">要更新的对象。</param>
        /// <returns>影响的实体数。</returns>
        async Task<int> IRepository.UpdateAsync(IEntity entity)
        {
            return await UpdateAsync((TEntity)entity);
        }

        /// <summary>
        /// 将对象的改动保存到库。
        /// </summary>
        /// <param name="entity">要保存的对象。</param>
        /// <returns>影响的实体数。</returns>
        async Task<int> IRepository.InsertOrUpdateAsync(IEntity entity)
        {
            return await InsertOrUpdateAsync((TEntity)entity);
        }

        /// <summary>
        /// 将指定的对象从库中删除。
        /// </summary>
        /// <param name="entity">要移除的对象。</param>
        /// <param name="logicalDelete">是否为逻辑删除。</param>
        /// <returns>影响的实体数。</returns>
        async Task<int> IRepository.DeleteAsync(IEntity entity, bool logicalDelete)
        {
            return await DeleteAsync((TEntity)entity, logicalDelete);
        }

        /// <summary>
        /// 将满足条件的一组对象从库中移除。
        /// </summary>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="logicalDelete">是否为逻辑删除</param>
        /// <returns>影响的实体数。</returns>
        async Task<int> IRepository.DeleteAsync(Expression predicate, bool logicalDelete)
        {
            return await DeleteAsync((Expression<Func<TEntity, bool>>)predicate, logicalDelete);
        }
#endif

        private IEntity GetCloneEntity(IEntity entity)
        {
            var kp = entity as IKeepStateCloneable;
            if (kp != null)
            {
                return (IEntity)kp.Clone();
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
