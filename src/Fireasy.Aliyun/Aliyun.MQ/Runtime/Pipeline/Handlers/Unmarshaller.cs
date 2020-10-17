using Aliyun.MQ.Runtime.Internal;
using Aliyun.MQ.Runtime.Internal.Transform;
using Aliyun.MQ.Runtime.Internal.Util;

namespace Aliyun.MQ.Runtime.Pipeline.Handlers
{
    public class Unmarshaller : PipelineHandler
    {
        public override void InvokeSync(IExecutionContext executionContext)
        {
            base.InvokeSync(executionContext);

            if (executionContext.ResponseContext.HttpResponse.IsSuccessStatusCode)
            {
                // Unmarshall the http response.
                Unmarshall(executionContext);  
            }                      
        }

        protected override void InvokeAsyncCallback(IAsyncExecutionContext executionContext)
        {
            // Unmarshall the response if an exception hasn't occured
            if (executionContext.ResponseContext.AsyncResult.Exception == null)
            {
                Unmarshall(ExecutionContext.CreateFromAsyncContext(executionContext));
            }            
            base.InvokeAsyncCallback(executionContext);
        }

        private void Unmarshall(IExecutionContext executionContext)
        {
            var requestContext = executionContext.RequestContext;
            var responseContext = executionContext.ResponseContext;

            try
            {
                var unmarshaller = requestContext.Unmarshaller;
                try
                {
                    var context = unmarshaller.CreateContext(responseContext.HttpResponse,
                            responseContext.HttpResponse.ResponseBody.OpenResponse());

                    var response = UnmarshallResponse(context, requestContext);
                    responseContext.Response = response;                    
                }
                finally
                {
                    if (!unmarshaller.HasStreamingProperty)
                        responseContext.HttpResponse.ResponseBody.Dispose();
                }
            }
            finally
            {
            }
        }

        private WebServiceResponse UnmarshallResponse(UnmarshallerContext context,
            IRequestContext requestContext)
        {
            var unmarshaller = requestContext.Unmarshaller;
            WebServiceResponse response = null;
            try
            {
                response = unmarshaller.UnmarshallResponse(context);
            }
            finally
            {
            }

            return response;
        }
    }
}
