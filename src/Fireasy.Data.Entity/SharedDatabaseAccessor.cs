// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.ComponentModel;
using System.Collections.Generic;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 用于在同一线程内共享 <see cref="IDatabase"/> 实例。
    /// </summary>
    public sealed class SharedDatabaseAccessor : DisposeableBase
    {
        private readonly Dictionary<string, IDatabase> _databases = new Dictionary<string, IDatabase>();

        /// <summary>
        /// 获取或设置连接串对应的 <see cref="IDatabase"/> 实例。
        /// </summary>
        /// <param name="constr"></param>
        /// <returns></returns>
        public IDatabase this[ConnectionString constr]
        {
            get
            {
                if (_databases.TryGetValue((string)constr, out IDatabase _db))
                {
                    return _db;
                }

                return null;
            }
            set
            {
                if (!_databases.ContainsKey((string)constr))
                {
                    _databases.Add((string)constr, value);
                }
            }
        }

    }
}