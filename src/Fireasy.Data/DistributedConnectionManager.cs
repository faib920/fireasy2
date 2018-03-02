// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Linq;

namespace Fireasy.Data
{
    /// <summary>
    /// 分布式数据库连接串管理器。
    /// </summary>
    public class DistributedConnectionManager
    {
        private static Random random = new Random();

        /// <summary>
        /// 根据调度算法从配置中获取一个 <see cref="DistributedConnectionString"/> 对象。
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public static DistributedConnectionString GetConnection(IDistributedDatabase database)
        {
            if (database.DistributedConnectionStrings == null)
            {
                return null;
            }

            var slaves = database.DistributedConnectionStrings.Where(s => s.Mode == DistributedMode.Slave);
            if (slaves.Any())
            {
                var total = slaves.Sum(s => s.Weight);
                var rand = random.NextDouble() * total;
                var sum = 0;

                foreach (var connStr in slaves)
                {
                    sum += connStr.Weight;
                    if (rand <= sum)
                    {
                        return connStr;
                    }
                }
            }

            return null;
        }
    }
}
