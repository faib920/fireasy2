// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Data.Provider;

namespace Fireasy.Data.Backup
{
    /// <summary>
    /// 提供对数据库备份与恢复的一组方法。
    /// </summary>
    public interface IBackupProvider : IProviderService
    {
        /// <summary>
        /// 对指定的数据库进行备份。
        /// </summary>
        /// <param name="database">提供给当前插件的 <see cref="IDatabase"/> 对象。</param>
        /// <param name="option">备份选项。</param>
        void Backup(IDatabase database, BackupOption option);

        /// <summary>
        /// 使用指定的备份文件恢复数据库。
        /// </summary>
        /// <param name="database">提供给当前插件的 <see cref="IDatabase"/> 对象。</param>
        /// <param name="option">备份选项。</param>
        void Restore(IDatabase database, BackupOption option);
    }
}
