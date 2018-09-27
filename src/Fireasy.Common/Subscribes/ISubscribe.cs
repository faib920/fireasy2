// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Common.Subscribes
{
    /// <summary>
    /// 订阅管理器接口。
    /// </summary>
    public interface ISubscribeManager
    {
        /// <summary>
        /// 向管理器发送主题。
        /// </summary>
        /// <typeparam name="TSubject"></typeparam>
        /// <param name="subject">主题内容。</param>
        void Publish<TSubject>(TSubject subject) where TSubject : class;

        /// <summary>
        /// 添加一个订阅方法。
        /// </summary>
        /// <typeparam name="TSubject"></typeparam>
        /// <param name="subscribe">读取主题的方法。</param>
        void AddSubscriber<TSubject>(Action<TSubject> subscribe) where TSubject : class;

        /// <summary>
        /// 添加一个订阅方法。
        /// </summary>
        /// <param name="subjectType">主题的类型。</param>
        /// <param name="subscriber">读取主题的方法。</param>
        void AddSubscriber(Type subjectType, Delegate subscriber);
    }
}
