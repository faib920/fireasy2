// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.ComponentModel;
using Fireasy.Data.Entity.Linq;
using Fireasy.Data.Entity.Metadata;
using Fireasy.Data.Entity.Providers;
using Fireasy.Data.Schema;
using System;
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
        /// <param name="entityType">实体类型。</param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static bool TryCreate(Type entityType, InternalContext context, Action<Type> succeed, Action<Type, Exception> failed)
        {
            var service = context.Database.Provider.GetTableGenerateProvider();
            if (service == null)
            {
                return false;
            }

            var metadata = EntityMetadataUnity.GetEntityMetadata(entityType);

            var cacheKey = string.Format("{0}:{1}", context.InstanceName, entityType.FullName);
            return cache.GetOrAdd(cacheKey, () =>
                {
                    try
                    {
                        //判断数据表是否已存在
                        if (service.IsExists(context.Database, metadata))
                        {
                            //尝试添加新的字段
                            service.TryAddFields(context.Database, metadata);
                        }
                        else
                        {
                            //尝试创建数据表
                            service.TryCreate(context.Database, metadata);
                        }

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
        }
    }
}
