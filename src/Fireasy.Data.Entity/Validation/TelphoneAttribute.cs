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
    /// 对电话号码的验证，配置文件对应的键为 Telphone。
    /// </summary>
    public sealed class TelphoneAttribute : ConfigurableRegularExpressionAttribute
    {
        /// <summary>
        /// 初始化 <see cref="TelphoneAttribute"/> 类的新实例。
        /// </summary>
        public TelphoneAttribute()
            : base("Telphone", "^(([0\\+]\\d{2,3}-)?(0\\d{2,3})-)?(\\d{7,8})(-(\\d{3,}))?$")
        {
            ErrorMessage = SR.GetString(SRKind.TelhponeValideError);
        }
    }
}
