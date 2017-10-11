using Fireasy.Common;

namespace Fireasy.Data.Entity.Linq.Translators
{
    public class TranslateScope : Scope<TranslateScope>
    {
        public TranslateScope(InternalContext context, ITranslateProvider translator)
        {
            Context = context;
            Translator = translator;
        }

        public InternalContext Context { get; set; }

        public ITranslateProvider Translator { get; set; }
    }
}
