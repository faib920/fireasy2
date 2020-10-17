// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Query
{
    /// <summary>
    /// 执行缓存键的生成器。
    /// </summary>
    public interface IExecuteCacheKeyGenerator
    {
        /// <summary>
        /// 生成 Lambda 表达式的缓存键。
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="prefix">前缀。</param>
        /// <returns></returns>
        string Generate(Expression expression, params string[] prefix);
    }
}
