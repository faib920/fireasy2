// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Common.Subscribes
{
    /// <summary>
    /// <typeparamref name="TSubject"/> 的订阅者。
    /// </summary>
    /// <typeparam name="TSubject"></typeparam>
    public interface ISubscriber<TSubject> where TSubject : class
    {
        /// <summary>
        /// 接收 <typeparamref name="TSubject"/> 实例。
        /// </summary>
        /// <param name="subject"></param>
        void Accept(TSubject subject);
    }
}
