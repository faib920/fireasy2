// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Entity.Generation;
using Fireasy.Data.Entity.Linq.Translators;
using Fireasy.Data.Provider;

namespace Fireasy.Data.Entity.Providers
{
    /// <summary>
    /// <see cref="IProvider"/> 辅助类。
    /// </summary>
    public static class ProviderHelper
    {
        /// <summary>
        /// 获取 <see cref="ITranslateProvider"/> 对象。
        /// </summary>
        /// <param name="provider">数据库提供者实例。</param>
        /// <returns></returns>
        public static ITranslateProvider GetTranslateProvider(this IProvider provider)
        {
            var service = provider.GetService<ITranslateProvider>();
            if (service != null)
            {
                return service;
            }

            var serviceType = typeof(ITranslateProvider);
            switch (provider)
            {
                case MsSqlProvider _:
                    provider.RegisterService(serviceType, () => new MsSqlTranslateProvider());
                    break;
                case OracleProvider _:
                    provider.RegisterService(serviceType, () => new OracleTranslateProvider());
                    break;
                case MySqlProvider _:
                    provider.RegisterService(serviceType, () => new MySqlTranslateProvider());
                    break;
                case SQLiteProvider _:
                    provider.RegisterService(serviceType, () => new SQLiteTranslateProvider());
                    break;
                case PostgreSqlProvider _:
                    provider.RegisterService(serviceType, () => new PostgreSqlTranslateProvider());
                    break;
                case FirebirdProvider _:
                    provider.RegisterService(serviceType, () => new FirebirdTranslateProvider());
                    break;
                case OleDbProvider _:
                    provider.RegisterService(serviceType, () => new OleDbTranslateProvider());
                    break;
                default:
                    return null;
            }

            return provider.GetService<ITranslateProvider>();
        }

        /// <summary>
        /// 获取 <see cref="ITableGenerateProvider"/> 对象。
        /// </summary>
        /// <param name="provider">数据库提供者实例。</param>
        /// <returns></returns>
        public static ITableGenerateProvider GetTableGenerateProvider(this IProvider provider)
        {
            var generator = provider.GetService<ITableGenerateProvider>();
            if (generator != null)
            {
                return generator;
            }

            var serviceType = typeof(ITableGenerateProvider);
            switch (provider)
            {
                case MsSqlProvider _:
                    provider.RegisterService(serviceType, () => new MsSqlTableGenerator());
                    break;
                case OracleProvider _:
                    provider.RegisterService(serviceType, () => new OracleTableGenerator());
                    break;
                case MySqlProvider _:
                    provider.RegisterService(serviceType, () => new MySqlTableGenerator());
                    break;
                case SQLiteProvider _:
                    provider.RegisterService(serviceType, () => new SQLiteTableGenerator());
                    break;
                case OleDbProvider _:
                    break;
                case PostgreSqlProvider _:
                    provider.RegisterService(serviceType, () => new PostgreSqlTableGenerator());
                    break;
                case FirebirdProvider _:
                    provider.RegisterService(serviceType, () => new FirebirdTableGenerator());
                    break;
                default:
                    return null;
            }

            return provider.GetService<ITableGenerateProvider>();
        }
    }
}
