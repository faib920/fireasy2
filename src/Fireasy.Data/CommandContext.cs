// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Data;

namespace Fireasy.Data
{
    /// <summary>
    /// 命令上下文对象。无法继承此类。
    /// </summary>
    public sealed class CommandContext
    {
        private string _commandText;
        private IDataSegment _segment;

        internal CommandContext(IDatabase database, IQueryCommand queryCommand, IDbCommand command, IDataSegment segment, ParameterCollection parameters)
        {
            Database = database;
            QueryCommand = queryCommand;
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

        /// <summary>
        /// 获取 <see cref="IQueryCommand"/> 对象。
        /// </summary>
        public IQueryCommand QueryCommand { get; private set; }

        public void SetCommand(string commandText)
        {
            _commandText = Command.CommandText;
            Command.CommandText = commandText;
        }

        public void SetSegment(IDataSegment segment)
        {
            _segment = Segment;
            Segment = segment;
        }

        public string TryChangeSegment(IDataSegment segment, Func<string> func)
        {
            _segment = Segment;
            Segment = segment;

            var commandText = func();

            if (!string.IsNullOrEmpty(_commandText))
            {
                Command.CommandText = _commandText;
            }

            if (_segment != null)
            {
                Segment = _segment;
            }

            return commandText;
        }

        public CommandContext Reset()
        {
            if (!string.IsNullOrEmpty(_commandText))
            {
                Command.CommandText = _commandText;
            }

            if (_segment != null)
            {
                Segment = _segment;
            }

            return this;
        }
    }
}
