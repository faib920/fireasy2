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
    /// 命令上下文对象。无法继承此类。
    /// </summary>
    public sealed class CommandContext
    {
        internal CommandContext(IDatabase database, IDbCommand command, IDataSegment segment, ParameterCollection parameters)
        {
            Database = database;
            Command = command;
            Segment = segment;
            Parameters = parameters;
        }

        /// <summary>
        /// 获取 <see cref="IDatabase"/> 对象。
        /// </summary>
        public IDatabase Database { get; private set; }

        /// <summary>
        /// 获取 <see cref="IDbCommand"/> 对象。
        /// </summary>
        public IDbCommand Command { get; private set; }

        /// <summary>
        /// 获取 <see cref="ParameterCollection"/> 集合。
        /// </summary>
        public ParameterCollection Parameters { get; private set; }

        /// <summary>
        /// 获取 <see cref="IDataSegment"/> 对象。
        /// </summary>
        public IDataSegment Segment { get; private set; }
    }
}
