// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
using System.Linq;
using System.Reflection;

namespace Fireasy.Common.Composition
{
    /// <summary>
    /// 提供一个注册协定和实现的组合目录。
    /// </summary>
    public class ConventionalCatalog : ComposablePartCatalog
    {
        private readonly Dictionary<Type, ComposablePartDefinition> _partDictionary = new Dictionary<Type, ComposablePartDefinition>();

        /// <summary>
        /// 注册一个指定协定类型的实现类型。
        /// </summary>
        /// <typeparam name="TContract">协定类型。</typeparam>
        /// <typeparam name="TImplementation">实现类型。</typeparam>
        /// <param name="replace">是否替换已有的部件定义。</param>
        /// <returns>当前的 <see cref="ConventionalCatalog"/> 对象。</returns>
        public ConventionalCatalog Register<TContract, TImplementation>(bool replace = true)
        {
            return Register(typeof(TContract), typeof(TImplementation), replace);
        }

        /// <summary>
        /// 注册一个指定协定类型的实现类型。
        /// </summary>
        /// <param name="contractType">协定类型。</param>
        /// <param name="implType">实现类型。</param>
        /// <param name="replace">是否替换已有的部件定义。</param>
        /// <returns>当前的 <see cref="ConventionalCatalog"/> 对象。</returns>
        public ConventionalCatalog Register(Type contractType, Type implType, bool replace = true)
        {
            Guard.ArgumentNull(contractType, nameof(contractType));
            Guard.ArgumentNull(implType, nameof(implType));
            if (!IsRegisted(contractType) || replace)
            {
                var part = ReflectionModelServices.CreatePartDefinition(
                    new Lazy<Type>(() => implType),
                    false,
                    new Lazy<IEnumerable<ImportDefinition>>(() => GetImportDefinitions(implType)),
                    new Lazy<IEnumerable<ExportDefinition>>(() => GetExportDefinitions(implType, contractType)), null, null);
                _partDictionary.AddOrReplace(contractType, part);
            }

            return this;
        }

        /// <summary>
        /// 注册一个指定协定类型的实现实例。
        /// </summary>
        /// <typeparam name="TContract">协定类型。</typeparam>
        /// <param name="creator">一个创建实例对象的函数。</param>
        /// <param name="replace">是否替换已有的部件定义。</param>
        /// <returns>当前的 <see cref="ConventionalCatalog"/> 对象。</returns>
        public ConventionalCatalog Register<TContract>(Func<TContract> creator, bool replace = true)
        {
            Guard.ArgumentNull(creator, nameof(creator));
            return Register(typeof(TContract), creator());
        }

        /// <summary>
        /// 注册一个指定协定类型的实现实例。
        /// </summary>
        /// <param name="contractType">协定类型。</param>
        /// <param name="instance">实现的对象。</param>
        /// <param name="replace">是否替换已有的部件定义。</param>
        /// <returns>当前的 <see cref="ConventionalCatalog"/> 对象。</returns>
        public ConventionalCatalog Register(Type contractType, object instance, bool replace = true)
        {
            if (instance == null)
            {
                return this;
            }

            if (!IsRegisted(contractType) || replace)
            {
                var implType = instance.GetType();
                var part = new ConventionalPartPartDefinition(
                instance,
                new Lazy<IEnumerable<ImportDefinition>>(() => new List<ImportDefinition>()),
                new Lazy<IEnumerable<ExportDefinition>>(() => GetExportDefinitions(implType, contractType)));
                _partDictionary.Add(contractType, part);
            }

            return this;
        }

        /// <summary>
        /// 获取目录中包含的部件定义。
        /// </summary>
        public override IQueryable<ComposablePartDefinition> Parts
        {
            get { return _partDictionary.Values.AsQueryable(); }
        }

        private bool IsRegisted(Type contractType)
        {
            return _partDictionary.ContainsKey(contractType);
        }

        private IEnumerable<ImportDefinition> GetImportDefinitions(Type implementationType)
        {
            var imports = new List<ImportDefinition>();
            var defaultConstructor = implementationType.GetConstructors().FirstOrDefault();
            if (defaultConstructor != null)
            {
                foreach (var param in defaultConstructor.GetParameters())
                {
                    imports.Add(
                        ReflectionModelServices.CreateImportDefinition(
                        new Lazy<ParameterInfo>(() => param),
                        AttributedModelServices.GetContractName(param.ParameterType),
                        AttributedModelServices.GetTypeIdentity(param.ParameterType),
                        Enumerable.Empty<KeyValuePair<string, Type>>(),
                        ImportCardinality.ExactlyOne,
                        CreationPolicy.Any,
                        null));
                }
            }

            return imports;
        }

        private IEnumerable<ExportDefinition> GetExportDefinitions(Type implementationType, Type contractType)
        {
            var lazyMember = new LazyMemberInfo(implementationType);
            var contracName = AttributedModelServices.GetContractName(contractType);
            var metadata = new Lazy<IDictionary<string, object>>(() =>
                {
                    var md = new Dictionary<string, object>
                    {
                        { CompositionConstants.ExportTypeIdentityMetadataName, AttributedModelServices.GetTypeIdentity(contractType) }
                    };
                    return md;
                });
            return new[]
                {
                    ReflectionModelServices.CreateExportDefinition(lazyMember, contracName, metadata, null)
                };
        }

        private class ConventionalPartPartDefinition : ComposablePartDefinition
        {
            private readonly ConventionalPart _part;
            private readonly Lazy<IEnumerable<ImportDefinition>> _imports;
            private readonly Lazy<IEnumerable<ExportDefinition>> _exports;

            public ConventionalPartPartDefinition(object exportValue, Lazy<IEnumerable<ImportDefinition>> imports, Lazy<IEnumerable<ExportDefinition>> exports)
            {
                _part = new ConventionalPart(exportValue, imports, exports);
                _imports = imports;
                _exports = exports;
            }

            public override ComposablePart CreatePart()
            {
                return _part;
            }

            public override IEnumerable<ExportDefinition> ExportDefinitions
            {
                get { return _exports?.Value; }
            }

            public override IEnumerable<ImportDefinition> ImportDefinitions
            {
                get { return _imports?.Value; }
            }
        }

        private class ConventionalPart : ComposablePart
        {
            private readonly object _exportValue;
            private readonly Lazy<IEnumerable<ImportDefinition>> _imports;
            private readonly Lazy<IEnumerable<ExportDefinition>> _exports;

            public ConventionalPart(object exportValue, Lazy<IEnumerable<ImportDefinition>> imports, Lazy<IEnumerable<ExportDefinition>> exports)
            {
                _exportValue = exportValue;
                _imports = imports;
                _exports = exports;
            }

            public override object GetExportedValue(ExportDefinition definition)
            {
                return _exportValue;
            }

            public override void SetImport(ImportDefinition definition, IEnumerable<Export> exports)
            {
            }

            public override IEnumerable<ExportDefinition> ExportDefinitions
            {
                get { return _exports?.Value; }
            }

            public override IEnumerable<ImportDefinition> ImportDefinitions
            {
                get { return _imports?.Value; }
            }
        }

    }
}
