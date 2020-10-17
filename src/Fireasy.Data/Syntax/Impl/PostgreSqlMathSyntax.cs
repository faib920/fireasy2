// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Data.Syntax
{
    public class PostgreSqlMathSyntax : MathSyntax
    {
        public override string ExclusiveOr(object sourceExp, object otherExp)
        {
            return $"({sourceExp} # {otherExp})";
        }

        public override string Log(object sourceExp)
        {
            return $"LN({sourceExp})";
        }

        public override string Log10(object sourceExp)
        {
            return $"LOG({sourceExp})";
        }

        public override string Ceiling(object sourceExp)
        {
            return $"CEIL({sourceExp})";
        }

        public override string Truncate(object sourceExp)
        {
            return $"TRUNC({sourceExp}, 0)";
        }

        public override string Random()
        {
            return "RANDOM()";
        }

        public override string Modulo(object sourceExp, object otherExp)
        {
            return $"MOD({sourceExp}, {otherExp})";
        }
    }
}
