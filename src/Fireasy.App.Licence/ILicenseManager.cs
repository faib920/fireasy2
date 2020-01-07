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
    /// 授权码管理器。
    /// </summary>
    public interface ILicenseManager
    {
        /// <summary>
        /// 获取本地请求码。
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        string GetLocalKey(LocalKeyOption option);

        /// <summary>
        /// 获取授权码。
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        string GetLicenseKey(LicenceGenerateOption option);

        /// <summary>
        /// 验证授权码是否正确。
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        bool VerifyLicenseKey(LicenceVerifyOption option);
    }
}
