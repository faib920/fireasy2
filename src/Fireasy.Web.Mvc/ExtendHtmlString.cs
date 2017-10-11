// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Web;
using System.Web.Mvc;

namespace Fireasy.Web.Mvc
{
    public class ExtendHtmlString : IHtmlString
    {
        public ExtendHtmlString(TagBuilder builder)
        {
            Builder = builder;
        }

        public TagBuilder Builder { get; private set; }

        public virtual string ToHtmlString()
        {
            return Builder.ToString(TagRenderMode.Normal);
        }

        public override string ToString()
        {
            return ToHtmlString();
        }
    }
}
