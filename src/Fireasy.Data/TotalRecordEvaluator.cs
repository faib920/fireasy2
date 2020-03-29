// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.Caching;
using Fireasy.Common.ComponentModel;
using Fireasy.Data.Extensions;
using Fireasy.Data.RecordWrapper;
using System;
using System.Data;

namespace Fireasy.Data
{
    /// <summary>
    /// 以记录总数作为评估依据，它需要计算出返回数据的总量，然后计算总页数。无法继承此类。
    /// </summary>
    public sealed class TotalRecordEvaluator : IDataPageEvaluator
    {
        /// <summary>
        /// 获取缓存记录数的时间间隔，默认为 null，不缓存。
        /// </summary>
        public TimeSpan? Expiration { get; set; }

        void IDataPageEvaluator.Evaluate(CommandContext context)
        {
            if (context.Segment is IPager dataPager)
            {
                dataPager.RecordCount = GetRecoredCount(context);
            }
        }

        private int GetRecoredCount(CommandContext context)
        {
            ICacheManager cacheManager;
            if (Expiration != null &&
                (cacheManager = CacheManagerFactory.CreateManager()) != null)
            {
                var key = context.Command.Output();
                return cacheManager.Contains(key) ?
                    (int)cacheManager.Get(key) :
                    cacheManager.Add(key, GetRecordCountFromDatabase(context), new RelativeTime((TimeSpan)Expiration));
            }

            return GetRecordCountFromDatabase(context);
        }

        private int GetRecordCountFromDatabase(CommandContext context)
        {
            var count = 0;
            var cullingOrderBy = DbUtility.CullingOrderBy(context.Command.CommandText);
            SqlCommand sqlCount = $"SELECT COUNT(1) FROM ({cullingOrderBy}) TEMP";

            using var reader = context.Database.ExecuteReader(sqlCount, parameters: context.Parameters, behavior: CommandBehavior.Default);
            var wrapper = context.Database.Provider.GetService<IRecordWrapper>();
            if (reader.Read())
            {
                switch (reader.GetFieldType(0).GetDbType())
                {
                    case DbType.Decimal:
                        count = (int)wrapper.GetDecimal(reader, 0);
                        break;
                    case DbType.Int32:
                        count = wrapper.GetInt32(reader, 0);
                        break;
                    case DbType.Int64:
                        count = (int)wrapper.GetInt64(reader, 0);
                        break;
                }
            }
            return count;
        }
    }
}
