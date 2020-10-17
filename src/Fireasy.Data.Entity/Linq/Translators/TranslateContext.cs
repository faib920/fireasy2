// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Data.Entity.Providers;
using Fireasy.Data.Entity.Query;
using Fireasy.Data.Provider;
using Fireasy.Data.Syntax;
using System;
using System.Collections.Generic;

namespace Fireasy.Data.Entity.Linq.Translators
{
    public sealed class TranslateContext
    {
        internal TranslateContext(IContextService contextService, TranslateOptions options)
        {
            Provider = contextService.Provider;
            ContextType = contextService.ContextType;
            ServiceProvider = contextService.ServiceProvider;
            TranslateProvider = Provider.GetTranslateProvider();
            SyntaxProvider = Provider.GetService<ISyntaxProvider>();
            InstanceName = contextService.As<IEntityPersistentInstanceContainer, string>(s => s.InstanceName);
            PersistentEnvironment = contextService.As<IEntityPersistentEnvironment, EntityPersistentEnvironment>(s => s.Environment);
            QueryPolicy = contextService.As<IQueryPolicyAware, IQueryPolicy>(s => s.QueryPolicy);
            Options = options;
            Translator = TranslateProvider.CreateTranslator(this);
        }

        public string InstanceName { get; }

        public EntityPersistentEnvironment PersistentEnvironment { get; }

        public Type ContextType { get; }

        public IProvider Provider { get; }

        public IServiceProvider ServiceProvider { get; }

        public ITranslateProvider TranslateProvider { get; }

        public ISyntaxProvider SyntaxProvider { get; }

        public IQueryPolicy QueryPolicy { get; }

        public TranslatorBase Translator { get; }

        public TranslateOptions Options { get; }

        public List<string> TemporaryBag { get; set; }
    }
}
