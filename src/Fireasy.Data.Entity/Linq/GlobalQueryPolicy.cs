// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq
{
    /// <summary>
    /// 全局查询策略管理。
    /// </summary>
    public class GlobalQueryPolicy
    {
        /// <summary>
        /// 缺省的策略会话。
        /// </summary>
        public static readonly GlobalQueryPolicySession Default = new GlobalQueryPolicySession();

        private static SafetyDictionary<string, GlobalQueryPolicySession> sessions = new SafetyDictionary<string, GlobalQueryPolicySession>();
        private static SafetyDictionary<string, GlobalQueryPolicySession> caches = new SafetyDictionary<string, GlobalQueryPolicySession>();

        /// <summary>
        /// 多用户环境下创建一个会话。
        /// </summary>
        /// <param name="sessionIdFactory">会话ID的函数。比如使用 System.Web.HttpContext.Current.Session.SessionID。</param>
        /// <param name="ident">用于统一管理的标识。当缓存中有该标识时，不会再创建新的会话。</param>
        /// <returns></returns>
        public static GlobalQueryPolicySession Create(Func<string> sessionIdFactory, string ident = null)
        {
            Guard.ArgumentNull(sessionIdFactory, nameof(sessionIdFactory));

            var sessionId = sessionIdFactory();
            var lazy = new Lazy<GlobalQueryPolicySession>(() => new GlobalQueryPolicySession() { SessionIdFactory = sessionIdFactory });
            return sessions.GetOrAdd(sessionId, () => string.IsNullOrEmpty(ident) ? lazy.Value : caches.GetOrAdd(ident, () => lazy.Value));
        }

        /// <summary>
        /// 移除会话。
        /// </summary>
        /// <param name="sessionId"></param>
        public static void Remove(string sessionId)
        {
            if (sessions.TryRemove(sessionId, out GlobalQueryPolicySession session))
            {
                session.Clear();
            }
        }

        /// <summary>
        /// 获取指定类型的所有查询策略，包括匿名策略。
        /// </summary>
        /// <param name="objType"></param>
        /// <returns></returns>
        public static IEnumerable<LambdaExpression> GetPolicies(Type objType)
        {
            var result = Default.GetPolicies(objType);
            if (sessions.Count > 0)
            {
                var sresult = sessions.Where(s => s.Value != null && s.Key == s.Value.SessionIdFactory())
                    .SelectMany(s => s.Value.GetPolicies(objType));

                result = sresult.Union(sresult);
            }

            return result;
        }
    }

    /// <summary>
    /// 全局查询策略会话。
    /// </summary>
    public class GlobalQueryPolicySession
    {
        private Dictionary<Type, List<LambdaExpression>> dic = new Dictionary<Type, List<LambdaExpression>>();
        
        /// <summary>
        /// 获取或设置会话ID的工厂。比如使用 System.Web.HttpContext.Current.Session.SessionID。
        /// </summary>
        public Func<string> SessionIdFactory { get; set; }

        /// <summary>
        /// 为类型 <typeparamref name="T"/> 注册一个查询策略。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        public GlobalQueryPolicySession Register<T>(Expression<Func<T, bool>> predicate)
        {
            Register(typeof(T), predicate);

            return this;
        }

        /// <summary>
        /// 为类型 <paramref name="objType"/> 注册一个查询策略。
        /// </summary>
        /// <param name="objType"></param>
        /// <param name="predicate"></param>
        public GlobalQueryPolicySession Register(Type objType, LambdaExpression predicate)
        {
            var expressions = dic.TryGetValue(objType, () => new List<LambdaExpression>());
            expressions.Add(predicate);

            return this;
        }

        /// <summary>
        /// 注册一个匿名的查询策略。可以应用于任何实体类，只需要属性满足 key。
        /// </summary>
        /// <param name="predicate"></param>
        public GlobalQueryPolicySession Register(Expression<Func<AnonymousMember, bool>> predicate)
        {
            Register(typeof(AnonymousMember), predicate);

            return this;
        }

        /// <summary>
        /// 获取注册的策略数目。
        /// </summary>
        public int Count
        {
            get { return dic.Count; }
        }

        /// <summary>
        /// 清除所有策略。
        /// </summary>
        public void Clear()
        {
            dic.Clear();
        }

        /// <summary>
        /// 获取指定类型的所有查询策略，包括匿名策略。
        /// </summary>
        /// <param name="objType"></param>
        /// <returns></returns>
        public IEnumerable<LambdaExpression> GetPolicies(Type objType)
        {
            return dic.Where(s => s.Key.IsAssignableFrom(objType) || s.Key == typeof(AnonymousMember)).SelectMany(s => s.Value);
        }
    }

    public struct AnonymousMember
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object this[string key]
        {
            get { return 0; }
        }
    }
}
