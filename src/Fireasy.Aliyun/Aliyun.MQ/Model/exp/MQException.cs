
using System;
using System.Net;
using Aliyun.MQ.Runtime;

namespace Aliyun.MQ.Model.Exp
{
    public class MQException : AliyunServiceException
    {
        public MQException(string message)
            : base(message)
        {
        }

        public MQException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public MQException(Exception innerException)
            : base(innerException.Message, innerException)
        {
        }

        public MQException(string message, string errorCode, string requestId, string hostId, HttpStatusCode statusCode)
            : base(message, errorCode, requestId, hostId, statusCode)
        {
        }

        public MQException(string message, Exception innerException, string errorCode, string requestId, string hostId, HttpStatusCode statusCode)
            : base(message, innerException, errorCode, requestId, hostId, statusCode)
        {
        }
    }
}
