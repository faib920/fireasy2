// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Linq;

namespace Fireasy.Data.Entity.Initializers
{
    /// <summary>
    /// 此初始化器用于对实体程序集进行代理编译。
    /// </summary>
    public class RecompileAssemblyPreInitializer : IEntityContextPreInitializer
    {
        void IEntityContextPreInitializer.PreInitialize(EntityContextPreInitializeContext context)
        {
            if (context.Mappers.Count > 0)
            {
                var injection = context.Service.Provider.GetService<IInjectionProvider>();

                foreach (var assembly in context.Mappers.GroupBy(s => s.EntityType.Assembly))
                {
                    EntityProxyManager.CompileAll(assembly.Key, injection);
                }
            }
        }
    }
}
