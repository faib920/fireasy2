// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Data.Configuration
{
    internal sealed class ConnectionStringHelper
    {
        internal static string GetConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                return string.Empty;
            }

            if (connectionString.StartsWith("m:"))
            {
                return ConnectionStringEncryptHelper.Decrypt(connectionString.Substring(2));
            }

            return connectionString;
        }
    }
}
