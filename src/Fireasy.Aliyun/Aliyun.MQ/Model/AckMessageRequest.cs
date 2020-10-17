using Aliyun.MQ.Runtime.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aliyun.MQ.Model
{
    public partial class AckMessageRequest: WebServiceRequest
    {
        private string _instanceId;
        private string _topicName;
        private string _consumer;
        private List<string> _receiptHandles = new List<string>();
        private string _trans;

        public AckMessageRequest(string topicName, string consumer, List<String> receiptHandles)
        {
            this._topicName = topicName;
            this._consumer = consumer;
            this._receiptHandles = receiptHandles;
        }

        public string TopicName
        {
            get { return this._topicName; }
        }

        public string Consumer
        {
            get { return this._consumer; }
        }

        public List<string> ReceiptHandles
        {
            get { return this._receiptHandles; }
            set { this._receiptHandles = value; }
        }

        public bool IsSetReceiptHandles()
        {
            return _receiptHandles.Any();
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
