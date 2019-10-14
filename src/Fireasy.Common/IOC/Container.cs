// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.Extensions;
using Fireasy.Common.Ioc.Registrations;
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
    public sealed class Container : IServiceProvider
    {
        private ParameterExpression parExp = null;
        private List<IRegistration> registrations = new List<IRegistration>();
        private readonly List<InstanceInitializer> instanceInitializers = new List<InstanceInitializer>();
        private ResolveScope scope = null;

        /// <summary>
        /// 注册服务类型及实现类型，<typeparamref name="TImplementation"/> 是 <typeparamref name="TService"/> 的实现类。
        /// </summary>
        /// <typeparam name="TService">服务类型。</typeparam>
        /// <typeparam name="TImplementation">实现类型。</typeparam>
        /// <returns>当前的 IOC 容器。</returns>
        public Container Register<TService, TImplementation>()
            where TImplementation : class, TService
            where TService : class
        {
            AddRegistration(new TransientRegistration<TService, TImplementation>());
            return this;
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
            if (lifetime == Lifetime.Singleton)
            {
                return RegisterSingleton<TService, TImplementation>();
            }
            else if (lifetime == Lifetime.Scoped)
            {
                AddRegistration(Creator.CreateScoped(typeof(TService), typeof(TImplementation)));
                return this;
            }

            return Register<TService, TImplementation>();
        }

        /// <summary>
        /// 注册服务类型及实现类型，<paramref name="implementationType"/> 是 <paramref name="serviceType"/> 的实现类。
        /// </summary>
        /// <param name="serviceType">服务类型。</param>
        /// <param name="implementationType">实现类型。</param>
        /// <returns>当前的 IOC 容器。</returns>
        public Container Register(Type serviceType, Type implementationType)
        {
            AddRegistration(Creator.CreateTransient(serviceType, implementationType));
            return this;
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
                return RegisterSingleton(serviceType, implementationType);
            }
            else if (lifetime == Lifetime.Scoped)
            {
                AddRegistration(Creator.CreateScoped(serviceType, implementationType));
                return this;
            }

            return Register(serviceType, implementationType);
        }

        /// <summary>
        /// 使用服务对象的构造器注册它的服务类型。
        /// </summary>
        /// <typeparam name="TService">服务类型。</typeparam>
        /// <param name="instanceCreator">实例的构造方法。</param>
        /// <returns>当前的 IOC 容器。</returns>
        public Container Register<TService>(Func<TService> instanceCreator) where TService : class
        {
            AddRegistration(new FuncRegistration<TService>(instanceCreator));
            return this;
        }

        /// <summary>
        /// 使用服务对象的构造器注册它的服务类型。
        /// </summary>
        /// <param name="serviceType">服务类型。</param>
        /// <param name="instanceCreator">实例的构造方法。</param>
        /// <returns>当前的 IOC 容器。</returns>
        public Container Register(Type serviceType, Func<object> instanceCreator)
        {
            var registration = Creator.CreateFunc(serviceType, instanceCreator);
            AddRegistration(registration);
            return this;
        }

        /// <summary>
        /// 使用服务对象的构造器注册它的服务类型，该对象是一个单例。
        /// </summary>
        /// <typeparam name="TService">服务类型。</typeparam>
        /// <param name="instanceCreator">实例的构造方法。</param>
        /// <returns>当前的 IOC 容器。</returns>
        public Container RegisterSingleton<TService>(Func<TService> instanceCreator) where TService : class
        {
            AddRegistration(new SingletonRegistration(typeof(TService), instanceCreator()));
            return this;
        }

        /// <summary>
        /// 使用服务对象的实例注册它的服务类型，该对象是一个单例。
        /// </summary>
        /// <typeparam name="TService">服务类型。</typeparam>
        /// <param name="instance">类型 <typeparamref name="TService"/> 的实例。</param>
        /// <returns>当前的 IOC 容器。</returns>
        public Container RegisterSingleton<TService>(TService instance) where TService : class
        {
            AddRegistration(new SingletonRegistration(typeof(TService), instance));
            return this;
        }

        /// <summary>
        /// 使用服务对象的构造器注册它的服务类型，该对象是一个单例。
        /// </summary>
        /// <param name="serviceType">服务类型。</param>
        /// <param name="instanceCreator">实例的构造方法。</param>
        /// <returns>当前的 IOC 容器。</returns>
        public Container RegisterSingleton(Type serviceType, Func<object> instanceCreator)
        {
            AddRegistration(new SingletonRegistration(serviceType, instanceCreator));
            return this;
        }

        /// <summary>
        /// 使用服务对象的构造器注册它的服务类型，该对象是一个单例。
        /// </summary>
        /// <typeparam name="TService">服务类型。</typeparam>
        /// <typeparam name="TImplementation">实现类型。</typeparam>
        /// <returns>当前的 IOC 容器。</returns>
        public Container RegisterSingleton<TService, TImplementation>()
            where TImplementation : class, TService
            where TService : class
        {
            return RegisterSingleton(typeof(TService), typeof(TImplementation));
        }

        /// <summary>
        /// 使用服务对象的构造器注册它的服务类型，该对象是一个单例。
        /// </summary>
        /// <param name="serviceType">服务类型。</param>
        /// <param name="implementationType">实现类型。</param>
        /// <returns>当前的 IOC 容器。</returns>
        public Container RegisterSingleton(Type serviceType, Type implementationType)
        {
            AddRegistration(Creator.RelyWithTransient(serviceType, implementationType, this));
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
            foreach (var type in assembly.GetExportedTypes())
            {
                if (type.IsInterface || type.IsAbstract || type.IsEnum)
                {
                    continue;
                }

                foreach (var interfaceType in type.GetInterfaces())
                {
                    if (interfaceType.IsDefined<IgnoreRegisterAttribute>())
                    {
                        continue;
                    }

                    Register(interfaceType, type, lifetime);
                }
            }

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
            if (scope == null)
            {
                scope = new ResolveScope();
            }

            if (IsEnumerableResolve(serviceType))
            {
                var elementType = serviceType.GetEnumerableElementType();
                var regs = GetRegistrations(elementType);
                var list = CreateEnumerable(elementType);

                foreach (var reg in regs)
                {
                    list.Add(reg.Resolve());
                }

                return list;
            }

            IRegistration registration;
            if ((registration = GetRegistrations(serviceType).LastOrDefault()) != null)
            {
                return registration.Resolve();
            }

            if (!serviceType.IsAbstract && !serviceType.IsInterface)
            {
                registration = Creator.CreateTransient(serviceType, serviceType);
                AddRegistration(registration);

                return registration.Resolve();
            }

            return null;
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
            if (IsEnumerableResolve(serviceType))
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

        private void AddRegistration(IRegistration registration)
        {
            registration.As<AbstractRegistration>(e => e.SetContainer(this));
            registrations.Add(registration);
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

        /// <summary>
        /// 判断是否需要反转为 <see cref="IEnumerable{T}"/> 类型的对象。
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public bool IsEnumerableResolve(Type serviceType)
        {
            Type definitionType;

            return serviceType.IsGenericType &&
                (definitionType = serviceType.GetGenericTypeDefinition()) != null
                && (definitionType == typeof(IEnumerable<>) || definitionType == typeof(IList<>) || definitionType == typeof(IList<>));
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
                        RegisterSingleton(serviceType, implementationType);
                    }
                    else
                    {
                        Register(serviceType, implementationType);
                    }
                }
            }
        }

        private IList CreateEnumerable(Type serviceType)
        {
            return typeof(List<>).MakeGenericType(serviceType).New<IList>();
        }
    }
}
