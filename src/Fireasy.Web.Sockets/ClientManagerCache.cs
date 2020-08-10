// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Extensions;
using System;

namespace Fireasy.Web.Sockets
{
    internal static class ClientManagerCache
    {
        private static readonly SafetyDictionary<Type, IClientManager> _managers = new SafetyDictionary<Type, IClientManager>();

        internal static IClientManager GetManager(Type handlerType, WebSocketAcceptContext acceptContext)
        {
            return _managers.GetOrAdd(handlerType, () =>
            {
                var manager = acceptContext.ServiceProvider.TryGetService<IClientManager>(() => new DefaultClientManager());
                manager.Initialize(acceptContext);
                return manager;
            });
        }
    }
}
