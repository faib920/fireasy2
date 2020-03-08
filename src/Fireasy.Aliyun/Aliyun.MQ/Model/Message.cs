
using System;
using System.Collections.Generic;
using Aliyun.MQ.Util;

namespace Aliyun.MQ.Model
{

    public partial class Message
    {
        private string _id;
        private string _receiptHandle;
        private string _bodyMD5;
        private string _body;

        private string _messageTag;
        private long _publishTime;
        private long _nextConsumeTime;
        private long _firstConsumeTime;
        
        private uint _consumedTimes;

        private readonly IDictionary<string, string> _properties = new Dictionary<string, string>();

        public Message() { }

      
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

        public string ReceiptHandle
        {
            get { return this._receiptHandle; }
            set { this._receiptHandle = value; }
        }

        internal bool IsSetReceiptHandle()
        {
            return this._receiptHandle != null;
        }

        public string Body
        {
            get { return this._body; }
            set { this._body = value; }
        }

        internal bool IsSetBody()
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

        public string MessageTag
        {
            get { return this._messageTag; }
            set { this._messageTag = value; }
        }

        public long PublishTime
        {
            get { return this._publishTime; }
            set { this._publishTime = value; }
        }

        public long NextConsumeTime
        {
            get { return this._nextConsumeTime; }
            set { this._nextConsumeTime = value; }
        }

        public long FirstConsumeTime
        {
            get { return this._firstConsumeTime; }
            set { this._firstConsumeTime = value; }
        }

        public uint ConsumedTimes
        {
            get { return this._consumedTimes; }
            set { this._consumedTimes = value; }
        }

        public IDictionary<string, string> Properties
        {
            get { return this._properties; }
        }

        public string GetProperty(string key)
        {
            return this._properties[key];
        }

        public string MessageKey
        {
            get { return this._properties[Constants.MESSAGE_PROPERTIES_MSG_KEY]; }
        }

        public long StartDeliverTime
        {
            get 
            {
                return _properties.ContainsKey(Constants.MESSAGE_PROPERTIES_TIMER_KEY)
                                  ? long.Parse(this._properties[Constants.MESSAGE_PROPERTIES_TIMER_KEY])
                                      : 0;
            }
        }

        public uint TransCheckImmunityTime
        {
            get 
            {
                return _properties.ContainsKey(Constants.MESSAGE_PROPERTIES_TRANS_CHECK_KEY)
                                  ? uint.Parse(this._properties[Constants.MESSAGE_PROPERTIES_TRANS_CHECK_KEY])
                                      : 0;
            }
        }

        public override string ToString()
        {
            return string.Format(
                "ID:{0}, PublishTime:{1}, NextConsumeTime:{2}, ConsumedTimes:{3}, " +
                "\nTag:{4}, BodyMD5:{5}, NextConsumeTime:{6}" +
                "\nBody:{7}" +
                "\nProperties:{8}" +
                "\nMessageKey:{9}",
                Id, PublishTime, NextConsumeTime, ConsumedTimes,
                MessageTag, BodyMD5, NextConsumeTime, Body, AliyunSDKUtils.DictToString(Properties),
                MessageKey
            );
        }
    }
}
