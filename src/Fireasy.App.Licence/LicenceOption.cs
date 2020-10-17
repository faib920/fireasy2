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
    /// 授权码参数。
    /// </summary>
    public class LicenceOption
    {
        /// <summary>
        /// 获取或设置应用标识。
        /// </summary>
        public string AppKey { get; set; }

        /// <summary>
        /// 获取或设置本地请求码。
        /// </summary>
        public string LocalKey { get; set; }

        /// <summary>
        /// 获取或设置加密方式。
        /// </summary>
        public LicenseEncryptKinds EncryptKind { get; set; }
    }
}
