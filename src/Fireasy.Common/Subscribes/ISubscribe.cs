using System;
// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Collections.Generic;

namespace Fireasy.Common.Subscribe
{
    /// <summary>
    /// 提供订阅的主题接口。
    /// </summary>
    public interface ISubject
    {
        /// <summary>
        /// 初始化主题数据。
        /// </summary>
        /// <param name="arguments"></param>
        void Initialize(params object[] arguments);

        /// <summary>
        /// 获取或设置订阅者的过滤器。
        /// </summary>
        Func<ISubscriber, bool> Filter { get; set; }
    }

    /// <summary>
    /// 主题的订阅者接口。
    /// </summary>
    public interface ISubscriber
    {
        /// <summary>
        /// 接收主题信息。
        /// </summary>
        /// <param name="subject"></param>
        void Accept(ISubject subject);
    }
}
