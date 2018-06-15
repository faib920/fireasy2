// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Configuration;

namespace Fireasy.Log4net
{
    public class LogFactory : IManagedFactory
    {
        object IManagedFactory.CreateInstance(string name)
        {
            return new Logger();
        }
    }
}
