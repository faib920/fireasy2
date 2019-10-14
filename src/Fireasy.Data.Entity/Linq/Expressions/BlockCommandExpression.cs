// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Expressions
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class BlockCommandExpression : CommandExpression
    {
        public BlockCommandExpression(IList<Expression> commands)
            : base(DbExpressionType.Block, false, commands[commands.Count - 1].Type)
        {
            Commands = commands.ToReadOnly();
        }

        public BlockCommandExpression(params Expression[] commands)
            : this((IList<Expression>)commands)
        {
        }

        public ReadOnlyCollection<Expression> Commands { get; private set; }
    }
}
