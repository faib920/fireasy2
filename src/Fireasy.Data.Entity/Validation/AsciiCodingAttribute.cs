// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Data.Entity.Validation
{
    /// <summary>
    /// 对 ASCII 码的验证，配置文件对应的键为 Ascii。
    /// </summary>
    public sealed class AsciiCodingAttribute : ConfigurableRegularExpressionAttribute
    {
        /// <summary>
        /// 初始化 <see cref="AsciiCodingAttribute"/> 类的新实例。
        /// </summary>
        public AsciiCodingAttribute()
            : base("Ascii", "^[\\x00-\\xFF]+$")
        {
            ErrorMessage = "{0} 字段必须全部为非中文字符";
        }
    }
}