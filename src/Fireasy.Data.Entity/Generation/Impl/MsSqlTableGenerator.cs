// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fireasy.Data.Entity.Generation
{
    public class MsSqlTableGenerator : BaseTableGenerateProvider
    {
        protected override SqlCommand[] BuildCreateTableCommands(ISyntaxProvider syntax, string tableName, IList<IProperty> properties)
        {
            var sb = new StringBuilder();
            sb.Append($"create table {Delimit(syntax, tableName)}\n(\n");

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

            //主键
            var primaryPeoperties = properties.Where(s => s.Info.IsPrimaryKey).ToArray();
            if (primaryPeoperties.Length > 0)
            {
                sb.AppendLine(",");
                sb.Append($"\nconstraint {Delimit(syntax, "PK_" + tableName)} primary key (");

                for (int i = 0, n = primaryPeoperties.Length; i < n; i++)
                {
                    if (i != 0)
                    {
                        sb.Append(",");
                    }

                    sb.Append(Delimit(syntax, Delimit(syntax, primaryPeoperties[i].Info.FieldName)));
                }

                sb.Append(")");
            }

            sb.Append(")\n");

            return new SqlCommand[] { sb.ToString() };
        }

        protected override SqlCommand[] BuildAddFieldCommands(ISyntaxProvider syntax, string tableName, IList<IProperty> properties)
        {
            var sb = new StringBuilder();
            sb.Append($"alter table {Delimit(syntax, tableName)} add ");

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

            return new SqlCommand[] { sb.ToString() };
        }
    }
}
