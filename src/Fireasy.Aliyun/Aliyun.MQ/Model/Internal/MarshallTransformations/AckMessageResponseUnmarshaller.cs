using Aliyun.MQ.Runtime;
using Aliyun.MQ.Runtime.Internal;
using Aliyun.MQ.Runtime.Internal.Transform;
using Aliyun.MQ.Util;
using Aliyun.MQ.Model.Exp;
using System;
using System.Net;
using System.Xml;

namespace Aliyun.MQ.Model.Internal.MarshallTransformations
{
    public class AckMessageResponseUnmarshaller : XmlResponseUnmarshaller
    {
        public override WebServiceResponse Unmarshall(XmlUnmarshallerContext context)
        {
            return new AckMessageResponse();
        }

        public override AliyunServiceException UnmarshallException(XmlUnmarshallerContext context, Exception innerException, HttpStatusCode statusCode)
        {
            XmlTextReader reader = new XmlTextReader(context.ResponseStream);

            ErrorResponse errorResponse = new ErrorResponse();
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.LocalName == Constants.XML_ROOT_ERROR_RESPONSE)
                        {
                            return UnmarshallNormalError(reader, innerException, statusCode);
                        }
                        else
                        {
                            AckMessageException ackMessageException = UnmarshallAckMessageError(reader);
                            ackMessageException.RequestId = context.ResponseData.GetHeaderValue("x-mq-request-id");
                            return ackMessageException;
                        }
                }
            }
            return new MQException(errorResponse.Message, innerException, errorResponse.Code, errorResponse.RequestId, errorResponse.HostId, statusCode);
        }

        private AckMessageException UnmarshallAckMessageError(XmlTextReader reader)
        {
            AckMessageException ackMessageException = new AckMessageException();
            AckMessageErrorItem item = null;
 
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.LocalName)
                        {
                            case Constants.XML_ROOT_ERROR_RESPONSE:
                                item = new AckMessageErrorItem();
                                break;
                            case Constants.XML_ELEMENT_ERROR_CODE:
                                reader.Read();
                                item.ErrorCode = reader.Value;
                                break;
                            case Constants.XML_ELEMENT_ERROR_MESSAGE:
                                reader.Read();
                                item.ErrorMessage = reader.Value;
                                break;
                            case Constants.XML_ELEMENT_RECEIPT_HANDLE:
                                reader.Read();
                                item.ReceiptHandle = reader.Value;
                                break;
                        }
                        break;
                    case XmlNodeType.EndElement:
                        if (reader.LocalName == Constants.XML_ROOT_ERROR_RESPONSE)
                        {
                            ackMessageException.ErrorItems.Add(item);
                        }
                        break;
                }
            }
            reader.Close();
            return ackMessageException;
        }

        private AliyunServiceException UnmarshallNormalError(XmlTextReader reader, Exception innerException, HttpStatusCode statusCode)
        {
            ErrorResponse errorResponse = ErrorResponseUnmarshaller.Instance.Unmarshall(reader);
            if (errorResponse.Code != null)
            {
                switch (errorResponse.Code)
                {
                    case ErrorCode.SubscriptionNotExist:
                        return new SubscriptionNotExistException(errorResponse.Message, innerException, errorResponse.Code, errorResponse.RequestId, errorResponse.HostId, statusCode);
                    case ErrorCode.TopicNotExist:
                        return new TopicNotExistException(errorResponse.Message, innerException, errorResponse.Code, errorResponse.RequestId, errorResponse.HostId, statusCode);
                    case ErrorCode.InvalidArgument:
                        return new InvalidArgumentException(errorResponse.Message, innerException, errorResponse.Code, errorResponse.RequestId, errorResponse.HostId, statusCode);
                    case ErrorCode.ReceiptHandleError:
                        return new ReceiptHandleErrorException(errorResponse.Message, innerException, errorResponse.Code, errorResponse.RequestId, errorResponse.HostId, statusCode);
                }
            }
            return new MQException(errorResponse.Message, innerException, errorResponse.Code, errorResponse.RequestId, errorResponse.HostId, statusCode);
        }

        private static AckMessageResponseUnmarshaller _instance = new AckMessageResponseUnmarshaller();
        public static AckMessageResponseUnmarshaller Instance
        {
            get
            {
                return _instance;
            }
        }
    }
}
