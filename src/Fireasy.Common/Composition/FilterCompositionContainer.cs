// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;

namespace Fireasy.Common.Composition
{
    /// <summary>
    /// 如果目录中的多个同一协定的导出，则使用一个函数过滤出部分的导出。
    /// </summary>
    public class FilterCompositionContainer : CompositionContainer
    {
        private readonly Func<IEnumerable<Export>, Export> filterFunc;

        /// <summary>
        /// 初始化 <see cref="FilterCompositionContainer"/> 类的新实例。
        /// </summary>
        /// <param name="catalog">对象的组合目录。</param>
        /// <param name="filter">用于对导出进行过滤的函数。</param>
        /// <param name="providers">附加一组 <see cref="ExportProvider"/> 对象。</param>
        public FilterCompositionContainer(ComposablePartCatalog catalog, Func<IEnumerable<Export>, Export> filter, params ExportProvider[] providers)
            : base(catalog, providers)
        {
            filterFunc = filter;
        }

        /// <summary>
        /// 获取与指定约束相匹配的所有导出。
        /// </summary>
        /// <param name="definition">定义要获取 <see cref="Export"/> 对象的条件对象。</param>
        /// <param name="atomicComposition">要使用的组合事务，或为 null 以禁用事务性组合。</param>
        /// <returns>与 <paramref name="definition"/> 相匹配的 <see cref="Export"/> 对象的集合。</returns>
        protected override IEnumerable<Export> GetExportsCore(ImportDefinition definition, AtomicComposition atomicComposition)
        {
            var exports = base.GetExportsCore(definition, atomicComposition);
            if (exports != null)
            {
                yield return filterFunc(exports);
            }
        }
    }
}
