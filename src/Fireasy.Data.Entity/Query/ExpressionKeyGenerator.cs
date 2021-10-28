// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Linq.Expressions;
using Fireasy.Common.Security;
using Fireasy.Data.Entity.Linq.Translators;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Query
{
    /// <summary>
    /// Lambda 表达式 Key 生成器。
    /// </summary>
    public class ExpressionKeyGenerator : IExecuteCacheKeyGenerator, IQueryCacheKeyGenerator
    {
        public readonly static ExpressionKeyGenerator Instance = new ExpressionKeyGenerator();

        /// <summary>
        /// 通过表达式计算出对应的缓存键。
        /// </summary>
        /// <param name="expression">作为 Key 的 Lambda 表达式。</param>
        /// <param name="prefix">用于区分缓存的前缀。</param>
        /// <returns></returns>
        public string Generate(Expression expression, params string[] prefix)
        {
            var evalExp = PartialEvaluator.Eval(expression, TranslateProviderBase.EvaluatedLocallyFunc);
            var cacheKey = ExpressionWriter.WriteToString(evalExp);

            return XxHashUnsafe.ComputeHash(cacheKey).ToString("X");
        }
    }
}
