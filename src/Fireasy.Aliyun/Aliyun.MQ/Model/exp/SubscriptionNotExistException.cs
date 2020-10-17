
using System;
using System.Net;

namespace Aliyun.MQ.Model.Exp
{
    public class SubscriptionNotExistException : MQException
    {
        public SubscriptionNotExistException(string message)
            : base(message)
        { }

        public SubscriptionNotExistException(string message, Exception innerException)
            : base(message, innerException)
        { }

        public SubscriptionNotExistException(Exception innerException)
            : base(innerException)
        { }

        public SubscriptionNotExistException(string message, string errorCode, string requestId, string hostId, HttpStatusCode statusCode)
            : base(message, errorCode, requestId, hostId, statusCode)
        { }

        public SubscriptionNotExistException(string message, Exception innerException, string errorCode, string requestId, string hostId, HttpStatusCode statusCode)
            : base(message, innerException, errorCode, requestId, hostId, statusCode)
        { }
    }
}
