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
    /// 表示使用SQL的查询命令。无法继承此类。
    /// </summary>
    public sealed class SqlCommand : IQueryCommand
    {
        private readonly string _sql;

        CommandType IQueryCommand.CommandType => CommandType.Text;

        /// <summary>
        /// 将字符串转换为 <see cref="SqlCommand"/> 实例。
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static implicit operator SqlCommand(string sql)
        {
            return new SqlCommand(sql);
        }

        /// <summary>
        /// 将 <see cref="SqlCommand"/> 转换为字符串。
        /// </summary>
        /// <param name="sqlCommand"></param>
        /// <returns></returns>
        public static explicit operator string(SqlCommand sqlCommand)
        {
            return sqlCommand != null ? sqlCommand._sql : string.Empty;
        }

        /// <summary>
        /// 将两个 <see cref="SqlCommand"/> 进行连接。
        /// </summary>
        /// <param name="command0"></param>
        /// <param name="command1"></param>
        /// <returns></returns>
        public static SqlCommand operator +(SqlCommand command0, SqlCommand command1)
        {
            return string.Concat(command0, command1);
        }

        /// <summary>
        /// 使用SQL语句初始化 <see cref="SqlCommand"/> 类的新实例。
        /// </summary>
        /// <param name="sql">查询语句。</param>
        public SqlCommand(string sql)
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
