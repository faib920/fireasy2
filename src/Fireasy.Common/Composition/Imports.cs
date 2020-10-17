// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Composition.Configuration;
using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;

namespace Fireasy.Common.Composition
{
    /// <summary>
    /// 基于 MEF 服务实例的导入管理器。
    /// </summary>
    public static class Imports
    {
        static Imports()
        {
            var section = ConfigurationUnity.GetSection<ImportConfigurationSection>();

            //如果有配置，使用 ConfigurationCatalog，否则需要在所有程序集中查找（性能有影响）
            var catalog = section == null || section.Settings.Count == 0 ?
                (ComposablePartCatalog)new AssemblyDirectoryCatalog() : new ConfigurationCatalog();
            Container = new CompositionContainer(catalog);
        }

        /// <summary>
        /// 获取或设置 MEF 容器。
        /// </summary>
        public static CompositionContainer Container { get; set; }

        /// <summary>
        /// 获取指定类型的服务组件。
        /// </summary>
        /// <typeparam name="T">服务类的类型。</typeparam>
        /// <param name="contractName">协定名称。</param>
        /// <returns></returns>
        public static T GetService<T>(string contractName = null) where T : class
        {
            return GetServices<T>(contractName).FirstOrDefault();
        }

        /// <summary>
        /// 获取指定类型的服务组件序列。
        /// </summary>
        /// <typeparam name="T">服务类的类型。</typeparam>
        /// <param name="contractName">协定名称。</param>
        /// <returns></returns>
        public static IEnumerable<T> GetServices<T>(string contractName = null) where T : class
        {
            var contractType = typeof(T);
            var result = new List<T>();

            //查找没有默认的导入类型
            var attr = contractType.GetCustomAttributes<DefaultImportAttribute>().FirstOrDefault();
            if (attr != null && attr.ImportType != null)
            {
                result.Add(attr.ImportType.New<T>());
                return result;
            }

            var import = CreateImportDefinition<T>(contractName);

            foreach (var export in Container.GetExports(import))
            {
                if (export == null || export.Value == null)
                {
                    continue;
                }

                result.Add((T)export.Value);
            }

            return result;
        }

        private static ImportDefinition CreateImportDefinition<T>(string contractName = null)
        {
            //使用 GetExports<T> 方法会导出一个空引用的 Lazy 对象，故使用 ImportDefinition 来调用另一个方法
            return new ContractBasedImportDefinition(
                string.IsNullOrEmpty(contractName) ? AttributedModelServices.GetContractName(typeof(T)) : contractName,
                AttributedModelServices.GetTypeIdentity(typeof(T)),
                Enumerable.Empty<KeyValuePair<string, Type>>(),
                ImportCardinality.ZeroOrMore, false, true, CreationPolicy.Shared);
        }
    }
}
