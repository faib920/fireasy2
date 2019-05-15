// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.ComponentModel;
using Fireasy.Data.Entity.Metadata;
using Fireasy.Data.Entity.Providers;
using System;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 实体仓储的创建者，用于检测仓储对应的表是否创建，如果没有则创建。
    /// </summary>
    public class RespositoryCreator
    {
        //存放实体类型对应的表有没有创建
        private static SafetyDictionary<string, bool> cache = new SafetyDictionary<string, bool>();

        /// <summary>
        /// 尝试创建对应的表。
        /// </summary>
        /// <param name="service"></param>
        /// <param name="entityType">实体类型。</param>
        /// <returns></returns>
        public static bool TryCreate(IContextService service, Type entityType)
        {
            var tbGen = service.Provider.GetTableGenerateProvider();
            if (service == null)
            {
                return false;
            }

            var metadata = EntityMetadataUnity.GetEntityMetadata(entityType);

            var cacheKey = string.Concat(service.Provider.ProviderName, ":", service.InitializeContext.ConnectionString);
            return cache.GetOrAdd(cacheKey, () =>
                {
                    try
                    {
                        //判断数据表是否已存在
                        if (tbGen.IsExists(service.Database, metadata))
                        {
                            //尝试添加新的字段
                            tbGen.TryAddFields(service.Database, metadata);
                        }
                        else
                        {
                            //尝试创建数据表
                            tbGen.TryCreate(service.Database, metadata);

                            //通知 context 仓储已经创建
                            service.OnRespositoryCreated?.Invoke(new RespositoryCreatedEventArgs { EntityType = entityType, Succeed = true });
                        }

                        return true;
                    }
                    catch (Exception exp)
                    {
                        service.OnRespositoryCreated?.Invoke(new RespositoryCreatedEventArgs { EntityType = entityType, Exception = exp });
                        return false;
                    }
                });
        }
    }
}
