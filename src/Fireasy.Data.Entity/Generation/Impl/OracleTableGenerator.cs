// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Entity.Metadata;
using Fireasy.Data.Syntax;
using System.Data;
using System.Linq;
using System.Text;

namespace Fireasy.Data.Entity.Generation
{
    public class OracleTableGenerator : BaseTableGenerateProvider
    {
        protected override SqlCommand[] BuildCreateTableCommands(ISyntaxProvider syntax, EntityMetadata metadata)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("create table {0}\n(\n", metadata.TableName);

            //获取实体类型中所有可持久化的属性，不包含引用类型的属性
            var properties = PropertyUnity.GetPersistentProperties(metadata.EntityType).ToArray();
            var primaryPeoperties = PropertyUnity.GetPrimaryProperties(metadata.EntityType).ToArray();
            var count = properties.Length;
            for (var i = 0; i < count; i++)
            {
                var property = properties[i];
                sb.AppendFormat(" {0}", property.Info.FieldName);

                //数据类型及长度精度等
                sb.AppendFormat(" {0}", syntax.Column((DbType)property.Info.DataType,
                    property.Info.Length,
                    property.Info.Precision,
                    property.Info.Scale));

                //自增
                if (property.Info.GenerateType == IdentityGenerateType.AutoIncrement &&
                    !string.IsNullOrEmpty(syntax.IdentityColumn))
                {
                    sb.AppendFormat(" {0}", syntax.IdentityColumn);
                }

                //不可空
                if (!property.Info.IsNullable)
                {
                    sb.AppendFormat(" not null");
                }

                //默认值
                if (!PropertyValue.IsEmpty(property.Info.DefaultValue))
                {
                    if (property.Type == typeof(string))
                    {
                        sb.AppendFormat(" default '{0}'", property.Info.DefaultValue);
                    }
                    else if (property.Type.IsEnum)
                    {
                        sb.AppendFormat(" default {0}", (int)property.Info.DefaultValue);
                    }
                    else if (property.Type == typeof(bool) || property.Type == typeof(bool?))
                    {
                        sb.AppendFormat(" default {0}", (bool)property.Info.DefaultValue ? 1 : 0);
                    }
                    else
                    {
                        sb.AppendFormat(" default {0}", property.Info.DefaultValue);
                    }
                }

                if (i != count - 1)
                {
                    sb.Append(",");
                }

                sb.AppendLine();
            }

            //主键
            if (primaryPeoperties.Length > 0)
            {
                sb.Append(",");
                sb.AppendFormat("constraint PK_{0} primary key (", metadata.TableName);

                for (var i = 0; i < primaryPeoperties.Length; i++)
                {
                    if (i != 0)
                    {
                        sb.Append(",");
                    }

                    sb.Append(primaryPeoperties[i].Info.FieldName);
                }

                sb.Append(")");
            }

            sb.Append(")\n");

            return new SqlCommand[] { sb.ToString() };
        }
    }
}
