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
using Fireasy.Data.Syntax;

namespace Fireasy.Data.Entity.Linq.Translators
{
    public class TranslateScope : Scope<TranslateScope>
    {
        internal TranslateScope(IContextService service, TranslateOptions options)
        {
            var provider = service.InitializeContext.Provider;
            TranslateProvider = provider.GetTranslateProvider();
            SyntaxProvider = provider.GetService<ISyntaxProvider>();
            InstanceName = service.As<IEntityPersistentInstanceContainer, string>(s => s.InstanceName);
            PersistentEnvironment = service.As<IEntityPersistentEnvironment, EntityPersistentEnvironment>(s => s.Environment);
            QueryPolicy = service.As<IQueryPolicyAware, IQueryPolicy>(s => s.QueryPolicy);
            Options = options;
        }

        public string InstanceName { get; private set; }

        public EntityPersistentEnvironment  PersistentEnvironment { get; private set; }

        public ITranslateProvider TranslateProvider { get; private set; }

        public ISyntaxProvider SyntaxProvider { get; private set; }

        public IQueryPolicy QueryPolicy { get; private set; }

        public TranslateOptions Options { get; private set; }
    }
}
