// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 工作单元接口。
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// 创建 <typeparamref name="TEntity"/> 类型的仓储实例。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        IRepository<TEntity> CreateRepository<TEntity>() where TEntity : IEntity;
    }
}
