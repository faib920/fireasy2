// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Extensions;
using Fireasy.Data.Entity.Metadata;
using Fireasy.Data.Entity.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 实体关系管理单元，用于从实体类型所属的程序集中获取所有 <see cref="RelationshipAttribute"/> 定义的实体关系。
    /// </summary>
    public static class RelationshipUnity
    {
        private static readonly SafetyDictionary<Assembly, SafetyDictionary<string, RelationshipMetadata>> _relationCache =
            new SafetyDictionary<Assembly, SafetyDictionary<string, RelationshipMetadata>>();

        /// <summary>
        /// 根据关联属性获取相应的实体关系。
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static RelationshipMetadata GetRelationship(IProperty property)
        {
            var relProperty = property.As<RelationProperty>();
            Guard.Argument(relProperty != null, nameof(property), SR.GetString(SRKind.NotRelationProperty));

            var relations = GetAssemblyRelationships(relProperty.EntityType.Assembly);
            RelationshipMetadata metadata = null;

            switch (relProperty.RelationalPropertyType)
            {
                case RelationPropertyType.EntitySet:
                    metadata = CheckSetKey(relations, relProperty);
                    break;
                case RelationPropertyType.RefProperty:
                case RelationPropertyType.Entity:
                    metadata = CheckSingleKey(relations, relProperty);
                    break;
            }

            if (metadata == null)
            {
                metadata = GetMetadataByAssignment(relProperty);
            }

            if (metadata == null)
            {
                throw new RelationshipException(SR.GetString(SRKind.NotDefinedRelationship, property.EntityType.Name, property.Name));
            }

            return metadata.Repair();
        }

        /// <summary>
        /// 获取指定程序集中定义的所有关系。
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="relationKey"></param>
        /// <returns></returns>
        public static RelationshipMetadata GetRelationship(Assembly assembly, string relationKey)
        {
            var metadata = GetRelationships(assembly);
            return metadata.ContainsKey(relationKey) ? metadata[relationKey].Repair() : null;
        }

        /// <summary>
        /// 添加元数据。
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="metadata"></param>
        internal static void InternalAddMetadata(string propertyName, RelationshipMetadata metadata)
        {
            var relations = _relationCache.GetOrAdd(metadata.PrincipalType.Assembly, () => new SafetyDictionary<string, RelationshipMetadata>());
            
            var key = string.Format("{0}:{1}:{2}",
               metadata.PrincipalType.Name, metadata.DependentType.Name, propertyName);

            relations.TryAdd(key, metadata);
        }

        /// <summary>
        /// 获取指定程序集中定义的所有关系。
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        private static IEnumerable<RelationshipMetadata> GetAssemblyRelationships(Assembly assembly)
        {
            return GetRelationships(assembly).Values;
        }

        private static SafetyDictionary<string, RelationshipMetadata> GetRelationships(Assembly assembly)
        {
            return _relationCache.GetOrAdd(assembly, key =>
            {
                try
                {
                    var dict = key.GetCustomAttributes<RelationshipAttribute>()
                        .ToDictionary(s => s.Name, s => new RelationshipMetadata(s.ThisType, s.OtherType,
                            GetDirection(s.KeyExpression), RelationshipSource.AssemblyAttribute, ParseRelationshipKeys(s.ThisType, s.OtherType, s.KeyExpression)));

                    return new SafetyDictionary<string, RelationshipMetadata>(dict);
                }
                catch (ArgumentException exp)
                {
                    throw new RelationshipException(SR.GetString(SRKind.RelationNameRepeated, key.GetName().Name), exp);
                }
            });
        }

        /// <summary>
        /// 根据规则获取实体关系。
        /// </summary>
        /// <param name="relProperty"></param>
        /// <returns></returns>
        private static RelationshipMetadata GetMetadataByAssignment(RelationProperty relProperty)
        {
            switch (relProperty.RelationalPropertyType)
            {
                case RelationPropertyType.Entity:
                    return CachedRelationshipMetadata(relProperty, relProperty.RelationalType, relProperty.EntityType);
                case RelationPropertyType.EntitySet:
                    return CachedRelationshipMetadata(relProperty, relProperty.EntityType, relProperty.RelationalType);
                case RelationPropertyType.RefProperty:
                    break;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="relProperty"></param>
        /// <param name="thisType"></param>
        /// <param name="otherType"></param>
        /// <returns></returns>
        private static RelationshipMetadata CachedRelationshipMetadata(RelationProperty relProperty, Type thisType, Type otherType)
        {
            if (_relationCache.TryGetValue(thisType.Assembly, out SafetyDictionary<string, RelationshipMetadata> relations))
            {
                var key = string.Format("{0}:{1}:{2}", 
                    thisType.Name, otherType.Name, relProperty.Info.ReflectionInfo.Name);

                return relations.GetOrAdd(key, () => MakeRelationshipMetadata(relProperty, thisType, otherType));
            }

            return null;
        }

        /// <summary>
        /// 使用主键和外键对应构造一对多的关系。
        /// </summary>
        /// <param name="relProperty"></param>
        /// <param name="thisType"></param>
        /// <param name="otherType"></param>
        /// <returns></returns>
        private static RelationshipMetadata MakeRelationshipMetadata(RelationProperty relProperty, Type thisType, Type otherType)
        {
            //是否使用了 ForeignKeyAttribute 来指定对应的外键
            var assignAttr = relProperty.Info.ReflectionInfo.GetCustomAttributes<RelationshipAssignAttribute>().FirstOrDefault();
            if (assignAttr != null)
            {
                var fkPro = PropertyUnity.GetProperty(otherType, assignAttr.ForeignKey);
                var pkPro = string.IsNullOrWhiteSpace(assignAttr.PrimaryKey) ? 
                    PropertyUnity.GetPrimaryProperties(thisType).FirstOrDefault() : PropertyUnity.GetProperty(thisType, assignAttr.PrimaryKey);
                if (fkPro != null && pkPro != null)
                {
                    if (!pkPro.Info.IsPrimaryKey)
                    {
                        throw new RelationshipException(SR.GetString(SRKind.IsNotPrimaryProperty, pkPro.Name));
                    }

                    var key = new RelationshipKey { PrincipalKey = pkPro.Name, PrincipalProperty = pkPro, DependentKey = fkPro.Name, DependentProperty = fkPro };
                    return new RelationshipMetadata(thisType, otherType, RelationshipStyle.One2Many, RelationshipSource.AutomaticallyAssign, new[] { key });
                }
                else
                {
                    throw new RelationshipException(SR.GetString(SRKind.InvalidRelationshipAssign, assignAttr.PrimaryKey, assignAttr.ForeignKey));
                }
            }

            //使用名称相同的主键进行匹配
            var pks = PropertyUnity.GetPrimaryProperties(thisType).ToList();
            if (pks.Count > 0)
            {
                var refId = thisType.Name.ToSingular() + "ID";
                var fks = pks.Select(s => PropertyUnity.GetProperty(otherType, s.Name) ?? PropertyUnity.GetProperty(otherType, refId)).ToList();
                var keys = new RelationshipKey[pks.Count];
                for (var i = 0; i < pks.Count; i++)
                {
                    if (fks[i] == null)
                    {
                        throw new RelationshipException(SR.GetString(SRKind.NotFoundRelationshipKey, thisType, otherType, pks[i].Name, refId));
                    }

                    keys[i] = new RelationshipKey { PrincipalKey = pks[i].Name, PrincipalProperty = pks[i], DependentKey = fks[i].Name, DependentProperty = fks[i] };
                }

                return new RelationshipMetadata(thisType, otherType, RelationshipStyle.One2Many, RelationshipSource.AutomaticallyAssign, keys);
            }

            return null;
        }

        private static RelationshipMetadata CheckSetKey(IEnumerable<RelationshipMetadata> list, RelationProperty relationPro)
        {
            return list.FirstOrDefault(item =>
                (item.PrincipalType == relationPro.EntityType &&
                item.DependentType == relationPro.RelationalType &&
                item.Style == RelationshipStyle.One2Many) ||
                (item.PrincipalType == relationPro.RelationalType &&
                item.DependentType == relationPro.EntityType &&
                item.Style == RelationshipStyle.Many2One));
        }

        private static RelationshipMetadata CheckSingleKey(IEnumerable<RelationshipMetadata> list, RelationProperty relationPro)
        {
            foreach (var item in list.Where(s => s.Source == RelationshipSource.AssemblyAttribute))
            {
                if (item.Style == RelationshipStyle.One2Many && item.PrincipalType == relationPro.RelationalType && item.DependentType == relationPro.EntityType)
                {
                    if (!string.IsNullOrEmpty(relationPro.RelationalKey))
                    {
                        if (item.Keys.Any(s => s.DependentKey == relationPro.RelationalKey))
                        {
                            return item;
                        }
                    }
                    else
                    {
                        return item;
                    }
                }
                else if (item.Style == RelationshipStyle.Many2One && item.PrincipalType == relationPro.EntityType && item.DependentType == relationPro.RelationalType)
                {
                    if (!string.IsNullOrEmpty(relationPro.RelationalKey))
                    {
                        if (item.Keys.Any(s => s.PrincipalKey == relationPro.RelationalKey))
                        {
                            return item;
                        }
                    }
                    else
                    {
                        return item;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 解析键对表达式。
        /// </summary>
        /// <param name="thisType"></param>
        /// <param name="otherType"></param>
        /// <param name="keyExpression">键对表达式，如 "父键1=>子键1,父键2=>子键2"。</param>
        private static IEnumerable<RelationshipKey> ParseRelationshipKeys(Type thisType, Type otherType, string keyExpression)
        {
            Guard.ArgumentNull(keyExpression, nameof(keyExpression));

            var keys = keyExpression.Split(',');

            var list = new List<RelationshipKey>();
            foreach (var pair in keys.Where(pair => !string.IsNullOrEmpty(pair)))
            {
                if (!ParseRelationKey(pair, out string thisKey, out string otherKey))
                {
                    continue;
                }
                list.Add(new RelationshipKey
                {
                    PrincipalKey = thisKey,
                    PrincipalProperty = PropertyUnity.GetProperty(thisType, thisKey),
                    DependentKey = otherKey,
                    DependentProperty = PropertyUnity.GetProperty(otherType, otherKey)
                });
            }
            return list;
        }

        private static RelationshipStyle GetDirection(string keyExpression)
        {
            if (keyExpression.IndexOf("=>") != -1)
            {
                return RelationshipStyle.One2Many;
            }
            if (keyExpression.IndexOf("<=") != -1)
            {
                return RelationshipStyle.Many2One;
            }
            if (keyExpression.IndexOf("==") != -1)
            {
                return RelationshipStyle.One2One;
            }
            throw new RelationshipException(SR.GetString(SRKind.NotDefinedRelationshipDirection, keyExpression));
        }

        private static bool ParseRelationKey(string data, out string thisKey, out string otherKey)
        {
            var index = 0;
            if ((index = data.IndexOf("=>")) != -1)
            {
                thisKey = data.Substring(0, index);
                otherKey = data.Substring(index + 2);
                return true;
            }
            if ((index = data.IndexOf("<=")) != -1)
            {
                otherKey = data.Substring(0, index);
                thisKey = data.Substring(index + 2);
                return true;
            }
            if ((index = data.IndexOf("==")) != -1)
            {
                thisKey = data.Substring(0, index);
                otherKey = data.Substring(index + 2);
                return true;
            }
            thisKey = otherKey = string.Empty;
            return false;
        }
    }
}
