// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Data.Common;

namespace Fireasy.Data.Provider
{
    /// <summary>
    /// 表示由自外部程序集提供的数据提供者。
    /// </summary>
    public abstract class AssemblyProvider : ProviderBase
    {
        private readonly List<string> typeNames;

        /// <summary>
        /// 使用程序集名称初始化 <see cref="AssemblyProvider"/> 类的新实例。
        /// </summary>
        /// <param name="typeNames">程序集组。</param>
        protected AssemblyProvider(params string[] typeNames)
            : base()
        {
            this.typeNames = new List<string>(typeNames);
        }

        /// <summary>
        /// 初始化 <see cref="DbProviderFactory"/> 对象。
        /// </summary>
        /// <returns></returns>
        protected override DbProviderFactory InitDbProviderFactory()
        {
            foreach (var typeName in typeNames)
            {
                if (AssemblyLoader.TryLoad(typeName, out DbProviderFactory factory))
                {
                    return factory;
                }
            }

            return null;
        }
    }
}
