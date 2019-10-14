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
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 表示对象的仓储。
    /// </summary>
    public interface IRepository
    {
        /// <summary>
        /// 通过一组主键值返回一个对象。
        /// </summary>
        /// <param name="primaryValues">一组主键值。</param>
        /// <returns></returns>
        IEntity Get(params PropertyValue[] primaryValues);

        /// <summary>
        /// 将一个新的对象插入到库。
        /// </summary>
        /// <param name="entity">要创建的对象。</param>
        /// <returns></returns>
        int Insert(IEntity entity);

        /// <summary>
        /// 更新一个对象。
        /// </summary>
        /// <param name="entity">要更新的对象。</param>
        /// <returns></returns>
        int Update(IEntity entity);

        /// <summary>
        /// 将对象的改动保存到库。
        /// </summary>
        /// <param name="entity">要保存的对象。</param>
        /// <returns></returns>
        int InsertOrUpdate(IEntity entity);

        /// <summary>
        /// 将指定的对象从库中删除。
        /// </summary>
        /// <param name="entity">要移除的对象。</param>
        /// <param name="logicalDelete">是否为逻辑删除。</param>
        /// <returns></returns>
        int Delete(IEntity entity, bool logicalDelete = true);

        /// <summary>
        /// 根据主键值将对象从库中删除。
        /// </summary>
        /// <param name="primaryValues">一组主键值。</param>
        /// <returns></returns>
        int Delete(params PropertyValue[] primaryValues);

        /// <summary>
        /// 根据主键值将对象从库中删除。
        /// </summary>
        /// <param name="primaryValues">一组主键值。</param>
        /// <param name="logicalDelete">是否为逻辑删除。</param>
        /// <returns></returns>
        int Delete(PropertyValue[] primaryValues, bool logicalDelete = true);

        /// <summary>
        /// 将满足条件的一组对象从库中移除。
        /// </summary>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="logicalDelete">是否为逻辑删除</param>
        /// <returns>影响的实体数。</returns>
        int Delete(Expression predicate, bool logicalDelete = true);

        /// <summary>
        /// 异步的，通过一组主键值返回一个对象。
        /// </summary>
        /// <param name="primaryValues">一组主键值。</param>
        /// <returns></returns>
        Task<IEntity> GetAsync(params PropertyValue[] primaryValues);

        /// <summary>
        /// 异步的，将一个新的对象插入到库。
        /// </summary>
        /// <param name="entity">要创建的对象。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        Task<int> InsertAsync(IEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// 异步的，更新一个对象。
        /// </summary>
        /// <param name="entity">要更新的对象。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        Task<int> UpdateAsync(IEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// 异步的，将对象的改动保存到库。
        /// </summary>
        /// <param name="entity">要保存的对象。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        Task<int> InsertOrUpdateAsync(IEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// 异步的，将指定的对象从库中删除。
        /// </summary>
        /// <param name="entity">要移除的对象。</param>
        /// <param name="logicalDelete">是否为逻辑删除。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        Task<int> DeleteAsync(IEntity entity, bool logicalDelete = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// 异步的，根据主键值将对象从库中删除。
        /// </summary>
        /// <param name="primaryValues">一组主键值。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        Task<int> DeleteAsync(params PropertyValue[] primaryValues);

        /// <summary>
        /// 异步的，根据主键值将对象从库中删除。
        /// </summary>
        /// <param name="primaryValues">一组主键值。</param>
        /// <param name="logicalDelete">是否为逻辑删除。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        Task<int> DeleteAsync(PropertyValue[] primaryValues, bool logicalDelete = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// 异步的，将满足条件的一组对象从库中移除。
        /// </summary>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="logicalDelete">是否为逻辑删除</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>影响的实体数。</returns>
        Task<int> DeleteAsync(Expression predicate, bool logicalDelete = true, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// 表示实体 <typeparamref name="TEntity"/> 的仓储。
    /// </summary>
    /// <typeparam name="TEntity">实体类型。</typeparam>
    public interface IRepository<TEntity> : IOrderedQueryable<TEntity>, IRepository where TEntity : IEntity
    {
        /// <summary>
        /// 批量将一组实体对象插入到库中。
        /// </summary>
        /// <param name="entities">一组要插入实体对象。</param>
        /// <param name="batchSize">每一个批次插入的实体数量。默认为 1000。</param>
        /// <param name="completePercentage">已完成百分比的通知方法。</param>
        void BatchInsert(IEnumerable<TEntity> entities, int batchSize = 1000, Action<int> completePercentage = null);

        /// <summary>
        /// 通过一组主键值返回一个实体对象。
        /// </summary>
        /// <param name="primaryValues">一组主键值。</param>
        /// <returns></returns>
        TEntity Get(params PropertyValue[] primaryValues);

        /// <summary>
        /// 将一个新的实体对象插入到库。
        /// </summary>
        /// <param name="entity">要创建的实体对象。</param>
        /// <returns>如果主键是自增类型，则为主键值，否则为影响的实体数。</returns>
        int Insert(TEntity entity);

        /// <summary>
        /// 使用一个 <see cref="MemberInitExpression"/> 表达式插入新的对象。
        /// </summary>
        /// <param name="factory">一个构造实例并成员绑定的表达式。</param>
        /// <returns>如果主键是自增类型，则为主键值，否则为影响的实体数。</returns>
        int Insert(Expression<Func<TEntity>> factory);

        /// <summary>
        /// 更新一个实体对象。
        /// </summary>
        /// <param name="entity">要更新的实体对象。</param>
        /// <returns>影响的实体数。</returns>
        int Update(TEntity entity);

        /// <summary>
        /// 使用一个参照的实体对象更新满足条件的一序列对象。
        /// </summary>
        /// <param name="entity">更新的参考对象。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <returns>影响的实体数。</returns>
        int Update(TEntity entity, Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// 使用一个 <see cref="MemberInitExpression"/> 表达式更新满足条件的一序列对象。
        /// </summary>
        /// <param name="factory">一个构造实例并成员绑定的表达式。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <returns>影响的实体数。</returns>
        int Update(Expression<Func<TEntity>> factory, Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// 使用一个累加器更新满足条件的一序列对象。
        /// </summary>
        /// <param name="calculator">一个计算器表达式。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <returns>影响的实体数。</returns>
        int Update(Expression<Func<TEntity, TEntity>> calculator, Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// 将实体对象的改动保存到库。
        /// </summary>
        /// <param name="entity">要保存的实体对象。</param>
        /// <returns>影响的实体数。</returns>
        int InsertOrUpdate(TEntity entity);

        /// <summary>
        /// 将指定的实体对象从库中删除。
        /// </summary>
        /// <param name="entity">要移除的实体对象。</param>
        /// <param name="logicalDelete">是否为逻辑删除。</param>
        /// <returns>影响的实体数。</returns>
        int Delete(TEntity entity, bool logicalDelete = true);

        /// <summary>
        /// 将满足条件的一组对象从库中移除。
        /// </summary>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="logicalDelete">是否为逻辑删除</param>
        /// <returns>影响的实体数。</returns>
        int Delete(Expression<Func<TEntity, bool>> predicate, bool logicalDelete = true);

        /// <summary>
        /// 对实体集合进行批量操作。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instances"></param>
        /// <param name="fnOperation"></param>
        /// <returns>影响的实体数。</returns>
        int Batch(IEnumerable<TEntity> instances, Expression<Func<IRepository<TEntity>, TEntity, int>> fnOperation);

        /// <summary>
        /// 异步的，批量将一组实体对象插入到库中。
        /// </summary>
        /// <param name="entities">一组要插入实体对象。</param>
        /// <param name="batchSize">每一个批次插入的实体数量。默认为 1000。</param>
        /// <param name="completePercentage">已完成百分比的通知方法。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        Task BatchInsertAsync(IEnumerable<TEntity> entities, int batchSize = 1000, Action<int> completePercentage = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 异步的，通过一组主键值返回一个对象。
        /// </summary>
        /// <param name="primaryValues">一组主键值。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        Task<TEntity> GetAsync(params PropertyValue[] primaryValues);

        /// <summary>
        /// 异步的，将一个新的实体对象插入到库。
        /// </summary>
        /// <param name="entity">要创建的实体对象。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>如果主键是自增类型，则为主键值，否则为影响的实体数。</returns>
        Task<int> InsertAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// 异步的，使用一个 <see cref="MemberInitExpression"/> 表达式插入新的对象。
        /// </summary>
        /// <param name="factory">一个构造实例并成员绑定的表达式。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>如果主键是自增类型，则为主键值，否则为影响的实体数。</returns>
        Task<int> InsertAsync(Expression<Func<TEntity>> factory, CancellationToken cancellationToken = default);

        /// <summary>
        /// 异步的，更新一个实体对象。
        /// </summary>
        /// <param name="entity">要更新的实体对象。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>影响的实体数。</returns>
        Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// 异步的，使用一个参照的实体对象更新满足条件的一序列对象。
        /// </summary>
        /// <param name="entity">更新的参考对象。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>影响的实体数。</returns>
        Task<int> UpdateAsync(TEntity entity, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// 异步的，使用一个 <see cref="MemberInitExpression"/> 表达式更新满足条件的一序列对象。
        /// </summary>
        /// <param name="factory">一个构造实例并成员绑定的表达式。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>影响的实体数。</returns>
        Task<int> UpdateAsync(Expression<Func<TEntity>> factory, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// 异步的，使用一个累加器更新满足条件的一序列对象。
        /// </summary>
        /// <param name="calculator">一个计算器表达式。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>影响的实体数。</returns>
        Task<int> UpdateAsync(Expression<Func<TEntity, TEntity>> calculator, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// 异步的，将实体对象的改动保存到库。
        /// </summary>
        /// <param name="entity">要保存的实体对象。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>影响的实体数。</returns>
        Task<int> InsertOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// 异步的，将指定的实体对象从库中删除。
        /// </summary>
        /// <param name="entity">要移除的实体对象。</param>
        /// <param name="logicalDelete">是否为逻辑删除。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>影响的实体数。</returns>
        Task<int> DeleteAsync(TEntity entity, bool logicalDelete = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// 异步的，将满足条件的一组对象从库中移除。
        /// </summary>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="logicalDelete">是否为逻辑删除</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>影响的实体数。</returns>
        Task<int> DeleteAsync(Expression<Func<TEntity, bool>> predicate, bool logicalDelete = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// 异步的，对实体集合进行批量操作。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instances"></param>
        /// <param name="fnOperation"></param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns>影响的实体数。</returns>
        Task<int> BatchAsync(IEnumerable<TEntity> instances, Expression<Func<IRepository<TEntity>, TEntity, int>> fnOperation, CancellationToken cancellationToken = default);
    }
}
