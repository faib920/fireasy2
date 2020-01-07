// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Fireasy.Redis
{
    internal struct CheckCache<T>
    {
        public CheckCache(bool hasVaue, T value)
        {
            HasValue = hasVaue;
            Value = value;
        }

        public static CheckCache<T> Null()
        {
            return new CheckCache<T>(false, default(T));
        }

        public static CheckCache<T> Result(T value)
        {
            return new CheckCache<T>(true, value);
        }

        public bool HasValue { get; set; }

        public T Value { get; set; }
    }
}
