// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;

namespace Fireasy.Common.Composition
{
    /// <summary>
    /// 如果目录中的多个同一协定的导出，则该容器只获取第一个导出。无法继承此类。
    /// </summary>
    public sealed class SingleCompositionContainer : FilterCompositionContainer
    {
        /// <summary>
        /// 初始化 <see cref="SingleCompositionContainer"/> 类的新实例。
        /// </summary>
        /// <param name="catalog">对象的组合目录。</param>
        /// <param name="providers">附加一组 <see cref="ExportProvider"/> 对象。</param>
        public SingleCompositionContainer(ComposablePartCatalog catalog, params ExportProvider[] providers)
            : base(catalog, exports => exports.FirstOrDefault(), providers)
        {
        }
    }
}
