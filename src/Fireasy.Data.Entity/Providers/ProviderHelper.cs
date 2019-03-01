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
using System;

namespace Fireasy.Data.Entity.Providers
{
    internal static class ProviderHelper
    {
        internal static ITranslateProvider GetTranslateProvider(this IProvider provider)
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
                    provider.RegisterService(serviceType, new MsSqlTranslateProvider());
                    break;
                case OracleProvider _:
                    provider.RegisterService(serviceType, new OracleTranslateProvider());
                    break;
                case MySqlProvider _:
                    provider.RegisterService(serviceType, new MySqlTranslateProvider());
                    break;
                case SQLiteProvider _:
                    provider.RegisterService(serviceType, new SQLiteTranslateProvider());
                    break;
                case PostgreSqlProvider _:
                    provider.RegisterService(serviceType, new PostgreSqlTranslateProvider());
                    break;
                case FirebirdProvider _:
                    provider.RegisterService(serviceType, new FirebirdTranslateProvider());
                    break;
                default:
                    throw new Exception(SR.GetString(SRKind.TranslatorNotSupported, provider.GetType().Name));
            }

            return provider.GetService<ITranslateProvider>();
        }

        internal static ITableGenerateProvider GetTableGenerateProvider(this IProvider provider)
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
                    provider.RegisterService(serviceType, new MsSqlTableGenerator());
                    break;
                case OracleProvider _:
                    provider.RegisterService(serviceType, new OracleTableGenerator());
                    break;
                case MySqlProvider _:
                    provider.RegisterService(serviceType, new MySqlTableGenerator());
                    break;
                case SQLiteProvider _:
                    provider.RegisterService(serviceType, new SQLiteTableGenerator());
                    break;
#if !NETSTANDARD
                case OleDbProvider _:
                    break;
#endif
                case PostgreSqlProvider _:
                    provider.RegisterService(serviceType, new PostgreSqlTableGenerator());
                    break;
                case FirebirdProvider _:
                    provider.RegisterService(serviceType, new FirebirdTableGenerator());
                    break;
                default:
                    throw new Exception(SR.GetString(SRKind.TranslatorNotSupported, provider.GetType().Name));
            }

            return provider.GetService<ITableGenerateProvider>();
        }

    }
}
