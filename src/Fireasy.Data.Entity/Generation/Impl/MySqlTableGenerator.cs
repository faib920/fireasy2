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
using System.Data;
using System.Linq;
using System.Text;

namespace Fireasy.Data.Entity.Generation
{
    public class MySqlTableGenerator : BaseTableGenerateProvider
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
                sb.Append("primary key (");

                for (var i = 0; i < primaryPeoperties.Length; i++)
                {
                    if (i != 0)
                    {
                        sb.Append(",");
                    }

                    sb.Append(Quote(syntax, primaryPeoperties[i].Info.FieldName));
                }

                sb.Append(")");
            }

            sb.Append(");\n");

            return new SqlCommand[] { sb.ToString() };
        }

        protected override SqlCommand[] BuildAddFieldCommands(ISyntaxProvider syntax, string tableName, IList<IProperty> properties)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("alter table {0}", Quote(syntax, tableName));

            var count = properties.Count;
            for (var i = 0; i < count; i++)
            {
                sb.Append(" add column ");

                AppendFieldToBuilder(sb, syntax, properties[i]);

                if (i != count - 1)
                {
                    sb.Append(",");
                }

                sb.AppendLine();
            }

            return new SqlCommand[] { sb.ToString() };
        }

        protected override bool IsExistsTable(IDataReader reader)
        {
            return !reader.IsDBNull(0);
        }
    }
}
