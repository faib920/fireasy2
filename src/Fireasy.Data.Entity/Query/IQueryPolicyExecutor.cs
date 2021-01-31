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

namespace Fireasy.Data.Entity.Query
{
    /// <summary>
    /// 查询的辅助策略的执行者。
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IQueryPolicyExecutor
    {
        void IncludeWith<TEntity, TProperty>(Expression<Func<TEntity, TProperty>> fnMember) where TEntity : IEntity;

        void AssociateWith<TEntity>(Expression<Func<TEntity, IEnumerable>> memberQuery) where TEntity : IEntity;
    }
}
