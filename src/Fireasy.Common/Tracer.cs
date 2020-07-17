// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Diagnostics;
#if NETFRAMEWORK
using Fireasy.Common.Extensions;
using System.Configuration;
#endif

namespace Fireasy.Common
{
    /// <summary>
    /// 跟踪器。
    /// </summary>
    public class Tracer
    {
#if NETFRAMEWORK
        static Tracer()
        {
            Disabled = ConfigurationManager.AppSettings["DisableTracer"].To<bool>();
        }
#endif

        /// <summary>
        /// 获取或设置是否禁用跟踪器，默认为 false。
        /// </summary>
        public static bool Disabled { get; set; }

        /// <summary>
        /// 输出调试信息。
        /// </summary>
        /// <param name="message"></param>
        public static void Debug(string message)
        {
            if (Disabled)
            {
                return;
            }

            Trace.WriteLine($"-->> {message}");
        }

        /// <summary>
        /// 输出错误信息。
        /// </summary>
        /// <param name="message"></param>
        public static void Error(string message)
        {
            if (Disabled)
            {
                return;
            }

            Trace.WriteLine($"-->> {message}");
        }
    }
}
