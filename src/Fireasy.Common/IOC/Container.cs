// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.ComponentModel;
using Fireasy.Common.Extensions;
using Fireasy.Common.Ioc.Registrations;
#if NETSTANDARD
using Microsoft.Extensions.DependencyInjection;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml;

namespace Fireasy.Common.Ioc
{
    /// <summary>
    /// 控制反转的容器，用于存放描述组件与服务的联系。
    /// </summary>
    public sealed class Container : DisposeableBase, IServiceProvider, IResolver
#if NETSTANDARD
        , IServiceScopeFactory
#endif
    {
        private ParameterExpression parExp = null;
        private readonly List<IRegistration> registrations = new List<IRegistration>();
        private readonly List<InstanceInitializer> instanceInitializers = new List<InstanceInitializer>();
        private readonly List<IDisposable> dispObjects = new List<IDisposable>();

        public Container()
        {
            Register<IServiceProvider>(this);
        }

        /// <summary>
        /// 注册服务类型及实现类型，<typeparamref name="TImplementation"/> 是 <typeparamref name="TService"/> 的实现类。
        /// </summary>
        /// <typeparam name="TService">服务类型。</typeparam>
        /// <typeparam name="TImplementation">实现类型。</typeparam>
        /// <param name="lifetime">生命周期。</param>
        /// <returns>当前的 IOC 容器。</returns>
        public Container Register<TService, TImplementation>(Lifetime lifetime)
            where TImplementation : class, TService
            where TService : class
        {
            return Register(typeof(TService), typeof(TImplementation), lifetime);
        }

        /// <summary>
        /// 注册服务类型，实现类型是其本身。
        /// </summary>
        /// <typeparam name="TService">服务类型。</typeparam>
        /// <param name="lifetime">生命周期。</param>
        /// <returns>当前的 IOC 容器。</returns>
        public Container Register<TService>(Lifetime lifetime)
            where TService : class
        {
            return Register(typeof(TService), typeof(TService), lifetime);
        }

        /// <summary>
        /// 注册服务类型，实现类型是其本身。
        /// </summary>
        /// <param name="serviceType">服务类型。</param>
        /// <param name="lifetime">生命周期。</param>
        /// <returns>当前的 IOC 容器。</returns>
        public Container Register(Type serviceType, Lifetime lifetime)
        {
            return Register(serviceType, serviceType, lifetime);
        }

        /// <summary>
        /// 注册服务类型及实现类型，<paramref name="implementationType"/> 是 <paramref name="serviceType"/> 的实现类。
        /// </summary>
        /// <param name="serviceType">服务类型。</param>
        /// <param name="implementationType">实现类型。</param>
        /// <param name="lifetime">生命周期。</param>
        /// <returns>当前的 IOC 容器。</returns>
        public Container Register(Type serviceType, Type implementationType, Lifetime lifetime)
        {
            if (lifetime == Lifetime.Singleton)
            {
                registrations.Add(this.CreateSingleton(serviceType, implementationType));
            }
            else if (lifetime == Lifetime.Scoped)
            {
                registrations.Add(this.CreateScoped(serviceType, implementationType));
            }
            else if (lifetime == Lifetime.Transient)
            {
                registrations.Add(this.CreateTransient(serviceType, implementationType));
            }

            return this;
        }

        /// <summary>
        /// 使用服务对象的构造器注册它的服务类型。
        /// </summary>
        /// <typeparam name="TService">服务类型。</typeparam>
        /// <param name="instanceCreator">实例的构造方法。</param>
        /// <param name="lifetime">生命周期。</param>
        /// <returns>当前的 IOC 容器。</returns>
        public Container Register<TService>(Func<IResolver, TService> instanceCreator, Lifetime lifetime) where TService : class
        {
            if (lifetime == Lifetime.Singleton)
            {
                registrations.Add(this.CreateSingleton(instanceCreator(this)));
            }
            else
            {
                registrations.Add(this.CreateFunc(typeof(TService), instanceCreator, lifetime));
            }

            return this;
        }

        /// <summary>
        /// 使用服务对象本身注册单例服务类型。
        /// </summary>
        /// <typeparam name="TService">服务类型。</typeparam>
        /// <param name="instance">实例的构造方法。</param>
        /// <returns>当前的 IOC 容器。</returns>
        public Container Register<TService>(TService instance) where TService : class
        {
            registrations.Add(this.CreateSingleton(typeof(TService), instance));
            return this;
        }

        /// <summary>
        /// 使用服务对象的构造器注册它的服务类型。
        /// </summary>
        /// <param name="serviceType">服务类型。</param>
        /// <param name="instanceCreator">实例的构造方法。</param>
        /// <param name="lifetime">生命周期。</param>
        /// <returns>当前的 IOC 容器。</returns>
        public Container Register(Type serviceType, Func<IResolver, object> instanceCreator, Lifetime lifetime)
        {
            if (lifetime == Lifetime.Singleton)
            {
                registrations.Add(this.CreateSingleton(serviceType, instanceCreator(this)));
            }
            else
            {
                registrations.Add(this.CreateFunc(serviceType, instanceCreator, lifetime));
            }

            return this;
        }

        /// <summary>
        /// 遍列程序集中的所有类型，以其接口类型注册服务类型。
        /// </summary>
        /// <param name="assembly">程序集。</param>
        /// <param name="lifetime">生命周期。</param>
        /// <returns></returns>
        public Container RegisterAssembly(Assembly assembly, Lifetime lifetime = Lifetime.Transient)
        {
            Helpers.DiscoverAssembly(assembly, (svrType, implType) => Register(svrType, implType, lifetime));

            return this;
        }

        /// <summary>
        /// 遍列程序集中的所有类型，以其接口类型注册服务类型。
        /// </summary>
        /// <param name="assemblyName">程序集名称。</param>
        /// <param name="lifetime">生命周期。</param>
        /// <returns></returns>
        public Container RegisterAssembly(string assemblyName, Lifetime lifetime = Lifetime.Transient)
        {
            var assembly = Assembly.Load(assemblyName);
            return RegisterAssembly(assembly, lifetime);
        }

        /// <summary>
        /// 注销该服务登记。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        public void UnRegister<TService>() where TService : class
        {
            UnRegister(typeof(TService));
        }

        /// <summary>
        /// 注销该服务登记。
        /// </summary>
        /// <param name="serviceType">服务类型。</param>
        public void UnRegister(Type serviceType)
        {
            var regs = registrations.Where(s => s.ServiceType == serviceType).ToArray();
            for (var i = regs.Length - 1; i >= 0; i--)
            {
                registrations.Remove(regs[i]);
            }
        }

        /// <summary>
        /// 清除容器内的所有注册的服务类型。
        /// </summary>
        public void Clear()
        {
            registrations.Clear();
        }

        /// <summary>
        /// 解析出类型 <typeparamref name="TService"/> 的实例。
        /// </summary>
        /// <typeparam name="TService">服务类型。</typeparam>
        /// <returns>类型的实例对象。如果没有注册，则为 null。</returns>
        public TService Resolve<TService>() where TService : class
        {
            return (TService)Resolve(typeof(TService));
        }

        /// <summary>
        /// 解析出类型 <paramref name="serviceType"/> 的实例。
        /// </summary>
        /// <param name="serviceType">服务类型。</param>
        /// <returns>类型的实例对象。如果没有注册，则为 null。</returns>
        public object Resolve(Type serviceType)
        {
            using var scope = new ResolveLoopScope();
            var obj = ResolveHelper.Resolve(this, serviceType, out Lifetime lifetime);
            if (lifetime == Lifetime.Singleton && obj is IDisposable dispObj)
            {
                TryAddDisposableObject(dispObj);
            }

            return obj;
        }

#if NETSTANDARD
        IServiceScope IServiceScopeFactory.CreateScope()
        {
            return new ResolveScope(this);
        }
#endif
        /// <summary>
        /// 创建一个当前请求范围内的反转服务。
        /// </summary>
        /// <returns></returns>
        public IResolver CreateScope()
        {
            return new ResolveScope(this);
        }

        /// <summary>
        /// 获取此容器中注册的所有 <see cref="IRegistration"/>。
        /// </summary>
        /// <returns>所有在该容器注册的注册器。</returns>
        public IEnumerable<IRegistration> GetRegistrations()
        {
            return registrations;
        }

        /// <summary>
        /// 获取指定类型的 <see cref="IRegistration"/> 实例。
        /// </summary>
        /// <typeparam name="TService">服务类型。</typeparam>
        /// <returns>类型的注册器。</returns>
        public IEnumerable<IRegistration> GetRegistrations<TService>() where TService : class
        {
            return GetRegistrations(typeof(TService));
        }

        /// <summary>
        /// 获取指定类型的 <see cref="IRegistration"/> 实例。
        /// </summary>
        /// <param name="serviceType">服务类型。</param>
        /// <returns>类型的注册器。</returns>
        public IEnumerable<IRegistration> GetRegistrations(Type serviceType)
        {
            if (Helpers.IsEnumerableResolve(serviceType))
            {
                var elementType = serviceType.GetEnumerableElementType();
                return registrations.Where(s => s.ServiceType == elementType);
            }

            return registrations.Where(s => s.ServiceType == serviceType);
        }

        /// <summary>
        /// 判断服务类是否已注册。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public bool IsRegistered<TService>()
        {
            return IsRegistered(typeof(TService));
        }

        /// <summary>
        /// 判断服务类是否已注册。
        /// </summary>
        /// <param name="serviceType">服务类型。</param>
        /// <returns></returns>
        public bool IsRegistered(Type serviceType)
        {
            return registrations.Any(s => s.ServiceType == serviceType);
        }

        /// <summary>
        /// 注册类型 <typeparamref name="TService"/> 的初始化器。
        /// </summary>
        /// <typeparam name="TService">服务类型。</typeparam>
        /// <param name="instanceInitializer">初始化器的构造方法。</param>
        /// <returns>当前的 IOC 容器。</returns>
        public Container RegisterInitializer<TService>(Action<TService> instanceInitializer) where TService : class
        {
            Guard.ArgumentNull(instanceInitializer, nameof(instanceInitializer));

            instanceInitializers.Add(new InstanceInitializer
            {
                ServiceType = typeof(TService),
                Action = instanceInitializer,
            });
            return this;
        }

        object IServiceProvider.GetService(Type serviceType)
        {
            return Resolve(serviceType);
        }

        /// <summary>
        /// 获取类型 <typeparamref name="TService"/> 的初始化器。
        /// </summary>
        /// <typeparam name="TService">服务类型。</typeparam>
        /// <returns>一个初始化器（如果注册过），否则为 null。</returns>
        public Action<TService> GetInitializer<TService>()
        {
            var initializers = GetInstanceInitializers<TService>();

            //只有一个，或是没有
            if (initializers.Length <= 1)
            {
                return initializers.FirstOrDefault();
            }

            //循环初始化
            return obj =>
                {
                    foreach (var t in initializers)
                    {
                        t(obj);
                    }
                };
        }

        private Action<T>[] GetInstanceInitializers<T>()
        {
            var typeHierarchy = typeof(T).GetHierarchyTypes();
            return (
                from instanceInitializer in instanceInitializers
                where typeHierarchy.Contains(instanceInitializer.ServiceType)
                select Helpers.CreateAction<T>(instanceInitializer.Action))
                .ToArray();
        }

        /// <summary>
        /// 通过 XML 配置文件进行注册。
        /// </summary>
        /// <param name="path">文件路径。</param>
        /// <param name="pattern">文件通配符。</param>
        /// <returns></returns>
        public Container Config(string path, string pattern)
        {
            var dir = new DirectoryInfo(path);
            if (!dir.Exists)
            {
                return this;
            }

            foreach (var file in dir.GetFiles(pattern))
            {
                LoadXmlConfig(file.FullName);
            }

            return this;
        }

        /// <summary>
        /// 通过配置文件进行注册。
        /// </summary>
        /// <param name="configFileName"></param>
        /// <returns></returns>
        public Container Config(string configFileName)
        {
            if (!File.Exists(configFileName))
            {
                return this;
            }

            LoadXmlConfig(configFileName);

            return this;
        }


        internal ParameterExpression GetParameterExpression()
        {
            return parExp ?? (parExp = Expression.Parameter(typeof(Container), "c"));
        }

        private void LoadXmlConfig(string fileName)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(fileName);

            var nodes = xmlDoc.SelectNodes("container/registration");
            if (nodes == null)
            {
                return;
            }

            foreach (XmlNode nd in nodes)
            {
                var serviceType = nd.GetAttributeValue("serviceType").ParseType();
                var implementationType = (nd.Attributes["implementationType"] ?? nd.Attributes["componentType"])?.Value.ParseType();
                var singleton = nd.GetAttributeValue<bool>("singleton");

                if (serviceType != null && implementationType != null)
                {
                    if (singleton)
                    {
                        Register(serviceType, implementationType, Lifetime.Singleton);
                    }
                    else
                    {
                        Register(serviceType, implementationType, Lifetime.Transient);
                    }
                }
            }
        }

        private void TryAddDisposableObject(IDisposable dispObj)
        {
            if (!dispObjects.Contains(dispObj))
            {
                dispObjects.Add(dispObj);
            }
        }

        protected override bool Dispose(bool disposing)
        {
            dispObjects.ForEach(s => s.Dispose());
            dispObjects.Clear();

            return base.Dispose(disposing);
        }

        private class ResolveScope : DisposeableBase, IServiceProvider, IResolver
#if NETSTANDARD
            ,IServiceScope
#endif
        {
            private readonly IResolver root;
            private readonly List<IDisposable> dispObjects = new List<IDisposable>();
            private readonly SafetyDictionary<Type, object> scopeObjects = new SafetyDictionary<Type, object>();

            public IServiceProvider ServiceProvider => this;

            public ResolveScope(IResolver root)
            {
                this.root = root;
            }

            public IResolver CreateScope()
            {
                return new ResolveScope(root);
            }

            public object Resolve(Type serviceType)
            {
                if (scopeObjects.TryGetValue(serviceType, out var obj))
                {
                    return obj;
                }

                using var scope = new ResolveLoopScope();
                obj = ResolveHelper.Resolve(this, serviceType, out Lifetime lifetime);

                if (lifetime == Lifetime.Scoped)
                {
                    scopeObjects.TryAdd(serviceType, obj);
                }

                if (obj is IDisposable dispObj)
                {
                    if (lifetime != Lifetime.Singleton)
                    {
                        dispObjects.Add(dispObj);
                    }
                    else if (root is Container container)
                    {
                        container.TryAddDisposableObject(dispObj);
                    }
                }

                return obj;
            }

            public TService Resolve<TService>() where TService : class
            {
                var obj = Resolve(typeof(TService));
                if (obj != null)
                {
                    return (TService)obj;
                }

                return default;
            }

            public IEnumerable<IRegistration> GetRegistrations(Type serviceType)
            {
                return root.GetRegistrations(serviceType);
            }

            object IServiceProvider.GetService(Type serviceType)
            {
                return Resolve(serviceType);
            }

            protected override bool Dispose(bool disposing)
            {
                dispObjects.ForEach(s => s.Dispose());
                dispObjects.Clear();
                scopeObjects.Clear();

                return base.Dispose(disposing);
            }
        }

        private class ResolveHelper
        {
            internal static object Resolve(IResolver resolver, Type serviceType, out Lifetime lifetime)
            {
                lifetime = Lifetime.Transient;

                if (Helpers.IsEnumerableResolve(serviceType))
                {
                    var elementType = serviceType.GetEnumerableElementType();
                    var regs = resolver.GetRegistrations(elementType);

                    var list = CreateEnumerable(elementType);

                    regs.Select(s => s.ServiceType).Distinct().ForEach(s =>
                    {
                        if (!ResolveLoopScope.Current.TryAddType(s))
                        {
                            throw new ResolveException(SR.GetString(SRKind.LoopResolveSameType, s));
                        }
                    });

                    foreach (var reg in regs)
                    {
                        lifetime = reg.Lifetime;
                        list.Add(reg.Resolve(resolver));
                    }

                    return list;
                }

                IRegistration registration;
                if ((registration = resolver.GetRegistrations(serviceType).LastOrDefault()) != null)
                {
                    if (!ResolveLoopScope.Current.TryAddType(registration.ServiceType))
                    {
                        throw new ResolveException(SR.GetString(SRKind.LoopResolveSameType, registration.ServiceType));
                    }

                    lifetime = registration.Lifetime;
                    return registration.Resolve(resolver);
                }

                if (!serviceType.IsAbstract && !serviceType.IsInterface)
                {
                    return serviceType.New();
                }

                return null;
            }

            private static IEnumerable<IRegistration> GetRegistrations(List<IRegistration> registrations, Type serviceType)
            {
                if (Helpers.IsEnumerableResolve(serviceType))
                {
                    var elementType = serviceType.GetEnumerableElementType();
                    return registrations.Where(s => s.ServiceType == elementType);
                }

                return registrations.Where(s => s.ServiceType == serviceType);
            }

            private static IList CreateEnumerable(Type serviceType)
            {
                return typeof(List<>).MakeGenericType(serviceType).New<IList>();
            }
        }
    }
}
