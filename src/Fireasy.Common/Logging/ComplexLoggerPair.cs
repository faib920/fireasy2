// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Fireasy.Common.Logging
{
    /// <summary>
    /// 复合的日志记录器配对。
    /// </summary>
    public sealed class ComplexLoggerPair
    {
        /// <summary>
        /// 获取日志级别。
        /// </summary>
        public LogLevel Level { get; set; }

        /// <summary>
        /// 获取或设置日志记录器。
        /// </summary>
        public ILogger Logger { get; set; }
    }
}
