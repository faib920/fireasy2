// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;
using Fireasy.Common.Extensions;
using Fireasy.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Fireasy.Data.Batcher
{
    /// <summary>
    /// 批量生成数据的检查辅助类。
    /// </summary>
    internal static class BatcherChecker
    {
        /// <summary>
        /// 检查 <see cref="DataTable"/> 是否可以批量生成其内部的数据。
        /// </summary>
        /// <param name="dataTable">要检查的 <see cref="DataTable"/> 对象。</param>
        /// <returns>可以生成，为 true，否则为 false。</returns>
        internal static bool CheckDataTable(DataTable dataTable)
        {
            Guard.ArgumentNull(dataTable, nameof(dataTable));

            if (dataTable.IsNullOrEmpty())
            {
                return false;
            }

            if (string.IsNullOrEmpty(dataTable.TableName))
            {
                throw new BatcherException(dataTable.Rows, new ArgumentException(SR.GetString(SRKind.BatcherDataTable), "dataTable"));
            }

            return true;
        }

        /// <summary>
        /// 检查 List 是否可以批量生成其每一个元素。
        /// </summary>
        /// <typeparam name="T">List 中元素的类型。</typeparam>
        /// <param name="list">要检查的 List 对象。</param>
        /// <param name="tableName">表的名称。</param>
        /// <returns>可以生成，为 true，否则为 false。</returns>
        internal static bool CheckList<T>(IEnumerable<T> list, string tableName)
        {
            Guard.ArgumentNull(list, nameof(list));

            if (list.IsNullOrEmpty())
            {
                return false;
            }

            if (string.IsNullOrEmpty(tableName))
            {
                throw new BatcherException(list.ToList(), new ArgumentException(SR.GetString(SRKind.BatcherDataTable), "table"));
            }

            return true;
        }
    }
}
