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
    /// 表示使用存储过程的查询语句。
    /// </summary>
    public sealed class ProcedureCommand : IQueryCommand
    {
        private readonly string _procedureName;
        CommandType IQueryCommand.CommandType => CommandType.StoredProcedure;

        /// <summary>
        /// 将字符串转换为 <see cref="ProcedureCommand"/> 实例。
        /// </summary>
        /// <param name="procedureName">存储过程的名称。</param>
        /// <returns></returns>
        public static implicit operator ProcedureCommand(string procedureName)
        {
            return new ProcedureCommand(procedureName);
        }

        /// <summary>
        /// 将 <see cref="ProcedureCommand"/> 转换为字符串。
        /// </summary>
        /// <param name="proCommand"></param>
        /// <returns></returns>
        public static explicit operator string(ProcedureCommand proCommand)
        {
            return proCommand != null ? proCommand.ToString() : string.Empty;
        }

        /// <summary>
        /// 初始化 <see cref="ProcedureCommand"/> 类的新实例。
        /// </summary>
        /// <param name="procedureName">存储过程的名称。</param>
        public ProcedureCommand(string procedureName)
        {
            _procedureName = procedureName;
        }

        /// <summary>
        /// 输出字符串。
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _procedureName;
        }
    }
}
