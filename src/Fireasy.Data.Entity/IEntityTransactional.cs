﻿// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Data;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 定义对事务的支持。
    /// </summary>
    public interface IEntityTransactional
    {
        void BeginTransaction(IsolationLevel level);

        void CommitTransaction();

        void RollbackTransaction();
    }
}
