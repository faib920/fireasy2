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

        public override string Log10(object sourceExp)
        {
            return $"LOG({sourceExp}, 10)";
        }

        public override string Truncate(object sourceExp)
        {
            return $"TRUNC({sourceExp}, 0)";
        }
    }
}
