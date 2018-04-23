// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Data.Provider
{
    /// <summary>
    /// 提供者插件服务接口。
    /// </summary>
    public interface IProviderService
    {
        /// <summary>
        /// 获取或设置当前的 <see cref="IProvider"/> 实例。
        /// </summary>
        IProvider Provider { get; set; }
    }
}
