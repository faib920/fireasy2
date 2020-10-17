using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aliyun.MQ.Runtime.Internal;

namespace Aliyun.MQ.Model
{
    public partial class ConsumeMessageRequest: WebServiceRequest
    {
        private string _instanceId;
        private string _topicName;
        private string _consumer;
        private string _messageTag;
        private uint _batchSize;
        private uint? _waitSeconds;
        private string _trans;

        public ConsumeMessageRequest(string topicName, string consumer)
        {
            this._topicName = topicName;
            this._consumer = consumer;
        }

        public ConsumeMessageRequest(string topicName, string consumer, string messageTag)
        {
            this._topicName = topicName;
            this._consumer = consumer;
            this._messageTag = messageTag;
        }

        public string TopicName
        {
            get { return this._topicName; }
        }

        public string Consumer
        {
            get { return this._consumer; }
        }

        public string MessageTag
        {
            get { return this._messageTag; }
        }

        public bool IsSetMessageTag()
        {
            return this._messageTag != null;
        }

        public bool IsSetWaitSeconds()
        {
            return this._waitSeconds.HasValue;
        }

        public uint WaitSeconds
        {
            get { return this._waitSeconds.GetValueOrDefault(); }
            set { this._waitSeconds = value; }
        }

        public uint BatchSize
        {
            get { return this._batchSize; }
            set { this._batchSize = value; }
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

        internal string Trasaction
        {
            set { this._trans = value; }
            get { return this._trans; }
        }

        internal bool IsSetTransaction()
        {
            return !string.IsNullOrEmpty(this._trans);
        }
    }
}
