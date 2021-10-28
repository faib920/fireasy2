// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Data;
using System.Data.Common;

namespace Fireasy.Data
{
    /// <summary>
    /// 提供 <see cref="DbConnection"/> 状态的通知。
    /// </summary>
    public interface IDbConnectionStateChangeTracker
    {
        /// <summary>
        /// 通知 <see cref="DbConnection"/> 的状态已变更。
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="originalState">原来的状态。</param>
        /// <param name="currentState">当前的新状态。</param>
        void OnChange(DbConnection connection, ConnectionState originalState, ConnectionState currentState);
    }
}
