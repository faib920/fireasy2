// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Entity.Linq;
using Fireasy.Data.Entity.Metadata;
using Fireasy.Data.Syntax;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Linq;
using System.Text;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 实体仓储的创建者，用于检测仓储对应的表是否创建，如果没有则创建。
    /// </summary>
    public class RespositoryCreator
    {
        //存放实体类型对应的表有没有创建
        private static ConcurrentDictionary<string, bool> cache = new ConcurrentDictionary<string, bool>();

        /// <summary>
        /// 尝试创建对应的表。
        /// </summary>
        /// <param name="entityType">实体类型。</param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static bool TryCreate(Type entityType, InternalContext context)
        {
            var metadata = EntityMetadataUnity.GetEntityMetadata(entityType);

            var lazy = new Lazy<bool>(() =>
                {
                    var syntax = context.Database.Provider.GetService<ISyntaxProvider>();

                    //判断表是否存在
                    SqlCommand sql = syntax.ExistsTable(metadata.TableName);
                    if (context.Database.ExecuteScalar<int>(sql) == 0)
                    {
                        if (InternalCreate(metadata, syntax, context))
                        {
                            //通知 context 仓储已经创建
                            context.OnRespositoryCreated?.Invoke(entityType);

                            return true;
                        }

                        return false;
                    }

                    return true;
                });

            var cacheKey = string.Format("{0}:{1}", context.InstanceName, entityType.FullName);
            return cache.GetOrAdd(cacheKey, key => lazy.Value);
        }

        private static bool InternalCreate(EntityMetadata metadata, ISyntaxProvider syntax, InternalContext context)
        {
            //表的名称
            var tableName = metadata.TableName;

            try
            {
                SqlCommand sql = BuildTableScript(metadata.EntityType, syntax, tableName);
                return context.Database.ExecuteNonQuery(sql) > 0;
            }
            catch (Exception exp)
            {
                return false;
            }
        }

        /// <summary>
        /// 构造创建表的语句。
        /// </summary>
        /// <param name="entityType">实体类型。</param>
        /// <param name="syntax">语法服务。</param>
        /// <param name="tableName">数据表名称。</param>
        /// <returns></returns>
        private static string BuildTableScript(Type entityType, ISyntaxProvider syntax, string tableName)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("create table {0}\n(\n", tableName);

            //获取实体类型中所有可持久化的属性，不包含引用类型的属性
            var properties = PropertyUnity.GetPersistentProperties(entityType).ToArray();
            var primaryPeoperties = PropertyUnity.GetPrimaryProperties(entityType).ToArray();
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
                sb.AppendFormat("constraint PK_{0} primary key (", tableName);

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

            return sb.ToString();
        }
    }
}
