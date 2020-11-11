// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Threading.Tasks;

namespace Fireasy.Common.Threading
{
    /// <summary>
    /// 用于 .Net Framework 4.5 下兼容 CompletedTask。
    /// </summary>
    public class TaskCompatible
    {
#if NET45
        public static Task CompletedTask => Task.Run(() => { });
#else 
        public static Task CompletedTask => Task.CompletedTask;
#endif
    }
}
