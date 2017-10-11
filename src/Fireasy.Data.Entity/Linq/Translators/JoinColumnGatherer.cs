using Fireasy.Data.Entity.Linq.Expressions;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Translators
{
    public class JoinColumnGatherer
    {
        HashSet<TableAlias> aliases;
        HashSet<ColumnExpression> columns = new HashSet<ColumnExpression>();

        private JoinColumnGatherer(HashSet<TableAlias> aliases)
        {
            this.aliases = aliases;
        }

        public static HashSet<ColumnExpression> Gather(HashSet<TableAlias> aliases, SelectExpression select)
        {
            var gatherer = new JoinColumnGatherer(aliases);
            gatherer.Gather(select.Where);
            return gatherer.columns;
        }

        private void Gather(Expression expression)
        {
            BinaryExpression b = expression as BinaryExpression;
            if (b != null)
            {
                switch (b.NodeType)
                {
                    case ExpressionType.Equal:
                    case ExpressionType.NotEqual:
                        if (IsExternalColumn(b.Left) && GetColumn(b.Right) != null)
                        {
                            this.columns.Add(GetColumn(b.Right));
                        }
                        else if (IsExternalColumn(b.Right) && GetColumn(b.Left) != null)
                        {
                            this.columns.Add(GetColumn(b.Left));
                        }
                        break;
                    case ExpressionType.And:
                    case ExpressionType.AndAlso:
                        if (b.Type == typeof(bool) || b.Type == typeof(bool?))
                        {
                            this.Gather(b.Left);
                            this.Gather(b.Right);
                        }
                        break;
                }
            }
        }

        private ColumnExpression GetColumn(Expression exp)
        {
            while (exp.NodeType == ExpressionType.Convert || exp.NodeType == ExpressionType.ConvertChecked)
                exp = ((UnaryExpression)exp).Operand;
            return exp as ColumnExpression;
        }

        private bool IsExternalColumn(Expression exp)
        {
            var col = GetColumn(exp);
            if (col != null && !this.aliases.Contains(col.Alias))
                return true;
            return false;
        }
    }
}
