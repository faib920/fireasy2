// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Data.Backup
{
    /// <summary>
    /// 备份或恢复的选项。无法继承此类。
    /// </summary>
    public sealed class BackupOption
    {
        /// <summary>
        /// 获取或设置数据库的名称。
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// 获取或设置备份文件的名称。
        /// </summary>
        public string FileName { get; set; }
    }
}
