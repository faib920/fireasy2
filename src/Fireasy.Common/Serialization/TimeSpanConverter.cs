// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using System;

namespace Fireasy.Common.Serialization
{
    /// <summary>
    /// <see cref="TimeSpan"/> 的转换器。
    /// </summary>
    public class TimeSpanConverter : ValueConverter<TimeSpan>
    {
        public override object ReadObject(ITextSerializer serializer, Type dataType, string text)
        {
            if (string.IsNullOrEmpty(text) && dataType.IsNullableType())
            {
                return null;
            }

            if (long.TryParse(text, out long l))
            {
                return new TimeSpan(l);
            }
            else
            {
                return TimeSpan.Zero;
            }
        }

        public override string WriteObject(ITextSerializer serializer, object obj)
        {
            if (obj == null)
            {
                return "null";
            }

            return ((TimeSpan)obj).Ticks.ToString();
        }
    }
}
