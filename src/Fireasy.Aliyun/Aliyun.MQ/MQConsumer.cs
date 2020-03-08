
using Aliyun.MQ.Model;
using Aliyun.MQ.Model.Internal.MarshallTransformations;
using Aliyun.MQ.Runtime;
using System.Collections.Generic;

namespace Aliyun.MQ
{
    public partial class MQConsumer
    {
        private string _instanceId;
        private string _topicName;
        private string _consumer;
        private string _messageTag;
        private readonly AliyunServiceClient _serviceClient;

        public MQConsumer(string instanceId, string topicName, string consumer, string messageTag, AliyunServiceClient serviceClient)
        {
            this._instanceId = instanceId;
            this._topicName = topicName;
            this._consumer = consumer;
            this._messageTag = messageTag;
            this._serviceClient = serviceClient;
        }

        public string IntanceId
        {
            get { return this._instanceId; }
        }

        public bool IsSetInstance()
        {
            return !string.IsNullOrEmpty(this._instanceId);
        }

        public string TopicName
        {
            get { return this._topicName; }
        }

        public bool IsSetTopicName()
        {
            return this._topicName != null;
        }

        public string Consumer
        {
            get { return this._consumer; }
        }

        public bool IsSetConsumer()
        {
            return this._consumer != null;
        }

        public string MessageTag
        {
            get { return this._messageTag; }
        }

        public bool IsSetMessageTag()
        {
            return this._messageTag != null;
        }

        public AckMessageResponse AckMessage(List<string> receiptHandles)
        {
            var request = new AckMessageRequest(this._topicName, this._consumer, receiptHandles);
            request.IntanceId = this._instanceId;
            var marshaller = new AckMessageRequestMarshaller();
            var unmarshaller = AckMessageResponseUnmarshaller.Instance;

            return _serviceClient.Invoke<AckMessageRequest, AckMessageResponse>(request, marshaller, unmarshaller);
        }

        public List<Message> ConsumeMessage(uint batchSize)
        {
            var request = new ConsumeMessageRequest(this._topicName, this._consumer, this._messageTag);
            request.IntanceId = this._instanceId;
            request.BatchSize = batchSize;
            var marshaller = ConsumeMessageRequestMarshaller.Instance;
            var unmarshaller = ConsumeMessageResponseUnmarshaller.Instance;

            ConsumeMessageResponse result = _serviceClient.Invoke<ConsumeMessageRequest, ConsumeMessageResponse>(request, marshaller, unmarshaller);

            return result.Messages;
        }

        public List<Message> ConsumeMessage(uint batchSize, uint waitSeconds)
        {
            var request = new ConsumeMessageRequest(this._topicName, this._consumer, this._messageTag);
            request.IntanceId = this._instanceId;
            request.BatchSize = batchSize;
            request.WaitSeconds = waitSeconds;
            var marshaller = ConsumeMessageRequestMarshaller.Instance;
            var unmarshaller = ConsumeMessageResponseUnmarshaller.Instance;

            ConsumeMessageResponse result = _serviceClient.Invoke<ConsumeMessageRequest, ConsumeMessageResponse>(request, marshaller, unmarshaller);

            return result.Messages;
        }
    }
}
