// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Entity.Linq;
using Fireasy.Data.Entity.Query;
using System;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 实体树更新时包含的相关数据。
    /// </summary>
    public class EntityTreeUpdatingEventArgs : EventArgs
    {
        /// <summary>
        /// 初始化 <see cref="EntityTreeUpdatingEventArgs"></see> 类的新实例。
        /// </summary>
        /// <param name="entity"></param>
        public EntityTreeUpdatingEventArgs(IEntity entity)
        {
            Current = entity;
        }

        /// <summary>
        /// 获取当前更新的实体。
        /// </summary>
        public IEntity Current { get; private set; }

        /// <summary>
        /// 获取实体更新前的相关值。
        /// </summary>
        public EntityTreeUpdatingBag OldValue { get; internal set; }

        /// <summary>
        /// 获取实体更新后的相关值。
        /// </summary>
        public EntityTreeUpdatingBag NewValue { get; internal set; }

        /// <summary>
        /// 获取更新的动作类型。
        /// </summary>
        public EntityTreeUpdatingAction Action { get; internal set; }

        /// <summary>
        /// 获取或设置是否取消当前的更新操作。
        /// </summary>
        public bool Cancel { get; set; }
    }

    /// <summary>
    /// 实体树更新时包含的相关数据。
    /// </summary>
    /// <typeparam name="TEntity">实体的类型。</typeparam>
    public sealed class EntityTreeUpdatingEventArgs<TEntity> : EntityTreeUpdatingEventArgs where TEntity : IEntity
    {
        /// <summary>
        /// 初始化 <see cref="T:Fireasy.Data.Entity.EntityTreeUpdatingEventArgs`1"></see> 类的新实例。
        /// </summary>
        /// <param name="entity"></param>
        public EntityTreeUpdatingEventArgs(TEntity entity)
            : base (entity)
        {
        }

        /// <summary>
        /// 初始化 <see cref="T:Fireasy.Data.Entity.EntityTreeUpdatingEventArgs`1"></see> 类的新实例。
        /// </summary>
        /// <param name="e"></param>
        public EntityTreeUpdatingEventArgs(EntityTreeUpdatingEventArgs e)
            : base ((TEntity)e.Current)
        {
            OldValue = e.OldValue;
            NewValue = e.NewValue;
            Action = e.Action;
        }

        /// <summary>
        /// 获取当前更新的实体。
        /// </summary>
        public new TEntity Current { get; private set; }

        /// <summary>
        /// 使用一个参照的实体对象更新满足条件的一序列对象。
        /// </summary>
        /// <typeparam name="T">实体类型。</typeparam>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="entity">保存的参考对象。</param>
        /// <returns>影响的实体数。</returns>
        public int Update<T>(T entity, Expression<Func<T, bool>> predicate = null) where T : IEntity
        {
            if (predicate == null)
            {
                return -1;
            }

            var instanceName = Current.GetInstanceName();
            var environment = Current.GetEnvironment();
            var identification = ContextInstanceManager.TryGet(instanceName);
            var contextProvider = identification.GetProviderService<IContextProvider>();
            using var service = contextProvider.CreateContextService(new ContextServiceContext(identification));
            service.InitializeEnvironment(environment).InitializeInstanceName(instanceName);

            var queryProvider = new EntityQueryProvider(service);
            return new QuerySet<T>(new QueryProvider(queryProvider), null)
                .UpdateWhere(entity, predicate);
        }

        /// <summary>
        /// 将满足条件的一组对象从库中移除。
        /// </summary>
        /// <typeparam name="T">实体类型。</typeparam>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数。</param>
        /// <param name="fake">如果具有 IsDeletedKey 的属性，则提供对数据假删除的支持。</param>
        /// <returns>影响的实体数。</returns>
        public int Remove<T>(Expression<Func<T, bool>> predicate = null, bool fake = true) where T : IEntity
        {
            if (predicate == null)
            {
                return -1;
            }

            var instanceName = Current.GetInstanceName();
            var environment = Current.GetEnvironment();
            var identification = ContextInstanceManager.TryGet(instanceName);
            var contextProvider = identification.GetProviderService<IContextProvider>();
            using var service = contextProvider.CreateContextService(new ContextServiceContext(identification));
            service.InitializeEnvironment(environment).InitializeInstanceName(instanceName);

            var queryProvider = new EntityQueryProvider(service);
            return new QuerySet<T>(new QueryProvider(queryProvider), null)
                .RemoveWhere(predicate, fake);
        }
    }
}
