using System;
using System.Net;
using System.IO;
using Aliyun.MQ.Runtime.Internal.Util;
using Aliyun.MQ.Util;

namespace Aliyun.MQ.Runtime.Internal.Transform
{
    public abstract class ResponseUnmarshaller : IResponseUnmarshaller<WebServiceResponse, UnmarshallerContext>
    {
        public virtual UnmarshallerContext CreateContext(IWebResponseData response, Stream stream)
        {
            if (response == null)
            {
                throw new WebException("The Web Response for a successful request is null!");
            }

            return ConstructUnmarshallerContext(stream, response);
        }

        internal virtual bool HasStreamingProperty
        {
            get { return false; }
        }

        #region IResponseUnmarshaller<WebServiceResponse,UnmarshallerContext> Members

        public virtual AliyunServiceException UnmarshallException(UnmarshallerContext input, Exception innerException, HttpStatusCode statusCode)
        {
            throw new NotImplementedException();
        }

        #endregion

        public WebServiceResponse UnmarshallResponse(UnmarshallerContext context)
        {
            var response = this.Unmarshall(context);
            response.ContentLength = context.ResponseData.ContentLength;
            response.HttpStatusCode = context.ResponseData.StatusCode;
            return response;
        }

        #region IUnmarshaller<WebServiceResponse,UnmarshallerContext> Members

        public abstract WebServiceResponse Unmarshall(UnmarshallerContext input);

        #endregion

        protected abstract UnmarshallerContext ConstructUnmarshallerContext(Stream responseStream, IWebResponseData response);
    }

    public abstract class XmlResponseUnmarshaller : ResponseUnmarshaller
    {
        public override WebServiceResponse Unmarshall(UnmarshallerContext input)
        {
            XmlUnmarshallerContext context = input as XmlUnmarshallerContext;
            if (context == null)
                throw new InvalidOperationException("Unsupported UnmarshallerContext");

            WebServiceResponse response = this.Unmarshall(context);

            foreach (var headerName in context.ResponseData.GetHeaderNames())
                response.Headers.Add(headerName, context.ResponseData.GetHeaderValue(headerName));

            return response;
        }
        public override AliyunServiceException UnmarshallException(UnmarshallerContext input, Exception innerException, HttpStatusCode statusCode)
        {
            XmlUnmarshallerContext context = input as XmlUnmarshallerContext;
            if (context == null)
                throw new InvalidOperationException("Unsupported UnmarshallerContext");

            return this.UnmarshallException(context, innerException, statusCode);
        }

        protected override UnmarshallerContext ConstructUnmarshallerContext(Stream responseStream, IWebResponseData response)
        {
            return new XmlUnmarshallerContext(responseStream, response);
        }

        public abstract WebServiceResponse Unmarshall(XmlUnmarshallerContext input);

        public abstract AliyunServiceException UnmarshallException(XmlUnmarshallerContext input, Exception innerException, HttpStatusCode statusCode);
    }
}
