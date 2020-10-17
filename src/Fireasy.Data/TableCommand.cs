// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Data;

namespace Fireasy.Data
{
    /// <summary>
    /// 表示使用表名称查询的命令。无法继承此类。
    /// </summary>
    public sealed class TableCommand : IQueryCommand
    {
        private readonly string _tableName;

        CommandType IQueryCommand.CommandType => CommandType.TableDirect;

        /// <summary>
        /// 将字符串转换为 <see cref="TableCommand"/> 实例。
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static implicit operator TableCommand(string tableName)
        {
            return new TableCommand(tableName);
        }

        /// <summary>
        /// 将 <see cref="TableCommand"/> 转换为字符串。
        /// </summary>
        /// <param name="tbCommand"></param>
        /// <returns></returns>
        public static explicit operator string(TableCommand tbCommand)
        {
            return tbCommand != null ? tbCommand.ToString() : string.Empty;
        }

        /// <summary>
        /// 使用表名称初始化 <see cref="TableCommand"/> 类的新实例。
        /// </summary>
        /// <param name="tableName">数据表的名称。</param>
        public TableCommand(string tableName)
        {
            _tableName = tableName;
        }

        /// <summary>
        /// 输出字符串。
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _tableName;
        }
    }
}
