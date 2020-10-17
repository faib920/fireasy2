// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Fireasy.App.Licence
{
    /// <summary>
    /// 授权码验证参数。
    /// </summary>
    public class LicenceVerifyOption : LicenceOption
    {
        /// <summary>
        /// 获取或设置用于验证的公钥。
        /// </summary>
        public string PublicKey { get; set; }

        /// <summary>
        /// 获取或设置授权码。
        /// </summary>
        public string LicenseKey { get; set; }

        /// <summary>
        /// 获取或设置是否验证请求码。
        /// </summary>
        public bool VerifyLocalKey { get; set; } = true;
    }
}
