using System.Xml;
using Aliyun.MQ.Runtime.Internal;
using Aliyun.MQ.Runtime.Internal.Transform;
using Aliyun.MQ.Util;

namespace Aliyun.MQ.Model.Internal.MarshallTransformations
{
    /// <summary>
    ///    Response Unmarshaller for all Errors
    /// </summary>
    internal class ErrorResponseUnmarshaller : IUnmarshaller<ErrorResponse, XmlUnmarshallerContext>
    {
        /// <summary>
        /// Build an ErrorResponse from XML 
        /// </summary>
        public ErrorResponse Unmarshall(XmlUnmarshallerContext context)
        {
            XmlTextReader reader = new XmlTextReader(context.ResponseStream);

            ErrorResponse errorResponse = new ErrorResponse();
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.LocalName == Constants.XML_ELEMENT_CODE)
                        {
                            reader.Read();
                            errorResponse.Code = reader.Value;
                        }
                        if (reader.LocalName == Constants.XML_ELEMENT_MESSAGE)
                        {
                            reader.Read();
                            errorResponse.Message = reader.Value;
                        }
                        if (reader.LocalName == Constants.XML_ELEMENT_REQUEST_ID)
                        {
                            reader.Read();
                            errorResponse.RequestId = reader.Value;
                        }
                        if (reader.LocalName == Constants.XML_ELEMENT_HOST_ID)
                        {
                            reader.Read();
                            errorResponse.HostId = reader.Value;
                        }
                        break;
                }
            }
            reader.Close();
            return errorResponse;
        }

        public ErrorResponse Unmarshall(XmlTextReader reader)
        {
            ErrorResponse errorResponse = new ErrorResponse();
            do
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.LocalName == Constants.XML_ELEMENT_CODE)
                        {
                            reader.Read();
                            errorResponse.Code = reader.Value;
                        }
                        if (reader.LocalName == Constants.XML_ELEMENT_MESSAGE)
                        {
                            reader.Read();
                            errorResponse.Message = reader.Value;
                        }
                        if (reader.LocalName == Constants.XML_ELEMENT_REQUEST_ID)
                        {
                            reader.Read();
                            errorResponse.RequestId = reader.Value;
                        }
                        if (reader.LocalName == Constants.XML_ELEMENT_HOST_ID)
                        {
                            reader.Read();
                            errorResponse.HostId = reader.Value;
                        }
                        break;
                }
            } while (reader.Read());
            reader.Close();
            return errorResponse;
        }

        private static ErrorResponseUnmarshaller _instance = new ErrorResponseUnmarshaller();
        public static ErrorResponseUnmarshaller Instance
        {
            get
            {
                return _instance;
            }
        }
    }
}