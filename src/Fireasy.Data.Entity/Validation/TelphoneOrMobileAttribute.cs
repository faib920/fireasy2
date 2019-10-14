// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.ComponentModel.DataAnnotations;

namespace Fireasy.Data.Entity.Validation
{
    /// <summary>
    /// 对电话或手机号码的验证。
    /// </summary>
    public sealed class TelphoneOrMobileAttribute : ValidationAttribute
    {
        /// <summary>
        /// 初始化 <see cref="TelphoneOrMobileAttribute"/> 类的新实例。
        /// </summary>
        public TelphoneOrMobileAttribute()
            : base (string.Empty)
        {
            ErrorMessage = "{0} 字段不符合电话号码和手机号码的格式";
        }

        public override bool IsValid(object value)
        {
            if (!new TelphoneAttribute().IsValid(value))
            {
                return new MobileAttribute().IsValid(value);
            }

            return true;
        }
    }
}
