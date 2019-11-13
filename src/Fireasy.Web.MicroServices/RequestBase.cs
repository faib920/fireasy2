// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Web.MicroServices
{
    public interface IClientRequst
    {
    }

    public interface IClientRequst<T> : IClientRequst
    {

    }

    /// <summary>
    /// 服务请求参数，抽象类。
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public class RequestBase<TRequest, TResult> : IClientRequst<TResult>
    {
        public RequestBase(TRequest body)
        {
            Body = body;
        }

        public TRequest Body { get; set; }
    }
}
