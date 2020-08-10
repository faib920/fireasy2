// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Fireasy.Windows.Forms
{
    /// <summary>
    /// <see cref="TreeListItem"/> 的比较器。
    /// </summary>
    public class TreeListItemComparer : IComparer<TreeListItem>
    {
        private readonly TreeListColumn _column;
        private readonly SortOrder _order;

        /// <summary>
        /// 初始化 <see cref="TreeListItemComparer"/> 类的新实例。
        /// </summary>
        /// <param name="column">当前排序的列。</param>
        /// <param name="order">排序方式。</param>
        public TreeListItemComparer(TreeListColumn column, SortOrder order)
        {
            _column = column;
            _order = order;
        }

        /// <summary>
        /// 比较 <paramref name="x"/> 和 <paramref name="y"/> 的大小。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>x 和 y 相等为 0；x 大于 y 为 1；x 小于 y 为 -1。</returns>
        public virtual int Compare(TreeListItem x, TreeListItem y)
        {
            var xvalue = x.Cells[_column.Index].Value;
            var yvalue = y.Cells[_column.Index].Value;
            if (xvalue == null)
            {
                return Reverse(-1);
            }

            if (yvalue == null)
            {
                return Reverse(1);
            }

            if (xvalue.GetType() != yvalue.GetType())
            {
                return 0;
            }

            if (xvalue is IComparable comparer)
            {
                return Reverse(comparer.CompareTo(yvalue));
            }

            return 0;
        }

        /// <summary>
        /// 根据 SortOrder 对比较值进行反转。
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private int Reverse(int result)
        {
            if (_order == SortOrder.Descending)
            {
                return 0 - result;
            }

            return result;
        }
    }
}
