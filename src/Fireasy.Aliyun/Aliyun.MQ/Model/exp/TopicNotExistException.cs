
using System;
using System.Net;

namespace Aliyun.MQ.Model.Exp
{
    ///<summary>
    /// TopicNotExistException
    /// </summary>
    public class TopicNotExistException : MQException
    {
        /// <summary>
        /// Constructs a new TopicNotExistException with the specified error message.
        /// </summary>
        public TopicNotExistException(string message)
            : base(message)
        { }

        public TopicNotExistException(string message, Exception innerException)
            : base(message, innerException)
        { }

        public TopicNotExistException(Exception innerException)
            : base(innerException)
        { }

        public TopicNotExistException(string message, string errorCode, string requestId, string hostId, HttpStatusCode statusCode)
            : base(message, errorCode, requestId, hostId, statusCode)
        { }

        public TopicNotExistException(string message, Exception innerException, string errorCode, string requestId, string hostId, HttpStatusCode statusCode)
            : base(message, innerException, errorCode, requestId, hostId, statusCode)
        { }
    }
}
