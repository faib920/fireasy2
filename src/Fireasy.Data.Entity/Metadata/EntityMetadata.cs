// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Fireasy.Common.Extensions;

namespace Fireasy.Data.Entity.Metadata
{
    /// <summary>
    /// 用于描述实体结构的元数据。无法继承此类。
    /// </summary>
    public sealed class EntityMetadata
    {
        private readonly MetadataPropertyDictionary properties = new MetadataPropertyDictionary();
        private readonly List<IProperty> concurrencyProperties = new List<IProperty>();

        /// <summary>
        /// 初始化 <see cref="EntityMetadata"/> 类的新实例。
        /// </summary>
        /// <param name="entityType">一个实体的类型。</param>
        internal EntityMetadata(Type entityType)
        {
            EntityType = entityType;

            //获取描述表名称映射的 EntityMappingAttribute
            var mapper = entityType.GetCustomAttributes<EntityMappingAttribute>(true).FirstOrDefault();
            if (mapper != null)
            {
                TableName = mapper.TableName;
                Description = mapper.Description;
            }
            else
            {
                TableName = entityType.Name;
            }

            //获取描述树结构映射的 EntityTreeMappingAttribute
            var treeMapper = entityType.GetCustomAttributes<EntityTreeMappingAttribute>().FirstOrDefault();
            if (treeMapper != null)
            {
                EntityTree = new EntityTreeMetadata(treeMapper);
            }

            //获取表名称变量映射的 EntityVariableAttribute
            var varMapper = entityType.GetCustomAttributes<EntityVariableAttribute>().FirstOrDefault();
            if (varMapper != null)
            {
                TableName = varMapper.Expression.Replace("$", TableName);
            }
        }

        /// <summary>
        /// 获取实体的类型。
        /// </summary>
        public Type EntityType { get; private set; }

        /// <summary>
        /// 获取数据表名称。
        /// </summary>
        public string TableName { get; internal set; }

        /// <summary>
        /// 获取或设置注释。
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// 获取标识逻辑删除的属性。
        /// </summary>
        public IProperty DeleteProperty { get; internal set; }

        /// <summary>
        /// 获取标识并发控制的属性列表。
        /// </summary>
        public ReadOnlyCollection<IProperty> ConcurrencyProperties
        {
            get 
            {
                return new ReadOnlyCollection<IProperty>(concurrencyProperties);
            }
        }

        /// <summary>
        /// 获取实体的所有属性列表。
        /// </summary>
        public MetadataPropertyDictionary Properties
        {
            get
            {
                return properties;
            }
        }

        /// <summary>
        /// 获取实体树结构的元数据。
        /// </summary>
        public EntityTreeMetadata EntityTree { get; private set; }

        /// <summary>
        /// 往集合中添加一个属性。
        /// </summary>
        /// <param name="property"></param>
        internal void InternalAddProperty(IProperty property)
        {
            if (properties.Keys.Any(s => s == property.Name))
            {
                return;
            }

            if (property.Info.IsDeletedKey)
            {
                DeleteProperty = property;
            }

            if (property.Info.IsConcurrencyKey)
            {
                concurrencyProperties.Add(property);
            }

            properties.Add(property.Name, property);

            if (EntityTree != null)
            {
                EntityTree.InitTreeMetadata(property);
            }
        }
    }

    public class MetadataPropertyDictionary
    {
        private Dictionary<string, IProperty> dic = new Dictionary<string, IProperty>();

        internal void Add(string name, IProperty property)
        {
            dic.Add(name, property);
        }

        public IEnumerable<string> Keys
        {
            get
            {
                return dic.Keys;
            }
        }

        public IEnumerable<IProperty> Values
        {
            get
            {
                return dic.Values;
            }
        }

        public IProperty this[string name]
        {
            get
            {
                if (dic.ContainsKey(name))
                {
                    return dic[name];
                }

                return null;
            }
        }
    }
}