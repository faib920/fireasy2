// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Provider;

namespace Fireasy.Data
{
    public class ConnectionTenancyInfo
    {
        public IProvider Provider { get; set; }

        public ConnectionString ConnectionString { get; set; }
    }
}
