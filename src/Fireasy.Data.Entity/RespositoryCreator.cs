// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Entity.Linq;
using Fireasy.Data.Entity.Metadata;
using Fireasy.Data.Entity.Providers;
using System;
using System.Collections.Concurrent;

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
        public static bool TryCreate(Type entityType, InternalContext context, Action<Type> succeed, Action<Type, Exception> failed)
        {
            var metadata = EntityMetadataUnity.GetEntityMetadata(entityType);

            var lazy = new Lazy<bool>(() =>
                {
                    var service = context.Database.Provider.GetTableGenerateProvider();
                    if (service == null || service.IsExists(context.Database, entityType))
                    {
                        return true;
                    }

                    try
                    {
                        service.TryCreate(context.Database, entityType);

                        //通知 context 仓储已经创建
                        succeed?.Invoke(entityType);

                        return true;
                    }
                    catch (Exception exp)
                    {
                        failed?.Invoke(entityType, exp);
                        return false;
                    }
                });

            var cacheKey = string.Format("{0}:{1}", context.InstanceName, entityType.FullName);
            return cache.GetOrAdd(cacheKey, key => lazy.Value);
        }
    }
}
