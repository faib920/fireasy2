using System;
using Aliyun.MQ.Runtime.Internal;
using Aliyun.MQ.Runtime.Internal.Util;

namespace Aliyun.MQ.Runtime.Pipeline
{
    public abstract partial class PipelineHandler : IPipelineHandler
    {
        public IPipelineHandler InnerHandler { get; set; }

        public IPipelineHandler OuterHandler { get; set; }

        public virtual void InvokeSync(IExecutionContext executionContext)
        {
            if (this.InnerHandler != null)
            {
                InnerHandler.InvokeSync(executionContext);
                return;
            }
            throw new InvalidOperationException("Cannot invoke InnerHandler. InnerHandler is not set.");
        }

        public virtual IAsyncResult InvokeAsync(IAsyncExecutionContext executionContext)
        {
            if (this.InnerHandler != null)
            {
                return InnerHandler.InvokeAsync(executionContext);
            }
            throw new InvalidOperationException("Cannot invoke InnerHandler. InnerHandler is not set.");
        }

        public void AsyncCallback(IAsyncExecutionContext executionContext)
        {
            try
            {
                this.InvokeAsyncCallback(executionContext);
            }
            catch (Exception exception)
            {

                // An unhandled exception occured in the callback implementation.
                // Capture the exception and end the callback processing by signalling the
                // wait handle.

                var asyncResult = executionContext.ResponseContext.AsyncResult;
                asyncResult.Exception = exception;
                asyncResult.SignalWaitHandle();
                if (asyncResult.AsyncCallback != null)
                {
                    asyncResult.AsyncCallback(asyncResult);
                }
            }
        }

        protected virtual void InvokeAsyncCallback(IAsyncExecutionContext executionContext)
        {
            if (this.OuterHandler!=null)
            {
                this.OuterHandler.AsyncCallback(executionContext);    
            }
            else
            {
                // No more outer handlers to process, signal completion
                executionContext.ResponseContext.AsyncResult.Response =
                    executionContext.ResponseContext.Response;                
                
                var asyncResult = executionContext.ResponseContext.AsyncResult;                
                asyncResult.SignalWaitHandle();
                if (asyncResult.AsyncCallback != null)
                {
                    asyncResult.AsyncCallback(asyncResult);
                }
            }
        }
    }
}
