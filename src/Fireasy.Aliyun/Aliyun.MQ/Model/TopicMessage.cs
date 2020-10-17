
using System;
using System.Collections.Generic;
using Aliyun.MQ.Util;

namespace Aliyun.MQ.Model
{

    public partial class TopicMessage
    {
        private string _id;
        private string _bodyMD5;
        // only transaction msg have
        private string _receiptHandle;

        private string _body;
        private string _messageTag;
        private IDictionary<string, string> _properties = new Dictionary<string, string>();


        public TopicMessage(string body) 
        {
            this._body = body;
        }

        public TopicMessage(string body, string messageTag)
        {
            this._body = body;
            this._messageTag = messageTag;
        }


        public string Id
        {
            get { return this._id; }
            set { this._id = value; }
        }

        // Check to see if Id property is set
        internal bool IsSetId()
        {
            return this._id != null;
        }

        public string Body
        {
            get { return this._body; }
        }

        public string MessageTag
        {
            get { return this._messageTag; }
            set { this._messageTag = value; }
        }

        public bool IsSetBody()
        {
            return this._body != null;
        }

        public string BodyMD5
        {
            get { return this._bodyMD5; }
            set { this._bodyMD5 = value; }
        }

        internal bool IsSetBodyMD5()
        {
            return this._bodyMD5 != null;
        }

        /// <summary>
        /// 发送事务消息的消息句柄，普通消息为空
        /// </summary>
        /// <value>The receipt handle.</value>
        public string ReceiptHandle
        {
            get { return this._receiptHandle; }
            set { this._receiptHandle = value; }
        }

        public IDictionary<string, string> Properties
        {
            get { return this._properties; }
            set { this._properties = value; }
        }

        public void PutProperty(string key, string value)
        {
            this._properties.Add(key, value);
        }

        /// <summary>
        /// 定时消息，单位毫秒（ms），在指定时间戳（当前时间之后）进行投递。如果被设置成当前时间戳之前的某个时刻，消息将立刻投递给消费者
        /// </summary>
        /// <param name="value">Millis.</param>
        public long StartDeliverTime
        {
            set { this._properties.Add(Constants.MESSAGE_PROPERTIES_TIMER_KEY, value.ToString()); }
        }

        /// <summary>
        /// Sets the message key.
        /// </summary>
        /// <param name="value">Key.</param>
        public string MessageKey
        {
            set { this._properties.Add(Constants.MESSAGE_PROPERTIES_MSG_KEY, value); }
        }

        /// <summary>
        /// 在消息属性中添加第一次消息回查的最快时间，单位秒，并且表征这是一条事务消息
        /// </summary>
        /// <value>The trans check immunity time.</value>
        public uint TransCheckImmunityTime
        {
            set { this._properties.Add(Constants.MESSAGE_PROPERTIES_TRANS_CHECK_KEY, value.ToString()); }
        }

        internal bool IsSetReeceiptHandle()
        {
            return !string.IsNullOrEmpty(_receiptHandle);
        }

        public override string ToString()
        {
            return IsSetReeceiptHandle()
                ? string.Format("(MessageId {0}, MessageBodyMD5 {1}, Handle {2})", _id, _bodyMD5, _receiptHandle)
                : string.Format("(MessageId {0}, MessageBodyMD5 {1})", _id, _bodyMD5);
        }
    }
}
