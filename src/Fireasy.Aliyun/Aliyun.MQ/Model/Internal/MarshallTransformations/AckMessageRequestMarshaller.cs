using Aliyun.MQ.Runtime;
using Aliyun.MQ.Runtime.Internal;
using Aliyun.MQ.Runtime.Internal.Transform;
using Aliyun.MQ.Util;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Aliyun.MQ.Model.Internal.MarshallTransformations
{
    public partial class AckMessageRequestMarshaller : IMarshaller<IRequest, AckMessageRequest>, IMarshaller<IRequest, WebServiceRequest>
    {
        public IRequest Marshall(WebServiceRequest input)
        {
            return this.Marshall((AckMessageRequest)input);
        }

        public IRequest Marshall(AckMessageRequest publicRequest)
        {
            MemoryStream stream = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8);
            writer.WriteStartDocument();
            writer.WriteStartElement(Constants.XML_ROOT_RECEIPT_HANDLES, Constants.MQ_XML_NAMESPACE);
            foreach (string receiptHandle in publicRequest.ReceiptHandles)
            {
                writer.WriteElementString(Constants.XML_ELEMENT_RECEIPT_HANDLE, receiptHandle);
            }
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Flush();
            stream.Seek(0, SeekOrigin.Begin);

            IRequest request = new DefaultRequest(publicRequest, Constants.MQ_SERVICE_NAME);
            request.HttpMethod = HttpMethod.DELETE.ToString();
            request.ContentStream = stream;
            request.ResourcePath = Constants.TOPIC_PRE_RESOURCE + publicRequest.TopicName
                + Constants.MESSAGE_SUB_RESOURCE;
            PopulateSpecialParameters(publicRequest, request.Parameters);
            return request;
        }

        private static void PopulateSpecialParameters(AckMessageRequest request, IDictionary<string, string> paramters)
        {
            paramters.Add(Constants.PARAMETER_CONSUMER, request.Consumer);
            if (request.IsSetInstance())
            {
                paramters.Add(Constants.PARAMETER_NS, request.IntanceId);
            }
            if (request.IsSetTransaction())
            {
                paramters.Add(Constants.PARAMETER_TRANSACTION, request.Trasaction);
            }
        }

        private static AckMessageRequestMarshaller _instance = new AckMessageRequestMarshaller();
        public static AckMessageRequestMarshaller Instance
        {
            get
            {
                return _instance;
            }
        }
    }
}
