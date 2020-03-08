// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Diagnostics;

namespace Fireasy.Common
{
    /// <summary>
    /// 跟踪器。
    /// </summary>
    public static class Tracer
    {
        /// <summary>
        /// 输出调试信息。
        /// </summary>
        /// <param name="message"></param>
        public static void Debug(string message)
        {
            Trace.WriteLine($"Debug: {message}");
        }

        /// <summary>
        /// 输出错误信息。
        /// </summary>
        /// <param name="message"></param>
        public static void Error(string message)
        {
            Trace.WriteLine($"Error: {message}");
        }
    }
}
