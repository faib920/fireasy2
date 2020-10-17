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
    /// 对邮政编码的验证，配置文件对应的键为 ZipCode。
    /// </summary>
    public sealed class ZipCodeAttribute : ConfigurableRegularExpressionAttribute
    {
        /// <summary>
        /// 初始化 <see cref="ZipCodeAttribute"/> 类的新实例。
        /// </summary>
        public ZipCodeAttribute()
            : base("ZipCode", "^\\d{6}$")
        {
            ErrorMessage = SR.GetString(SRKind.ZipCodeValideError);
        }
    }
}
