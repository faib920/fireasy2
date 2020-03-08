using System;
using Aliyun.MQ.Runtime.Internal.Util;

namespace Aliyun.MQ.Runtime.Pipeline.ErrorHandler
{
    public abstract class ExceptionHandler<T> : IExceptionHandler<T> where T : Exception
    {
        protected ExceptionHandler()
        {
        }

        public bool Handle(IExecutionContext executionContext, Exception exception)
        {
            return HandleException(executionContext, exception as T);
        }

        public abstract bool HandleException(IExecutionContext executionContext, T exception);
    }
}
