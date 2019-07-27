// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.ComponentModel;
using System;

namespace Fireasy.Data
{
    /// <summary>
    /// 数据分页类。
    /// </summary>
    public class DataPager : IDataSegment, IDataPageEvaluatable, IPager
    {
        private int currentPageIndex;
        private int recordCount;

        /// <summary>
        /// 使用默认每页 20 条记录初始化 <see cref="DataPager"/> 类的新实例。
        /// </summary>
        public DataPager()
            : this (20)
        {
        }

        /// <summary>
        /// 初始化 <see cref="DataPager"/> 类的新实例。
        /// </summary>
        /// <param name="pageSize">每页的记录数。</param>
        public DataPager(int pageSize)
        {
            PageSize = pageSize;

            //默认是小数据量情况下，计算出总记录数及总页数
            Evaluator = new TotalRecordEvaluator();
        }

        /// <summary>
        /// 初始化 <see cref="DataPager"/> 类的新实例。
        /// </summary>
        /// <param name="pageSize">每页的记录数。</param>
        /// <param name="currentPageIndex">当前页索引。</param>
        public DataPager(int pageSize, int currentPageIndex)
            : this (pageSize)
        {
            PageSize = pageSize;
            this.currentPageIndex = currentPageIndex;
        }

        /// <summary>
        /// 获取或设置分页评估器。默认使用 <see cref="TotalRecordEvaluator"/>，你也可以使用 <see cref="TryNextEvaluator"/>。
        /// </summary>
        public IDataPageEvaluator Evaluator { get; set; }

        /// <summary>
        /// 获取或设置当前页码，该值从0开始。
        /// </summary>
        public virtual int CurrentPageIndex
        {
            get
            {
                if (currentPageIndex < 0)
                {
                    currentPageIndex = 0;
                }

                if (PageCount > 0 && currentPageIndex > PageCount - 1)
                {
                    currentPageIndex = PageCount - 1;
                }

                return currentPageIndex;
            }

            set
            {
                currentPageIndex = value;
            }
        }

        /// <summary>
        /// 获取或设置每页的记录数。
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 返回或设置总页数。
        /// </summary>
        public virtual int PageCount { get; set; }

        /// <summary>
        /// 获取或设置记录数。
        /// </summary>
        public int RecordCount
        {
            get
            {
                return recordCount;
            }
            set
            {
                recordCount = value;
                var num = 0;
                if (PageSize > 0)
                {
                    num = recordCount / PageSize;
                    if ((num * PageSize) < recordCount)
                    {
                        num++;
                    }
                }

                PageCount = num;
            }
        }

        /// <summary>
        /// 输出对象的相关字符串表述。
        /// </summary>
        /// <returns>表示记录数、页数和当前页码的文本。</returns>
        public override string ToString()
        {
            return string.Format("[PageSize={0},CurrentPageIndex={1}]", PageSize, CurrentPageIndex);
        }

        #region IDataSegment 成员

        public int? Start
        {
            get { return (currentPageIndex * PageSize) + 1; }
            set { throw new NotSupportedException(); }
        }

        public int? End
        {
            get { return (currentPageIndex + 1) * PageSize; }
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// 获取数据的长度。
        /// </summary>
        public int Length
        {
            get { return PageSize; }
        }
        #endregion
    }
}
