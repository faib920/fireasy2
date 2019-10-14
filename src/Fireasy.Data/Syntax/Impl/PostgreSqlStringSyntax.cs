// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Reflection;
namespace Fireasy.Data.Syntax
{
    public class PostgreSqlStringSyntax : StringSyntax
    {
        public override string IndexOf(object sourceExp, object searchExp, object startExp = null, object countExp = null)
        {
            if (startExp != null || countExp != null)
            {
                throw new SyntaxParseException(MethodInfo.GetCurrentMethod().Name);
            }

            return string.Format("POSITION({1} IN {0})", sourceExp, searchExp);
        }

        public override string Concat(params object[] strExps)
        {
            return string.Join(" || ", strExps);
        }
    }
}
