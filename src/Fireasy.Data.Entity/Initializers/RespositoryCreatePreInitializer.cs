// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Extensions;
using Fireasy.Data.Entity.Generation;
using Fireasy.Data.Entity.Metadata;
using Fireasy.Data.Entity.Providers;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Fireasy.Data.Entity.Initializers
{
    /// <summary>
    /// 此初始化器用于处理 CodeFirst 模式下实体与数据表之间结构上的同步。
    /// </summary>
    public class RespositoryCreatePreInitializer : IEntityContextPreInitializer
    {
        //存放实体类型对应的表有没有创建
        private static SafetyDictionary<string, bool> cache = new SafetyDictionary<string, bool>();

        /// <summary>
        /// 获取或设置事件通知。
        /// </summary>
        public Action<RespositoryChangedEventArgs> EventHandler { get; set; }

        void IEntityContextPreInitializer.PreInitialize(EntityContextPreInitializeContext context)
        {
            var tbGen = context.Service.Provider.GetTableGenerateProvider();
            context.Mappers.ForEach(s => TryCreate(context.Service, tbGen, s.EntityType));
        }

        protected virtual bool TryCreate(IContextService service, ITableGenerateProvider tbGen, Type entityType)
        {
            var metadata = EntityMetadataUnity.GetEntityMetadata(entityType);

            var tableName = string.Empty;
            if (service is IEntityPersistentEnvironment env && env.Environment != null)
            {
                tableName = env.Environment.GetVariableTableName(metadata);
            }
            else
            {
                tableName = metadata.TableName;
            }

            var instanceName = ContextInstanceManager.TryAdd(service.InitializeContext);

            var cacheKey = string.Concat(instanceName, ":", entityType.FullName);
            return cache.GetOrAdd(cacheKey, () =>
            {
                //判断数据表是否已存在
                if (tbGen.IsExists(service.Database, tableName))
                {
                    try
                    {
                        //尝试添加新的字段
                        var properties = tbGen.TryAddFields(service.Database, metadata, tableName);
                        if (properties.Any())
                        {
                            //通知 context 仓储已经以身改变
                            EventHandler?.Invoke(new RespositoryChangedEventArgs
                            {
                                Succeed = true,
                                EntityType = entityType,
                                EventType = RespositoryChangeEventType.Modify,
                                AddedProperties = new ReadOnlyCollection<IProperty>(properties)
                            });
                        }
                    }
                    catch (Exception exp)
                    {
                        EventHandler?.Invoke(new RespositoryChangedEventArgs
                        {
                            EntityType = entityType,
                            EventType = RespositoryChangeEventType.Modify,
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
                        if (tbGen.TryCreate(service.Database, metadata, tableName))
                        {
                            //通知 context 仓储已经创建
                            EventHandler?.Invoke(new RespositoryChangedEventArgs
                            {
                                Succeed = true,
                                EntityType = entityType,
                                EventType = RespositoryChangeEventType.Create
                            });
                        }
                    }
                    catch (Exception exp)
                    {
                        EventHandler?.Invoke(new RespositoryChangedEventArgs
                        {
                            EntityType = entityType,
                            EventType = RespositoryChangeEventType.Create,
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
