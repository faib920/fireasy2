using Fireasy.Data.Entity.Linq.Expressions;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Translators
{
    public class JoinColumnGatherer
    {
        private readonly HashSet<TableAlias> _aliases;
        private readonly HashSet<ColumnExpression> _columns = new HashSet<ColumnExpression>();

        private JoinColumnGatherer(HashSet<TableAlias> aliases)
        {
            _aliases = aliases;
        }

        public static HashSet<ColumnExpression> Gather(HashSet<TableAlias> aliases, SelectExpression select)
        {
            var gatherer = new JoinColumnGatherer(aliases);
            gatherer.Gather(select.Where);
            return gatherer._columns;
        }

        private void Gather(Expression expression)
        {
            if (expression is BinaryExpression bin)
            {
                switch (bin.NodeType)
                {
                    case ExpressionType.Equal:
                    case ExpressionType.NotEqual:
                        if (IsExternalColumn(bin.Left) && GetColumn(bin.Right) != null)
                        {
                            _columns.Add(GetColumn(bin.Right));
                        }
                        else if (IsExternalColumn(bin.Right) && GetColumn(bin.Left) != null)
                        {
                            _columns.Add(GetColumn(bin.Left));
                        }
                        break;
                    case ExpressionType.And:
                    case ExpressionType.AndAlso:
                        if (bin.Type == typeof(bool) || bin.Type == typeof(bool?))
                        {
                            Gather(bin.Left);
                            Gather(bin.Right);
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
            if (col != null && !_aliases.Contains(col.Alias))
                return true;
            return false;
        }
    }
}
