// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using System.Linq;
using System.Reflection;

namespace Fireasy.Common.Subscribes
{
    public static class Extensions
    {
        /// <summary>
        /// 在指定的程序集中发现 <see cref="ISubscriber&lt;TSubject&gt;"/> 的实现。
        /// </summary>
        /// <typeparam name="TSubject"></typeparam>
        /// <param name="subscribeMgr"><see cref="ISubscribeManager"/> 实例。</param>
        /// <param name="assembly">指定的程序集。</param>
        /// <returns></returns>
        public static ISubscribeManager Discovery<TSubject>(this ISubscribeManager subscribeMgr, Assembly assembly) where TSubject : class
        {
            Guard.ArgumentNull(subscribeMgr, nameof(subscribeMgr));
            Guard.ArgumentNull(assembly, nameof(assembly));

            foreach (var type in assembly.GetExportedTypes().Where(s => !s.IsInterface && !s.IsAbstract && s.IsImplementInterface(typeof(ISubscriber<TSubject>))))
            {
                subscribeMgr.AddSubscriber<TSubject>(subject => type.New<ISubscriber<TSubject>>().Accept(subject));
            }

            return subscribeMgr;
        }

        /// <summary>
        /// 添加一个订阅者实例。
        /// </summary>
        /// <typeparam name="TSubject"></typeparam>
        /// <param name="subscribeMgr"><see cref="ISubscribeManager"/> 实例。</param>
        /// <param name="subscriber">主题订阅者。</param>
        /// <returns></returns>
        public static ISubscribeManager AddSubscriber<TSubject>(this ISubscribeManager subscribeMgr, ISubscriber<TSubject> subscriber) where TSubject : class
        {
            Guard.ArgumentNull(subscribeMgr, nameof(subscribeMgr));
            Guard.ArgumentNull(subscriber, nameof(subscriber));

            subscribeMgr.AddSubscriber<TSubject>(subject => subscriber.Accept(subject));
            return subscribeMgr;
        }
    }
}
