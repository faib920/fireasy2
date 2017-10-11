using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Expressions
{
    [DebuggerDisplay("DbNodeType={DbNodeType},Name={name},Value={value},SqlType={sqlType}")]
    public class NamedValueExpression : DbExpression
    {
        private readonly string name;
        private readonly Expression value;

        public NamedValueExpression(string name, Expression value)
            : base(DbExpressionType.NamedValue, value.Type)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (value == null)
                throw new ArgumentNullException("value");
            this.name = name;
            this.value = value;
        }

        public string Name
        {
            get { return this.name; }
        }

        public Expression Value
        {
            get { return this.value; }
        }
    }
}
