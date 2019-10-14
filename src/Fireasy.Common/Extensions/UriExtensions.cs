// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace Fireasy.Common.Extensions
{
    public static class UriExtensions
    {
        /// <summary>
        /// 确保最后一个字符为反斜杠。
        /// </summary>
        /// <param name="url">链接地址字符串。</param>
        /// <returns></returns>
        public static string EnsureEndingBackslash(this string url)
        {
            if (!url.EndsWith("/"))
            {
                return string.Concat(url, "/");
            }

            return url;
        }

        /// <summary>
        /// 移除第一个反斜杠。
        /// </summary>
        /// <param name="url">链接地址字符串。</param>
        /// <returns></returns>
        public static string RemoveFirstBackslash(this string url)
        {
            if (!url.StartsWith("/"))
            {
                return url;
            }

            return url.Substring(1);
        }

        /// <summary>
        /// 移除最后一个反斜杠。
        /// </summary>
        /// <param name="url">链接地址字符串。</param>
        /// <returns></returns>
        public static string RemoveLastBackslash(this string url)
        {
            if (!url.EndsWith("/"))
            {
                return url;
            }

            return url.Substring(0, url.Length - 1);
        }

        /// <summary>
        /// 清除url路径。
        /// </summary>
        /// <param name="url">链接地址字符串。</param>
        /// <returns></returns>
        public static string CleanUrlPath(this string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                url = "/";
            }

            if (url != "/" && url.EndsWith("/"))
            {
                url = url.Substring(0, url.Length - 1);
            }

            return url;
        }

        /// <summary>
        /// 是否为本地链接地址。
        /// </summary>
        /// <param name="url">链接地址字符串。</param>
        /// <returns></returns>
        public static bool IsLocalUrl(this string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                if (url[0] != '/' || (url.Length != 1 && (url[1] == '/' || url[1] == '\\')))
                {
                    if (url.Length > 1 && url[0] == '~')
                    {
                        return url[1] == '/';
                    }

                    return false;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// 往 url 里添加参数。
        /// </summary>
        /// <param name="url">链接地址字符串。</param>
        /// <param name="query">参数。</param>
        /// <returns></returns>
        public static string AddQueryString(this string url, string query)
        {
            if (!url.Contains("?"))
            {
                url += "?";
            }

            else if (!url.EndsWith("&"))
            {
                url += "&";
            }

            return url + query;
        }

        /// <summary>
        /// 往 url 里添加参数。
        /// </summary>
        /// <param name="url">链接地址字符串。</param>
        /// <param name="name">参数名称。</param>
        /// <param name="value">参数值。</param>
        /// <returns></returns>
        public static string AddQueryString(this string url, string name, string value)
        {
            return url.AddQueryString(string.Concat(name, "=" , Uri.EscapeUriString(value)));
        }
    }
}
