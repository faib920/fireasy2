// <copyright company="Faib Studio"
//      email="faib920@126.com"
//      qq="55570729"
//      date="2011-2-15">
//   (c) Copyright Faib Studio 2011. All rights reserved.
// </copyright>
// ---------------------------------------------------------------

namespace Fireasy.Data
{
    /// <summary>
    /// 一个堆栈，用于事务控制。
    /// </summary>
    internal sealed class TransactionStack
    {
        private int pos;

        /// <summary>
        /// 入栈。
        /// </summary>
        internal void Push()
        {
            pos++;
        }

        /// <summary>
        /// 出栈，并判断是否已至栈底。
        /// </summary>
        /// <returns></returns>
        internal bool Pop()
        {
            pos--;
            return pos == 0;
        }
    }
}
