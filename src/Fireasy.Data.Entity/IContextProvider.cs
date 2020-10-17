// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Provider;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 数据上下文的服务接口。
    /// </summary>
    [DefaultProviderService(typeof(DefaultContextProvider))]
    public interface IContextProvider : IProviderService
    {
        /// <summary>
        /// 创建 <see cref="IContextService"/> 服务组件对象。
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        IContextService CreateContextService(ContextServiceContext context);
    }
}
