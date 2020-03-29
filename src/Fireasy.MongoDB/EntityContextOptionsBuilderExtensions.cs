// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Entity;

namespace Fireasy.MongoDB
{
    public static class EntityContextOptionsBuilderExtensions
    {
        /// <summary>
        /// 配置 <see cref="EntityContext"/> 使用 MongoDB 数据库。
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="connectionString">连接字符串。<para>示例：server=mongodb://localhost;database=test。</para></param>
        public static void UseMongoDB(this EntityContextOptionsBuilder builder, string connectionString)
        {
            builder.Options.Provider = MongoDBProvider.Instance;
            builder.Options.ConnectionString = connectionString;
        }
    }
}
