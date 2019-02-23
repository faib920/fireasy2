// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using System;
using System.Reflection;

namespace Fireasy.Common.Ioc.Registrations
{
    internal class Creator
    {
        internal static IRegistration CreateFunc(Type serviceType, Func<object> instanceCreator)
        {
            var type = typeof(FuncRegistration<>).MakeGenericType(serviceType);
            var constructor = type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0];
            return (IRegistration)constructor.Invoke(new[] { instanceCreator });
        }

        internal static IRegistration CreateTransient(Type serviceType, Type implementationType)
        {
            var type = typeof(TransientRegistration<,>).MakeGenericType(serviceType, implementationType);
            return type.New<IRegistration>();
        }

        internal static IRegistration CreateScoped(Type serviceType, Type implementationType)
        {
            var type = typeof(ScopedRegistration<,>).MakeGenericType(serviceType, implementationType);
            return type.New<IRegistration>();
        }

        internal static IRegistration RelyWithTransient(Type serviceType, Type implementationType, Container container)
        {
            Func<object> func = () => typeof(TransientRegistration<,>)
                .MakeGenericType(serviceType, implementationType)
                .New<AbstractRegistration>()
                .SetContainer(container)
                .Resolve();

            return new SingletonRegistration(serviceType, implementationType, func).SetContainer(container);
        }
    }
}
