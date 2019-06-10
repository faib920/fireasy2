// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Threading.Tasks;

namespace Fireasy.Web.Sockets
{
    /// <summary>
    /// 客户端代理。
    /// </summary>
    public interface IClientProxy
    {
        /// <summary>
        /// 发送消息。
        /// </summary>
        /// <param name="method">消息方法。</param>
        /// <param name="arguments">方法的参数。</param>
        /// <returns></returns>
        Task SendAsync(string method, params object[] arguments);

        /// <summary>
        /// 获取存活检测的时间。
        /// </summary>
        DateTime AliveTime { get; }
    }

    internal abstract class InternalClientProxy : IClientProxy
    {
        public DateTime AliveTime { get; set; }

        public abstract Task SendAsync(string method, params object[] arguments);
    }
}
