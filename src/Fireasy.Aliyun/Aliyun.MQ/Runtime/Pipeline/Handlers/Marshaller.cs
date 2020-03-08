using System;
using System.Collections.Generic;
using System.Globalization;
using Aliyun.MQ.Runtime.Internal;
using Aliyun.MQ.Util;
using Aliyun.MQ.Runtime.Internal.Util;

namespace Aliyun.MQ.Runtime.Pipeline.Handlers
{
    public class Marshaller : GenericHandler
    {
        protected override void PreInvoke(IExecutionContext executionContext)
        {
            try
            {

                IRequestContext requestContext = executionContext.RequestContext;
                requestContext.Request = requestContext.Marshaller.Marshall(requestContext.OriginalRequest);
                requestContext.Request.Endpoint = requestContext.ClientConfig.RegionEndpoint;
                AddRequiredHeaders(requestContext);
                AddOptionalHeaders(requestContext);
            }
            finally
            {
            }
        }

        private void AddRequiredHeaders(IRequestContext requestContext)
        {
            IDictionary<string, string> headers = requestContext.Request.Headers;
            headers[Constants.UserAgentHeader] = requestContext.ClientConfig.UserAgent;
            if (requestContext.Request.ContentStream != null)
                headers[Constants.ContentLengthHeader] = requestContext.Request.ContentStream.Length.ToString(CultureInfo.InvariantCulture);
            headers[Constants.DateHeader] = AliyunSDKUtils.FormattedCurrentTimestampRFC822;
            headers[Constants.XMQVersionHeader] = requestContext.ClientConfig.ServiceVersion;
            if (!headers.ContainsKey(Constants.HostHeader))
            {
                Uri requestEndpoint = requestContext.Request.Endpoint;
                var hostHeader = requestEndpoint.Host;
                if (!requestEndpoint.IsDefaultPort)
                    hostHeader += ":" + requestEndpoint.Port;
                headers.Add(Constants.HostHeader, hostHeader);
            }
        }

        private void AddOptionalHeaders(IRequestContext requestContext)
        {
            WebServiceRequest originalRequest = requestContext.Request.OriginalRequest;
            IDictionary<string, string> headers = requestContext.Request.Headers;
            if (originalRequest.IsSetContentType())
                headers[Constants.ContentTypeHeader] = originalRequest.ContentType;
            else
                headers[Constants.ContentTypeHeader] = Constants.ContentTypeTextXml;
            if (originalRequest.IsSetContentMD5())
                headers[Constants.ContentMD5Header] = originalRequest.ContentMD5;
        }
    }
}
