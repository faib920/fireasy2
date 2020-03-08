
using System;
using Aliyun.MQ.Runtime.Internal;

namespace Aliyun.MQ.Model
{
    public partial class PublishMessageRequest: WebServiceRequest
    {
        private string _messageBody;
        private string _messageTag;
        private string _properties;
        private string _topicName;
        private string _instanceId;

        public PublishMessageRequest() { }

        public PublishMessageRequest(string messageBody)
            : this(messageBody, null)
        {
        }

        public PublishMessageRequest(string messageBody, string messageTag)
        {
            _messageBody = messageBody;
            _messageTag = messageTag;
        }

        public string MessageBody
        {
            get { return this._messageBody; }
            set { this._messageBody = value; }
        }

        internal bool IsSetMessageBody()
        {
            return this._messageBody != null;
        }

        public string MessageTag
        {
            get { return this._messageTag; }
            set { this._messageTag = value; }
        }

        internal bool IsSetMessageTag()
        {
            return this._messageTag != null;
        }

        public string TopicName
        {
            get { return this._topicName; }
            set { this._topicName = value; }
        }

        internal bool IsSetTopicName()
        {
            return this._topicName != null;
        }

        public string IntanceId
        {
            get { return this._instanceId; }
            set { this._instanceId = value; }
        }

        public bool IsSetInstance()
        {
            return !string.IsNullOrEmpty(this._instanceId);
        }

        public string Properties
        {
            get { return this._properties; }
            set { this._properties = value; }
        }

        public bool IsSetProperties()
        {
            return !string.IsNullOrEmpty(this._properties);
        }
    }
}
