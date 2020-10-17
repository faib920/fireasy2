using System;
using System.Net;
using Aliyun.MQ.Runtime;
using Aliyun.MQ.Runtime.Internal;
using Aliyun.MQ.Runtime.Internal.Transform;
using Aliyun.MQ.Util;
using Aliyun.MQ.Model.Exp;
using System.Xml;

namespace Aliyun.MQ.Model.Internal.MarshallTransformations
{
    internal class PublishMessageResponseUnmarshaller : XmlResponseUnmarshaller
    {
        public override WebServiceResponse Unmarshall(XmlUnmarshallerContext context)
        {
            XmlTextReader reader = new XmlTextReader(context.ResponseStream);
            PublishMessageResponse response = new PublishMessageResponse();

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.LocalName)
                        {
                            case Constants.XML_ELEMENT_MESSAGE_ID:
                                reader.Read();
                                response.MessageId = reader.Value;
                                break;
                            case Constants.XML_ELEMENT_MESSAGE_BODY_MD5:
                                reader.Read();
                                response.MessageBodyMD5 = reader.Value;
                                break;
                            case Constants.XML_ELEMENT_RECEIPT_HANDLE:
                                reader.Read();
                                response.ReeceiptHandle = reader.Value;
                                break;
                        }
                        break;
                }
            }
            reader.Close();
            return response;
        }

        public override AliyunServiceException UnmarshallException(XmlUnmarshallerContext context, Exception innerException, HttpStatusCode statusCode)
        {
            ErrorResponse errorResponse = ErrorResponseUnmarshaller.Instance.Unmarshall(context);
            if (errorResponse.Code != null && errorResponse.Code.Equals(ErrorCode.TopicNotExist))
            {
                return new TopicNotExistException(errorResponse.Message, innerException, errorResponse.Code, errorResponse.RequestId, errorResponse.HostId, statusCode);
            }
            if (errorResponse.Code != null && errorResponse.Code.Equals(ErrorCode.MalformedXML))
            {
                return new MalformedXMLException(errorResponse.Message, innerException, errorResponse.Code, errorResponse.RequestId, errorResponse.HostId, statusCode);
            }
            if (errorResponse.Code != null && errorResponse.Code.Equals(ErrorCode.InvalidArgument))
            {
                return new InvalidArgumentException(errorResponse.Message, innerException, errorResponse.Code, errorResponse.RequestId, errorResponse.HostId, statusCode);
            }
            return new MQException(errorResponse.Message, innerException, errorResponse.Code, errorResponse.RequestId, errorResponse.HostId, statusCode);
        }

        private static PublishMessageResponseUnmarshaller _instance = new PublishMessageResponseUnmarshaller();
        public static PublishMessageResponseUnmarshaller Instance
        {
            get
            {
                return _instance;
            }
        }

    }
}