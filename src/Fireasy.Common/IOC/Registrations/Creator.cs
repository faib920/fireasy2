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
    internal static class Creator
    {
        internal static IRegistration CreateFunc(this Container container, Type serviceType, Func<IResolver, object> instanceCreator, Lifetime lifetime)
        {
            var type = typeof(FuncRegistration<>).MakeGenericType(serviceType);
            var constructor = type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0];
            return (IRegistration)constructor.Invoke(new object[] { container, instanceCreator, lifetime });
        }

        internal static IRegistration CreateFunc<TService>(this Container container, Func<IResolver, TService> instanceCreator, Lifetime lifetime) where TService : class
        {
            return new FuncRegistration<TService>(container, instanceCreator, lifetime);
        }

        internal static IRegistration CreateTransient(this Container container, Type serviceType, Type implementationType)
        {
            var type = typeof(TransientRegistration<,>).MakeGenericType(serviceType, implementationType);
            return type.New<IRegistration>(container);
        }

        internal static IRegistration CreateSingleton<TService>(this Container container, TService instance) where TService : class
        {
            return new SingletonRegistration(container, typeof(TService), instance);
        }

        internal static IRegistration CreateSingleton(this Container container, Type serviceType, Type implementationType)
        {
            return container.RelyWithTransient(serviceType, implementationType);
        }

        internal static IRegistration CreateSingleton(this Container container, Type serviceType, object instance)
        {
            return new SingletonRegistration(container, serviceType, instance);
        }

        internal static IRegistration CreateScoped(this Container container, Type serviceType, Type implementationType)
        {
            var type = typeof(ScopedRegistration<,>).MakeGenericType(serviceType, implementationType);
            return type.New<IRegistration>(container);
        }

        internal static IRegistration RelyWithTransient(this Container container, Type serviceType, Type implementationType)
        {
            Func<IResolver, object> func = r => typeof(TransientRegistration<,>)
                .MakeGenericType(serviceType, implementationType)
                .New<AbstractRegistration>(r)
                .Resolve(r);

            return new SingletonRegistration(container, serviceType, func);
        }
    }
}
