
namespace Fireasy.Data.Syntax
{
    public class PostgreSqlMathSyntax : MathSyntax
    {
        public override string ExclusiveOr(object sourceExp, object otherExp)
        {
            return string.Format("({0} # {1})", sourceExp, otherExp);
        }

        public override string Log10(object sourceExp)
        {
            return string.Format("LOG({0}, 10)", sourceExp);
        }

        public override string Truncate(object sourceExp)
        {
            return string.Format("TRUNC({0}, 0)", sourceExp);
        }
    }
}
