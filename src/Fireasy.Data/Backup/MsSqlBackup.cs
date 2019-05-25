// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Text;
using Fireasy.Common;
using Fireasy.Data.Extensions;
using Fireasy.Data.Provider;

namespace Fireasy.Data.Backup
{
    /// <summary>
    /// 实用于 MsSql 的数据库备份与恢复。无法继承此类。
    /// </summary>
    public sealed class MsSqlBackup : IBackupProvider
    {
        public IProvider Provider { get; set; }

        /// <summary>
        /// 对指定的数据库进行备份。
        /// </summary>
        /// <param name="database">提供给当前插件的 <see cref="IDatabase"/> 对象。</param>
        /// <param name="option">备份选项。</param>
        public void Backup(IDatabase database, BackupOption option)
        {
            Guard.ArgumentNull(option, nameof(option));

            var sb = new StringBuilder();
            sb.AppendFormat("BACKUP DATABASE {0} TO DISK = '{1}'", option.Database, option.FileName);
            using (var connection = database.CreateConnection())
            {
                try
                {
                    if (string.IsNullOrEmpty(option.Database))
                    {
                        option.Database = connection.Database;
                    }

                    connection.OpenClose(() =>
                        {
                            using (var command = database.Provider.CreateCommand(connection, null, sb.ToString()))
                            {
                                command.ExecuteNonQuery();
                            }
                        });
                }
                catch (Exception exp)
                {
                    throw new BackupException(exp);
                }
            }
        }

        /// <summary>
        /// 使用指定的备份文件恢复数据库。
        /// </summary>
        /// <param name="database">提供给当前插件的 <see cref="IDatabase"/> 对象。</param>
        /// <param name="option">备份选项。</param>
        public void Restore(IDatabase database, BackupOption option)
        {
            Guard.ArgumentNull(option, nameof(option));

            var sb = new StringBuilder();
            sb.AppendFormat("RESTORE DATABASE {0} FROM DISK = '{1}'", option.Database, option.FileName);
            using (var connection = database.CreateConnection())
            {
                try
                {
                    if (string.IsNullOrEmpty(option.Database))
                    {
                        option.Database = connection.Database;
                    }

                    connection.OpenClose(() =>
                        {
                            using (var command = database.Provider.CreateCommand(connection, null, sb.ToString()))
                            {
                                command.ExecuteNonQuery();
                            }
                        });
                }
                catch (Exception exp)
                {
                    throw new BackupException(exp);
                }
            }
        }
    }
}
