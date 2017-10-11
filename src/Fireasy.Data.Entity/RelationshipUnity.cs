// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;
using Fireasy.Common.Extensions;
using Fireasy.Data.Entity.Metadata;
using Fireasy.Data.Entity.Properties;
using System;
using System.Collections.Concurrent;
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
        private static readonly ConcurrentDictionary<Assembly, ConcurrentDictionary<string, RelationshipMetadata>> relationCache =
            new ConcurrentDictionary<Assembly, ConcurrentDictionary<string, RelationshipMetadata>>();

        /// <summary>
        /// 根据关联属性获取相应的实体关系。
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static RelationshipMetadata GetRelationship(IProperty property)
        {
            var relProperty = property.As<RelationProperty>();
            Guard.Argument(relProperty != null, "property", SR.GetString(SRKind.NotRelationProperty));

            var relations = GetAssemblyRelationships(relProperty.EntityType.Assembly);

            RelationshipMetadata metadta = null;
            switch (relProperty.RelationPropertyType)
            {
                case RelationPropertyType.EntitySet:
                    metadta = CheckSetKey(relations, relProperty);
                    break;
                case RelationPropertyType.RefProperty:
                case RelationPropertyType.Entity:
                    metadta = CheckSingleKey(relations, relProperty);
                    break;
            }

            if (metadta == null)
            {
                metadta = GetMetadataByRule(relProperty);
            }

            if (metadta == null)
            {
                throw new RelationshipException(SR.GetString(SRKind.NotDefinedRelationship));
            }

            return metadta;
        }

        /// <summary>
        /// 获取指定程序集中定义的所有关系。
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        private static IEnumerable<RelationshipMetadata> GetAssemblyRelationships(Assembly assembly)
        {
            var lazy = new Lazy<ConcurrentDictionary<string, RelationshipMetadata>>(() =>
                {
                    try
                    {
                        var dict = assembly.GetCustomAttributes<RelationshipAttribute>()
                            .ToDictionary(s => s.Name, s => new RelationshipMetadata(s.ThisType, s.OtherType,
                                GetDirection(s.KeyExpression), ParseRelationshipKeys(s.ThisType, s.OtherType, s.KeyExpression)));
                        
                        return new ConcurrentDictionary<string, RelationshipMetadata>(dict);
                    }
                    catch (ArgumentException exp)
                    {
                        throw new ArgumentException(SR.GetString(SRKind.RelationNameRepeated), exp);
                    }
                });

            return relationCache.GetOrAdd(assembly, s => lazy.Value).Values;
        }

        /// <summary>
        /// 根据规则获取实体关系。
        /// </summary>
        /// <param name="relProperty"></param>
        /// <returns></returns>
        private static RelationshipMetadata GetMetadataByRule(RelationProperty relProperty)
        {
            switch (relProperty.RelationPropertyType)
            {
                case RelationPropertyType.Entity:
                    return MakeOne2ManyMetadataCached(relProperty.RelationType, relProperty.EntityType);
                case RelationPropertyType.EntitySet:
                    return MakeOne2ManyMetadataCached(relProperty.EntityType, relProperty.RelationType);
                case RelationPropertyType.RefProperty:
                    var p = relProperty as ReferenceProperty;
                    break;
            }

            return null;
        }

        private static RelationshipMetadata MakeOne2ManyMetadataCached(Type thisType, Type otherType)
        {
            RelationshipMetadata metadata = null;
            var key = string.Format("{0}:{1}",
                EntityMetadataUnity.GetEntityMetadata(thisType).TableName,
                EntityMetadataUnity.GetEntityMetadata(otherType).TableName);

            ConcurrentDictionary<string, RelationshipMetadata> relations;
            if (relationCache.TryGetValue(thisType.Assembly, out relations))
            {
                if (!relations.TryGetValue(key, out metadata))
                {
                    var lazy = new Lazy<RelationshipMetadata>(() => MakeOne2ManyMetadata(thisType, otherType));
                    metadata = relations.GetOrAdd(key, s => lazy.Value);
                }
            }

            return metadata;
        }

        /// <summary>
        /// 使用主键和外键对应构造一对多的关系。
        /// </summary>
        /// <param name="thisType"></param>
        /// <param name="otherType"></param>
        /// <returns></returns>
        private static RelationshipMetadata MakeOne2ManyMetadata(Type thisType, Type otherType)
        {
            var pks = PropertyUnity.GetPrimaryProperties(thisType).ToList();
            if (pks.Count > 0)
            {
                var fks = pks.Select(s => PropertyUnity.GetProperty(otherType, s.Name)).ToList();
                var keys = new RelationshipKey[pks.Count];
                for (var i = 0; i < pks.Count; i++)
                {
                    if (fks[i] == null)
                    {
                        throw new Exception();
                    }

                    keys[i] = new RelationshipKey { ThisKey = pks[i].Name, ThisProperty = pks[i], OtherKey = fks[i].Name, OtherProperty = fks[i] };
                }

                return new RelationshipMetadata(thisType, otherType, RelationshipStyle.One2Many, keys);
            }

            return null;
        }

        private static RelationshipMetadata CheckSetKey(IEnumerable<RelationshipMetadata> list, RelationProperty relationPro)
        {
            return list.FirstOrDefault(item =>
                (item.ThisType == relationPro.EntityType &&
                item.OtherType == relationPro.RelationType &&
                item.Style == RelationshipStyle.One2Many) ||
                (item.ThisType == relationPro.RelationType &&
                item.OtherType == relationPro.EntityType &&
                item.Style == RelationshipStyle.Many2One));
        }

        private static RelationshipMetadata CheckSingleKey(IEnumerable<RelationshipMetadata> list, RelationProperty relationPro)
        {
            return list.FirstOrDefault(item =>
                (item.ThisType == relationPro.RelationType &&
                item.OtherType == relationPro.EntityType &&
                item.Style == RelationshipStyle.One2Many) ||
                (item.ThisType == relationPro.EntityType &&
                item.OtherType == relationPro.RelationType &&
                item.Style == RelationshipStyle.Many2One) ||
                (((item.ThisType == relationPro.EntityType &&
                item.OtherType == relationPro.RelationType) ||
                (item.ThisType == relationPro.RelationType &&
                item.OtherType == relationPro.EntityType)) &&
                item.Style == RelationshipStyle.One2One));
        }

        /// <summary>
        /// 解析键对表达式。
        /// </summary>
        /// <param name="thisType"></param>
        /// <param name="otherType"></param>
        /// <param name="keyExpression">键对表达式，如 "父键1=>子键1,父键2=>子键2"。</param>
        private static IEnumerable<RelationshipKey> ParseRelationshipKeys(Type thisType, Type otherType, string keyExpression)
        {
            Guard.ArgumentNull(keyExpression, "keyExpression");

            var keys = keyExpression.Split(',');

            var list = new List<RelationshipKey>();
            foreach (var pair in keys.Where(pair => !string.IsNullOrEmpty(pair)))
            {
                string thisKey, otherKey;
                if (!ParseRelationKey(pair, out thisKey, out otherKey))
                {
                    continue;
                }
                list.Add(new RelationshipKey
                    {
                        ThisKey = thisKey,
                        ThisProperty = PropertyUnity.GetProperty(thisType, thisKey),
                        OtherKey = otherKey,
                        OtherProperty = PropertyUnity.GetProperty(otherType, otherKey)
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
            throw new RelationshipException(SR.GetString(SRKind.NotDefinedRelationshipDirection));
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
