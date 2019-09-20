// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;
using Fireasy.Data.Entity.Metadata;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 实体持久化环境，以使对数据库进行分表配置。无法继承此类。
    /// </summary>
    [Serializable]
    public sealed class EntityPersistentEnvironment
    {
        private readonly Dictionary<string, object> parameters = new Dictionary<string, object>();
        private Func<string, string> formatter;

        /// <summary>
        /// 添加一个环境变量，如果当前环境中已经存在该变量名称，则使用新值进行替换。
        /// </summary>
        /// <remarks>
        /// 如果实体类使用了 <see cref="EntityVariableAttribute"/> 标记，则该变量名称应包含在 TableName 中。
        /// </remarks>
        /// <param name="name">变量名称。</param>
        /// <param name="value">变量的值。</param>
        public EntityPersistentEnvironment AddVariable(string name, object value)
        {
            Guard.ArgumentNull(name, nameof(name));
            Guard.ArgumentNull(value, nameof(value));

            object v;
            if (!parameters.TryGetValue(name, out v))
            {
                parameters.Add(name, value);
            }
            else if (!v.Equals(value))
            {
                parameters[name] = value;
            }

            return this;
        }

        /// <summary>
        /// 设置格式化器。
        /// </summary>
        /// <param name="formatter"></param>
        /// <returns></returns>
        public EntityPersistentEnvironment SetFormatter(Func<string, string> formatter)
        {
            this.formatter = formatter;
            return this;
        }

        /// <summary>
        /// 移除指定的环境变量。
        /// </summary>
        /// <param name="name">变量名称。</param>
        public EntityPersistentEnvironment RemoveVariable(string name)
        {
            Guard.ArgumentNull(name, nameof(name));
            if (parameters.ContainsKey(name))
            {
                parameters.Remove(name);
            }

            return this;
        }

        /// <summary>
        /// 使用所添加的变量解析实体映射的数据表名称。
        /// </summary>
        /// <param name="entityType">实体类型。</param>
        /// <returns></returns>
        public string GetVariableTableName(Type entityType)
        {
            Guard.ArgumentNull(entityType, nameof(entityType));
            return GetVariableTableName(EntityMetadataUnity.GetEntityMetadata(entityType));
        }

        /// <summary>
        /// 使用所添加的变量解析实体映射的数据表名称。
        /// </summary>
        /// <param name="entityType">实体类型。</param>
        /// <param name="entity">当前实体</param>
        /// <returns></returns>
        public string GetVariableTableName(Type entityType, IEntity entity)
        {
            Guard.ArgumentNull(entityType, nameof(entityType));
            var metadata = EntityMetadataUnity.GetEntityMetadata(entityType);

            var regx = new Regex(@"(<\w+>)");
            var matches = regx.Matches(metadata.TableName);
            if (matches.Count == 0)
            {
                return metadata.TableName;
            }
            var tableName = metadata.TableName;
            var flag = new AssertFlag();
            foreach (Match match in matches)
            {
                var key = match.Value.TrimStart('<').TrimEnd('>');
                if (parameters.TryGetValue(key, out object v) && v is IProperty p)
                {
                    var value = entity.GetValue(p);
                    if (!PropertyValue.IsEmpty(value))
                    {
                        flag.AssertTrue();
                        tableName = tableName.Replace(match.Value, value.ToString());
                    }
                }
            }
            if (flag.AssertTrue())
            {
                return GetVariableTableName(metadata);
            }
            return tableName;
        }

        /// <summary>
        /// 使用所添加的变量解析实体映射的数据表名称。
        /// </summary>
        /// <param name="metadata">实体元数据。</param>
        /// <returns></returns>
        public string GetVariableTableName(EntityMetadata metadata)
        {
            if (formatter != null)
            {
                return formatter(metadata.TableName);
            }

            var regx = new Regex(@"(<\w+>)");
            var matches = regx.Matches(metadata.TableName);
            if (matches.Count == 0)
            {
                return metadata.TableName;
            }
            var tableName = metadata.TableName;
            foreach (Match match in matches)
            {
                var key = match.Value.TrimStart('<').TrimEnd('>');
                if (parameters.TryGetValue(key, out object v) && v != null)
                {
                    tableName = tableName.Replace(match.Value, v.ToString());
                }
            }
            return tableName;
        }
    }
}
