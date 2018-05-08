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
    /// 对网址的验证，配置文件对应的键为 WebSite。
    /// </summary>
    public sealed class WebSiteAttribute : ConfigurableRegularExpressionAttribute
    {
        /// <summary>
        /// 初始化 <see cref="WebSiteAttribute"/> 类的新实例。
        /// </summary>
        public WebSiteAttribute()
            : base("WebSite", "^http[s]?:\\/\\/([\\w-]+\\.)+[\\w-]+([\\w-./?%&=]*)?$")
        {
            ErrorMessage = SR.GetString(SRKind.WebSiteValideError);
        }
    }
}
