// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Threading.Tasks;

namespace Fireasy.Web.MicroServices
{
    /// <summary>
    /// 定义服务请求的处理接口。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRequestHandler
    {
        /// <summary>
        /// 执行请求，返回响应信息。
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<IClientResponse> ProcessAsync(IClientRequst request);
    }

    public abstract class RequestHandlerBase<TRequest, TResponse> : IRequestHandler 
    {
        public abstract Task<TResponse> ProcessAsync(TRequest request);

        async Task<IClientResponse> IRequestHandler.ProcessAsync(IClientRequst request)
        {
            try
            {
                var result = await ProcessAsync((TRequest)request);
                return new ResponseBase<TResponse> { Succeed = true, Data = result };
            }
            catch (Exception exp)
            {
                return new ResponseBase<TResponse> { ErrorMessage = exp.Message };
            }
        }
    }
}
