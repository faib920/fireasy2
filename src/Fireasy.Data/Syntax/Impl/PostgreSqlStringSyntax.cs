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

            return $"POSITION({searchExp} IN {sourceExp})";
        }

        public override string Concat(params object[] strExps)
        {
            return string.Join(" || ", strExps);
        }

        public override string GroupConcat(object sourceExp, object separator)
        {
            return $"STRING_AGG({sourceExp}, {separator})";
        }

    }
}
