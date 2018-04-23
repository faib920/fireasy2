// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Caching;
using Fireasy.Common.Configuration;
using Fireasy.Common.Linq.Expressions;
using Fireasy.Data.Entity.Linq.Translators;
using Fireasy.Data.Entity.Linq.Translators.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;

namespace Fireasy.Data.Entity.Linq
{
    /// <summary>
    /// 执行缓存管理器。
    /// </summary>
    internal class ExecuteCache
    {
        //缓存着每个实体类型所产生的相关联的缓存键名
        private static ConcurrentDictionary<Type, List<string>> referKeys = new ConcurrentDictionary<Type, List<string>>();

        /// <summary>
        /// 判断是否被缓存。
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        internal static bool CanCache(Expression expression)
        {
            return !typeof(IQueryable).IsAssignableFrom(expression.Type);
        }

        /// <summary>
        /// 尝试通过表达式获取执行后的结果缓存。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        internal static T TryGet<T>(Expression expression, Func<T> func)
        {
            var section = ConfigurationUnity.GetSection<TranslatorConfigurationSection>();
            var option = section == null ? TranslateOptions.Default : section.Options;

            //没有开启数据缓存
            if (!option.DataCacheEnabled)
            {
                return func();
            }

            var cacheMgr = CacheManagerFactory.CreateManager();
            if (cacheMgr == null)
            {
                return func();
            }

            var cacheKey = GetKey(expression);

            Reference(cacheKey, expression);

            var segment = SegmentFinder.Find(expression);
            var pager = segment as DataPager;

            var cacheFunc = new Func<CacheItem<T>>(() =>
                {
                    var data = func();
                    var total = 0;
                    if (pager != null)
                    {
                        total = pager.RecordCount;
                    }

                    return new CacheItem<T> { Data = data, Total = total };
                });

            var cacheItem = cacheMgr.TryGet(cacheKey, cacheFunc, () => new RelativeTime(TimeSpan.FromSeconds(option.DataCacheExpired)));

            if (pager != null)
            {
                pager.RecordCount = cacheItem.Total;
            }

            return cacheItem.Data;
        }

        /// <summary>
        /// 使相关的缓存过期。
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        internal static bool TryExpire(Expression expression)
        {
            //查找是不是属于操作(新增，修改，删除)表达式，如果是，则需要清除关联缓存键
            var operateType = OperateFinder.Find(expression);
            if (operateType != null)
            {
                ClearKeys(operateType);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 数组缓存项。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class CacheItem<T>
        {
            /// <summary>
            /// 缓存的数据。
            /// </summary>
            public T Data { get; set; }

            /// <summary>
            /// 数据的总记录数。
            /// </summary>
            public int Total { get; set; }
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
        /// 找出表达式中相关联的实体类型，进行关系维护
        /// </summary>
        /// <param name="key">缓存键。</param>
        /// <param name="expression"></param>
        private static void Reference(string key, Expression expression)
        {
            var types = RelationshipFinder.Find(expression);

            foreach (var type in types)
            {
                if (referKeys.TryGetValue(type, out List<string> list))
                {
                    if (!list.Contains(key))
                    {
                        list.Add(key);
                    }
                }
                else
                {
                    var lazy = new Lazy<List<string>>(() => new List<string> { key });
                    referKeys.GetOrAdd(type, k => lazy.Value);
                }
            }
        }

        /// <summary>
        /// 清理实体类型的全部缓存键。
        /// </summary>
        /// <param name="type"></param>
        private static void ClearKeys(Type type)
        {
            if (referKeys.TryRemove(type, out List<string> list))
            {
                var cacheMgr = CacheManagerFactory.CreateManager();
                list.ForEach(s => cacheMgr.Remove(s));
            }
        }

        /// <summary>
        /// <see cref="IDataSegment"/> 查找器。
        /// </summary>
        private class SegmentFinder : Common.Linq.Expressions.ExpressionVisitor
        {
            private IDataSegment dataSegment;

            /// <summary>
            /// 查找表达式中的 <see cref="IDataSegment"/> 对象。
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
        /// 操作(新增，删除，修改)查找器。
        /// </summary>
        private class OperateFinder : Common.Linq.Expressions.ExpressionVisitor
        {
            private Type operateType;

            /// <summary>
            /// 检查表达式，查找操作的实体类型。
            /// </summary>
            /// <param name="expression"></param>
            /// <returns></returns>
            internal static Type Find(Expression expression)
            {
                var checker = new OperateFinder();
                checker.Visit(expression);
                return checker.operateType;
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                //增删改的操作
                switch (node.Method.Name)
                {
                    case nameof(Extensions.RemoveWhere):
                    case nameof(Extensions.UpdateWhere):
                    case nameof(Extensions.CreateEntity):
                    case nameof(Extensions.BatchOperate):
                    case nameof(IRepository.Insert):
                    case nameof(IRepository.Update):
                    case nameof(IRepository.Delete):
                        VisitExpressionList(node.Arguments);

                        break;
                }

                return node;
            }

            protected override Expression VisitConstant(ConstantExpression constExp)
            {
                if (typeof(IQueryable).IsAssignableFrom(constExp.Type))
                {
                    var elementType = constExp.Type.GetGenericArguments()[0];
                    if (typeof(IEntity).IsAssignableFrom(elementType))
                    {
                        operateType = elementType;
                    }
                }

                return constExp;
            }
        }

        /// <summary>
        /// 关联关系查找器。
        /// </summary>
        private class RelationshipFinder : Common.Linq.Expressions.ExpressionVisitor
        {
            private List<Type> types = new List<Type>();

            /// <summary>
            /// 在表达式中查找关联查询的实体类型。
            /// </summary>
            /// <param name="expression"></param>
            /// <returns></returns>
            internal static List<Type> Find(Expression expression)
            {
                var finder = new RelationshipFinder();
                finder.Visit(expression);
                return finder.types;
            }

            protected override Expression VisitMember(MemberExpression memberExp)
            {
                if (typeof(IEntity).IsAssignableFrom(memberExp.Member.DeclaringType))
                {
                    types.Add(memberExp.Member.DeclaringType);
                }

                return memberExp;
            }

            protected override Expression VisitConstant(ConstantExpression constExp)
            {
                if (typeof(IQueryable).IsAssignableFrom(constExp.Type))
                {
                    var elementType = constExp.Type.GetGenericArguments()[0];
                    if (typeof(IEntity).IsAssignableFrom(elementType))
                    {
                        types.Add(elementType);
                    }
                }

                return constExp;
            }
        }
    }
}
