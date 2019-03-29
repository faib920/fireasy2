// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Common.Serialization
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class TextPropertyConverterAttribute : Attribute
    {
        public TextPropertyConverterAttribute(Type converterType)
        {
            ConverterType = converterType;
        }

        public Type ConverterType { get; set; }
    }
}
