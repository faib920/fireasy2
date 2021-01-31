// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Web.Sockets
{
    /// <summary>
    /// 客户端管理器接口。
    /// </summary>
    public interface IClientManager
    {
        /// <summary>
        /// 初始化管理器。
        /// </summary>
        /// <param name="acceptContext"></param>
        void Initialize(WebSocketAcceptContext acceptContext);

        /// <summary>
        /// 将一个代理添加到管理器。
        /// </summary>
        /// <param name="connectionId">连接ID。</param>
        /// <param name="clientProxy"></param>
        void Add(string connectionId, IClientProxy clientProxy);

        /// <summary>
        /// 将一个客户端添加到指定的组。
        /// </summary>
        /// <param name="connectionId">连接ID。</param>
        /// <param name="groupName">组的名称。</param>
        void AddToGroup(string connectionId, string groupName);

        /// <summary>
        /// 移除客户端。
        /// </summary>
        /// <param name="connectionId">连接ID。</param>
        void Remove(string connectionId);

        /// <summary>
        /// 将客户端从组内移除。
        /// </summary>
        /// <param name="connectionId">连接ID。</param>
        /// <param name="groupName">组的名称。</param>
        void RemoveFromGroup(string connectionId, string groupName);

        /// <summary>
        /// 刷新客户端连接状态。
        /// </summary>
        /// <param name="connectionId"></param>
        void Refresh(string connectionId);

        /// <summary>
        /// 获取指定客户端连接标识的代理。
        /// </summary>
        /// <param name="connectionId">连接ID。</param>
        /// <returns></returns>
        IClientProxy Client(string connectionId);

        /// <summary>
        /// 获取指定的多个客户端连接标识的代理。
        /// </summary>
        /// <param name="connectionIds">一组连接ID。</param>
        /// <returns></returns>
        IClientProxy Clients(params string[] connectionIds);

        /// <summary>
        /// 获取所有客户端代理。
        /// </summary>
        IClientProxy All { get; }

        /// <summary>
        /// 获取其他客户端代理。
        /// </summary>
        IClientProxy Other { get; }

        /// <summary>
        /// 获取指定分组的所有客户端代理。
        /// </summary>
        /// <param name="groupName">组的名称。</param>
        /// <returns></returns>
        IClientProxy Group(string groupName);
    }
}
