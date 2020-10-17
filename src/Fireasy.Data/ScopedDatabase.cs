// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Provider;
using System.Collections.Generic;

namespace Fireasy.Data
{
    /// <summary>
    /// 表示使用 <see cref="DatabaseScope"/> 在同一线程内共享  <see cref="IDatabase"/> 实例。无法继承此类。
    /// </summary>
    public sealed class ScopedDatabase : Database
    {
        private DatabaseScope _dbScope;
        private bool _isScoped;

        /// <summary>
        /// 初始化 <see cref="Database"/> 类的新实例。
        /// </summary>
        /// <param name="provider">数据库提供者。</param>
        public ScopedDatabase(IProvider provider)
            : base(provider)
        {
            InitDatabaseScope();
        }

        /// <summary>
        /// 初始化 <see cref="Database"/> 类的新实例。
        /// </summary>
        /// <param name="connectionString">数据库连接字符串。</param>
        /// <param name="provider">数据库提供者。</param>
        public ScopedDatabase(ConnectionString connectionString, IProvider provider)
            : base(connectionString, provider)
        {
            InitDatabaseScope();
        }

        /// <summary>
        /// 初始化 <see cref="Database"/> 类的新实例。
        /// </summary>
        /// <param name="connectionStrings">数据库连接字符串组。</param>
        /// <param name="provider">数据库提供者。</param>
        public ScopedDatabase(List<DistributedConnectionString> connectionStrings, IProvider provider)
            : base(connectionStrings, provider)
        {
            InitDatabaseScope();
        }

        private void InitDatabaseScope()
        {
            if (DatabaseScope.Current == null)
            {
                _isScoped = true;
                _dbScope = new DatabaseScope(this);
            }
            else
            {
                _dbScope = DatabaseScope.Current;
            }
        }

        protected override bool Dispose(bool disposing)
        {
            if (_isScoped && _dbScope != null)
            {
                _dbScope.Dispose();
                _dbScope = null;
            }

            return base.Dispose(disposing);
        }
    }
}
