// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;

namespace Fireasy.Data.Entity.Linq.Translators
{
    public class TranslateScope : Scope<TranslateScope>
    {
        internal TranslateScope(IContextService service, ITranslateProvider translator, TranslateOptions options)
        {
            ContextService = service;
            Translator = translator;
            Options = options;
        }

        internal IContextService ContextService { get; private set; }

        public ITranslateProvider Translator { get; private set; }

        public TranslateOptions Options { get; private set; }
    }
}
