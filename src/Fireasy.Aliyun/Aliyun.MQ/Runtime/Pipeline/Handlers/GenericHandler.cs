using System;
using Aliyun.MQ.Runtime.Internal;

namespace Aliyun.MQ.Runtime.Pipeline.Handlers
{    
    public abstract class GenericHandler : PipelineHandler
    {
        public override void InvokeSync(IExecutionContext executionContext)
        {
            PreInvoke(executionContext);
            base.InvokeSync(executionContext);
            PostInvoke(executionContext);            
        }

        public override IAsyncResult InvokeAsync(IAsyncExecutionContext executionContext)
        {
            PreInvoke(ExecutionContext.CreateFromAsyncContext(executionContext));
            return base.InvokeAsync(executionContext);
        }

        protected override void InvokeAsyncCallback(IAsyncExecutionContext executionContext)
        {
            PostInvoke(ExecutionContext.CreateFromAsyncContext(executionContext));
            base.InvokeAsyncCallback(executionContext);           
        }

        protected virtual void PreInvoke(IExecutionContext executionContext) { }

        protected virtual void PostInvoke(IExecutionContext executionContext) { }        
    }
}
