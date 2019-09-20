// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Entity.Metadata;
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
            sb.AppendFormat("create table {0}\n(\n", Quote(syntax, tableName));

            var count = properties.Count;
            for (var i = 0; i < count; i++)
            {
                AppendFieldToBuilder(sb, syntax, properties[i]);

                if (i != count - 1)
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
                sb.AppendFormat("\nconstraint {0} primary key (", Quote(syntax, "PK_" + tableName));

                for (var i = 0; i < primaryPeoperties.Length; i++)
                {
                    if (i != 0)
                    {
                        sb.Append(",");
                    }

                    sb.Append(Quote(syntax, Quote(syntax, primaryPeoperties[i].Info.FieldName)));
                }

                sb.Append(")");
            }

            sb.Append(")\n");

            return new SqlCommand[] { sb.ToString() };
        }

        protected override SqlCommand[] BuildAddFieldCommands(ISyntaxProvider syntax, string tableName, IList<IProperty> properties)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("alter table {0} add ", Quote(syntax, tableName));

            var count = properties.Count;
            for (var i = 0; i < count; i++)
            {
                AppendFieldToBuilder(sb, syntax, properties[i]);

                if (i != count - 1)
                {
                    sb.Append(",");
                }

                sb.AppendLine();
            }

            return new SqlCommand[] { sb.ToString() };
        }
    }
}
