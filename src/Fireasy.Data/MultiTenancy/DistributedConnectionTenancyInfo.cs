// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Provider;
using System.Collections.Generic;

namespace Fireasy.Data.MultiTenancy
{
    /// <summary>
    /// 分布式数据库连接字符串的租户信息。
    /// </summary>
    public class DistributedConnectionTenancyInfo
    {
        public IProvider Provider { get; set; }

        public List<DistributedConnectionString> DistributedConnectionStrings { get; set; } = new List<DistributedConnectionString>();
    }
}
