using Fireasy.Common.ComponentModel;
// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;

namespace Fireasy.Data
{
    /// <summary>
    /// 表示分页数据结果。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DataPaginalResult<T> : IPaginalResult<T>
    {
        /// <summary>
        /// 初始化 <see cref="DataPaginalResult"/> 类的新实例。
        /// </summary>
        public DataPaginalResult()
        {

        }

        /// <summary>
        /// 初始化 <see cref="DataPaginalResult"/> 类的新实例。
        /// </summary>
        /// <param name="count">记录数。</param>
        /// <param name="data">当前页的数据。</param>
        public DataPaginalResult(int count, IEnumerable<T> data)
        {
            RecordCount = count;
            Data = data;
        }

        /// <summary>
        /// 获取或设置记录数。
        /// </summary>
        public int RecordCount { get; set; }

        /// <summary>
        /// 返回或设置总页数。
        /// </summary>
        public int PageCount { get; set; }

        /// <summary>
        /// 获取或设置当前页的记录。
        /// </summary>
        public IEnumerable<T> Data { get; set; }

        IEnumerable IPaginalResult.Data
        {
            get { return Data; }
            set { Data = (IEnumerable<T>)value; }
        }
    }
}
