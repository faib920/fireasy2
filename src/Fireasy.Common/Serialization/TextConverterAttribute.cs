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
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TextConverterAttribute : Attribute
    {
        public TextConverterAttribute(Type converterType)
        {
            ConverterType = converterType;
        }

        public Type ConverterType { get; set; }
    }
}
