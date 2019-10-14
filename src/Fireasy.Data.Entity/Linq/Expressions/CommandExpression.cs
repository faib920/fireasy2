// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Expressions
{
    /// <summary>
    /// 一个抽象类，表示命令类型的表达式。
    /// </summary>
    public abstract class CommandExpression : DbExpression
    {
        public CommandExpression(DbExpressionType eType, bool isAsync, Type type)
            : base(eType, type)
        {
            AssociatedCommands = new List<CommandExpression>();
            IsAsync = isAsync;
        }

        public List<CommandExpression> AssociatedCommands { get; private set; }

        public bool IsAsync { get; private set; }
    }
}
