// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;

namespace Fireasy.Redis
{
    /// <summary>
    /// 用于避免事务嵌套的上下文。
    /// </summary>
    internal class RedisTransactionContext : Scope<RedisTransactionContext>
    {
    }
}
