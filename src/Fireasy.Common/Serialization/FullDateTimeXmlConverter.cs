// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Common.Serialization
{
    /// <summary>
    /// 完整的日期时间格式转换器。
    /// </summary>
    public sealed class FullDateTimeXmlConverter : DateTimeXmlConverter
    {
        public FullDateTimeXmlConverter()
            : base("yyyy-MM-dd HH:mm:ss")
        {
        }
    }
}
