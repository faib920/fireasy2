// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using Fireasy.Common.Caching;
using Fireasy.Common.Extensions;
using Fireasy.Common.Ioc.Registrations;
using System.Reflection;
using System.IO;
using System.Xml;
using Fireasy.Common.Aop;

namespace Fireasy.Common.Ioc
{
    /// <summary>
    /// 控制反转的容器，用于存放描述组件与服务的联系。
    /// </summary>
    public sealed class Container : IServiceProvider, IDisposable
    {
        private ConcurrentDictionary<Type, IRegistration> registrations = new ConcurrentDictionary<Type, IRegistration>();
        private readonly List<InstanceInitializer> instanceInitializers = new List<InstanceInitializer>();

        /// <summary>
        /// 使用选项初始化 <see cref="Container"/> 类的新实例。
        /// </summary>
        /// <param name="options">容器的选项。</param>
        public Container(ContainerOptions options)
        {
            Options = options;
        }

        /// <summary>
        /// 初始化 <see cref="Container"/> 类的新实例。
        /// </summary>
        public Container()
            : this(new ContainerOptions())
        {
        }

        /// <summary>
        /// 获取或设置 IOC 容器的选项。
        /// </summary>
        public ContainerOptions Options { get; set; }

        /// <summary>
        /// 注册服务类型及实现类型，<typeparamref name="TComponent"/> 是 <typeparamref name="TService"/> 的实现类。
        /// </summary>
        /// <typeparam name="TService">服务类型。</typeparam>
        /// <typeparam name="TComponent">组件类型。</typeparam>
        /// <returns>当前的 IOC 容器。</returns>
        public Container Register<TService, TComponent>()
            where TComponent : class, TService
            where TService : class
        {
            AddRegistration(new TransientRegistration<TService, TComponent>());
            return this;
        }

        /// <summary>
        /// 注册服务类型及实现类型，<typeparamref name="TComponent"/> 是 <typeparamref name="TService"/> 的实现类。
        /// </summary>
        /// <typeparam name="TService">服务类型。</typeparam>
        /// <typeparam name="TComponent">组件类型。</typeparam>
        /// <param name="lifetime">生命周期。</param>
        /// <returns>当前的 IOC 容器。</returns>
        public Container Register<TService, TComponent>(Lifetime lifetime)
            where TComponent : class, TService
            where TService : class
        {
            if (lifetime == Lifetime.Singleton)
            {
                return RegisterSingleton(() => typeof(TComponent).New<TService>());
            }

            return Register<TService, TComponent>();
        }

        /// <summary>
        /// 注册服务类型及实现类型，<paramref name="componentType"/> 是 <paramref name="serviceType"/> 的实现类。
        /// </summary>
        /// <param name="serviceType">服务类型。</param>
        /// <param name="componentType">组件类型。</param>
        /// <returns>当前的 IOC 容器。</returns>
        public Container Register(Type serviceType, Type componentType)
        {
            var type = typeof(TransientRegistration<,>).MakeGenericType(serviceType, componentType);
            AddRegistration(type.New<IRegistration>());
            return this;
        }

        /// <summary>
        /// 注册服务类型及实现类型，<paramref name="componentType"/> 是 <paramref name="serviceType"/> 的实现类。
        /// </summary>
        /// <param name="serviceType">服务类型。</param>
        /// <param name="componentType">组件类型。</param>
        /// <param name="lifetime">生命周期。</param>
        /// <returns>当前的 IOC 容器。</returns>
        public Container Register(Type serviceType, Type componentType, Lifetime lifetime)
        {
            if (lifetime == Lifetime.Singleton)
            {
                return RegisterSingleton(serviceType, componentType);
            }

            return Register(serviceType, componentType);
        }

        /// <summary>
        /// 使用服务对象的构造器注册它的服务类型。
        /// </summary>
        /// <typeparam name="TService">服务类型。</typeparam>
        /// <param name="instanceCreator">实例的构造方法。</param>
        /// <returns>当前的 IOC 容器。</returns>
        public Container Register<TService>(Func<TService> instanceCreator) where TService : class
        {
#if NET35
            var creator = new Func<object>(() => instanceCreator());
            AddRegistration(new FuncRegistration<TService>(creator));
#else
            AddRegistration(new FuncRegistration<TService>(instanceCreator));
#endif
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
            var type = typeof(FuncRegistration<>).MakeGenericType(serviceType);
            var constructor = type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0];
            AddRegistration((IRegistration)constructor.Invoke(new[] { instanceCreator }));
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
            AddRegistration(new SingletonRegistration<TService>(instanceCreator));
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
            AddRegistration(new SingletonRegistration<TService>(instance));
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
        /// <param name="serviceType">服务类型。</param>
        /// <param name="componentType">组件类型。</param>
        /// <returns>当前的 IOC 容器。</returns>
        public Container RegisterSingleton(Type serviceType, Type componentType)
        {
            AddRegistration(new SingletonRegistration(serviceType, componentType.New()));
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
                if (type.IsInterface || type.IsEnum)
                {
                    continue;
                }

                Register(type, type);

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
            IRegistration registration;
            registrations.TryRemove(serviceType, out registration);
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
            return Resolve(typeof(TService)).As<TService>();
        }

        /// <summary>
        /// 解析出类型 <paramref name="serviceType"/> 的实例。
        /// </summary>
        /// <param name="serviceType">服务类型。</param>
        /// <returns>类型的实例对象。如果没有注册，则为 null。</returns>
        public object Resolve(Type serviceType)
        {
            var registration = GetRegistration(serviceType);
            if (registration == null && !serviceType.IsAbstract && !serviceType.IsInterface)
            {
                var type = typeof(TransientRegistration<,>).MakeGenericType(serviceType, serviceType);
                registration = type.New<IRegistration>();
                AddRegistration(registration);
            }

            if (registration != null)
            {
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
            return registrations.Values;
        }

        /// <summary>
        /// 获取指定类型的 <see cref="IRegistration"/> 实例。
        /// </summary>
        /// <typeparam name="TService">服务类型。</typeparam>
        /// <returns>类型的注册器。</returns>
        public IRegistration GetRegistration<TService>() where TService : class
        {
            return GetRegistration(typeof(TService));
        }

        /// <summary>
        /// 获取指定类型的 <see cref="IRegistration"/> 实例。
        /// </summary>
        /// <param name="serviceType">服务类型。</param>
        /// <returns>类型的注册器。</returns>
        public IRegistration GetRegistration(Type serviceType)
        {
            IRegistration registration;
            registrations.TryGetValue(serviceType, out registration);
            return registration;
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
            return registrations.ContainsKey(serviceType);
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
            return GetRegistration(serviceType);
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
            if (!Options.AllowOverriding && registrations.ContainsKey(registration.ServiceType))
            {
                return;
            }

            registration.As<AbstractRegistration>(e => e.Container = this);
            registrations.AddOrReplace(registration.ServiceType, registration);
        }

        /// <summary>
        /// 释放对象所占用的所有资源。
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// 通过配置文件进行注册。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="pattern"></param>
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

        private void LoadXmlConfig(string fileName)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(fileName);

            foreach (XmlNode nd in xmlDoc.SelectNodes("container/registration"))
            {
                var serviceType = nd.GetAttributeValue("serviceType").ParseType();
                var componentType = nd.GetAttributeValue("componentType").ParseType();
                var singleton = nd.GetAttributeValue<bool>("singleton");

                if (serviceType != null && componentType != null)
                {
                    if (singleton)
                    {
                        RegisterSingleton(serviceType, componentType);
                    }
                    else
                    {
                        Register(serviceType, componentType);
                    }
                }
            }
        }
    }
}
