// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Linq.Expressions;
using Fireasy.Data.Entity.Linq.Translators;
using System;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;

namespace Fireasy.Data.Entity.Linq
{
    /// <summary>
    /// 表达式 Key 生成器。
    /// </summary>
    public static class ExpressionKeyGenerator
    {
        /// <summary>
        /// 通过表达式计算出对应的缓存键。
        /// </summary>
        /// <param name="expression">作为 Key 的 Lambda 表达式。</param>
        /// <param name="prefix">用于区分缓存的前缀。</param>
        /// <returns></returns>
        public static string GetKey(Expression expression, string prefix)
        {
            var evalExp = PartialEvaluator.Eval(expression, TranslateProviderBase.EvaluatedLocallyFunc);
            var cacheKey = ExpressionWriter.WriteToString(evalExp);

            //使用md5进行hash编码
            var md5 = new MD5CryptoServiceProvider();
            byte[] data = md5.ComputeHash(Encoding.Unicode.GetBytes(cacheKey));
            return string.Concat("$.", prefix, "_", Convert.ToBase64String(data, Base64FormattingOptions.None));
        }
    }
}
