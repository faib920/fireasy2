// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.ComponentModel;
using System;

namespace Fireasy.Web.Sockets
{
    internal static class ClientManagerCache
    {
        private static SafetyDictionary<Type, ClientManager> managers = new SafetyDictionary<Type, ClientManager>();

        internal static ClientManager GetManager(Type handlerType, WebSocketAcceptContext acceptContext)
        {
            return managers.GetOrAdd(handlerType, () =>
            {
                ClientManager manager = null;
#if NETSTANDARD
                    if (acceptContext.HttpContext != null)
                    {
                        manager = acceptContext.HttpContext.RequestServices.GetService(typeof(ClientManager)) as ClientManager;
                    }
#endif
                if (manager == null)
                {
                    manager = acceptContext.Option.Distributed ? new DistributedClientManager(acceptContext.Option) : new ClientManager(acceptContext.Option);
                }

                return manager;
            });
        }
    }
}
