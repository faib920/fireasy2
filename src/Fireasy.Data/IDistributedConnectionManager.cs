// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Fireasy.Data
{
    /// <summary>
    /// 分布式数据库连接串管理器。
    /// </summary>
    public interface IDistributedConnectionManager
    {
        /// <summary>
        /// 根据调度算法从配置中获取一个 <see cref="ConnectionString"/> 对象。
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        ConnectionString GetConnection(IDistributedDatabase database);
    }
}
