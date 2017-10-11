// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Configuration;
using Fireasy.Common.Linq.Expressions;
using Fireasy.Data.Entity.Linq.Translators;
using Fireasy.Data.Entity.Linq.Translators.Configuration;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;

namespace Fireasy.Data.Entity.Linq
{
    /// <summary>
    /// Expression 解析缓存。
    /// </summary>
    internal class TranslateCache
    {
        private static ConcurrentDictionary<string, CacheItem> cache = new ConcurrentDictionary<string, CacheItem>();

        /// <summary>
        /// 尝试获取指定表达式的缓存。
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="func">当缓存不存在时，创建缓存数据的函数。</param>
        /// <returns></returns>
        internal static Expression<Func<IDatabase, object>> TryGet(Expression expression, Func<Expression<Func<IDatabase, object>>> func)
        {
            if (!CachaebleChecker.Check(expression))
            {
                return func();
            }

            var section = ConfigurationUnity.GetSection<TranslatorConfigurationSection>();
            var option = section == null ? TranslateOptions.Default : section.Options;

            if (!option.ParseCacheEnabled)
            {
                return func();
            }

            ClearExpiredKeys();

            var lazy = new Lazy<CacheItem>(() => new CacheItem
                {
                    Expression = func(),
                    Expired = DateTime.Now.AddSeconds(option.ParseCacheExpired)
                });

            var cacheKey = GetKey(expression);
            var result = cache.GetOrAdd(cacheKey, k => lazy.Value);

            //在现有的表达式中查找 IDataSegment，去替换缓存中的 IDataSegment
            //原因是前端需要获得分页信息，如果不进行替换，将无法返回信息
            var segment = SegmentFinder.Find(expression);
            if (segment != null)
            {
                result.Expression = (Expression<Func<IDatabase, object>>)SegmentReplacer.Repalce(result.Expression, segment);
            }

            return result.Expression;
        }

        /// <summary>
        /// 清理过期缓存。
        /// </summary>
        private static void ClearExpiredKeys()
        {
            var expiredKeys = cache.Where(s => s.Value.Expired < DateTime.Now).Select(s => s.Key).ToList();
            expiredKeys.ForEach(s => cache.TryRemove(s, out CacheItem result));
        }

        /// <summary>
        /// 缓存项。
        /// </summary>
        private class CacheItem
        {
            /// <summary>
            /// 进行缓存的表达式。
            /// </summary>
            internal Expression<Func<IDatabase, object>> Expression { get; set; }

            /// <summary>
            /// 过期时间。
            /// </summary>
            internal DateTime Expired { get; set; }
        }

        /// <summary>
        /// 通过表达式计算出对应的缓存键。
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private static string GetKey(Expression expression)
        {
            var evalExp = PartialEvaluator.Eval(expression, TranslateProviderBase.EvaluatedLocallyFunc);
            var cacheKey = ExpressionWriter.WriteToString(evalExp);

            //使用md5进行hash编码
            var md5 = new MD5CryptoServiceProvider();
            byte[] data = md5.ComputeHash(Encoding.Unicode.GetBytes(cacheKey));
            return "$." + Convert.ToBase64String(data, Base64FormattingOptions.None);
        }

        /// <summary>
        /// 缓存检查器。
        /// </summary>
        private class CachaebleChecker : Common.Linq.Expressions.ExpressionVisitor
        {
            private bool cacheable = true;

            /// <summary>
            /// 检查表达式是否能够被缓存。
            /// </summary>
            /// <param name="expression"></param>
            /// <returns></returns>
            public static bool Check(Expression expression)
            {
                var checker = new CachaebleChecker();
                checker.Visit(expression);
                return checker.cacheable;
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                //增删改的操作，不能缓存
                switch (node.Method.Name)
                {
                    case "RemoveWhere":
                    case "UpdateWhere":
                    case "CreateEntity":
                    case "BatchOperate":
                    case "Insert":
                    case "Update":
                    case "Delete":
                        cacheable = false;
                        break;
                }

                return node;
            }
        }

        /// <summary>
        /// <see cref="IDataSegment"/> 查找器。
        /// </summary>
        private class SegmentFinder : Common.Linq.Expressions.ExpressionVisitor
        {
            private IDataSegment dataSegment;

            /// <summary>
        /// <see cref="IDataSegment"/> 查找器。
            /// </summary>
            /// <param name="expression"></param>
            /// <returns></returns>
            public static IDataSegment Find(Expression expression)
            {
                var replaer = new SegmentFinder();
                replaer.Visit(expression);
                return replaer.dataSegment;
            }

            protected override Expression VisitConstant(ConstantExpression constExp)
            {
                if (constExp.Value is IDataSegment)
                {
                    dataSegment = constExp.Value as IDataSegment;
                }

                return constExp;
            }
        }

        /// <summary>
        /// <see cref="IDataSegment"/> 替换器。
        /// </summary>
        private class SegmentReplacer : Common.Linq.Expressions.ExpressionVisitor
        {
            private IDataSegment dataSegment;

            /// <summary>
            /// 使用 <paramref name="segment"/> 替换表达式中的 <see cref="IDataSegment"/> 对象。
            /// </summary>
            /// <param name="expression"></param>
            /// <param name="segment"></param>
            /// <returns></returns>
            public static Expression Repalce(Expression expression, IDataSegment segment)
            {
                var replaer = new SegmentReplacer { dataSegment = segment };
                return replaer.Visit(expression);
            }

            protected override Expression VisitConstant(ConstantExpression constExp)
            {
                if (constExp.Value is IDataSegment)
                {
                    //如果不相同则用新的 IDataSegment 替换
                    if ((constExp.Value as IDataSegment) != dataSegment)
                    {
                        return Expression.Constant(dataSegment);
                    }
                }

                return constExp;
            }
        }

    }
}
