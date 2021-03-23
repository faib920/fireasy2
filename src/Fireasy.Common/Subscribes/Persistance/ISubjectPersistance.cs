// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Common.Subscribes.Persistance
{
    /// <summary>
    /// 为订阅主题提供持久化的方法。
    /// </summary>
    public interface ISubjectPersistance
    {
        /// <summary>
        /// 读取主题对象。
        /// </summary>
        /// <param name="provider">提供者，用于存储中的隔离。</param>
        /// <param name="readAndAccept">读取并接受的方法。读取后如果返回 true，那么将取消该主题的持久化。</param>
        void ReadSubjects(string provider, Func<StoredSubject, SubjectRetryStatus> readAndAccept);

        /// <summary>
        /// 存储主题对象。
        /// </summary>
        /// <param name="provider">提供者，用于存储中的隔离。</param>
        /// <param name="subject"></param>
        /// <returns></returns>
        bool SaveSubject(string provider, StoredSubject subject);
    }
}
