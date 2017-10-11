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
    /// 对用户名的验证，配置文件对应的键为 UserName。
    /// </summary>
    public class UserNameAttribute : ConfigurableRegularExpressionAttribute
    {
        /// <summary>
        /// 初始化 <see cref="UserNameAttribute"/> 类的新实例。
        /// </summary>
        public UserNameAttribute()
            : base("UserName", "^[a-zA-Z][a-zA-Z0-9_]{1,15}$")
        {
            ErrorMessage = "{0} 必须以字母开头，包含字母、数字或下划线";
        }
    }
}
