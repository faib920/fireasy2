// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;
using System.Data.Common;

namespace Fireasy.Data
{
    /// <summary>
    /// 线程范围内的数据库事务。无法继承此类。
    /// </summary>
    public sealed class DbTransactionScope : Scope<DbTransactionScope>
    {
        /// <summary>
        /// 初始化 <see cref="DbTransactionScope"/> 类的新实例。
        /// </summary>
        /// <param name="connectionString">数据库连接字符串。</param>
        /// <param name="dbTransaction">数据库事务实例。</param>
        public DbTransactionScope(ConnectionString connectionString, DbTransaction dbTransaction)
            : base (false)
        {
            ConnectionString = connectionString;
            DbTransaction = dbTransaction;
        }

        /// <summary>
        /// 获取数据库连接串。
        /// </summary>
        public ConnectionString ConnectionString { get; }

        /// <summary>
        /// 获取当前的 <see cref="DbTransaction"/> 实例。
        /// </summary>
        public DbTransaction DbTransaction { get; }

        /// <summary>
        /// 检查数据库连接串是否以当前匹配，如果是，则返回当前的事务。
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public DbTransaction GetCurrentTransaction(ConnectionString connectionString)
        {
            return Match(s => s.ConnectionString == connectionString)?.DbTransaction;
        }
    }
}
