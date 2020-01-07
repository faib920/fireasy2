// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
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

    public abstract class BaseClientProxy : IClientProxy
    {
        public DateTime AliveTime { get; set; }

        public abstract Task SendAsync(string method, params object[] arguments);
    }

    /// <summary>
    /// 枚举器，表示多个连接代理。
    /// </summary>
    public class EnumerableClientProxy : BaseClientProxy
    {
        private Func<IEnumerable<IClientProxy>> proxyFactory;

        public EnumerableClientProxy(Func<IEnumerable<IClientProxy>> proxyFactory)
        {
            this.proxyFactory = proxyFactory;
        }

        /// <summary>
        ///  发送消息。
        /// </summary>
        /// <param name="method"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public override Task SendAsync(string method, params object[] arguments)
        {
            foreach (var proxy in proxyFactory())
            {
                proxy.SendAsync(method, arguments);
            }

#if NETSTANDARD
            return Task.CompletedTask;
#else
            return new Task(null);
#endif
        }
    }

    /// <summary>
    /// 表示不做任何处理的连接代理。
    /// </summary>
    public class NullClientProxy : BaseClientProxy
    {
        public static NullClientProxy Instance = new NullClientProxy();

        /// <summary>
        ///  发送消息。
        /// </summary>
        /// <param name="method"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public override Task SendAsync(string method, params object[] arguments)
        {
#if NETSTANDARD
            return Task.CompletedTask;
#else
            return new Task(null);
#endif
        }
    }
}
