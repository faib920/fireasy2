
using System;
using Aliyun.MQ.Model.Exp;
using Aliyun.MQ.Runtime;
using Aliyun.MQ.Runtime.Internal.Auth;

namespace Aliyun.MQ
{
    public partial class MQClient : AliyunServiceClient
    {
        #region Constructors

        public MQClient(string accessKeyId, string secretAccessKey, string regionEndpoint)
            : base(accessKeyId, secretAccessKey, new MQConfig { RegionEndpoint = new Uri(regionEndpoint) }, null)
        {
        }

		public MQClient(string accessKeyId, string secretAccessKey, string regionEndpoint, string stsToken)
			: base(accessKeyId, secretAccessKey, new MQConfig { RegionEndpoint = new Uri(regionEndpoint) }, stsToken)
		{
		}

        #endregion

        #region Overrides

        protected override IServiceSigner CreateSigner()
        {
            return new MQSigner();
        }

        #endregion

        #region Dispose

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        #endregion


        public MQProducer GetProducer(string instanceId, string topicName)
        {
            if (string.IsNullOrEmpty(topicName)) {
                throw new MQException("TopicName is null or empty");
            }
            return new MQProducer(instanceId, topicName, this);
        }

        public MQTransProducer GetTransProdcuer(string instanceId, string topicName, string groupId)
        {
            if (string.IsNullOrEmpty(topicName))
            {
                throw new MQException("TopicName is null or empty");
            }

            if (string.IsNullOrEmpty(groupId))
            {
                throw new MQException("TopicName is null or empty");
            }

            return new MQTransProducer(instanceId, topicName, groupId, this);
        }

        public MQConsumer GetConsumer(string instanceId, string topicName, String consumer, String messageTag)
        {
            if (string.IsNullOrEmpty(topicName))
            {
                throw new MQException("TopicName is null or empty");
            }
            if (string.IsNullOrEmpty(consumer))
            {
                throw new MQException("Consumer is null or empty");
            }
            return new MQConsumer(instanceId, topicName, consumer, messageTag, this);
        }
    }
}
