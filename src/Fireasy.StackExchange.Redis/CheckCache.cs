// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Fireasy.Redis
{
    internal class CheckCache<T>
    {
        internal CheckCache(bool hasVaue, T value)
        {
            HasValue = hasVaue;
            Value = value;
        }

        internal static CheckCache<T> Null()
        {
            return new CheckCache<T>(false, default);
        }

        internal static CheckCache<T> Result(T value)
        {
            return new CheckCache<T>(true, value);
        }

        internal bool HasValue { get; set; }

        internal T Value { get; set; }
    }
}
