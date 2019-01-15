using System;
using System.Threading.Tasks;

namespace Fireasy.Web.Sockets
{
    /// <summary>
    /// 消息处理器。
    /// </summary>
    public interface ILifetimeManager
    {
        /// <summary>
        /// 发送消息。
        /// </summary>
        /// <param name="method">消息方法。</param>
        /// <param name="arguments">方法的参数。</param>
        /// <returns></returns>
        Task SendAsync(InvokeMessage message);

        /// <summary>
        /// 接收消息。
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        Task LisitenAsync();

        void AddUser();

        Task SendToUser(InvokeMessage message);

        Task SendToGroup(InvokeMessage message);

        Task SendToUsers(InvokeMessage message);
    }
}
