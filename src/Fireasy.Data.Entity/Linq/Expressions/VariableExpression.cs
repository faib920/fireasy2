using System;

namespace Fireasy.Data.Entity.Linq.Expressions
{
    public class VariableExpression : DbExpression
    {
        public VariableExpression(string name, Type type)
            : base(DbExpressionType.Variable, type)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
