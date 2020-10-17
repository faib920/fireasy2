using System;

namespace Fireasy.Common.Ioc
{
    public static class Extensions
    {
        /// <summary>
        /// 注册瞬时。
        /// </summary>
        /// <typeparam name="TService">服务类型。</typeparam>
        /// <typeparam name="TImplementation">实现类型。</typeparam>
        /// <returns>当前的 IOC 容器。</returns>
        public static Container RegisterTransient<TService, TImplementation>(this Container container)
            where TImplementation : class, TService
            where TService : class
        {
            return container.RegisterTransient(typeof(TService), typeof(TImplementation));
        }

        /// <summary>
        /// 注册瞬时。
        /// </summary>
        /// <typeparam name="TService">服务类型。</typeparam>
        /// <returns>当前的 IOC 容器。</returns>
        public static Container RegisterTransient<TService>(this Container container)
            where TService : class
        {
            return container.RegisterTransient(typeof(TService), typeof(TService));
        }

        /// <summary>
        /// 注册瞬时。
        /// </summary>
        /// <typeparam name="TService">服务类型。</typeparam>
        /// <returns>当前的 IOC 容器。</returns>
        public static Container RegisterTransient<TService>(this Container container, Func<IResolver, TService> instanceCreator)
            where TService : class
        {
            return container.Register(instanceCreator, Lifetime.Transient);
        }

        /// <summary>
        /// 注册瞬时。
        /// </summary>
        /// <param name="serviceType">服务类型。</param>
        /// <param name="implementationType">实现类型。</param>
        /// <returns>当前的 IOC 容器。</returns>
        public static Container RegisterTransient(this Container container, Type serviceType, Type implementationType)
        {
            return container.Register(serviceType, implementationType, Lifetime.Transient);
        }

        /// <summary>
        /// 注册作用域。
        /// </summary>
        /// <typeparam name="TService">服务类型。</typeparam>
        /// <typeparam name="TImplementation">实现类型。</typeparam>
        /// <returns>当前的 IOC 容器。</returns>
        public static Container RegisterScoped<TService, TImplementation>(this Container container)
            where TImplementation : class, TService
            where TService : class
        {
            return container.RegisterScoped(typeof(TService), typeof(TImplementation));
        }

        /// <summary>
        /// 注册作用域。
        /// </summary>
        /// <typeparam name="TService">服务类型。</typeparam>
        /// <returns>当前的 IOC 容器。</returns>
        public static Container RegisterScoped<TService>(this Container container)
            where TService : class
        {
            return container.RegisterScoped(typeof(TService), typeof(TService));
        }

        /// <summary>
        /// 注册作用域。
        /// </summary>
        /// <typeparam name="TService">服务类型。</typeparam>
        /// <typeparam name="TImplementation">实现类型。</typeparam>
        /// <returns>当前的 IOC 容器。</returns>
        public static Container RegisterScoped<TService>(this Container container, Func<IResolver, TService> instanceCreator)
            where TService : class
        {
            return container.Register(instanceCreator, Lifetime.Scoped);
        }

        /// <summary>
        /// 注册作用域。
        /// </summary>
        /// <param name="serviceType">服务类型。</param>
        /// <param name="implementationType">实现类型。</param>
        /// <returns>当前的 IOC 容器。</returns>
        public static Container RegisterScoped(this Container container, Type serviceType, Type implementationType)
        {
            return container.Register(serviceType, implementationType, Lifetime.Scoped);
        }

        /// <summary>
        /// 注册单例。
        /// </summary>
        /// <typeparam name="TService">服务类型。</typeparam>
        /// <typeparam name="TImplementation">实现类型。</typeparam>
        /// <returns>当前的 IOC 容器。</returns>
        public static Container RegisterSingleton<TService, TImplementation>(this Container container)
            where TImplementation : class, TService
            where TService : class
        {
            return container.RegisterSingleton(typeof(TService), typeof(TImplementation));
        }

        /// <summary>
        /// 注册单例。
        /// </summary>
        /// <typeparam name="TService">服务类型。</typeparam>
        /// <returns>当前的 IOC 容器。</returns>
        public static Container RegisterSingleton<TService>(this Container container)
            where TService : class
        {
            return container.RegisterSingleton(typeof(TService), typeof(TService));
        }

        /// <summary>
        /// 注册单例。
        /// </summary>
        /// <param name="serviceType">服务类型。</param>
        /// <param name="implementationType">实现类型。</param>
        /// <returns>当前的 IOC 容器。</returns>
        public static Container RegisterSingleton(this Container container, Type serviceType, Type implementationType)
        {
            return container.Register(serviceType, implementationType, Lifetime.Singleton);
        }

        /// <summary>
        /// 注册单例。
        /// </summary>
        /// <typeparam name="TService">服务类型。</typeparam>
        /// <param name="instanceCreator">实例的构造方法。</param>
        /// <returns>当前的 IOC 容器。</returns>
        public static Container RegisterSingleton<TService>(this Container container, Func<IResolver, TService> instanceCreator) where TService : class
        {
            return container.Register(instanceCreator, Lifetime.Singleton);
        }

        /// <summary>
        /// 注册单例。
        /// </summary>
        /// <param name="serviceType">服务类型。</param>
        /// <param name="instanceCreator">实例的构造方法。</param>
        /// <returns>当前的 IOC 容器。</returns>
        public static Container RegisterSingleton(this Container container, Type serviceType, Func<IResolver, object> instanceCreator)
        {
            return container.Register(serviceType, instanceCreator, Lifetime.Singleton);
        }
    }
}
