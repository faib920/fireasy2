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
    /// 仓储提供者接口。
    /// </summary>
    public interface IRepositoryProvider
    {
        /// <summary>
        /// 创建一个 <see cref="IRepository"/> 实例。
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        IRepository CreateRepository(EntityContextOptions options);

        /// <summary>
        /// 获取 <see cref="IQueryable"/> 对象。
        /// </summary>
        IQueryable Queryable { get; }

        /// <summary>
        /// 获取 <see cref="IQueryProvider"/> 对象。
        /// </summary>
        IQueryProvider QueryProvider { get; }
    }

    /// <summary>
    /// 实体仓储提供者接口。
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IRepositoryProvider<TEntity> : IRepositoryProvider where TEntity : IEntity
    {
        /// <summary>
        /// 将一个新的实体对象插入到库。
        /// </summary>
        /// <param name="entity">要创建的实体对象。</param>
        /// <returns>影响的实体数。</returns>
        Task<int> InsertAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// 更新一个实体对象。
        /// </summary>
        /// <param name="entity">要更新的实体对象。</param>
        /// <returns>影响的实体数。</returns>
        Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// 批量将一组实体对象插入到库中。
        /// </summary>
        /// <param name="entities">一组要插入实体对象。</param>
        /// <param name="batchSize">每一个批次插入的实体数量。默认为 1000。</param>
        /// <param name="completePercentage">已完成百分比的通知方法。</param>
        Task BatchInsertAsync(IEnumerable<TEntity> entities, int batchSize = 1000, Action<int> completePercentage = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 将指定的实体对象从库中删除。
        /// </summary>
        /// <param name="entity">要移除的实体对象。</param>
        /// <param name="logicalDelete">是否为逻辑删除。</param>
        /// <returns>影响的实体数。</returns>
        Task<int> DeleteAsync(TEntity entity, bool logicalDelete = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// 根据主键值将对象从库中删除。
        /// </summary>
        /// <param name="primaryValues">一组主键值。</param>
        /// <param name="logicalDelete">是否为逻辑删除。</param>
        /// <returns></returns>
        Task<int> DeleteAsync(PropertyValue[] primaryValues, bool logicalDelete = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// 通过一组主键值返回一个实体对象。
        /// </summary>
        /// <param name="primaryValues">一组主键值。</param>
        /// <returns>影响的实体数。</returns>
        Task<TEntity> GetAsync(PropertyValue[] primaryValues, CancellationToken cancellationToken = default);

        /// <summary>
        /// 将满足条件的一组对象从库中移除。
        /// </summary>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="logicalDelete">是否为逻辑删除</param>
        /// <returns>影响的实体数。</returns>
        Task<int> DeleteAsync(Expression<Func<TEntity, bool>> predicate, bool logicalDelete = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// 使用一个参照的实体对象更新满足条件的一序列对象。
        /// </summary>
        /// <param name="entity">更新的参考对象。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <returns>影响的实体数。</returns>
        Task<int> UpdateAsync(TEntity entity, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// 使用一个累加器更新满足条件的一序列对象。
        /// </summary>
        /// <param name="calculator">一个计算器表达式。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <returns>影响的实体数。</returns>
        Task<int> UpdateAsync(Expression<Func<TEntity, TEntity>> calculator, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// 对实体集合进行批量操作。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instances"></param>
        /// <param name="fnOperation"></param>
        /// <returns>影响的实体数。</returns>
        Task<int> BatchAsync(IEnumerable<TEntity> instances, Expression<Func<IRepository<TEntity>, TEntity, int>> fnOperation, CancellationToken cancellationToken = default);
    }
}
