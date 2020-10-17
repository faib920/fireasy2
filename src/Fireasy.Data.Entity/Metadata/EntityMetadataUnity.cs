﻿// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;
using Fireasy.Common.ComponentModel;
using System;

namespace Fireasy.Data.Entity.Metadata
{
    /// <summary>
    /// 实体元数据的管理单元。
    /// </summary>
    public static class EntityMetadataUnity
    {
        private static readonly SafetyDictionary<Type, EntityMetadata> _cache = new SafetyDictionary<Type, EntityMetadata>();

        /// <summary>
        /// 获取指定类型的实体元数据。
        /// </summary>
        /// <param name="entityType">实体类型。</param>
        /// <returns></returns>
        public static EntityMetadata GetEntityMetadata(Type entityType)
        {
            Guard.ArgumentNull(entityType, nameof(entityType));

            var mapType = entityType.GetMapEntityType();

            var result = _cache.GetOrAdd(mapType, key =>
            {
                var metadata = new EntityMetadata(key);

                //由于此代码段是缓存项创建工厂函数，此时的 metadata 并未添加到缓存中，接下来 PropertyUnity 
                //会再一次获取 EntityMetadata，因此需要在此线程中共享出 EntityMetadata
                using (var scope = new MetadataInitializeScope(metadata))
                {
                    metadata.Attach(key);
                }

                return metadata;
            });

            return result.Attach(mapType);
        }

        /// <summary>
        /// 判断是否元数据化。
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static bool IsMetadata(Type entityType)
        {
            var mapType = entityType.GetMapEntityType();

            return _cache.ContainsKey(mapType);
        }

        /// <summary>
        /// 添加实体元数据。
        /// </summary>
        /// <param name="entityType">实体类型。</param>
        /// <param name="metadata"></param>
        public static void AddEntityMetadata(Type entityType, EntityMetadata metadata)
        {
            _ = _cache.GetOrAdd(entityType, () => metadata);
        }

        /// <summary>
        /// 获取指定类型的实体元数据的内部方法。
        /// </summary>
        /// <param name="entityType">实体类型。</param>
        /// <returns></returns>
        internal static EntityMetadata InternalGetEntityMetadata(Type entityType)
        {
            return MetadataInitializeScope.Current != null ? MetadataInitializeScope.Current.Metadata : GetEntityMetadata(entityType);
        }
    }

    public class MetadataInitializeScope : Scope<MetadataInitializeScope>
    {
        public MetadataInitializeScope(EntityMetadata metadata)
        {
            Metadata = metadata;
        }

        public EntityMetadata Metadata { get; }
    }
}