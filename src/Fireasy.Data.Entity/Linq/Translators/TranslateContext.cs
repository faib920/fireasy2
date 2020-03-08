// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;
using Fireasy.Common.Extensions;
using Fireasy.Data.Entity.Providers;
using Fireasy.Data.Provider;
using Fireasy.Data.Syntax;
using System;

namespace Fireasy.Data.Entity.Linq.Translators
{
    public class TranslateScope : Scope<TranslateScope>
    {
        internal TranslateScope(IContextService contextService, TranslateOptions options)
        {
            Provider = contextService.Provider;
            ServiceProvider = contextService.ServiceProvider;
            TranslateProvider = Provider.GetTranslateProvider();
            SyntaxProvider = Provider.GetService<ISyntaxProvider>();
            InstanceName = contextService.As<IEntityPersistentInstanceContainer, string>(s => s.InstanceName);
            PersistentEnvironment = contextService.As<IEntityPersistentEnvironment, EntityPersistentEnvironment>(s => s.Environment);
            QueryPolicy = contextService.As<IQueryPolicyAware, IQueryPolicy>(s => s.QueryPolicy);
            Options = options;
        }

        public string InstanceName { get; private set; }

        public EntityPersistentEnvironment  PersistentEnvironment { get; private set; }

        public IProvider Provider { get; private set; }

        public IServiceProvider ServiceProvider { get; private set; }

        public ITranslateProvider TranslateProvider { get; private set; }

        public ISyntaxProvider SyntaxProvider { get; private set; }

        public IQueryPolicy QueryPolicy { get; private set; }

        public TranslateOptions Options { get; private set; }
    }
}
