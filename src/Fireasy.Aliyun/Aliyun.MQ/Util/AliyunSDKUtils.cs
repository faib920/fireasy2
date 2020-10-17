using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Win32;
using Aliyun.MQ.Model.Exp;

namespace Aliyun.MQ.Util
{
    public static partial class AliyunSDKUtils
    {
        #region Internal Constants

        internal const string SDKVersionNumber = "1.0.1";
        internal static string _userAgentBaseName = "mq-csharp-sdk";

        private const int DefaultConnectionLimit = 50;
        private const int DefaultMaxIdleTime = 50 * 1000; // 50 seconds

        internal static readonly DateTime EPOCH_START = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        internal const int DefaultBufferSize = 8192;

        internal static Dictionary<int, string> RFCEncodingSchemes = new Dictionary<int, string>
        {
            { 3986,  ValidUrlCharacters },
            { 1738,  ValidUrlCharactersRFC1738 }
        };

        #endregion

        #region Public Constants

        public const string ValidUrlCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";

        public const string ValidUrlCharactersRFC1738 = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.";

        private static string ValidPathCharacters = DetermineValidPathCharacters();

        private static string DetermineValidPathCharacters()
        {
            const string basePathCharacters = "/:'()!*[]";

            var sb = new StringBuilder();
            foreach (var c in basePathCharacters)
            {
                var escaped = Uri.EscapeUriString(c.ToString());
                if (escaped.Length == 1 && escaped[0] == c)
                    sb.Append(c);
            }
            return sb.ToString();
        }

        public const string ISO8601DateFormat = "yyyy-MM-dd\\THH:mm:ss.fff\\Z";

        public const string RFC822DateFormat = "ddd, dd MMM yyyy HH:mm:ss \\G\\M\\T";

        #endregion

        #region UserAgent

        static string _versionNumber;
        static string _sdkUserAgent;

        public static string SDKUserAgent
        {
            get
            {
                return _sdkUserAgent;
            }
        }

        static AliyunSDKUtils()
        {
            BuildUserAgentString();
        }

        static void BuildUserAgentString()
        {
            if (_versionNumber == null)
            {
                _versionNumber = SDKVersionNumber;
            }

            try
            {
                _sdkUserAgent = string.Format(CultureInfo.InvariantCulture, "{0}/{1}(.NET/{2} {3}/{4})",
                    _userAgentBaseName,
                    _versionNumber,
                    DetermineRuntime(),
                    Environment.OSVersion.Platform,
                    DetermineOSVersion());
            }
            catch (Exception)
            {
                _sdkUserAgent = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", _userAgentBaseName, _versionNumber);
            }
        }

        static string DetermineRuntime()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}.{1}", Environment.Version.Major, Environment.Version.MajorRevision);
        }

        static string DetermineOSVersion()
        {
            return Environment.OSVersion.Version.ToString();
        }

        #endregion

        #region HTTP Connection Configurations

        private const int _defaultConnectionLimit = 2;
        internal static int GetConnectionLimit(int? clientConfigValue)
        {
            // Connection limit has been explicitly set on the client.
            if (clientConfigValue.HasValue)
                return clientConfigValue.Value;

            // If default has been left at the system default return the SDK default.
            if (ServicePointManager.DefaultConnectionLimit == _defaultConnectionLimit)
                return DefaultConnectionLimit;

            // The system default has been explicitly changed so we will honor that value.
            return ServicePointManager.DefaultConnectionLimit;
        }

        private const int _defaultMaxIdleTime = 100 * 1000;
        internal static int GetMaxIdleTime(int? clientConfigValue)
        {
            // MaxIdleTime has been explicitly set on the client.
            if (clientConfigValue.HasValue)
                return clientConfigValue.Value;

            // If default has been left at the system default return the SDK default.
            if (ServicePointManager.MaxServicePointIdleTime == _defaultMaxIdleTime)
                return DefaultMaxIdleTime;

            // The system default has been explicitly changed so we will honor that value.
            return ServicePointManager.MaxServicePointIdleTime;
        }

        #endregion

        #region Internal Methods


        internal static string GetParametersAsString(IDictionary<string, string> parameters)
        {
            string[] keys = new string[parameters.Keys.Count];
            parameters.Keys.CopyTo(keys, 0);
            Array.Sort<string>(keys);

            StringBuilder data = new StringBuilder(512);
            foreach (string key in keys)
            {
                string value = parameters[key];
                if (value != null)
                {
                    data.Append(key);
                    data.Append('=');
                    data.Append(value);
                    data.Append('&');
                }
            }
            string result = data.ToString();
            if (result.Length == 0)
                return string.Empty;

            return result.Remove(result.Length - 1);
        }

        public static void CopyTo(this Stream input, Stream output)
        {
            byte[] buffer = new byte[16 * 1024];
            int bytesRead;

            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, bytesRead);
            }
        }

        /// <summary>
        /// Utility method for converting Unix epoch seconds to DateTime structure.
        /// </summary>
        /// <param name="milliSeconds">The number of milliSeconds since January 1, 1970.</param>
        /// <returns>Converted DateTime structure</returns>
        public static DateTime ConvertFromUnixEpochSeconds(long milliSeconds)
        {
            return new DateTime(milliSeconds * 10000L + EPOCH_START.Ticks, DateTimeKind.Utc).ToLocalTime();
        }

        public static long GetUnixTimeStamp(DateTime dateTime)
        {
            TimeSpan ts = dateTime - EPOCH_START;
            return (long)ts.TotalMilliseconds;
        }

        public static long GetNowTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalMilliseconds);
        }

        #endregion

        #region Public Methods and Properties

        /// <summary>
        /// Formats the current date as ISO 8601 timestamp
        /// </summary>
        /// <returns>An ISO 8601 formatted string representation
        /// of the current date and time
        /// </returns>
        public static string FormattedCurrentTimestampRFC822
        {
            get
            {
                return GetFormattedTimestampRFC822(0);
            }
        }

        /// <summary>
        /// Gets the RFC822 formatted timestamp that is minutesFromNow
        /// in the future.
        /// </summary>
        /// <param name="minutesFromNow">The number of minutes from the current instant
        /// for which the timestamp is needed.</param>
        /// <returns>The ISO8601 formatted future timestamp.</returns>
        public static string GetFormattedTimestampRFC822(int minutesFromNow)
        {
            DateTime dateTime = DateTime.UtcNow.AddMinutes(minutesFromNow);
            DateTime formatted = new DateTime(
                dateTime.Year,
                dateTime.Month,
                dateTime.Day,
                dateTime.Hour,
                dateTime.Minute,
                dateTime.Second,
                dateTime.Millisecond,
                DateTimeKind.Local
                );
            return formatted.ToString(
                RFC822DateFormat,
                CultureInfo.InvariantCulture
                );
        }


        /// <summary>
        /// URL encodes a string per RFC3986. If the path property is specified,
        /// the accepted path characters {/+:} are not encoded.
        /// </summary>
        /// <param name="data">The string to encode</param>
        /// <param name="path">Whether the string is a URL path or not</param>
        /// <returns>The encoded string</returns>
        public static string UrlEncode(string data, bool path)
        {
            return UrlEncode(3986, data, path);
        }

        /// <summary>
        /// URL encodes a string per the specified RFC. If the path property is specified,
        /// the accepted path characters {/+:} are not encoded.
        /// </summary>
        /// <param name="rfcNumber">RFC number determing safe characters</param>
        /// <param name="data">The string to encode</param>
        /// <param name="path">Whether the string is a URL path or not</param>
        /// <returns>The encoded string</returns>
        /// <remarks>
        /// Currently recognised RFC versions are 1738 (Dec '94) and 3986 (Jan '05). 
        /// If the specified RFC is not recognised, 3986 is used by default.
        /// </remarks>
        internal static string UrlEncode(int rfcNumber, string data, bool path)
        {
            StringBuilder encoded = new StringBuilder(data.Length * 2);
            string validUrlCharacters;
            if (!RFCEncodingSchemes.TryGetValue(rfcNumber, out validUrlCharacters))
                validUrlCharacters = ValidUrlCharacters;

            string unreservedChars = String.Concat(validUrlCharacters, (path ? ValidPathCharacters : ""));

            foreach (char symbol in Encoding.UTF8.GetBytes(data))
            {
                if (unreservedChars.IndexOf(symbol) != -1)
                {
                    encoded.Append(symbol);
                }
                else
                {
                    encoded.Append("%").Append(string.Format(CultureInfo.InvariantCulture, "{0:X2}", (int)symbol));
                }
            }

            return encoded.ToString();
        }

        public static void Sleep(int ms)
        {
            Thread.Sleep(ms);
        }

        static readonly object _preserveStackTraceLookupLock = new object();
        static bool _preserveStackTraceLookup = false;
        static MethodInfo _preserveStackTrace;

        /// <summary>
        /// This method is used preserve the stacktrace used from clients that support async calls.  This 
        /// make sure that exceptions thrown during EndXXX methods has the orignal stacktrace that happen 
        /// in the background thread.
        /// </summary>
        /// <param name="exception"></param>
        internal static void PreserveStackTrace(Exception exception)
        {
            if (!_preserveStackTraceLookup)
            {
                lock (_preserveStackTraceLookupLock)
                {
                    _preserveStackTraceLookup = true;
                    try
                    {
                        _preserveStackTrace = typeof(Exception).GetMethod("InternalPreserveStackTrace",
                            BindingFlags.Instance | BindingFlags.NonPublic);
                    }
                    catch { }
                }
            }

            if (_preserveStackTrace != null)
            {
                _preserveStackTrace.Invoke(exception, null);
            }
        }

        /// <summary>
        /// IDictionary to string.
        /// </summary>
        /// <returns>The to string.</returns>
        /// <param name="dict">Dict.</param>
        public static string DictToString(IDictionary<string, string> dict)
        {
            if (dict == null || dict.Count == 0)
            {
                return string.Empty;
            }
            StringBuilder data = new StringBuilder(64);
            foreach(string key in dict.Keys)
            {
                string value = dict[key];
                CheckPropValid(key, value);
                data.Append(key).Append(":").Append(value).Append("|");
            }
            return data.ToString();
        }

        /// <summary>
        /// Strings to IDictionary.
        /// </summary>
        /// <param name="str">String.</param>
        /// <param name="dict">Dict.</param>
        public static void StringToDict(string str, IDictionary<string, string> dict)
        {
            if (string.IsNullOrEmpty(str) || dict == null)
            {
                return;
            }

            string[] str_array = str.Split('|');
            foreach(string kv in str_array)
            {
                string[] kv_array = kv.Split(':');
                if (kv_array.Length != 2)
                {
                    continue;
                }
                dict.Add(kv_array[0], kv_array[1]);
            }
        }

        internal static void CheckPropValid(string key, string value)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
            {
                throw new MQException("Message's property can't be null or empty");
            }

            if (IsContainSpecialChar(key) || IsContainSpecialChar(value)) 
            {
                throw new MQException(
                    string.Format(
                        "Message's property[{0}:{1}] can't contains: & \" ' < > : |",
                        key, value
                    )
                );
            }
        }

        internal static bool IsContainSpecialChar(string str)
        {
            return str.Contains("&") || str.Contains("<") || str.Contains(">")
                      || str.Contains("\"") || str.Contains("\'")
                      || str.Contains("|") || str.Contains(":");
        }

        #endregion
    }
}
