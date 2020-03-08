using Aliyun.MQ.Runtime;
using System.Collections.Generic;

namespace Aliyun.MQ.Model.Exp
{
    public class AckMessageException : AliyunServiceException
    {
        private List<AckMessageErrorItem> _errorItems = new List<AckMessageErrorItem>();

        public AckMessageException() : base(Aliyun.MQ.Model.Exp.ErrorCode.AckMessageFail)
        {
            this.ErrorCode = Aliyun.MQ.Model.Exp.ErrorCode.AckMessageFail;
        }
      
        public List<AckMessageErrorItem> ErrorItems
        {
            get { return this._errorItems; }
            set { this._errorItems = value; }
        }
    }

    public class AckMessageErrorItem
    {
        private string _errorCode;
        private string _errorMessage;
        private string _receiptHandle;

        public AckMessageErrorItem()
        {
        }

        public AckMessageErrorItem(string errorCode, string errorMessage, string receiptHandle)
        {
            this._errorCode = errorCode;
            this._errorMessage = errorMessage;
            this._receiptHandle = receiptHandle;
        }

        public override string ToString()
        {
            return string.Format("ErrorCode is {0}, ErrorMessage is {1}, ReceiptHandle is {2}",
                _errorCode, _errorMessage, _receiptHandle);
        }

        public string ErrorCode
        {
            get { return this._errorCode; }
            set { this._errorCode = value; }
        }

        public string ErrorMessage
        {
            get { return this._errorMessage; }
            set { this._errorMessage = value; }
        }

        public string ReceiptHandle
        {
            get { return this._receiptHandle; }
            set { this._receiptHandle = value; }
        }
    }
}
