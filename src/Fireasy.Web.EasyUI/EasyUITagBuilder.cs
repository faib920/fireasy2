// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Web.Mvc;

namespace Fireasy.Web.EasyUI
{
    public class EasyUITagBuilder : TagBuilder
    {
        public EasyUITagBuilder(string tagName, string className, SettingsBase settings)
            : base (tagName)
        {
            AddCssClass(className);
            MergeAttribute("data-options", SettingsSerializer.Serialize(settings));
        }
    }
}
