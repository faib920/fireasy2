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
    /// 本地请求码参数。
    /// </summary>
    public class LocalKeyOption
    {
        /// <summary>
        /// 获取或设置机器码类型。
        /// </summary>
        public MachineKeyKinds MachineKeyKind { get; set; }

        /// <summary>
        /// 获取或设置加密类型。
        /// </summary>
        public LocalKeyEncryptKinds EncryptKind { get; set; }

        /// <summary>
        /// 获取或设置应用标识。
        /// </summary>
        public string AppKey { get; set; }
    }
}
