// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.App.Licence
{
    /// <summary>
    /// 授权数据。
    /// </summary>
    [Serializable]
    public class LicenceData
    {
        /// <summary>
        /// 获取或设置本地请求码。
        /// </summary>
        public string LocalKey { get; set; }

        /// <summary>
        /// 获取或设置授权码。
        /// </summary>
        public string LicenceKey { get; set; }
    }
}
