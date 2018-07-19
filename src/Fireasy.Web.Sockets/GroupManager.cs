// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Fireasy.Web.Sockets
{
    /// <summary>
    /// 分组管理器。
    /// </summary>
    public class GroupManager
    {
        private ClientManager clientManager;

        internal static GroupManager GetManager(ClientManager clientManager)
        {
            return new GroupManager { clientManager = clientManager };
        }

        /// <summary>
        /// 将客户端连接标识添加到指定的组中。
        /// </summary>
        /// <param name="connectionId">客户端连接标识。></param>
        /// <param name="groupName">组的名称。</param>
        public void Add(string connectionId, string groupName)
        {
            clientManager.AddToGroup(connectionId, groupName);
        }

        /// <summary>
        /// 将客户端连接标识从组中移除。
        /// </summary>
        /// <param name="connectionId">客户端连接标识。></param>
        /// <param name="groupName">组的名称。</param>
        public void Remove(string connectionId, string groupName)
        {
            clientManager.RemoveFromGroup(connectionId, groupName);
        }
    }
}
