using System;
using System.Collections.Generic;
using System.IO;
using Aliyun.MQ.Model;

namespace Aliyun.MQ.Runtime.Internal
{
    internal class DefaultRequest : IRequest
    {
        readonly IDictionary<string, string> parameters = new Dictionary<string, string>(StringComparer.Ordinal);
        readonly IDictionary<string, string> headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        readonly IDictionary<string, string> subResources = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        Uri endpoint;
        string resourcePath;
        string serviceName;
        readonly WebServiceRequest originalRequest;
        byte[] content;
        Stream contentStream;
        string httpMethod = "GET";
        bool useQueryString = false;
        string requestName;
        long originalStreamLength;

        public DefaultRequest(WebServiceRequest request, String serviceName)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (string.IsNullOrEmpty(serviceName)) throw new ArgumentNullException("serviceName");

            this.serviceName = serviceName;
            this.originalRequest = request;
            this.requestName = this.originalRequest.GetType().Name;

            foreach (var header in request.Headers)
                this.Headers.Add(header.Key, header.Value);
            foreach (var param in request.Parameters)
                this.Parameters.Add(param.Key, param.Value);
        }

        public string RequestName
        {
            get { return this.requestName; }
        }

        public string HttpMethod
        {
            get
            {
                return this.httpMethod;
            }
            set
            {
                this.httpMethod = value;
            }
        }

        public bool UseQueryString
        {
            get
            {
                return this.useQueryString;
            }
            set
            {
                this.useQueryString = value;
            }
        }

        public WebServiceRequest OriginalRequest
        {
            get
            {
                return originalRequest;
            }
        }

        public IDictionary<string, string> Headers
        {
            get
            {
                return this.headers;
            }
        }


        public IDictionary<string, string> Parameters
        {
            get
            {
                return this.parameters;
            }
        }

        public IDictionary<string, string> SubResources
        {
            get
            {
                return this.subResources;
            }
        }

        public void AddSubResource(string subResource)
        {
            AddSubResource(subResource, null);
        }

        public void AddSubResource(string subResource, string value)
        {
            SubResources.Add(subResource, value);
        }

        public Uri Endpoint
        {
            get
            {
                return this.endpoint;
            }
            set
            {
                this.endpoint = value;
            }
        }

        public string ResourcePath
        {
            get
            {
                return this.resourcePath;
            }
            set
            {
                this.resourcePath = value;
            }
        }

        public byte[] Content
        {
            get
            {
                return this.content;
            }
            set
            {
                this.content = value;
            }
        }

        public Stream ContentStream
        {
            get { return this.contentStream; }
            set
            {
                this.contentStream = value;
                OriginalStreamPosition = -1;
                if (this.contentStream != null && this.contentStream.CanSeek)
                    OriginalStreamPosition = this.contentStream.Position;
            }
        }

        public long OriginalStreamPosition
        {
            get { return this.originalStreamLength; }
            set { this.originalStreamLength = value; }
        }

        public string ServiceName
        {
            get
            {
                return this.serviceName;
            }
        }

        public bool Suppress404Exceptions
        {
            get;
            set;
        }

        public bool IsRequestStreamRewindable()
        {
            // Retries may not be possible with a stream
            if (this.ContentStream != null)
            {
                // Retry is possible if stream is seekable
                return this.ContentStream.CanSeek;
            }
            return true;
        }

        public bool MayContainRequestBody()
        {
            return !this.UseQueryString &&
                (this.HttpMethod == "POST" ||
                 this.HttpMethod == "PUT" ||
                 this.HttpMethod == "DELETE");
        }

        public bool HasRequestBody()
        {
            return (this.HttpMethod == "POST" ||
                    this.HttpMethod == "PUT" ||
                    this.HttpMethod == "DELETE") &&
                ((this.Content != null) ||
                        this.ContentStream != null);
        }
    }
}
