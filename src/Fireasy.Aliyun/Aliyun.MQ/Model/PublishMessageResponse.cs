
using Aliyun.MQ.Runtime;

namespace Aliyun.MQ.Model
{
    public partial class PublishMessageResponse : WebServiceResponse
    {
        private string _messageBodyMD5;
        private string _messageId;
        private string _receiptHandle;

        public string MessageBodyMD5
        {
            get { return this._messageBodyMD5; }
            set { this._messageBodyMD5 = value; }
        }

        // Check to see if BodyMD5 property is set
        internal bool IsSetMessageBodyMD5()
        {
            return this._messageBodyMD5 != null;
        }

        public string MessageId
        {
            get { return this._messageId; }
            set { this._messageId = value; }
        }

        // Check to see if MessageId property is set
        internal bool IsSetMessageId()
        {
            return this._messageId != null;
        }

        public string ReeceiptHandle
        {
            get { return this._receiptHandle; }
            set { this._receiptHandle = value; }
        }

        internal bool IsSetReeceiptHandle()
        {
            return !string.IsNullOrEmpty(_receiptHandle);
        }

        public override string ToString()
        {
            return IsSetReeceiptHandle()
                ? string.Format("(MessageId {0}, MessageBodyMD5 {1}, Handle {2})", _messageId, _messageBodyMD5, _receiptHandle)
                : string.Format("(MessageId {0}, MessageBodyMD5 {1})", _messageId, _messageBodyMD5);
        }
    }
}
