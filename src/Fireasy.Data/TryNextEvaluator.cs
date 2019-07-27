// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.ComponentModel;
using Fireasy.Common.Extensions;
using Fireasy.Data.Extensions;
using Fireasy.Data.Syntax;

namespace Fireasy.Data
{
    /// <summary>
    /// 以尝试下一页作为评估依据，它并不计算数据的总量及总页数，始终只判断下一页是否有效，该评估器一般用于数据量非常大的环境中。无法继承此类。
    /// </summary>
    public sealed class TryNextEvaluator : IDataPageEvaluator
    {
        void IDataPageEvaluator.Evaluate(CommandContext context)
        {
            var dataPager = context.Segment as IPager;
            if (dataPager == null)
            {
                return;
            }

            var syntax = context.Database.Provider.GetService<ISyntaxProvider>();
            var nextPage = new DataPager(dataPager.PageSize, dataPager.CurrentPageIndex + 1);
            var sql = string.Concat("select count(*) from (", syntax.Segment(context.Command.CommandText, nextPage), ") t");
            using (var connection = context.Database.CreateConnection())
            {
                connection.OpenClose(() =>
                    {
                        using (var command = context.Database.Provider.CreateCommand(connection, null, sql, parameters: context.Parameters))
                        {
                            //查询下一页是否有数据
                            var result = command.ExecuteScalar().To<int>();
                            if (result == 0)
                            {
                                //查询当前页剩余的记录
                                nextPage.CurrentPageIndex --;
                                sql = string.Concat("select count(*) from (", syntax.Segment(context.Command.CommandText, nextPage), ") t");
                                command.CommandText = sql;
                                result = command.ExecuteScalar().To<int>();

                                dataPager.RecordCount = dataPager.PageSize * dataPager.CurrentPageIndex + result;
                                HasNextPage = false;
                            }
                            else
                            {
                                dataPager.RecordCount = dataPager.PageSize * (dataPager.CurrentPageIndex + 1) + 1;
                                HasNextPage = true;
                            }
                        }
                    });
            }
        }

        /// <summary>
        /// 获取是否还有下一页。
        /// </summary>
        public bool HasNextPage { get; private set; }
    }
}
