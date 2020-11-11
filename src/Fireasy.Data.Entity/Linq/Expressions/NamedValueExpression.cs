// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;
using System.Data;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Expressions
{
    [DebuggerDisplay("DbNodeType={DbNodeType},Name={Name},Value={Value},DataType={DataType}")]
    public class NamedValueExpression : DbExpression
    {
        public NamedValueExpression(string name, Expression value, bool useEscape = false, DbType dbType = DbType.String)
            : base(DbExpressionType.NamedValue, value.Type)
        {
            Guard.ArgumentNull(name, nameof(name));
            Guard.ArgumentNull(value, nameof(value));

            Name = name;
            Value = value;
            DataType = dbType;
            UseEscape = useEscape;
        }

        public string Name { get; }

        public Expression Value { get; }

        public DbType DataType { get; }

        public bool UseEscape { get; }

        public override string ToString()
        {
            return $"Named({Name}:{Value})";
        }
    }
}
