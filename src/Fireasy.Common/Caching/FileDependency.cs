// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;
using System.Security.Permissions;

namespace Fireasy.Common.Caching
{
    /// <summary>
    /// 使用一个外部文件作为缓存的过期检测策略，缓存项生命周期在文件发生修改后终止。无法继承此类。
    /// </summary>
    [Serializable]
    public sealed class FileDependency : ICacheItemExpiration
    {
        /// <summary>
        /// 记录文件最后修改的时间。
        /// </summary>
        private readonly DateTime _lastModifiedTime;

        /// <summary>
        /// 初始化 <see cref="FileDependency"/> 类的新实例。
        /// </summary>
        /// <param name="filePath">作为缓存依赖的文件路径。</param>
        public FileDependency(string filePath)
        {
            FilePath = filePath;
            if (!File.Exists(FilePath))
            {
                throw new ArgumentException(FilePath, nameof(filePath));
            }

            _lastModifiedTime = File.GetLastWriteTime(FilePath);
        }

        /// <summary>
        /// 获取或设置文件路径。
        /// </summary>
        public string FilePath { get; private set; }

        /// <summary>
        /// 检查缓存项是否达到过期时间。
        /// </summary>
        /// <returns>过期为 true，有效为 false。</returns>
        public bool HasExpired()
        {
#if !NETSTANDARD
            var permission = new FileIOPermission(FileIOPermissionAccess.Read, FilePath);
            permission.Demand();
#endif

            if (!File.Exists(FilePath))
            {
                return true;
            }

            var currentModifiedTime = File.GetLastWriteTime(FilePath);
            return DateTime.Compare(_lastModifiedTime, currentModifiedTime) != 0;
        }

        /// <summary>
        /// 获取到期时间。
        /// </summary>
        /// <returns></returns>
        public TimeSpan? GetExpirationTime()
        {
            throw new NotImplementedException();
        }
    }
}
