// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace Fireasy.Data.Entity.Metadata
{
    /// <summary>
    /// 用于描述实体结构的元数据。无法继承此类。
    /// </summary>
    public sealed class EntityMetadata
    {
        private readonly MetadataPropertyDictionary properties = new MetadataPropertyDictionary();
        private readonly List<IProperty> concurrencyProperties = new List<IProperty>();
        private List<Type> inheritedTypes = new List<Type>();

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
                EntityTree = new EntityTreeMetadata(this, treeMapper);
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
        /// 使用指定的实体类型进行过滤。
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public IDictionary<string, IProperty> Filter(Type entityType)
        {
            return properties.GetDictionary()
                .Where(s => s.Value.Info != null && s.Value.Info.ReflectionInfo != null && s.Value.Info.ReflectionInfo.DeclaringType.IsAssignableFrom(entityType))
                .ToDictionary(s => s.Key, s => s.Value);
        }

        internal EntityMetadata Attach(Type entityType)
        {
            if (inheritedTypes.Contains(entityType))
            {
                return this;
            }

            inheritedTypes.Add(entityType);

            if (typeof(ICompilableEntity).IsAssignableFrom(entityType))
            {
                PropertyUnity.Initialize(entityType);
            }
            else
            {
                //需要找出其中的一个字段，然后以引发 RegisterProperty 调用
                var field = entityType.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).FirstOrDefault();
                if (field != null)
                {
                    field.GetValue(null);
                }
            }

            return this;
        }

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

        internal IDictionary<string, IProperty> GetDictionary()
        {
            return dic;
        }

        internal bool FirstOrDefault()
        {
            throw new NotImplementedException();
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