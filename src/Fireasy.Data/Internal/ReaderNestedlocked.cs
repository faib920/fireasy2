// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Data;
using System.Threading;

namespace Fireasy.Data.Internal
{
    /// <summary>
    /// 用于控制嵌套的 <see cref="IDataReader"/>。
    /// </summary>
    internal class ReaderNestedlocked
    {
        private int _count;

        /// <summary>
        /// 数量增加1。
        /// </summary>
        /// <returns></returns>
        public int Increment()
        {
            return Interlocked.Increment(ref _count);
        }

        /// <summary>
        /// 数量减少1。
        /// </summary>
        /// <returns></returns>
        public int Decrement()
        {
            return Interlocked.Decrement(ref _count);
        }
    }
}
