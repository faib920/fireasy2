// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Data;
using Fireasy.Data.Syntax;

namespace Fireasy.Data.Syntax
{
    /// <summary>
    /// 异常辅助类。
    /// </summary>
    internal class ExceptionHelper
    {
        internal static string ThrowSyntaxCreteException(DbType dbType)
        {
            return string.Empty;
        }

        internal static string ThrowSyntaxConvertException(DbType dbType)
        {
            return string.Empty;
        }

        internal static string ThrowSyntaxParseException(string methodName)
        {
            throw new SyntaxParseException(methodName);
        }
    }
}
