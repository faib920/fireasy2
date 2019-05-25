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
using System.Collections.ObjectModel;
using System.Linq;

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

            var instanceName = ContextInstanceManager.TryAdd(service.InitializeContext);

            var cacheKey = string.Concat(instanceName, ":", entityType.FullName);
            return cache.GetOrAdd(cacheKey, () =>
                {
                    //判断数据表是否已存在
                    if (tbGen.IsExists(service.Database, metadata))
                    {
                        try
                        {
                            //尝试添加新的字段
                            var properties = tbGen.TryAddFields(service.Database, metadata);
                            if (properties.Any())
                            {
                                //通知 context 仓储已经以身改变
                                service.OnRespositoryChanged?.Invoke(new RespositoryChangedEventArgs
                                {
                                    Succeed = true,
                                    EntityType = entityType,
                                    EventType = RespositoryChangeEventType.AddFields,
                                    AddedProperties = new ReadOnlyCollection<IProperty>(properties)
                                });
                            }
                        }
                        catch (Exception exp)
                        {
                            service.OnRespositoryChanged?.Invoke(new RespositoryChangedEventArgs
                            {
                                EntityType = entityType,
                                EventType = RespositoryChangeEventType.AddFields,
                                Exception = exp
                            });
                            return false;
                        }
                    }
                    else
                    {
                        try
                        {
                            //尝试创建数据表
                            if (tbGen.TryCreate(service.Database, metadata))
                            {
                                //通知 context 仓储已经创建
                                service.OnRespositoryChanged?.Invoke(new RespositoryChangedEventArgs
                                {
                                    Succeed = true,
                                    EntityType = entityType,
                                    EventType = RespositoryChangeEventType.CreateTable
                                });
                            }
                        }
                        catch (Exception exp)
                        {
                            service.OnRespositoryChanged?.Invoke(new RespositoryChangedEventArgs
                            {
                                EntityType = entityType,
                                EventType = RespositoryChangeEventType.CreateTable,
                                Exception = exp
                            });
                            return false;
                        }
                    }

                    return true;
                });
        }
    }
}
