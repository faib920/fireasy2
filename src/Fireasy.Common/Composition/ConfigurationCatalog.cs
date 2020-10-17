// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Caching;
using Fireasy.Common.Composition.Configuration;
using Fireasy.Common.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Reflection;

namespace Fireasy.Common.Composition
{
    /// <summary>
    /// 基于配置的对象组合部件目录。
    /// </summary>
    public class ConfigurationCatalog : ComposablePartCatalog
    {
        private IQueryable<ComposablePartDefinition> _partsQuery;
        private List<string> _files = null;

        /// <summary>
        /// 获取目录中包含的部件定义。
        /// </summary>
        public override IQueryable<ComposablePartDefinition> Parts
        {
            get
            {
                return _partsQuery ?? (_partsQuery = CreateDefinitions());
            }
        }

        public ReadOnlyCollection<string> LoadedFiles
        {
            get
            {
                if (_files == null)
                {
                    _partsQuery = CreateDefinitions();
                }

                return _files.AsReadOnly();
            }
        }

        /// <summary>
        /// 从 <see cref="ImportConfigurationSetting"/> 对象中解析出 <see cref="ComposablePartCatalog"/> 对象。
        /// </summary>
        /// <param name="setting">用于配置目录的配置对象。</param>
        /// <returns>从 <paramref name="setting"/> 解析出的 <see cref="ComposablePartCatalog"/> 对象。</returns>
        protected ComposablePartCatalog ResolveCatalog(ImportConfigurationSetting setting)
        {
            if (!string.IsNullOrEmpty(setting.Assembly))
            {
                _files = new List<string>();
                var cacheMgr = MemoryCacheManager.Instance;
                return cacheMgr.TryGet(setting.Assembly, () =>
                    {
                        try
                        {
                            var assembly = Assembly.Load(new AssemblyName(setting.Assembly));
                            _files.Add(assembly.Location);
                            return new AssemblyCatalog(assembly);
                        }
                        catch (Exception ex)
                        {
                            throw new CompositionException(SR.GetString(SRKind.UnableLoadAssembly, setting.Assembly), ex);
                        }
                    });
            }

            return setting.ImportType == null || setting.ContractType == null
                       ? null : new TypeCatalog(setting.ImportType);
        }

        /// <summary>
        /// 通过配置节创建 <see cref="ComposablePartDefinition"/> 序列。
        /// </summary>
        /// <returns></returns>
        protected virtual IQueryable<ComposablePartDefinition> CreateDefinitions()
        {
            var section = ConfigurationUnity.GetSection<ImportConfigurationSection>();
            if (section == null)
            {
                return null;
            }

            var list = section.Settings
                .Select(setting => ResolveCatalog(setting.Value))
                .Where(catalog => catalog != null).ToList();

            return list.SelectMany(s => s.Parts).AsQueryable();
        }
    }

    /// <summary>
    /// 基于配置的对象组合部件目录。
    /// </summary>
    /// <typeparam name="T">要导入的协定类型。</typeparam>
    public sealed class ConfigurationCatalog<T> : ConfigurationCatalog
    {
        /// <summary>
        /// 通过配置节创建 <see cref="ComposablePartDefinition"/> 序列。
        /// </summary>
        /// <returns></returns>
        protected override IQueryable<ComposablePartDefinition> CreateDefinitions()
        {
            var section = ConfigurationUnity.GetSection<ImportConfigurationSection>();
            if (section == null)
            {
                return null;
            }

            var contractType = typeof(T);
            var list = section.Settings
                .Where(s => s.Value.ContractType == contractType)
                .Select(setting => ResolveCatalog(setting.Value))
                .Where(catalog => catalog != null).ToList();

            return list.SelectMany(s => s.Parts).AsQueryable();
        }
    }
}
