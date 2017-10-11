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
#if NET35
            var str = new string[strExps.Length];
            for (var i = 0; i < strExps.Length; i++)
            {
                str[i] = strExps[i].ToString();
            }
            return string.Join(" || ", str);
#else
            return string.Join(" || ", strExps);
#endif
        }
    }
}
