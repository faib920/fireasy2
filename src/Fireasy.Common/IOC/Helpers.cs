// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Fireasy.Common.Ioc
{
    internal static class Helpers
    {
        internal static Action<T> CreateAction<T>(object action)
        {
            var actionArgumentType = action.GetType().GetGenericArguments()[0];

            var objParameter = Expression.Parameter(typeof(T), "s");

            var instanceInitializer = Expression.Lambda<Action<T>>(
                Expression.Invoke(
                    Expression.Constant(action),
                    new[] { Expression.Convert(objParameter, actionArgumentType) }),
                new ParameterExpression[] { objParameter });

            return instanceInitializer.Compile();
        }

        /// <summary>
        /// 发现程序集中的所有依赖类型。
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="regAct"></param>
        internal static void DiscoverServices(Assembly assembly, Action<Type, Type, Lifetime> regAct)
        {
            foreach (var type in assembly.GetExportedTypes())
            {
                if (type.IsInterface || type.IsAbstract || type.IsEnum || type.IsDefined(typeof(IgnoreRegisterAttribute)))
                {
                    continue;
                }

                Lifetime? lifetime;
                var interfaceTypes = type.GetDirectImplementInterfaces().ToArray();

                if (typeof(IRepeatableService).IsAssignableFrom(type) && interfaceTypes.Length > 1)
                {
                    throw new InvalidOperationException(SR.GetString(SRKind.OnlyImplOneInterface, type));
                }

                //如果使用标注
                if (type.IsDefined(typeof(ServiceRegisterAttribute)))
                {
                    lifetime = type.GetCustomAttribute<ServiceRegisterAttribute>().Lifetime;
                }
                else
                {
                    lifetime = GetLifetimeFromType(type);
                }

                if (lifetime == null)
                {
                    continue;
                }

                if (interfaceTypes.Length > 0)
                {
                    interfaceTypes.ForEach(s => regAct(s, type, (Lifetime)lifetime));

                    if (type.IsDefined<RegisterOneselfAttribute>())
                    {
                        regAct(type, type, (Lifetime)lifetime);
                    }
                }
                else
                {
                    regAct(type, type, (Lifetime)lifetime);
                }
            }
        }

        internal static bool IsEnumerableResolve(Type serviceType)
        {
            Type definitionType;

            return serviceType.IsGenericType &&
                (definitionType = serviceType.GetGenericTypeDefinition()) != null
                && (definitionType == typeof(IEnumerable<>) || definitionType == typeof(IList<>) || definitionType == typeof(IList<>));
        }

        private static Lifetime? GetLifetimeFromType(Type type)
        {
            if (typeof(ISingletonService).IsAssignableFrom(type))
            {
                return Lifetime.Singleton;
            }
            else if (typeof(ITransientService).IsAssignableFrom(type))
            {
                return Lifetime.Transient;
            }
            else if (typeof(IScopedService).IsAssignableFrom(type))
            {
                return Lifetime.Scoped;
            }

            return null;
        }
    }
}
