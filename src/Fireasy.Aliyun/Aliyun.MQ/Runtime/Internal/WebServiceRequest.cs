using System;
using System.Collections.Generic;

namespace Aliyun.MQ.Runtime.Internal
{
    public abstract partial class WebServiceRequest
    {
        readonly IDictionary<string, string> _parameters = new Dictionary<string, string>(StringComparer.Ordinal);
        readonly IDictionary<string, string> _headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private string _contentType;
        private string _contentMD5;

        public IDictionary<string, string> Headers
        {
            get
            {
                return this._headers;
            }
        }

        public void AddHeader(string headerName, string value)
        {
            Headers.Add(headerName, value);
        }

        public IDictionary<string, string> Parameters
        {
            get
            {
                return this._parameters;
            }
        }

        public void AddParameter(string paramName, string value)
        {
            Parameters.Add(paramName, value);
        }

        public string ContentType
        {
            get { return this._contentType; }
            set { this._contentType = value; }
        }

        public bool IsSetContentType()
        {
            return this._contentType != null;
        }

        public string ContentMD5
        {
            get { return this._contentMD5; }
            set { this._contentMD5 = value; }
        }

        public bool IsSetContentMD5()
        {
            return this._contentMD5 != null;
        }

        private TimeSpan? _timeoutInternal;

        internal TimeSpan? TimeoutInternal
        {
            get { return this._timeoutInternal; }
            set
            {
                ClientConfig.ValidateTimeout(value);
                this._timeoutInternal = value;
            }
        }

        private TimeSpan? _readWriteTimeoutInternal;

        internal TimeSpan? ReadWriteTimeoutInternal
        {
            get { return this._readWriteTimeoutInternal; }
            set
            {
                ClientConfig.ValidateTimeout(value);
                this._readWriteTimeoutInternal = value;
            }
        }

        internal virtual bool Expect100Continue
        {
            get { return true; }            
        }

        internal virtual bool KeepAlive
        {
            get { return true; }
        }
    }
}
