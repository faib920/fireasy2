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
            : this(true)
        {
        }

        /// <summary>
        /// 初始化 <see cref="MobileAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="simple">采用简便的正则表达式。</param>
        public MobileAttribute(bool simple)
            : base("Mobile", simple ? "^1[0-9]{10}$" : "^13[0-9]{9}$|^14[01456879][0-9]{8}$|^15[012356789][0-9]{8}$|^16[2567][0-9]{8}$|^17[01235678][0-9]{8}$|^18[0-9][0-9]{8}$|^19[012356789][0-9]{8}$")
        {
            ErrorMessage = SR.GetString(SRKind.MobileValideError);
        }
    }
}
