// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Collections.Generic;

namespace Fireasy.Common
{
    public sealed class StringIgnoreComparer : IEqualityComparer<string>
    {
        public readonly static StringIgnoreComparer Default = new StringIgnoreComparer();

        public bool Equals(string x, string y)
        {
            return string.Compare(x, y, true) == 0;
        }

        public int GetHashCode(string obj)
        {
            return 0;
        }
    }
}
