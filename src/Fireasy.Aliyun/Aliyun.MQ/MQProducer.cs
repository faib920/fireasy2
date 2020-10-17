
using Aliyun.MQ.Model;
using Aliyun.MQ.Model.Exp;
using Aliyun.MQ.Model.Internal.MarshallTransformations;
using Aliyun.MQ.Runtime;
using Aliyun.MQ.Util;
using System.Collections.Generic;

namespace Aliyun.MQ
{
    public partial class MQProducer
    {

        protected string _topicName;
        protected string _instanceId;
        protected readonly AliyunServiceClient _serviceClient;

        public MQProducer(string instanceId, string topicName, AliyunServiceClient serviceClient)
        {
            this._instanceId = instanceId;
            this._topicName = topicName;
            this._serviceClient = serviceClient;
        }

        public string TopicName
        {
            get { return this._topicName; }
        }

        public bool IsSetTopicName()
        {
            return this._topicName != null;
        }

        public string IntanceId
        {
            get { return this._instanceId; }
        }

        public bool IsSetInstance()
        {
            return !string.IsNullOrEmpty(this._instanceId);
        }

        public TopicMessage PublishMessage(TopicMessage topicMessage)
        {
            var request = new PublishMessageRequest(topicMessage.Body, topicMessage.MessageTag);
            request.TopicName = this._topicName;
            request.IntanceId = this._instanceId;
            request.Properties = AliyunSDKUtils.DictToString(topicMessage.Properties);

            var marshaller = PublishMessageRequestMarshaller.Instance;
            var unmarshaller = PublishMessageResponseUnmarshaller.Instance;

            PublishMessageResponse result = _serviceClient.Invoke<PublishMessageRequest, PublishMessageResponse>(request, marshaller, unmarshaller);

            TopicMessage retMsg = new TopicMessage(null);
            retMsg.Id = result.MessageId;
            retMsg.BodyMD5 = result.MessageBodyMD5;
            retMsg.ReceiptHandle = result.ReeceiptHandle;

            return retMsg;
        }

    }

    public partial class MQTransProducer : MQProducer
    {
        private readonly string _groupId;

        public MQTransProducer(string instanceId, string topicName, string groupId, AliyunServiceClient serviceClient) : base(instanceId, topicName, serviceClient)
        {
            if (string.IsNullOrEmpty(groupId))
            {
                throw new MQException("GroupId is null or empty!");
            }
            this._groupId = groupId;
        }

        public string GroupId
        {
            get { return this._groupId; }
        }

        /// <summary>
        /// commit transaction msg, the consumer will receive the msg.
        /// </summary>
        /// <returns>The commit.</returns>
        /// <param name="receiptHandle">Receipt handle.</param>
        public AckMessageResponse Commit(string receiptHandle)
        {
            List<string> handlers = new List<string>
            {
                receiptHandle
            };

            var request = new AckMessageRequest(this._topicName, this._groupId, handlers);
            request.IntanceId = this._instanceId;
            request.Trasaction = "commit";
            var marshaller = new AckMessageRequestMarshaller();
            var unmarshaller = AckMessageResponseUnmarshaller.Instance;

            return _serviceClient.Invoke<AckMessageRequest, AckMessageResponse>(request, marshaller, unmarshaller);
        }

        /// <summary>
        /// rollback transaction msg, the consumer will not receive the msg.
        /// </summary>
        /// <returns>The rollback.</returns>
        /// <param name="receiptHandle">Receipt handle.</param>
        public AckMessageResponse Rollback(string receiptHandle)
        {
            List<string> handlers = new List<string>
            {
                receiptHandle
            };

            var request = new AckMessageRequest(this._topicName, this._groupId, handlers);
            request.IntanceId = this._instanceId;
            request.Trasaction = "rollback";
            var marshaller = new AckMessageRequestMarshaller();
            var unmarshaller = AckMessageResponseUnmarshaller.Instance;

            return _serviceClient.Invoke<AckMessageRequest, AckMessageResponse>(request, marshaller, unmarshaller);
        }

        /// <summary>
        /// Consumes the half tranaction message.
        /// </summary>
        /// <returns>The half message.</returns>
        /// <param name="batchSize">Batch size. 1~16</param>
        /// <param name="waitSeconds">Wait seconds. 1~30 is valid, others will be ignored.</param>
        public List<Message> ConsumeHalfMessage(uint batchSize, uint waitSeconds)
        {
            var request = new ConsumeMessageRequest(this._topicName, this._groupId, null);
            request.IntanceId = this._instanceId;
            request.BatchSize = batchSize;
            request.WaitSeconds = waitSeconds;
            request.Trasaction = "pop";
            var marshaller = ConsumeMessageRequestMarshaller.Instance;
            var unmarshaller = ConsumeMessageResponseUnmarshaller.Instance;

            ConsumeMessageResponse result = _serviceClient.Invoke<ConsumeMessageRequest, ConsumeMessageResponse>(request, marshaller, unmarshaller);

            return result.Messages;
        }
    }
}
