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
    /// 表示包含标识符的SQL执行命令。无法继承此类。
    /// </summary>
    public sealed class IIdenticalSqlCommand : IQueryCommand
    {
        private readonly string _sql;

        CommandType IQueryCommand.CommandType => CommandType.Text;

        /// <summary>
        /// 将字符串转换为 <see cref="IIdenticalSqlCommand"/> 实例。
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static implicit operator IIdenticalSqlCommand(string sql)
        {
            return new IIdenticalSqlCommand(sql);
        }

        /// <summary>
        /// 将 <see cref="IIdenticalSqlCommand"/> 转换为字符串。
        /// </summary>
        /// <param name="sqlCommand"></param>
        /// <returns></returns>
        public static explicit operator string(IIdenticalSqlCommand sqlCommand)
        {
            return sqlCommand != null ? sqlCommand._sql : string.Empty;
        }

        /// <summary>
        /// 将两个 <see cref="IIdenticalSqlCommand"/> 进行连接。
        /// </summary>
        /// <param name="command0"></param>
        /// <param name="command1"></param>
        /// <returns></returns>
        public static SqlCommand operator +(IIdenticalSqlCommand command0, IIdenticalSqlCommand command1)
        {
            return string.Concat(command0, command1);
        }

        /// <summary>
        /// 使用SQL语句初始化 <see cref="IIdenticalSqlCommand"/> 类的新实例。
        /// </summary>
        /// <param name="sql">查询语句。</param>
        public IIdenticalSqlCommand(string sql)
        {
            _sql = sql;
        }

        /// <summary>
        /// 输出字符串。
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _sql;
        }
    }
}
