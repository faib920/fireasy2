// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NET45
using System.Threading.Tasks;

namespace Fireasy.Common.Threading
{
    /// <summary>
    /// 用于兼容非 .net framework 4.5 的 CompletedTask。
    /// </summary>
    public class TaskCompatible
    {
        public Task CompletedTask { get; }
    }
}
#endif