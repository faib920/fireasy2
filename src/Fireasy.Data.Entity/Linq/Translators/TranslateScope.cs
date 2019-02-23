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
        internal TranslateScope(InternalContext context, ITranslateProvider translator, TranslateOptions options)
        {
            Context = context;
            Translator = translator;
            Options = options;
        }

        internal InternalContext Context { get; private set; }

        public ITranslateProvider Translator { get; private set; }

        public TranslateOptions Options { get; private set; }
    }
}
