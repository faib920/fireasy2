using System.Globalization;
using System.Net;
using Aliyun.MQ.Runtime.Internal.Util;

namespace Aliyun.MQ.Runtime.Pipeline.ErrorHandler
{
    public class WebExceptionHandler : ExceptionHandler<WebException>
    {
        public WebExceptionHandler() :
            base()
        {
        }

        public override bool HandleException(IExecutionContext executionContext, WebException exception)
        {
            var requestContext = executionContext.RequestContext;
            var httpErrorResponse = exception.Response as HttpWebResponse;

            var message = string.Format(CultureInfo.InvariantCulture,
                    "A WebException with status {0} was thrown, caused by {1}", exception.Status, exception.Message);
            throw new AliyunServiceException(message, exception);
        }
    }
}
