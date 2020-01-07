// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD
using System;

namespace Fireasy.Common.Configuration
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class ConfigurationBinderAttribute : Attribute
    {
        public ConfigurationBinderAttribute(Type type)
        {
            BinderType = type;
        }

        public Type BinderType { get; private set; }
    }
}
#endif
