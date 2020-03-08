using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using Aliyun.MQ.Util;

namespace Aliyun.MQ.Runtime.Pipeline.RetryHandler
{
    public class DefaultRetryPolicy : RetryPolicy
    {
        private int _maxBackoffInMilliseconds = (int)TimeSpan.FromSeconds(30).TotalMilliseconds;

        // Set of web exception status codes to retry on.
        private ICollection<WebExceptionStatus> _webExceptionStatusesToRetryOn = new HashSet<WebExceptionStatus>
        {
            WebExceptionStatus.ConnectFailure,
            WebExceptionStatus.ConnectionClosed,
            WebExceptionStatus.KeepAliveFailure,
            WebExceptionStatus.NameResolutionFailure,
            WebExceptionStatus.ReceiveFailure
        };

        // Set of error codes to retry on.
        private ICollection<string> _errorCodesToRetryOn = new HashSet<string>
        {
            "Throttling",
            "RequestTimeout"
        };

        public int MaxBackoffInMilliseconds
        {
            get { return _maxBackoffInMilliseconds; }
            set { _maxBackoffInMilliseconds = value; }
        }

        public ICollection<string> ErrorCodesToRetryOn
        {
            get { return _errorCodesToRetryOn; }
        }

        public ICollection<WebExceptionStatus> WebExceptionStatusesToRetryOn
        {
            get { return _webExceptionStatusesToRetryOn; }
        }

        public DefaultRetryPolicy(int maxRetries)
        {
            this.MaxRetries = maxRetries;
        }

        public override bool CanRetry(IExecutionContext executionContext)
        {            
            return executionContext.RequestContext.Request.IsRequestStreamRewindable();
        }

        public override bool RetryForException(IExecutionContext executionContext, Exception exception)
        {            
            // An IOException was thrown by the underlying http client.
            if (exception is IOException)
            {
                // Don't retry IOExceptions that are caused by a ThreadAbortException
                if (IsInnerException<ThreadAbortException>(exception))
                    return false;

                // Retry all other IOExceptions
                return true;
            }

            // A AliyunServiceException was thrown by ErrorHandler
            var serviceException = exception as AliyunServiceException;
            if (serviceException != null)
            {
                /*
                * For 500 internal server errors and 503 service
                * unavailable errors, we want to retry, but we need to use
                * an exponential back-off strategy so that we don't overload
                * a server with a flood of retries. If we've surpassed our
                * retry limit we handle the error response as a non-retryable
                * error and go ahead and throw it back to the user as an exception.
                */
                if (serviceException.StatusCode == HttpStatusCode.InternalServerError ||
                    serviceException.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    return true;
                }

                /*
                 * Throttling is reported as a 400 or 503 error from services. To try and
                 * smooth out an occasional throttling error, we'll pause and retry,
                 * hoping that the pause is long enough for the request to get through
                 * the next time.
                */
                if ((serviceException.StatusCode == HttpStatusCode.BadRequest ||
                    serviceException.StatusCode == HttpStatusCode.ServiceUnavailable))
                {
                    string errorCode = serviceException.ErrorCode;
                    if (this.ErrorCodesToRetryOn.Contains(errorCode))
                    {
                        return true;
                    }
                }

                WebException webException;
                if (IsInnerException<WebException>(exception, out webException))
                {
                    if (this.WebExceptionStatusesToRetryOn.Contains(webException.Status))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public override bool RetryLimitReached(IExecutionContext executionContext)
        {
            return executionContext.RequestContext.Retries >= this.MaxRetries;
        }

        public override void WaitBeforeRetry(IExecutionContext executionContext)
        {            
            DefaultRetryPolicy.WaitBeforeRetry(executionContext.RequestContext.Retries, this.MaxBackoffInMilliseconds);
        }

        public static void WaitBeforeRetry(int retries, int maxBackoffInMilliseconds)
        {
            int delay = (int)(Math.Pow(4, retries) * 100);
            delay = Math.Min(delay, maxBackoffInMilliseconds);
            AliyunSDKUtils.Sleep(delay);
        }

        protected static bool IsInnerException<T>(Exception exception)
            where T : Exception
        {
            T innerException;
            return IsInnerException<T>(exception, out innerException);
        }

        protected static bool IsInnerException<T>(Exception exception, out T inner)
            where T : Exception
        {
            inner = null;
            var innerExceptionType = typeof(T);
            var currentException = exception;
            while (currentException.InnerException != null)
            {
                inner = currentException.InnerException as T;
                if (inner != null)
                {
                    return true;
                }
                currentException = currentException.InnerException;
            }
            return false;
        }
    }
}
