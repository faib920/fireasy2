using System;
using System.Collections.Generic;
using System.IO;

namespace Aliyun.MQ.Runtime.Internal
{
    public interface IRequest
    {
        string RequestName
        {
            get;
        }

        IDictionary<string, string> Headers
        {
            get;
        }

        bool UseQueryString
        {
            get;
            set;
        }

        IDictionary<String, String> Parameters
        {
            get;
        }

        IDictionary<string, string> SubResources
        {
            get;
        }

        void AddSubResource(string subResource);

        void AddSubResource(string subResource, string value);

        string HttpMethod
        {
            get;
            set;
        }

        Uri Endpoint
        {
            get;
            set;
        }

        string ResourcePath
        {
            get;
            set;
        }

        byte[] Content
        {
            get;
            set;
        }

        Stream ContentStream
        {
            get;
            set;
        }

        long OriginalStreamPosition
        {
            get;
            set;
        }

        string ServiceName
        {
            get;
        }

        WebServiceRequest OriginalRequest
        {
            get;
        }

        bool Suppress404Exceptions
        {
            get;
            set;
        }

        bool IsRequestStreamRewindable();

        bool MayContainRequestBody();

        bool HasRequestBody();
    }
}
