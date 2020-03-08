// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Data.Provider
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public class DefaultProviderServiceAttribute : Attribute
    {
        public DefaultProviderServiceAttribute(Type serviceType)
        {
            ServiceType = serviceType;
        }

        public Type ServiceType { get; set; }
    }
}
