// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Web.MicroServices
{
    public interface IClientResponse
    {
    }

    public interface IClientResponse<T>
    {
        T Data { get; set; }
    }

    public class ResponseBase : IClientResponse
    {
        public bool Succeed { get; set; }

        public string ErrorCode { get; set; }

        public string ErrorMessage { get; set; }
    }

    public class ResponseBase<T> : ResponseBase, IClientResponse<T>
    {
        public T Data { get; set; }
    }
}
