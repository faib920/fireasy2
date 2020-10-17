
using System;
using System.Net;

namespace Aliyun.MQ.Model.Exp
{
    public class MalformedXMLException : MQException 
    {
        /// <summary>
        /// Constructs a new MalformedXMLException with the specified error message.
        /// </summary>
        public MalformedXMLException(string message) 
            : base(message) {}
          
        public MalformedXMLException(string message, Exception innerException) 
            : base(message, innerException) {}
            
        public MalformedXMLException(Exception innerException) 
            : base(innerException) {}

        public MalformedXMLException(string message, string errorCode, string requestId, string hostId, HttpStatusCode statusCode)
            : base(message, errorCode, requestId, hostId, statusCode) { }

        public MalformedXMLException(string message, Exception innerException, string errorCode, string requestId, string hostId, HttpStatusCode statusCode)
            : base(message, innerException, errorCode, requestId, hostId, statusCode) { }
    }
}
