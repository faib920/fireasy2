
using System;
using System.Net;

namespace Aliyun.MQ.Model.Exp
{
    ///<summary>
    /// MessageNotExistException
    /// </summary>
    public class MessageNotExistException : MQException 
    {
        /// <summary>
        /// Constructs a new MessageNotExistException with the specified error message.
        /// </summary>
        public MessageNotExistException(string message) 
            : base(message) {}
          
        public MessageNotExistException(string message, Exception innerException) 
            : base(message, innerException) {}
            
        public MessageNotExistException(Exception innerException) 
            : base(innerException) {}

        public MessageNotExistException(string message, string errorCode, string requestId, string hostId, HttpStatusCode statusCode)
            : base(message, errorCode, requestId, hostId, statusCode) { }

        public MessageNotExistException(string message, Exception innerException, string errorCode, string requestId, string hostId, HttpStatusCode statusCode)
            : base(message, innerException, errorCode, requestId, hostId, statusCode) { }
    }
}
