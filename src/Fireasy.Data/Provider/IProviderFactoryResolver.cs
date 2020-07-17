// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Data.Common;
using System.IO;
using System.Linq;

namespace Fireasy.Data.Provider
{
    /// <summary>
    /// 定义 <see cref="DbProviderFactory"/> 的解析来源。
    /// </summary>
    public interface IProviderFactoryResolver
    {
        /// <summary>
        /// 解析返回 <see cref="DbProviderFactory"/> 对象。
        /// </summary>
        /// <returns></returns>
        DbProviderFactory Resolve();

        /// <summary>
        /// 获取解析失败时的异常信息。
        /// </summary>
        Exception Exception { get; }
    }

#if NETFRAMEWORK
    /// <summary>
    /// 通过 DbProviderFactories.GetFactory 从系统环境中安装的驱动中解析。
    /// </summary>
    public class InstallerProviderFactoryResolver : IProviderFactoryResolver
    {
        private readonly string _providerName;

        /// <summary>
        /// 初始化类 <see cref="InstallerProviderFactoryResolver"/> 类的新实例。
        /// </summary>
        /// <param name="providerName">固定的驱动名称。</param>
        public InstallerProviderFactoryResolver(string providerName)
        {
            _providerName = providerName;
        }

        public Exception Exception { get; private set; }

        public DbProviderFactory Resolve()
        {
            var factory = DbProviderFactories.GetFactory(_providerName);
            if (factory != null)
            {
                return factory;
            }

            Exception = new FileNotFoundException(SR.GetString(SRKind.ProviderNoFound, _providerName));
            return null;
        }
    }
#endif

    /// <summary>
    /// 从本地一组程序集中动态加载解析。
    /// </summary>
    public class AssemblyProviderFactoryResolver : IProviderFactoryResolver
    {
        private readonly string[] _typeNames;

        /// <summary>
        /// 初始化类 <see cref="AssemblyProviderFactoryResolver"/> 类的新实例。
        /// </summary>
        /// <param name="typeNames">一组程序集标识。</param>
        public AssemblyProviderFactoryResolver(params string[] typeNames)
        {
            _typeNames = typeNames;
        }

        public Exception Exception { get; private set; }

        public DbProviderFactory Resolve()
        {
            foreach (var typeName in _typeNames)
            {
                if (AssemblyLoader.TryLoad(typeName, out DbProviderFactory factory))
                {
                    return factory;
                }
            }

            Exception = new FileNotFoundException(SR.GetString(SRKind.InstallProviderAssembly, string.Join("、", _typeNames.Select(s => s.Substring(s.LastIndexOf(",") + 1).Trim() + ".dll").ToArray())));
            return null;
        }
    }
}
