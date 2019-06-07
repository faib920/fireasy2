// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Provider;
using System;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// <see cref=" EntityContext"/> 初始化的上下文对象。
    /// </summary>
    public sealed class EntityContextInitializeContext
    {
        internal EntityContextInitializeContext(EntityContextOptions options, IProvider provider, ConnectionString connectionStr)
            : this(provider, connectionStr)
        {
            Options = options;
        }

        public EntityContextInitializeContext()
        {
        }

        public EntityContextInitializeContext(Func<IDatabase> databaseFactory)
        {
            DatabaseFactory = new Func<IProvider, ConnectionString, IDatabase>((p, s) => databaseFactory());
        }

        public EntityContextInitializeContext(IProvider provider, ConnectionString connectionStr, Func<IProvider, ConnectionString, IDatabase> databaseFactory = null)
        {
            Provider = provider;
            ConnectionString = connectionStr;
            DatabaseFactory = databaseFactory;
        }

        public static bool operator ==(EntityContextInitializeContext context1, EntityContextInitializeContext context2)
        {
            if (Equals(context1, null) && Equals(context2, null))
            {
                return true;
            }

            if ((Equals(context1, null) && !Equals(context2, null)) || (!Equals(context1, null) && Equals(context2, null)))
            {
                return false;
            }

            return context1.Provider != null && context2.Provider != null && context1.Provider.ProviderName == context2.Provider.ProviderName &&
                context1.ConnectionString == context2.ConnectionString &&
                context1.Options.Equals(context2.Options);
        }

        public static bool operator !=(EntityContextInitializeContext context1, EntityContextInitializeContext context2)
        {
            if ((Equals(context1, null) && !Equals(context2, null)) || (!Equals(context1, null) && Equals(context2, null)))
            {
                return true;
            }

            return (context1.Provider != null && context2.Provider != null && context1.Provider.ProviderName != context2.Provider.ProviderName) ||
                context1.ConnectionString != context2.ConnectionString ||
                !context1.Options.Equals(context2.Options);
        }

        /// <summary>
        /// 获取或设置 <see cref="IProvider"/> 数据库提供者对象。
        /// </summary>
        public IProvider Provider { get; set; }

        /// <summary>
        /// 获取或设置数据库连接串。
        /// </summary>
        public ConnectionString ConnectionString { get; set; }

        /// <summary>
        /// 获取 <see cref="EntityContext"/> 的参数。
        /// </summary>
        public EntityContextOptions Options { get; internal set; }

        /// <summary>
        /// 获取或设置 <see cref="IDatabase"/> 的实例工厂。
        /// </summary>
        public Func<IProvider, ConnectionString, IDatabase> DatabaseFactory { get; private set; }
    }
}
