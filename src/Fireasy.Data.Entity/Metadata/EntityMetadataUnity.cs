// -----------------------------------------------------------------------
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
        private static SafetyDictionary<Type, EntityMetadata> cache = new SafetyDictionary<Type, EntityMetadata>();

        /// <summary>
        /// 获取指定类型的实体元数据。
        /// </summary>
        /// <param name="entityType">实体类型。</param>
        /// <returns></returns>
        public static EntityMetadata GetEntityMetadata(Type entityType)
        {
            Guard.ArgumentNull(entityType, nameof(entityType));

            var mapType = entityType.GetMapEntityType();

            var result = cache.GetOrAdd(mapType, () =>
            {
                var metadata = new EntityMetadata(mapType);

                //由于此代码段是缓存项创建工厂函数，此时的 metadata 并未添加到缓存中，接下来 PropertyUnity 
                //会再一次获取 EntityMetadata，因此需要在此线程中共享出 EntityMetadata
                using (var scope = new MetadataScope { Metadata = metadata })
                {
                    metadata.Attach(mapType);
                }

                return metadata;
            });

            return result.Attach(mapType);
        }

        /// <summary>
        /// 获取指定类型的实体元数据的内部方法。
        /// </summary>
        /// <param name="entityType">实体类型。</param>
        /// <returns></returns>
        internal static EntityMetadata InternalGetEntityMetadata(Type entityType)
        {
            return MetadataScope.Current != null ? MetadataScope.Current.Metadata : GetEntityMetadata(entityType);
        }

        private class MetadataScope : Scope<MetadataScope>
        {
            internal EntityMetadata Metadata { get; set; }
        }
    }
}