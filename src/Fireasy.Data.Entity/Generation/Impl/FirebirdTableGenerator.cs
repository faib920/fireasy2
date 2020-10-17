// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Syntax;
using System.Collections.Generic;
using System.Text;

namespace Fireasy.Data.Entity.Generation
{
    public class FirebirdTableGenerator : BaseTableGenerateProvider
    {
        protected override SqlCommand[] BuildCreateTableCommands(ISyntaxProvider syntax, string tableName, IList<IProperty> properties)
        {
            var sb = new StringBuilder();
            sb.Append($"create table {syntax.DelimitTable(tableName)}\n(\n");

            var count = properties.Count;
            var last = count - 1;
            for (var i = 0; i < count; i++)
            {
                AppendFieldToBuilder(sb, syntax, properties[i]);

                if (i != last)
                {
                    sb.Append(",");
                }

                sb.AppendLine();
            }

            sb.Append(");\n");

            return new SqlCommand[] { sb.ToString() };
        }

        protected override SqlCommand[] BuildAddFieldCommands(ISyntaxProvider syntax, string tableName, IList<IProperty> properties)
        {
            var sb = new StringBuilder();
            var count = properties.Count;
            for (var i = 0; i < count; i++)
            {
                sb.Append($"alter table {syntax.DelimitTable(tableName)} add ");

                AppendFieldToBuilder(sb, syntax, properties[i]);

                sb.AppendLine(";");
            }

            return new SqlCommand[] { sb.ToString() };
        }

        protected override void ProcessPrimaryKeyField(StringBuilder builder, ISyntaxProvider syntax, IProperty property)
        {
            builder.Append(" primary key");
        }
    }
}
