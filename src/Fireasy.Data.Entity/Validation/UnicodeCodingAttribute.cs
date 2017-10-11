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
    /// 对 UNICODE 码的验证，配置文件对应的键为 Unicode。
    /// </summary>
    public sealed class UnicodeCodingAttribute : ConfigurableRegularExpressionAttribute
    {
        /// <summary>
        /// 初始化 <see cref="UnicodeCodingAttribute"/> 类的新实例。
        /// </summary>
        public UnicodeCodingAttribute()
            : base("Unicode", "^[\\u4E00-\\u9FA5\\uF900-\\uFA2D]+$")
        {
            ErrorMessage = "{0} 字段必须全部为中文字符";
        }
    }
}
