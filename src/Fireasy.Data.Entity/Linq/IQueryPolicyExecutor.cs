// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq
{
    /// <summary>
    /// 查询的辅助策略的执行者。
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IQueryPolicyExecutor<TEntity> where TEntity : IEntity
    {
        void IncludeWith(Expression<Func<TEntity, object>> fnMember);

        void AssociateWith(Expression<Func<TEntity, IEnumerable>> memberQuery);
    }
}
