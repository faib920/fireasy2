// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Provider;

namespace Fireasy.Data.MultiTenancy
{
    /// <summary>
    /// 数据库连接字符串的租户信息。
    /// </summary>
    public class ConnectionTenancyInfo
    {
        public IProvider Provider { get; set; }

        public ConnectionString ConnectionString { get; set; }
    }
}
