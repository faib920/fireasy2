using Fireasy.Common;

namespace Fireasy.Data.Entity.Linq.Translators
{
    public class TranslateScope : Scope<TranslateScope>
    {
        public TranslateScope(InternalContext context, ITranslateProvider translator, TranslateOptions options)
        {
            Context = context;
            Translator = translator;
            Options = options;
        }

        public InternalContext Context { get; private set; }

        public ITranslateProvider Translator { get; private set; }

        public TranslateOptions Options { get; private set; }
    }
}
