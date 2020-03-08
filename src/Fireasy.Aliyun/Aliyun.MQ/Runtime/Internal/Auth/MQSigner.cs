using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
using Aliyun.MQ.Runtime.Internal.Util;
using Aliyun.MQ.Util;

namespace Aliyun.MQ.Runtime.Internal.Auth
{
    internal class MQSigner : IServiceSigner
    {
        #region Immutable Properties

        static readonly Regex CompressWhitespaceRegex = new Regex("\\s+");
        const SigningAlgorithm SignerAlgorithm = SigningAlgorithm.HmacSHA1;

        #endregion

        #region Public Signing Methods

        public void Sign(IRequest request, 
                                  string accessKeyId, 
                                  string secretAccessKey,
                                  string stsToken)
        {
            var signingRequest = SignRequest(request, secretAccessKey);
            var signingResult = new StringBuilder();
            signingResult.AppendFormat("{0} {1}:{2}", 
                                             Constants.MQ_AUTHORIZATION_HEADER_PREFIX,
                                             accessKeyId, 
                                             signingRequest);
            request.Headers[Constants.AuthorizationHeader] = signingResult.ToString();

            if (!string.IsNullOrEmpty(stsToken))
            {
                request.Headers[Constants.SecurityToken] = stsToken;
            }
        }
       
        public string SignRequest(IRequest request, string secretAccessKey)
        {
            InitializeHeaders(request.Headers);

            var parametersToCanonicalize = GetParametersToCanonicalize(request);
            var canonicalParameters = CanonicalizeQueryParameters(parametersToCanonicalize);
            var canonicalResource = CanonicalizeResource(canonicalParameters, request.ResourcePath);
            var canonicalHeaders = CanonoicalizeHeaders(request.Headers);
            
            var canonicalRequest = CanonicalizeRequest(request.HttpMethod,
                                                       request.Headers,
                                                       canonicalHeaders,
                                                       canonicalResource);

            return ComputeSignature(secretAccessKey, canonicalRequest);
        }

        #endregion

        #region Public Signing Helpers

        private static void InitializeHeaders(IDictionary<string, string> headers)
        {
            // clean up any prior signature in the headers if resigning
            headers.Remove(Constants.AuthorizationHeader);
        }

        public static string ComputeSignature(string secretAccessKey, string canonicalRequest)
        {
            return ComputeKeyedHash(SignerAlgorithm, secretAccessKey, canonicalRequest);
        }

        public static string ComputeKeyedHash(SigningAlgorithm algorithm, string key, string data)
        {
            return CryptoUtilFactory.CryptoInstance.HMACSign(data, key, algorithm);
        }

        #endregion

        #region Private Signing Helpers

        protected static string CanonoicalizeHeaders(IDictionary<string, string> headers)
        {
            var headersToCanonicalize = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (headers != null && headers.Count > 0)
            {
                foreach (var header in headers.Where(header =>
                    header.Key.ToLowerInvariant().StartsWith(Constants.X_MQ_HEADER_PREFIX)))
                {
                    headersToCanonicalize.Add(header.Key.ToLowerInvariant(), header.Value);
                }
            }
            return CanonicalizeHeaders(headersToCanonicalize);
        }

        protected static string CanonicalizeResource(string canonicalQueryString, string resourcePath)
        {
            var canonicalResource = new StringBuilder();
            canonicalResource.Append(CanonicalizeResourcePath(resourcePath));
            if (canonicalQueryString != string.Empty)
                canonicalResource.AppendFormat("?{0}", canonicalQueryString);
            return canonicalResource.ToString();
        }

        protected static string CanonicalizeResourcePath(string resourcePath)
        {
            if (string.IsNullOrEmpty(resourcePath) || resourcePath == "/")
                return "/";

            var pathSegments = resourcePath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var canonicalizedPath = new StringBuilder();
            foreach (var segment in pathSegments)
            {
                canonicalizedPath.AppendFormat("/{0}", segment);
            }

            if (resourcePath.EndsWith("/", StringComparison.Ordinal))
                canonicalizedPath.Append("/");

            return canonicalizedPath.ToString();
        }

        protected static string CanonicalizeRequest(string httpMethod,
                                                    IDictionary<string, string> headers,
                                                    string canonicalHeaders,
                                                    string canonicalResource)
        {
            var canonicalRequest = new StringBuilder();
            canonicalRequest.AppendFormat("{0}\n", httpMethod);
            
            var contentMD5 = string.Empty;
            if (headers.ContainsKey(Constants.ContentMD5Header))
                contentMD5 = headers[Constants.ContentMD5Header];
            canonicalRequest.AppendFormat("{0}\n", contentMD5);
            
            var contentType = string.Empty;
            if (headers.ContainsKey(Constants.ContentTypeHeader))
                contentType = headers[Constants.ContentTypeHeader];
            canonicalRequest.AppendFormat("{0}\n", contentType);

            canonicalRequest.AppendFormat("{0}\n", headers[Constants.DateHeader]);
            canonicalRequest.Append(canonicalHeaders);
            canonicalRequest.Append(canonicalResource);

            return canonicalRequest.ToString();
        }

        protected static string CanonicalizeHeaders(ICollection<KeyValuePair<string, string>> sortedHeaders)
        {
            if (sortedHeaders == null || sortedHeaders.Count == 0)
                return string.Empty;

            var builder = new StringBuilder();
            foreach (var entry in sortedHeaders)
            {
                builder.Append(entry.Key.ToLower(CultureInfo.InvariantCulture));
                builder.Append(":");
                builder.Append(CompressSpaces(entry.Value));
                builder.Append("\n");
            }

            return builder.ToString();
        }

        protected static IDictionary<string, string> GetParametersToCanonicalize(IRequest request)
        {
            var parametersToCanonicalize = new Dictionary<string, string>();

            if (request.SubResources != null && request.SubResources.Count > 0)
            {
                foreach (var subResource in request.SubResources)
                {
                    parametersToCanonicalize.Add(subResource.Key, subResource.Value);
                }
            }

            if (request.Parameters != null && request.Parameters.Count > 0)
            {
                foreach (var queryParameter in request.Parameters.Where(queryParameter => queryParameter.Value != null))
                {
                    parametersToCanonicalize.Add(queryParameter.Key, queryParameter.Value);
                }
            }

            return parametersToCanonicalize;
        }

        protected static string CanonicalizeQueryParameters(IDictionary<string, string> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return string.Empty;

            var canonicalQueryString = new StringBuilder();
            var queryParams = new SortedDictionary<string, string>(parameters, StringComparer.Ordinal);
            foreach (var p in queryParams)
            {
                if (canonicalQueryString.Length > 0)
                    canonicalQueryString.Append("&");
                if (string.IsNullOrEmpty(p.Value))
                    canonicalQueryString.AppendFormat("{0}=", p.Key);
                else
                    canonicalQueryString.AppendFormat("{0}={1}", p.Key, p.Value);
            }

            return canonicalQueryString.ToString();
        }

        static string CompressSpaces(string data)
        {
            if (data == null || !data.Contains(" "))
                return data;

            var compressed = CompressWhitespaceRegex.Replace(data, " ");
            return compressed;
        }

        #endregion
    }
}
