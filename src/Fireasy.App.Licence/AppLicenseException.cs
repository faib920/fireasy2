// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.App.Licence
{
    public class AppLicenseException : Exception
    {
        public AppLicenseException(string message, Exception exception)
            : base (message, exception)
        {
        }
    }
}
