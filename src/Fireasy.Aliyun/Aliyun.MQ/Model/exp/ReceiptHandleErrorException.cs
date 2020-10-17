
using System;
using System.Net;

namespace Aliyun.MQ.Model.Exp
{
    ///<summary>
    /// ReceiptHandleErrorException
    /// </summary>
    public class ReceiptHandleErrorException : MQException 
    {
        /// <summary>
        /// Constructs a new ReceiptHandleErrorException with the specified error message.
        /// </summary>
        public ReceiptHandleErrorException(string message) 
            : base(message) {}
          
        public ReceiptHandleErrorException(string message, Exception innerException) 
            : base(message, innerException) {}
            
        public ReceiptHandleErrorException(Exception innerException) 
            : base(innerException) {}

        public ReceiptHandleErrorException(string message, string errorCode, string requestId, string hostId, HttpStatusCode statusCode)
            : base(message, errorCode, requestId, hostId, statusCode) { }

        public ReceiptHandleErrorException(string message, Exception innerException, string errorCode, string requestId, string hostId, HttpStatusCode statusCode)
            : base(message, innerException, errorCode, requestId, hostId, statusCode) { }
    }
}
