// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Data.Syntax
{
    public class OleDbSyntax4Excel : OleDbSyntax
    {
        /// <summary>
        /// 给表名添加界定符。
        /// </summary>
        /// <param name="tableName"></param>
        public override string DelimitTable(string tableName)
        {
            if (!tableName.EndsWith("$") && !tableName.EndsWith("$" + Delimiter[1]))
            {
                tableName += "$";
            }

            return DbUtility.FormatByDelimiter(this, tableName, true);
        }
    }
}
