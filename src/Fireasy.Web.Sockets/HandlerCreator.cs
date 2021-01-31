// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Linq;

namespace Fireasy.Web.Sockets
{
    internal class HandlerCreator
    {
        internal static WebSocketHandler CreateHandler(IServiceProvider serviceProvider, WebSocketBuildOption option, Type handlerType)
        {
            if (handlerType == null || !typeof(WebSocketHandler).IsAssignableFrom(handlerType))
            {
                return null;
            }

            var constructor = handlerType.GetConstructors().FirstOrDefault();
            if (constructor == null)
            {
                throw new Exception($"No default constructor of {handlerType}.");
            }

            var parameters = constructor.GetParameters();
            var arguments = new object[parameters.Length];

            if (serviceProvider != null)
            {
                for (var i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i].ParameterType == typeof(IServiceProvider))
                    {
                        arguments[i] = serviceProvider;
                    }
                    else if (parameters[i].ParameterType == typeof(WebSocketBuildOption))
                    {
                        arguments[i] = option;
                    }
                    else
                    {
                        arguments[i] = serviceProvider.GetService(parameters[i].ParameterType);
                    }
                }
            }

            return (WebSocketHandler)constructor.Invoke(arguments);
        }
    }
}
