// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.ComponentModel;
using Fireasy.Data.Extensions;
using Fireasy.Data.RecordWrapper;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Data
{
    /// <summary>
    /// 以记录总数作为评估依据，它需要计算出返回数据的总量，然后计算总页数。无法继承此类。
    /// </summary>
    public sealed class TotalRecordEvaluator : IDataPageEvaluator
    {
        void IDataPageEvaluator.Evaluate(CommandContext context)
        {
            if (context.Segment is IPager dataPager)
            {
                dataPager.RecordCount = GetRecordCountFromDatabase(context);
            }
        }

        async Task IDataPageEvaluator.EvaluateAsync(CommandContext context, CancellationToken cancellationToken)
        {
            if (context.Segment is IPager dataPager)
            {
                dataPager.RecordCount = await GetRecordCountFromDatabaseAsync(context, cancellationToken);
            }
        }

        private int GetRecordCountFromDatabase(CommandContext context)
        {
            using var reader = context.Database.ExecuteReader(GetCommand(context), parameters: context.Parameters, behavior: CommandBehavior.Default);
            var wrapper = context.Database.Provider.GetService<IRecordWrapper>();
            return GetRecordCount(reader, wrapper);
        }

        private async Task<int> GetRecordCountFromDatabaseAsync(CommandContext context, CancellationToken cancellationToken = default)
        {
            using var reader = await context.Database.ExecuteReaderAsync(GetCommand(context), parameters: context.Parameters, behavior: CommandBehavior.Default, cancellationToken: cancellationToken);
            var wrapper = context.Database.Provider.GetService<IRecordWrapper>();
            return GetRecordCount(reader, wrapper);
        }

        private SqlCommand GetCommand(CommandContext context)
        {
            var cullingOrderBy = DbUtility.CullingOrderBy(context.Command.CommandText);
            SqlCommand sqlCount = $"SELECT COUNT(1) FROM ({cullingOrderBy}) TEMP";
            return sqlCount;
        }

        private int GetRecordCount(IDataReader reader, IRecordWrapper wrapper)
        {
            var count = 0;
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
