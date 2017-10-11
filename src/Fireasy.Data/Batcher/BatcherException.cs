// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections;
using System.Data;

namespace Fireasy.Data.Batcher
{
    /// <summary>
    /// 批处理在执行过程中发生异常。无法继承此类。
    /// </summary>
    public sealed class BatcherException : Exception
    {
        /// <summary>
        /// 初始化 <see cref="BatcherException"/> 类的新实例。
        /// </summary>
        /// <param name="collection">批量插入的数据列表。</param>
        /// <param name="exception">内部的异常对象。</param>
        public BatcherException(ICollection collection, Exception exception)
            : base(SR.GetString(SRKind.BatcherException, exception.Message), exception)
        {
            Collection = collection;
        }

        /// <summary>
        /// 获取批量插入的数据表。
        /// </summary>
        public ICollection Collection { get; private set; }
    }
}
