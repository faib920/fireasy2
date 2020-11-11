// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace Fireasy.Common.Tasks
{
    /// <summary>
    /// 启动选项。
    /// </summary>
    public class StartOptions
    {
        /// <summary>
        /// 初始化 <see cref="StartOptions"/> 类的新实例。
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="period"></param>
        public StartOptions(TimeSpan delay, TimeSpan period)
        {
            Delay = delay;
            Period = period;
            Arguments = new Dictionary<string, object>();
        }

        /// <summary>
        /// 获取或设置延迟时间。
        /// </summary>
        public TimeSpan Delay { get; set; }

        /// <summary>
        /// 获取或设置执行触发间隔时间。
        /// </summary>
        public TimeSpan Period { get; set; }

        /// <summary>
        /// 获取或设置执行参数。
        /// </summary>
        public IDictionary<string, object> Arguments { get; set; }
    }

    /// <summary>
    /// 启动选项。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class StartOptions<T> : StartOptions
    {
        /// <summary>
        /// 初始化 <see cref="StartOptions{T}"/> 类的新实例。
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="period"></param>
        public StartOptions(TimeSpan delay, TimeSpan period)
            : base(delay, period)
        {
        }

        /// <summary>
        /// 获取或设置初始化器。
        /// </summary>
        public Action<T> Initializer { get; set; }
    }
}
