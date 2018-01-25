// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD2_0
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
#else
using System.Web;
using System.Web.Mvc;
#endif

namespace Fireasy.Web.Mvc
{
    public class ExtendHtmlString :
#if NETSTANDARD2_0
        IHtmlContent
#else
        IHtmlString
#endif
    {
        public ExtendHtmlString(TagBuilder builder)
        {
            Builder = builder;
        }

        public TagBuilder Builder { get; private set; }

        public virtual string ToHtmlString()
        {
#if NETSTANDARD2_0
            using (var writer = new StringWriter())
            {
                Builder.WriteTo(writer, HtmlEncoder.Default);
                return writer.ToString();
            }
#else
            return Builder.ToString(TagRenderMode.Normal);
#endif
        }

        public override string ToString()
        {
            return ToHtmlString();
        }

#if NETSTANDARD2_0
        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            Builder.WriteTo(writer, encoder);
        }
#endif
    }
}
