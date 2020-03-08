using System.IO;
using Aliyun.MQ.Runtime;
using Aliyun.MQ.Runtime.Internal;
using Aliyun.MQ.Runtime.Internal.Transform;
using Aliyun.MQ.Util;
using System.Text;
using System.Xml;
using System.Collections.Generic;

namespace Aliyun.MQ.Model.Internal.MarshallTransformations
{
    /// <summary>
    /// PublishMessage Request Marshaller
    /// </summary>       
    internal class PublishMessageRequestMarshaller : IMarshaller<IRequest, PublishMessageRequest>, IMarshaller<IRequest, WebServiceRequest>
    {
        public IRequest Marshall(WebServiceRequest input)
        {
            return this.Marshall((PublishMessageRequest)input);
        }

        public IRequest Marshall(PublishMessageRequest publicRequest)
        {
            MemoryStream stream = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8);
            writer.WriteStartDocument();
            writer.WriteStartElement(Constants.XML_ROOT_MESSAGE, Constants.MQ_XML_NAMESPACE);
            if (publicRequest.IsSetMessageBody())
                writer.WriteElementString(Constants.XML_ELEMENT_MESSAGE_BODY, publicRequest.MessageBody);
            if (publicRequest.IsSetMessageTag())
                writer.WriteElementString(Constants.XML_ELEMENT_MESSAGE_TAG, publicRequest.MessageTag);
            if (publicRequest.IsSetProperties())
                writer.WriteElementString(Constants.XML_ELEMENT_MESSAGE_PROPERTIES, publicRequest.Properties);
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Flush();

            stream.Seek(0, SeekOrigin.Begin);

            IRequest request = new DefaultRequest(publicRequest, Constants.MQ_SERVICE_NAME);

            request.HttpMethod = HttpMethod.POST.ToString();
            request.ContentStream = stream;
            request.ResourcePath = Constants.TOPIC_PRE_RESOURCE + publicRequest.TopicName
                + Constants.MESSAGE_SUB_RESOURCE;
            PopulateSpecialParameters(publicRequest, request.Parameters);
            return request;
        }

        private static void PopulateSpecialParameters(PublishMessageRequest request, IDictionary<string, string> paramters)
        {
            if (request.IsSetInstance()) {
                paramters.Add(Constants.PARAMETER_NS, request.IntanceId);
            }
        }

        private static PublishMessageRequestMarshaller _instance = new PublishMessageRequestMarshaller();
        public static PublishMessageRequestMarshaller Instance
        {
            get
            {
                return _instance;
            }
        }
    }

}