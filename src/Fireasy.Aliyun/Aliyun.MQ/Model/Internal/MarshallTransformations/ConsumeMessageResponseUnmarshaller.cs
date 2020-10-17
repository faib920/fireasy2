using Aliyun.MQ.Runtime.Internal.Transform;
using System;
using Aliyun.MQ.Runtime;
using System.Net;
using Aliyun.MQ.Util;
using Aliyun.MQ.Runtime.Internal;
using Aliyun.MQ.Model.Exp;

namespace Aliyun.MQ.Model.Internal.MarshallTransformations
{
    class ConsumeMessageResponseUnmarshaller : XmlResponseUnmarshaller
    {
        public override WebServiceResponse Unmarshall(XmlUnmarshallerContext context)
        {
            System.Xml.XmlTextReader reader = new System.Xml.XmlTextReader(context.ResponseStream);
            ConsumeMessageResponse consumeMessageResponse = new ConsumeMessageResponse();
            Message message = null;

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case System.Xml.XmlNodeType.Element:
                        switch (reader.LocalName)
                        {
                            case Constants.XML_ROOT_MESSAGE:
                                message = new Message();
                                break;
                            case Constants.XML_ELEMENT_MESSAGE_ID:
                                reader.Read();
                                message.Id = reader.Value;
                                break;
                            case Constants.XML_ELEMENT_RECEIPT_HANDLE:
                                reader.Read();
                                message.ReceiptHandle = reader.Value;
                                break;
                            case Constants.XML_ELEMENT_MESSAGE_BODY_MD5:
                                reader.Read();
                                message.BodyMD5 = reader.Value;
                                break;
                            case Constants.XML_ELEMENT_MESSAGE_BODY:
                                reader.Read();
                                message.Body = reader.Value;
                                break;
                            case Constants.XML_ELEMENT_PUBLISH_TIME:
                                reader.Read();
                                message.PublishTime = long.Parse(reader.Value);
                                break;
                            case Constants.XML_ELEMENT_NEXT_CONSUME_TIME:
                                reader.Read();
                                message.NextConsumeTime = long.Parse(reader.Value);
                                break;
                            case Constants.XML_ELEMENT_FIRST_CONSUME_TIME:
                                reader.Read();
                                message.FirstConsumeTime = long.Parse(reader.Value);
                                break;
                            case Constants.XML_ELEMENT_CONSUMED_TIMES:
                                reader.Read();
                                message.ConsumedTimes = uint.Parse(reader.Value);
                                break;
                            case Constants.XML_ELEMENT_MESSAGE_TAG:
                                reader.Read();
                                message.MessageTag = reader.Value;
                                break;
                            case Constants.XML_ELEMENT_MESSAGE_PROPERTIES:
                                reader.Read();
                                AliyunSDKUtils.StringToDict(reader.Value, message.Properties);
                                break;
                        }
                        break;
                    case System.Xml.XmlNodeType.EndElement:
                        if (reader.LocalName == Constants.XML_ROOT_MESSAGE)
                        {
                            consumeMessageResponse.Messages.Add(message);
                        }
                        break;
                }
            }
            reader.Close();
            return consumeMessageResponse;
        }

        public override AliyunServiceException UnmarshallException(XmlUnmarshallerContext context, Exception innerException, HttpStatusCode statusCode)
        {
            ErrorResponse errorResponse = ErrorResponseUnmarshaller.Instance.Unmarshall(context);
            if (errorResponse.Code != null && errorResponse.Code.Equals(ErrorCode.TopicNotExist))
            {
                return new TopicNotExistException(errorResponse.Message, innerException, errorResponse.Code, errorResponse.RequestId, errorResponse.HostId, statusCode);
            }
            if (errorResponse.Code != null && errorResponse.Code.Equals(ErrorCode.SubscriptionNotExist))
            {
                return new SubscriptionNotExistException(errorResponse.Message, innerException, errorResponse.Code, errorResponse.RequestId, errorResponse.HostId, statusCode);
            }
            if (errorResponse.Code != null && errorResponse.Code.Equals(ErrorCode.MessageNotExist))
            {
                return new MessageNotExistException(errorResponse.Message, innerException, errorResponse.Code, errorResponse.RequestId, errorResponse.HostId, statusCode);
            }
            return new MQException(errorResponse.Message, innerException, errorResponse.Code, errorResponse.RequestId, errorResponse.HostId, statusCode);
        }

        private static ConsumeMessageResponseUnmarshaller _instance = new ConsumeMessageResponseUnmarshaller();
        public static ConsumeMessageResponseUnmarshaller Instance
        {
            get
            {
                return _instance;
            }
        }
    }
}
