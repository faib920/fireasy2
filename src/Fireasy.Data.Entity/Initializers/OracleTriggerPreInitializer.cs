// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Entity.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fireasy.Data.Entity.Initializers
{
    /// <summary>
    /// 此初始化器为 Oracle 数据库主键的生成提供一种方案，定义触发器通过获取序列值来进行替代。
    /// </summary>
    public class OracleTriggerPreInitializer : IEntityContextPreInitializer
    {
        private List<Type> entityTypes = null;

        /// <summary>
        /// 指定需要使用触发器的实体类型。
        /// </summary>
        /// <param name="entityType">实体类型。</param>
        public void Add(Type entityType)
        {
            if (entityTypes == null)
            {
                entityTypes = new List<Type>();
            }

            entityTypes.Add(entityType);
        }

        void IEntityContextPreInitializer.PreInitialize(EntityContextPreInitializeContext context)
        {
            foreach (var map in context.Mappers)
            {
                if (entityTypes != null && entityTypes.Count != 0 && !entityTypes.Contains(map.EntityType))
                {
                    continue;
                }

                var pk = PropertyUnity.GetPrimaryProperties(map.EntityType).FirstOrDefault(s => s.Info.GenerateType != IdentityGenerateType.None);
                if (pk != null)
                {
                    var database = context.EntityContext.Database;
                    var metadata = EntityMetadataUnity.GetEntityMetadata(map.EntityType);
                    var tableName = metadata.TableName.ToUpper();
                    var columnName = pk.Info.FieldName.ToUpper();

                    var sequenceName = FixSequenceName(tableName, columnName);

                    //创建序列
                    SqlCommand sql = $"SELECT 1 FROM USER_SEQUENCES WHERE SEQUENCE_NAME = '{sequenceName}'";
                    var result = database.ExecuteScalar(sql);

                    //不存在的话先创建序列
                    if (result == DBNull.Value || result == null)
                    {
                        //取表中该列的最大值 + 1
                        sql = $"SELECT MAX({columnName}) FROM {tableName}";
                        var value = database.ExecuteScalar<int>(sql) + 1;

                        sql = @"CREATE SEQUENCE {sequenceName} START WITH {value}";
                        database.ExecuteNonQuery(sql);
                    }

                    //创建触发器
                    sql = $"SELECT 1 FROM ALL_TRIGGERS WHERE TRIGGER_NAME = 'TRIG_{tableName}'";
                    result = database.ExecuteScalar(sql);

                    //不存在的话先创建序列
                    if (result == DBNull.Value || result == null)
                    {
                        sql = $@"
CREATE OR REPLACE TRIGGER TRIG_{tableName}
BEFORE INSERT ON {tableName} FOR EACH ROW WHEN (NEW.{columnName} IS NULL OR NEW.{columnName} = 0)
BEGIN
SELECT {sequenceName}.NEXTVAL INTO:NEW.{columnName} FROM DUAL;
END;";

                        database.ExecuteNonQuery(sql);
                    }

                    pk.Info.GenerateType = IdentityGenerateType.None;
                }
            }
        }

        /// <summary>
        /// 截断序列名称
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        private string FixSequenceName(string tableName, string columnName)
        {
            if (tableName.Length + columnName.Length >= 25)
            {
                return string.Format("SQ$_{0}_{1}", tableName.Substring(0, 25 - columnName.Length), columnName);
            }

            return string.Format("SQ$_{0}_{1}", tableName, columnName);
        }
    }
}
