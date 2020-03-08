// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using System.Linq;

namespace Fireasy.Data.Entity.Initializers
{
    /// <summary>
    /// 此初始化器用于对实体程序集进行代理编译。
    /// </summary>
    public sealed class RecompileAssemblyPreInitializer : IEntityContextPreInitializer
    {
        void IEntityContextPreInitializer.PreInitialize(EntityContextPreInitializeContext context)
        {
            if (context.Mappers.Count > 0)
            {
                var providerName = context.ContextService.Provider.ProviderName;
                var injection = context.ContextService.Provider.GetService<IInjectionProvider>();

                context.Mappers.GroupBy(s => s.EntityType.Assembly)
                    .Select(s => new { assembly = s.Key, types = s.Select(t => t.EntityType).Distinct().ToArray() })
                    .ForEach(s => EntityProxyManager.CompileAll(providerName, s.assembly, s.types, injection));
            }
        }
    }
}
