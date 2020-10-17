﻿// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Provider;
using System;

namespace Fireasy.Data.Identity
{
    /// <summary>
    /// Oracle 中使用序列自动生成列的值。无法继承此类。
    /// </summary>
    public sealed class OracleSequenceGenerator : IGeneratorProvider
    {
        IProvider IProviderService.Provider { get; set; }

        /// <summary>
        /// 自动生成列的值。
        /// </summary>
        /// <param name="database">提供给当前插件的 <see cref="IDatabase"/> 对象。</param>
        /// <param name="tableName">表的名称。</param>
        /// <param name="columnName">列的名称。</param>
        /// <returns>用于标识唯一性的值。</returns>
        public int GenerateValue(IDatabase database, string tableName, string columnName = null)
        {
            tableName = tableName.ToUpper();
            columnName = columnName.ToUpper();
            var value = 0;
            var sequenceName = FixSequenceName(tableName, columnName);
            SqlCommand sql;

            if (GeneratorCache.IsSequenceCreated(tableName, columnName, () =>
                {
                    //查找是否存在序列
                    sql = $"SELECT 1 FROM USER_SEQUENCES WHERE SEQUENCE_NAME = '{sequenceName}'";
                    var result = database.ExecuteScalar(sql);

                    //不存在的话先创建序列
                    if (result == DBNull.Value || result == null)
                    {
                        //取表中该列的最大值 + 1
                        sql = $"SELECT MAX({columnName}) FROM {tableName}";
                        value = database.ExecuteScalar<int>(sql) + 1;

                        sql = $"CREATE SEQUENCE {sequenceName} START WITH {value}";
                        try
                        {
                            database.ExecuteNonQuery(sql);
                        }
                        catch (Exception exp)
                        {
                            throw new Exception(SR.GetString(SRKind.UnableCreateSequence, sequenceName), exp);
                        }
                    }

                    return true;
                }))
            {
                //查询下一个值
                sql = $"SELECT {sequenceName}.NEXTVAL FROM DUAL";
                value = database.ExecuteScalar<int>(sql);
            }

            return value;
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
                return $"SQ$_{tableName.Substring(0, 25 - columnName.Length)}_{columnName}";
            }

            return $"SQ$_{tableName}_{columnName}";
        }
    }
}
