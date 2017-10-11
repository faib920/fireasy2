// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Data.Common;

namespace Fireasy.Data.Provider
{
    /// <summary>
    /// 表示由自外部程序集提供的数据提供者。
    /// </summary>
    public abstract class AssemblyProvider : ProviderBase
    {
        private readonly string assemblyName;
        private readonly string fieldName;

        /// <summary>
        /// 使用程序集名称初始化 <see cref="AssemblyProvider"/> 类的新实例。
        /// </summary>
        /// <param name="assemblyName">程序集名称。</param>
        /// <param name="fieldName">单例变量名称。</param>
        protected AssemblyProvider(string assemblyName, string fieldName)
            : base()
        {
            this.assemblyName = assemblyName;
            this.fieldName = fieldName;
        }

        /// <summary>
        /// 初始化 <see cref="DbProviderFactory"/> 对象。
        /// </summary>
        /// <returns></returns>
        protected override DbProviderFactory InitDbProviderFactory()
        {
            return string.IsNullOrEmpty(fieldName) ? 
                AssemblyLoader.Load(assemblyName) : 
                AssemblyLoader.Load(assemblyName, fieldName);
        }
    }
}
