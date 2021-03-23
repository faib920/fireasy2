// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD
using AutoMapper;
using Fireasy.Common.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fireasy.AutoMapper
{
    public class AutoMapperOptions
    {
        private readonly IServiceCollection _serviceCollection;
        private readonly HashSet<Assembly> _existsAssemblies = new HashSet<Assembly>();

        public List<Action<IMapperConfigurationExpression>> Configurators { get; } = new List<Action<IMapperConfigurationExpression>>();

        internal AutoMapperOptions(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

        /// <summary>
        /// 添加配置文件。
        /// </summary>
        /// <typeparam name="TProfile"></typeparam>
        public AutoMapperOptions AddProfile<TProfile>() where TProfile : Profile, new()
        {
            Configurators.Add(c => c.AddProfile<TProfile>());

            RegisterDependencyServices(typeof(TProfile).Assembly);

            return this;
        }

        /// <summary>
        /// 添加程序集里的 <see cref="Profile"/> 及定义了 <see cref="AutoMapAttribute"/> 特性的类。
        /// </summary>
        /// <param name="assembly"></param>
        [Obsolete]
        public AutoMapperOptions AddAssembly(Assembly assembly = null)
        {
            assembly ??= Assembly.GetCallingAssembly();
            Configurators.Add(c => c.AddMaps(assembly));

            RegisterDependencyServices(assembly);

            return this;
        }

        /// <summary>
        /// 添加程序集里的 <see cref="Profile"/> 及定义了 <see cref="AutoMapAttribute"/> 特性的类。
        /// </summary>
        /// <param name="assemblies">要搜索的程序集数组，如果没有指定，则为当前调用的程序集。</param>
        public AutoMapperOptions AddProfiles(params Assembly[] assemblies)
        {
            if (assemblies == null)
            {
                Configurators.Add(c => c.AddMaps(Assembly.GetCallingAssembly()));
            }
            else
            {
                assemblies.ForEach(s => Configurators.Add(c => c.AddMaps(s)));
            }

            RegisterDependencyServices(assemblies);

            return this;
        }

        private void RegisterDependencyServices(params Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length == 0)
            {
                return;
            }

            var allTypes = assemblies.Where(a => !_existsAssemblies.Contains(a) && !a.IsDynamic && a.GetName().Name != "AutoMapper")
                .Distinct().SelectMany(a => a.DefinedTypes).ToArray();
            foreach (TypeInfo item in new Type[]
            {
                typeof(IValueResolver<,,>),
                typeof(IMemberValueResolver<,,,>),
                typeof(ITypeConverter<,>),
                typeof(IValueConverter<,>),
                typeof(IMappingAction<,>)
            }.SelectMany(openType => allTypes.Where(t => t.IsClass && !t.IsAbstract && ImplementsGenericInterface(t.AsType(), openType))))
            {
                _existsAssemblies.Add(item.Assembly);
                _serviceCollection.AddTransient(item.AsType());
            }
        }

        private static bool ImplementsGenericInterface(Type type, Type interfaceType)
        {
            if (!IsGenericType(type, interfaceType))
            {
                return type.GetTypeInfo().ImplementedInterfaces.Any((Type @interface) => IsGenericType(@interface, interfaceType));
            }

            return true;
        }

        private static bool IsGenericType(Type type, Type genericType)
        {
            if (type.GetTypeInfo().IsGenericType)
            {
                return type.GetGenericTypeDefinition() == genericType;
            }

            return false;
        }

    }
}
#endif