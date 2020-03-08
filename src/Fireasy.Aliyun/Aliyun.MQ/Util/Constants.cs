namespace Aliyun.MQ.Util
{
    internal static class Constants
    {
        public const string MQ_SERVICE_NAME = "Aliyun.MQ";

        public const string MQ_XML_NAMESPACE = "http://mq.aliyuncs.com/doc/v1";
        public const string MQ_AUTHORIZATION_HEADER_PREFIX = "MQ";
        public const string X_MQ_VERSION = "2015-06-06";
        public const string X_MQ_HEADER_PREFIX = "x-mq-";

        public const string XML_ROOT_ERRORS = "Errors";
        public const string XML_ROOT_ERROR_RESPONSE = "Error";
        public const string XML_ROOT_TOPICS = "Topics";
        public const string XML_ROOT_MESSAGES = "Messages";
        public const string XML_ROOT_MESSAGE = "Message";
        public const string XML_ROOT_RECEIPT_HANDLES = "ReceiptHandles";

        public const string XML_ELEMENT_CODE = "Code";
        public const string XML_ELEMENT_ERROR_CODE = "ErrorCode";
        public const string XML_ELEMENT_MESSAGE = "Message";
        public const string XML_ELEMENT_ERROR_MESSAGE = "ErrorMessage";
        public const string XML_ELEMENT_REQUEST_ID = "RequestId";
        public const string XML_ELEMENT_HOST_ID = "HostId";

        public const string XML_ELEMENT_MESSAGE_BODY = "MessageBody";
        public const string XML_ELEMENT_MESSAGE_TAG = "MessageTag";
        public const string XML_ELEMENT_MESSAGE_PROPERTIES = "Properties";
        public const string XML_ELEMENT_MESSAGE_BODY_MD5 = "MessageBodyMD5";
        public const string XML_ELEMENT_MESSAGE_ID = "MessageId";
        public const string XML_ELEMENT_RECEIPT_HANDLE = "ReceiptHandle";
        public const string XML_ELEMENT_PUBLISH_TIME = "PublishTime";
        public const string XML_ELEMENT_NEXT_CONSUME_TIME = "NextConsumeTime";
        public const string XML_ELEMENT_FIRST_CONSUME_TIME = "FirstConsumeTime";
        public const string XML_ELEMENT_CONSUMED_TIMES = "ConsumedTimes";

        public const string PARAMETER_WAIT_SECONDS = "waitseconds";
        public const string PARAMETER_BATCH_SIZE = "numOfMessages";
        public const string PARAMETER_CONSUMER = "consumer";
        public const string PARAMETER_CONSUME_TAG = "tag";
        public const string PARAMETER_NS = "ns";
        public const string PARAMETER_TRANSACTION = "trans";

        public const string MESSAGE_SUB_RESOURCE = "/messages";
        public const string TOPIC_PRE_RESOURCE = "topics/";


        public const string ContentTypeTextXml = "text/xml";

        public const string ContentTypeHeader = "Content-Type";
        public const string ContentLengthHeader = "Content-Length";
        public const string ContentMD5Header = "Content-MD5";
        public const string AuthorizationHeader = "Authorization";
        public const string SecurityToken = "security-token";


        public const string UserAgentHeader = "User-Agent";
        public const string LocationHeader = "Location";
        public const string HostHeader = "Host";
        public const string DateHeader = "Date";
        public const string AcceptHeader = "Accept";

        public const string XMQVersionHeader = "x-mq-version";

        public const string MESSAGE_PROPERTIES_TIMER_KEY = "__STARTDELIVERTIME";
        public const string MESSAGE_PROPERTIES_TRANS_CHECK_KEY = "__TransCheckT";
        public const string MESSAGE_PROPERTIES_MSG_KEY = "KEYS";
    }
}
