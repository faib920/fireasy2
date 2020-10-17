// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Data.Entity
{
    //todo
    /*
    /// <summary>
    /// 实体树更新时的通知方法。
    /// </summary>
    /// <typeparam name="TEntity">实体类型。</typeparam>
    /// <param name="persister"></param>
    /// <param name="e">当前更新的实体的相关数据。</param>
    public delegate void EntityTreeUpdatingEventHandler<TEntity>(EntityTreePersister<TEntity> persister, EntityTreeUpdatingEventArgs<TEntity> e) where TEntity : class, IEntity;
    */

    /// <summary>
    /// 实体树更新时的通知方法。
    /// </summary>
    /// <param name="e">当前更新的实体的相关数据。</param>
    public delegate void EntityTreeUpdatingEventHandler(EntityTreeUpdatingEventArgs e);
}
