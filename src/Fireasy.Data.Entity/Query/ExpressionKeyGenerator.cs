// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Common.Linq.Expressions;
using Fireasy.Data.Entity.Linq.Translators;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;

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

            //使用md5进行hash编码
            var md5 = new MD5CryptoServiceProvider();
            var data = md5.ComputeHash(Encoding.Unicode.GetBytes(cacheKey));

            var sb = new StringBuilder();
            foreach (var p in prefix)
            {
                if (!string.IsNullOrEmpty(p))
                {
                    sb.AppendFormat("{0}:", p);
                }
            }

            sb.Append(data.ToHex(true));

            return sb.ToString();
        }
    }
}
