// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common;
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Extensions;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 实体持久化工作区，为持久化对象提供环境事务的支持。无法继承此类。
    /// </summary>
    public class EntityTransactionScope : Scope<EntityTransactionScope>
    {
        private readonly SafetyDictionary<string, IEntityTransactional> _transactions = new SafetyDictionary<string, IEntityTransactional>();
        private bool _isDisposed;

        /// <summary>
        /// 初始化 <see cref="EntityTransactionScope"/> 类的新实例。
        /// </summary>
        /// <param name="option">选项。</param>
        public EntityTransactionScope(EntityTransactionScopeOption option = null)
        {
            Option = option ?? new EntityTransactionScopeOption { IsolationLevel = System.Data.IsolationLevel.ReadCommitted };
        }

        /// <summary>
        /// 获取或设置持久化工作区的选项。
        /// </summary>
        public EntityTransactionScopeOption Option { get; set; }

        /// <summary>
        /// 释放对象所占用的非托管和托管资源。
        /// </summary>
        /// <param name="disposing">为 true 则释放托管资源和非托管资源；为 false 则仅释放非托管资源。</param>
        protected override bool Dispose(bool disposing)
        {
            Rollback();

            return base.Dispose(disposing);
        }

        /// <summary>
        /// 提交当前工作区内数据的改动。
        /// </summary>
        public virtual void Complete()
        {
            if (Current != this || _isDisposed)
            {
                return;
            }

            base.Dispose(true);

            foreach (var transaction in _transactions.Values)
            {
                transaction.CommitTransaction();
                transaction.TryDispose(true);
            }

            _isDisposed = true;
        }

        /// <summary>
        /// 撤销当前工作区内数据的改动。
        /// </summary>
        protected virtual void Rollback()
        {
            if (Current != this || _isDisposed)
            {
                return;
            }

            base.Dispose(true);

            foreach (var transaction in _transactions.Values)
            {
                transaction.RollbackTransaction();
                transaction.TryDispose(true);
            }

            _isDisposed = true;
        }

        /// <summary>
        /// 设置当前持久化工作区内的 <see cref="IDatabase"/> 对象。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="transaction"></param>
        public virtual void Addransaction(string key, IEntityTransactional transaction)
        {
            _transactions.TryAdd(key, transaction);
        }

        /// <summary>
        /// 返回当前持久化工作区内的 <see cref="IDatabase"/> 对象。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual IEntityTransactional GetDatabase(string key)
        {
            if (_transactions == null)
            {
                return null;
            }

            _transactions.TryGetValue(key, out IEntityTransactional transaction);
            return transaction;
        }

        /// <summary>
        /// 获取是否在事务范围之内。
        /// </summary>
        /// <returns></returns>
        public static bool IsInTransaction()
        {
            return Current != null;
        }
    }
}
