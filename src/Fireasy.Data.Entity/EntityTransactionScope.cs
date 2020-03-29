// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common;
using Fireasy.Common.ComponentModel;
using System.Collections.Concurrent;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 实体持久化工作区，为持久化对象提供环境事务的支持。无法继承此类。
    /// </summary>
    public class EntityTransactionScope : Scope<EntityTransactionScope>
    {
        private SafetyDictionary<string, IDatabase> databases = new SafetyDictionary<string, IDatabase>();
        private bool isDisposed;

        /// <summary>
        /// 初始化 <see cref="EntityTransactionScope"/> 类的新实例。
        /// </summary>
        /// <param name="option">选项。</param>
        public EntityTransactionScope(EntityTransactionScopeOption option = null)
        {
            var database = DatabaseFactory.GetDatabaseFromScope();
            if (database != null)
            {
                SetDatabase(DatabaseScope.Current.InstanceName, database);
                EntityDatabaseFactory.StartTransaction(database, option);
            }

            Option = option;
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
            if (Current != this || isDisposed)
            {
                return;
            }

            base.Dispose(true);

            foreach (var database in databases.Values)
            {
                if (database.CommitTransaction())
                {
                    database.Dispose();
                }
            }

            isDisposed = true;
        }

        /// <summary>
        /// 撤销当前工作区内数据的改动。
        /// </summary>
        protected virtual void Rollback()
        {
            if (Current != this || isDisposed)
            {
                return;
            }

            base.Dispose(true);

            foreach (var database in databases.Values)
            {
                if (database.RollbackTransaction())
                {
                    database.Dispose();
                }
            }

            isDisposed = true;
        }

        /// <summary>
        /// 设置当前持久化工作区内的 <see cref="IDatabase"/> 对象。
        /// </summary>
        /// <param name="instanceName"></param>
        /// <param name="database"></param>
        public virtual void SetDatabase(string instanceName, IDatabase database)
        {
            databases.TryAdd(GetDefaultInstanceName(instanceName), database);
        }

        /// <summary>
        /// 返回当前持久化工作区内的 <see cref="IDatabase"/> 对象。
        /// </summary>
        /// <param name="instanceName"></param>
        /// <returns></returns>
        public virtual IDatabase GetDatabase(string instanceName)
        {
            if (databases == null)
            {
                return null;
            }

            IDatabase database;
            databases.TryGetValue(GetDefaultInstanceName(instanceName), out database);
            return database;
        }

        protected string GetDefaultInstanceName(string instanceName)
        {
            return string.IsNullOrEmpty(instanceName) ? "$$default" : instanceName;
        }
    }
}
