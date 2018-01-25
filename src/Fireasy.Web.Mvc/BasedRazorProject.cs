// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD2_0
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Razor.Language;
using System.Collections.Generic;
using System.Linq;

namespace Fireasy.Web.Mvc
{
    internal class BasedRazorProject : FileProviderRazorProject
    {
        public BasedRazorProject(IRazorViewEngineFileProviderAccessor accessor)
            : base(accessor)
        {
        }

        public override IEnumerable<RazorProjectItem> FindHierarchicalItems(string basePath, string path, string fileName)
        {
            IEnumerable<RazorProjectItem> items = base.FindHierarchicalItems(basePath, path, fileName);

            return items.Append(GetItem("/Views/" + fileName));
        }
    }
}
#endif