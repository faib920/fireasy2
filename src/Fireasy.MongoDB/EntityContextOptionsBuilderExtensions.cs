// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.MongoDB;

namespace Fireasy.Data.Entity
{
    public static class EntityContextOptionsBuilderExtensions
    {
        /// <summary>
        /// 配置 <see cref="EntityContext"/> 使用 SqlServer 数据库。
        /// </summary>
        /// <param name="connectionString"></param>
        public static void UseMongoDB(this EntityContextOptionsBuilder builder, string connectionString)
        {
            builder.Options.ContextFactory = () => new EntityContextInitializeContext(MongoDBProvider.Instance, connectionString);
        }
    }
}
