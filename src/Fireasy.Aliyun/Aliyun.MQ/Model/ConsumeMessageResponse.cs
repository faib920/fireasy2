using Aliyun.MQ.Model.Internal.MarshallTransformations;
using Aliyun.MQ.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aliyun.MQ.Model
{
    public partial class ConsumeMessageResponse : WebServiceResponse
    {
        private List<Message> _messages = new List<Message>();

        public List<Message> Messages
        {
            get { return this._messages; }
            set { this._messages = value; }
        }
    }
}
