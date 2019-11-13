// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Globalization;

namespace Fireasy.Common.Localization
{
    internal class NullStringLocalizer : IStringLocalizer
    {
        internal static NullStringLocalizer Instance = new NullStringLocalizer();

        string IStringLocalizer.this[string name] => name;

        string IStringLocalizer.this[string name, params object[] args] => name;

        CultureInfo IStringLocalizer.CultureInfo => null;
    }
}
