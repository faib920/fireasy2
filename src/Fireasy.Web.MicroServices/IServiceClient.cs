// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Threading.Tasks;

namespace Fireasy.Web.MicroServices
{
    /// <summary>
    /// 定义服务的客户端。
    /// </summary>
    public interface IServiceClient
    {
        /// <summary>
        /// 执行请求，返回响应信息。
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<T> ExecuteAsync<T>(IClientRequst<T> request) where T : ResponseBase;
    }
}
