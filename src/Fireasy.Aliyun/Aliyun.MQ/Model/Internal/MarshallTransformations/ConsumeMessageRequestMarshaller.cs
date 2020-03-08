using Aliyun.MQ.Runtime;
using Aliyun.MQ.Runtime.Internal;
using Aliyun.MQ.Runtime.Internal.Transform;
using Aliyun.MQ.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aliyun.MQ.Model.Internal.MarshallTransformations
{
    class ConsumeMessageRequestMarshaller : IMarshaller<IRequest, ConsumeMessageRequest>, IMarshaller<IRequest, WebServiceRequest>
    {
        public IRequest Marshall(WebServiceRequest input)
        {
            return this.Marshall((ConsumeMessageRequest)input);
        }

        public IRequest Marshall(ConsumeMessageRequest publicRequest)
        {
            IRequest request = new DefaultRequest(publicRequest, Constants.MQ_SERVICE_NAME);
            request.HttpMethod = HttpMethod.GET.ToString();
            request.ResourcePath = Constants.TOPIC_PRE_RESOURCE + publicRequest.TopicName
                + Constants.MESSAGE_SUB_RESOURCE;
            PopulateSpecialParameters(publicRequest, request.Parameters);
            return request;
        }

        private static void PopulateSpecialParameters(ConsumeMessageRequest request, IDictionary<string, string> paramters)
        {
            paramters.Add(Constants.PARAMETER_CONSUMER, request.Consumer);
            if (request.IsSetInstance()) 
            {
                paramters.Add(Constants.PARAMETER_NS, request.IntanceId);
            }
            if (request.IsSetWaitSeconds() && request.WaitSeconds > 0 && request.WaitSeconds < 31)
            {
                paramters.Add(Constants.PARAMETER_WAIT_SECONDS, request.WaitSeconds.ToString());
            }
            paramters.Add(Constants.PARAMETER_BATCH_SIZE, request.BatchSize.ToString());
            if (request.IsSetMessageTag()) 
            {
                paramters.Add(Constants.PARAMETER_CONSUME_TAG, request.MessageTag);
            }
            if (request.IsSetTransaction())
            {
                paramters.Add(Constants.PARAMETER_TRANSACTION, request.Trasaction);
            }
        }

        private static ConsumeMessageRequestMarshaller _instance = new ConsumeMessageRequestMarshaller();
        public static ConsumeMessageRequestMarshaller Instance
        {
            get
            {
                return _instance;
            }
        }
    }
}
