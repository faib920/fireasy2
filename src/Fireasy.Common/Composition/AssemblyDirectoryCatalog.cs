// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.ComponentModel.Composition.Hosting;

namespace Fireasy.Common.Composition
{
    /// <summary>
    /// 使用程序集目录作为对象的可组合部件目录。
    /// </summary>
    public sealed class AssemblyDirectoryCatalog : DirectoryCatalog
    {
        /// <summary>
        /// 初始化 <see cref="AssemblyDirectoryCatalog"/> 类的新实例。
        /// </summary>
        public AssemblyDirectoryCatalog() :
            base(GetWorkDirectory(), "*.dll")
        {
        }

        /// <summary>
        /// 初始化 <see cref="AssemblyDirectoryCatalog"/> 类的新实例。
        /// </summary>
        /// <param name="searchPattern">搜索所依据的模式。</param>
        public AssemblyDirectoryCatalog(string searchPattern)
            : base (GetWorkDirectory(), searchPattern)
        {
        }

        /// <summary>
        /// 获取工作目录，即程序集所在的目录。
        /// </summary>
        /// <returns>工作目录。</returns>
        private static string GetWorkDirectory()
        {
            var directory = AppDomain.CurrentDomain.BaseDirectory;
            if (!string.IsNullOrEmpty(directory) &&
                (directory.StartsWith("file:\\")))
            {
                directory = directory.Substring(6);
            }

            return directory;
        }
    }
}
