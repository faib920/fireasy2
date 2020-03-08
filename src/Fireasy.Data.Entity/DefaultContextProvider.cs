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
    /// 默认的数据上下文服务提供者。
    /// </summary>
    public sealed class DefaultContextProvider : IContextProvider
    {
        IProvider IProviderService.Provider { get; set; }

        IContextService IContextProvider.CreateContextService(ContextServiceContext context)
        {
            return new DefaultContextService(context);
        }
    }
}
