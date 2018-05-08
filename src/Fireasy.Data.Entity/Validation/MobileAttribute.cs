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
    /// 对手机号的验证，配置文件对应的键为 Mobile。
    /// </summary>
    public sealed class MobileAttribute : ConfigurableRegularExpressionAttribute
    {
        /// <summary>
        /// 初始化 <see cref="MobileAttribute"/> 类的新实例。
        /// </summary>
        public MobileAttribute()
            : base("Mobile", "^13[0-9]{9}|15[012356789][0-9]{8}|18[0-9][0-9]{8}|14[5678][0-9]{8}|17[0235678][0-9]{8}|166[0-9]{8}|19[89][0-9]{8}$")
        {
            ErrorMessage = SR.GetString(SRKind.MobileValideError);
        }
    }
}
