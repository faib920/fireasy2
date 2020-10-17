// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Caching;
using System;

namespace Fireasy.Data.Identity
{
    /// <summary>
    /// 生成器的缓存。
    /// </summary>
    internal class GeneratorCache
    {
        /// <summary>
        /// 从缓存里判断序列或表是否已经创建，如果缓存里没有，则调用 <paramref name="checkFunc"/> 去判断。
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="checkFunc"></param>
        /// <returns></returns>
        public static bool IsSequenceCreated(string tableName, string columnName, Func<bool> checkFunc)
        {
            var cacheMgr = CacheManagerFactory.CreateManager();
            return cacheMgr.TryGet(string.Format("SEQ_{0}_{1}", tableName, columnName), checkFunc);
        }
    }
}
