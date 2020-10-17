// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 实体仓储初始化器。
    /// </summary>
    public interface IRespositoryInitializer<TContext, TEntity>
        where TContext : EntityContext
        where TEntity : IEntity
    {
        /// <summary>
        /// 使用 <see cref="EntityContext"/> 初始化。
        /// </summary>
        /// <param name="context"></param>
        void Initialize(TContext context);
    }
}
