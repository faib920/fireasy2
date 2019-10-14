// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
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
    /// 提供树实体类型的仓储。
    /// </summary>
    public interface ITreeRepository
    {
        /// <summary>
        /// 将一个实体插入到参照实体的相应位置。
        /// </summary>
        /// <param name="entity">插入的实体。</param>
        /// <param name="referEntity">参照的实体。</param>
        /// <param name="position">插入的位置。</param>
        /// <param name="isolation">数据隔离表达式。</param>
        void Insert(IEntity entity, IEntity referEntity, EntityTreePosition position = EntityTreePosition.Children, Expression isolation = null);

        /// <summary>
        /// 异步的，将一个实体插入到参照实体的相应位置。
        /// </summary>
        /// <param name="entity">插入的实体。</param>
        /// <param name="referEntity">参照的实体。</param>
        /// <param name="position">插入的位置。</param>
        /// <param name="isolation">数据隔离表达式。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        Task InsertAsync(IEntity entity, IEntity referEntity, EntityTreePosition position = EntityTreePosition.Children, Expression isolation = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 将一组实体插入到参照实体的相应位置。
        /// </summary>
        /// <param name="entities">插入的实体集。</param>
        /// <param name="referEntity">参照的实体。</param>
        /// <param name="position">插入的位置。</param>
        /// <param name="isolation">数据隔离表达式。</param>
        void BatchInsert(IEnumerable entities, IEntity referEntity, EntityTreePosition position = EntityTreePosition.Children, Expression isolation = null);

        /// <summary>
        /// 异步的，将一组实体插入到参照实体的相应位置。
        /// </summary>
        /// <param name="entities">插入的实体集。</param>
        /// <param name="referEntity">参照的实体。</param>
        /// <param name="position">插入的位置。</param>
        /// <param name="isolation">数据隔离表达式。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        Task BatchInsertAsync(IEnumerable entities, IEntity referEntity, EntityTreePosition position = EntityTreePosition.Children, Expression isolation = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 将一个实体移动到参照实体的相应位置。
        /// </summary>
        /// <param name="entity">要移动的实体。</param>
        /// <param name="referEntity">参照的实体。</param>
        /// <param name="position">移动的位置。</param>
        /// <param name="isolation">数据隔离表达式。</param>
        void Move(IEntity entity, IEntity referEntity, EntityTreePosition? position = EntityTreePosition.Children, Expression isolation = null);

        /// <summary>
        /// 异步的，将一个实体移动到参照实体的相应位置。
        /// </summary>
        /// <param name="entity">要移动的实体。</param>
        /// <param name="referEntity">参照的实体。</param>
        /// <param name="position">移动的位置。</param>
        /// <param name="isolation">数据隔离表达式。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        Task MoveAsync(IEntity entity, IEntity referEntity, EntityTreePosition? position = EntityTreePosition.Children, Expression isolation = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 判断实体是否具有孩子。
        /// </summary>
        /// <param name="entity">当前实体。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <returns></returns>
        bool HasChildren(IEntity entity, Expression predicate = null);

        /// <summary>
        /// 异步的，判断实体是否具有孩子。
        /// </summary>
        /// <param name="entity">当前实体。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        Task<bool> HasChildrenAsync(IEntity entity, Expression predicate = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 查询指定实体的孩子。
        /// </summary>
        /// <param name="entity">当前实体。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="recurrence">是否递归出所有孩子。</param>
        /// <returns></returns>
        IQueryable QueryChildren(IEntity entity, Expression predicate = null, bool recurrence = false);

        /// <summary>
        /// 递归返回实体的父亲实体。
        /// </summary>
        /// <param name="entity">当前实体。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <returns></returns>
        IQueryable RecurrenceParent(IEntity entity, Expression predicate = null);
    }

    /// <summary>
    /// 提供树实体类型 <typeparamref name="TEntity"/> 的仓储。
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface ITreeRepository<TEntity> : ITreeRepository where TEntity : IEntity
    {
        /// <summary>
        /// 将一个实体插入到参照实体的相应位置。
        /// </summary>
        /// <param name="entity">插入的实体。</param>
        /// <param name="referEntity">参照的实体。</param>
        /// <param name="position">插入的位置。</param>
        /// <param name="isolation">数据隔离表达式。</param>
        void Insert(TEntity entity, TEntity referEntity, EntityTreePosition position = EntityTreePosition.Children, Expression<Func<TEntity>> isolation = null);

        /// <summary>
        /// 异步的，将一个实体插入到参照实体的相应位置。
        /// </summary>
        /// <param name="entity">插入的实体。</param>
        /// <param name="referEntity">参照的实体。</param>
        /// <param name="position">插入的位置。</param>
        /// <param name="isolation">数据隔离表达式。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        Task InsertAsync(TEntity entity, TEntity referEntity, EntityTreePosition position = EntityTreePosition.Children, Expression<Func<TEntity>> isolation = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 将一组实体插入到参照实体的相应位置。
        /// </summary>
        /// <param name="entities">插入的实体集。</param>
        /// <param name="referEntity">参照的实体。</param>
        /// <param name="position">插入的位置。</param>
        /// <param name="isolation">数据隔离表达式。</param>
        void BatchInsert(IEnumerable<TEntity> entities, TEntity referEntity, EntityTreePosition position = EntityTreePosition.Children, Expression<Func<TEntity>> isolation = null);

        /// <summary>
        /// 异步的，将一组实体插入到参照实体的相应位置。
        /// </summary>
        /// <param name="entities">插入的实体集。</param>
        /// <param name="referEntity">参照的实体。</param>
        /// <param name="position">插入的位置。</param>
        /// <param name="isolation">数据隔离表达式。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        Task BatchInsertAsync(IEnumerable<TEntity> entities, TEntity referEntity, EntityTreePosition position = EntityTreePosition.Children, Expression<Func<TEntity>> isolation = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 将一个实体移动到参照实体的相应位置。
        /// </summary>
        /// <param name="entity">要移动的实体。</param>
        /// <param name="referEntity">参照的实体。</param>
        /// <param name="position">移动的位置。</param>
        /// <param name="isolation">数据隔离表达式。</param>
        void Move(TEntity entity, TEntity referEntity, EntityTreePosition? position = EntityTreePosition.Children, Expression<Func<TEntity>> isolation = null);

        /// <summary>
        /// 异步的，将一个实体移动到参照实体的相应位置。
        /// </summary>
        /// <param name="entity">要移动的实体。</param>
        /// <param name="referEntity">参照的实体。</param>
        /// <param name="position">移动的位置。</param>
        /// <param name="isolation">数据隔离表达式。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        Task MoveAsync(TEntity entity, TEntity referEntity, EntityTreePosition? position = EntityTreePosition.Children, Expression<Func<TEntity>> isolation = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 判断实体是否具有孩子。
        /// </summary>
        /// <param name="entity">当前实体。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <returns></returns>
        bool HasChildren(TEntity entity, Expression<Func<TEntity, bool>> predicate = null);

        /// <summary>
        /// 异步的，判断实体是否具有孩子。
        /// </summary>
        /// <param name="entity">当前实体。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="cancellationToken">取消操作的通知。</param>
        /// <returns></returns>
        Task<bool> HasChildrenAsync(TEntity entity, Expression<Func<TEntity, bool>> predicate = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 查询指定实体的孩子。
        /// </summary>
        /// <param name="entity">当前实体。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="recurrence">是否递归出所有孩子。</param>
        /// <returns></returns>
        IQueryable<TEntity> QueryChildren(TEntity entity, Expression<Func<TEntity, bool>> predicate = null, bool recurrence = false);

        /// <summary>
        /// 递归返回实体的父亲实体。
        /// </summary>
        /// <param name="entity">当前实体。</param>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <returns></returns>
        IQueryable<TEntity> RecurrenceParent(TEntity entity, Expression<Func<TEntity, bool>> predicate = null);
    }
}
