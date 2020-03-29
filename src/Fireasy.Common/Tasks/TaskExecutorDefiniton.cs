// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Common.Tasks
{
    public sealed class TaskExecutorDefiniton
    {
        /// <summary>
        /// 获取或设置延迟时间，默认为 0 毫秒。
        /// </summary>
        public TimeSpan Delay { get; set; }

        /// <summary>
        /// 获取或设置执行触发间隔时间，默认为 60000 毫秒。
        /// </summary>
        public TimeSpan Period { get; set; }

        /// <summary>
        /// 获取或设置执行器的类型。
        /// </summary>
        public Type ExecutorType { get; set; }
    }
}
