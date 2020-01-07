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
    /// 授权码生成参数。
    /// </summary>
    public class LicenceGenerateOption : LicenceOption
    {
        /// <summary>
        /// 获取或设置用于生成授权码的私钥。
        /// </summary>
        public string PrivateKey { get; set; }
    }
}
