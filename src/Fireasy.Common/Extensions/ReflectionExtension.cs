// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Aop;
using Fireasy.Common.Caching;
using Fireasy.Common.Emit;
using Fireasy.Common.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Fireasy.Common.Extensions
{
    /// <summary>
    /// 反射的扩展方法。
    /// </summary>
    public static class ReflectionExtension
    {

        public static bool IsUnsignedInt(this Type type)
        {
            type = type.GetNonNullableType();
            if (!type.IsEnum)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                        return true;
                }
            }

            return false;
        }

        public static bool IsUnsigned(this Type type)
        {
            type = type.GetNonNullableType();
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Char:
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
            }
            return false;
        }

        public static bool IsNumeric(this Type type)
        {
            type = type.GetNonNullableType();
            if (!type.IsEnum)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Char:
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                    case TypeCode.Single:
                    case TypeCode.Double:
                        return true;
                }
            }
            return false;
        }

        public static bool IsIntegerOrBool(this Type type)
        {
            type = type.GetNonNullableType();
            if (!type.IsEnum)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Boolean:
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                        return true;
                }
            }
            return false;
        }

        public static bool IsStringOrDateTime(this Type type)
        {
            type = type.GetNonNullableType();
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.String:
                case TypeCode.DateTime:
                    return true;
            }

            return false;
        }

        public static bool IsInteger(this Type type)
        {
            type = type.GetNonNullableType();
            if (!type.IsEnum)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                        return true;
                }
            }
            return false;
        }

        public static bool IsConvertible(this Type type)
        {
            type = type.GetNonNullableType();
            if (type.IsEnum)
            {
                return true;
            }
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                    return true;
            }
            return false;
        }

        public static bool IsArithmetic(this Type type)
        {
            type = type.GetNonNullableType();
            if (!type.IsEnum)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                    case TypeCode.Single:
                    case TypeCode.Double:
                        return true;
                }
            }
            return false;
        }

        public static bool IsImplicitNumericConversion(this Type source, Type destination)
        {
            TypeCode typeCode = Type.GetTypeCode(source);
            TypeCode code2 = Type.GetTypeCode(destination);
            switch (typeCode)
            {
                case TypeCode.Char:
                    switch (code2)
                    {
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;

                case TypeCode.SByte:
                    switch (code2)
                    {
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;

                case TypeCode.Byte:
                    switch (code2)
                    {
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;

                case TypeCode.Int16:
                    switch (code2)
                    {
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;

                case TypeCode.UInt16:
                    switch (code2)
                    {
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;

                case TypeCode.Int32:
                    switch (code2)
                    {
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;

                case TypeCode.UInt32:
                    switch (code2)
                    {
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;

                case TypeCode.Int64:
                case TypeCode.UInt64:
                    switch (code2)
                    {
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;

                case TypeCode.Single:
                    return (code2 == TypeCode.Double);

                default:
                    return false;
            }
            return false;
        }

        /// <summary>
        /// 获取 <see cref="Type"/> 对象。
        /// </summary>
        /// <param name="typeName">类型的名称。</param>
        /// <returns>一个 <see cref="Type"/> 对象。</returns>
        public static Type ParseType(this string typeName)
        {
            var type = Type.GetType(typeName, false, true);
            if (type != null)
            {
                return type;
            }

            if (typeName.IndexOf("Version") == -1)
            {
                return null;
            }

            //如果没有指定Version，则从当前装配件空间取出Version
            var assembly = typeof(string).Assembly.ToString();
            var index = assembly.IndexOf("Version");

            typeName += "," + assembly.Substring(index);

            return Type.GetType(typeName, false, true);
        }

        /// <summary>
        /// 创建指定类型的实例对象，并转换为类型 <typeparamref name="T"/> 的对象。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static T New<T>(this Type type, params object[] args)
        {
            Guard.ArgumentNull(type, nameof(type));

            var instance = type.New(args);
            if (instance is T)
            {
                return (T)instance;
            }

            return default(T);
        }

        /// <summary>
        /// 创建指定类型的实例对象。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static object New(this Type type, params object[] args)
        {
            Guard.ArgumentNull(type, nameof(type));

            //如果支持 Aop，则用Aop生成
            if (typeof(IAopSupport).IsAssignableFrom(type) && !typeof(IAopImplement).IsAssignableFrom(type))
            {
                return AspectFactory.BuildProxy(type, args);
            }

            if (type.IsInterface || type.IsAbstract)
            {
                type = type.BuildImplementType();
            }

            return InternalNew(type, args);
        }

        internal static object InternalNew(this Type type, params object[] args)
        {
            //两个方法存在较大的性能问题，所以需要判断args参数
            if (args == null || args.Length == 0)
            {
                var cons = type.GetConstructor(Type.EmptyTypes);

                //查找有默认值参数的构造函数
                if (cons == null)
                {
                    cons = type.GetConstructors().FirstOrDefault(s => s.GetParameters().Count(t => t.DefaultValue == DBNull.Value) == 0);
                }

                if (cons == null)
                {
                    throw new MissingMethodException(SR.GetString(SRKind.NoDefaultOrDefaultValueConstructor));
                }

                args = new object[cons.GetParameters().Length];
                return ReflectionCache.GetInvoker(cons).Invoke(args);
            }

            return Activator.CreateInstance(type, args);
        }

        /// <summary>
        /// 获取类型的默认值。
        /// </summary>
        /// <param name="type">源类型。</param>
        /// <returns></returns>
        public static object GetDefaultValue(this Type type)
        {
            Guard.ArgumentNull(type, nameof(type));

            var isNullable = !type.IsValueType || type.IsNullableType();
            if (!isNullable)
            {
                return Activator.CreateInstance(type);
            }

            return null;
        }

        /// <summary>
        /// 判断类型是否为可空。
        /// </summary>
        /// <param name="type">源类型。</param>
        /// <returns></returns>
        public static bool IsNullableType(this Type type)
        {
            Guard.ArgumentNull(type, nameof(type));
            return (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>)));
        }

        /// <summary>
        /// 判断类型是否为实现类，即区别于抽象的类型。
        /// </summary>
        /// <param name="type">源类型。</param>
        /// <returns></returns>
        public static bool IsConcreteType(this Type type)
        {
            return !type.IsAbstract && !type.IsGenericTypeDefinition && !type.IsArray;
        }

        /// <summary>
        /// 获取指定类型的继承层次，包括实现的接口。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type[] GetHierarchyTypes(this Type type)
        {
            var types = new List<Type> { type };
            types.AddRange(type.EachBaseTypes());
            types.AddRange(type.GetInterfaces());
            return types.ToArray();
        }

        /// <summary>
        /// 枚举出指定类型的所有父类型。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<Type> EachBaseTypes(this Type type)
        {
            var baseType = type.BaseType;
            while (baseType != null)
            {
                yield return baseType;
                baseType = baseType.BaseType;
            }
        }

        /// <summary>
        /// 获取 <see cref="Nullable"/> 类型中的类型。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type GetNonNullableType(this Type type)
        {
            if (type.IsNullableType())
            {
                return type.GetGenericArguments()[0];
            }

            return type;
        }

        /// <summary>
        /// 包装类型的 <see cref="Nullable"/> 类型。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type GetNullableType(this Type type)
        {
            if (type.IsValueType && !type.IsNullableType())
            {
                return typeof(Nullable<>).MakeGenericType(new[] { type });
            }

            return type;
        }

        /// <summary>
        /// 判断类型是否为数字类型。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsNumericType(this Type type)
        {
            type = type.GetNonNullableType();
            if (!type.IsEnum)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Char:
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                    case TypeCode.Single:
                    case TypeCode.Decimal:
                    case TypeCode.Double:
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 获取实现泛型定义类型的基类或接口。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="genericDefinitionType"></param>
        /// <returns></returns>
        public static Type GetGenericImplementType(this Type type, Type genericDefinitionType)
        {
            foreach (var t in type.GetHierarchyTypes())
            {
                if (t.IsGenericType && t.GetGenericTypeDefinition() == genericDefinitionType)
                {
                    return t;
                }
            }

            return null;
        }

        /// <summary>
        /// 在父类结构中搜索实现接口类的类型。
        /// </summary>
        /// <param name="type">源类型。</param>
        /// <param name="interfaceType">接口类型。</param>
        /// <returns></returns>
        public static Type GetImplementType(this Type type, Type interfaceType)
        {
            Guard.ArgumentNull(type, nameof(type));
            Guard.ArgumentNull(interfaceType, nameof(interfaceType));
            var baseType = type.BaseType;
            while (baseType != typeof(object))
            {
                if (baseType.IsImplementInterface(interfaceType))
                {
                    return baseType;
                }

                baseType = baseType.BaseType;
            }
            return null;
        }

        /// <summary>
        /// 判断类型是否实现了指定的接口类型。
        /// </summary>
        /// <param name="type">源类型。</param>
        /// <param name="interfaceType">接口类型。</param>
        /// <returns></returns>
        public static bool IsImplementInterface(this Type type, Type interfaceType)
        {
            Guard.ArgumentNull(type, nameof(type));
            Guard.ArgumentNull(interfaceType, nameof(interfaceType));
            Guard.Argument(interfaceType.IsInterface, nameof(interfaceType), SR.GetString(SRKind.NotInterfaceType, interfaceType.FullName));
            return interfaceType.IsAssignableFrom(type);
        }

        /// <summary>
        /// 判断类型是否直接地实现了指定的接口类型。
        /// </summary>
        /// <param name="type">源类型。</param>
        /// <param name="interfaceType">接口类型。</param>
        /// <returns></returns>
        public static bool IsDirectImplementInterface(this Type type, Type interfaceType)
        {
            Guard.ArgumentNull(type, nameof(type));
            Guard.ArgumentNull(interfaceType, nameof(interfaceType));
            Guard.Argument(interfaceType.IsInterface, nameof(interfaceType), SR.GetString(SRKind.NotInterfaceType, interfaceType.FullName));
            foreach (var type1 in type.GetInterfaces())
            {
                if (type1 != interfaceType)
                {
                    continue;
                }

                var im = type.GetInterfaceMap(type1);
                if (im.TargetMethods.Length > 0 &&
                    im.TargetMethods[0].DeclaringType == type)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取直接实现指定接口类的接口。
        /// </summary>
        /// <param name="type">源类型。</param>
        /// <param name="interfaceType">接口类型。</param>
        /// <returns></returns>
        public static Type GetDirectImplementInterface(this Type type, Type interfaceType)
        {
            Guard.ArgumentNull(type, nameof(type));
            Guard.ArgumentNull(interfaceType, nameof(interfaceType));
            Guard.Argument(interfaceType.IsInterface, nameof(interfaceType), SR.GetString(SRKind.NotInterfaceType, interfaceType.FullName));
            foreach (var type1 in type.GetInterfaces())
            {
                if (interfaceType.IsAssignableFrom(type1))
                {
                    return type1;
                }
            }
            return null;
        }

        /// <summary>
        /// 判断是否为匿名类型。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsAnonymousType(this Type type)
        {
            Guard.ArgumentNull(type, nameof(type));

            var fullName = type.FullName;
            return fullName.Length > 18 && fullName.Substring(0, 18) == "<>f__AnonymousType";
        }

        /// <summary>
        /// 查找枚举器 <see cref="T:System.Collections.Generic.IEnumerable`1"/> 的元素类型。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type GetEnumerableElementType(this Type type)
        {
            if (!type.IsGenericType)
            {
                return type.GetElementType();
            }

            var ienum = GetEnumerableType(type);
            return ienum == null ? null : ienum.GetGenericArguments()[0];
        }

        /// <summary>
        /// 获取元素的 <see cref="T:System.Collections.Generic.IEnumerable`1"/> 类型。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type GetEnumerableType(this Type type)
        {
            if (type == null || type == typeof(string))
            {
                return null;
            }

            if (type.IsArray)
            {
                return typeof(IEnumerable<>).MakeGenericType(type.GetElementType());
            }

            if (type.IsGenericType)
            {
                foreach (var arg in type.GetGenericArguments())
                {
                    var ienum = typeof(IEnumerable<>).MakeGenericType(arg);
                    if (ienum.IsAssignableFrom(type))
                    {
                        return ienum;
                    }
                }
            }

            var ifaces = type.GetInterfaces();
            if (ifaces.Length > 0)
            {
                foreach (var iface in ifaces)
                {
                    var ienum = GetEnumerableType(iface);
                    if (ienum != null) return ienum;
                }
            }

            if (type.BaseType != null && type.BaseType != typeof(object))
            {
                return GetEnumerableType(type.BaseType);
            }

            return null;
        }

        /// <summary>
        /// 获取 <see cref="MemberInfo"/> 的类型。
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public static Type GetMemberType(this MemberInfo member)
        {
            switch (member.MemberType)
            {
                case  MemberTypes.Property:
                    return (member as PropertyInfo).PropertyType;
                case  MemberTypes.Field:
                    return (member as FieldInfo).FieldType;
                case  MemberTypes.Event:
                    return (member as EventInfo).EventHandlerType;
                case MemberTypes.Method:
                    return (member as MethodInfo).ReturnType;
            }

            return null;
        }

        /// <summary>
        /// 获取 <see cref="MemberInfo"/> 的值。
        /// </summary>
        /// <param name="member"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static object GetMemberValue(this MemberInfo member, object instance)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Property:
                    return (member as PropertyInfo).GetValue(instance, null);
                case MemberTypes.Field:
                    return (member as FieldInfo).GetValue(instance);
            }

            return null;
        }

        /// <summary>
        /// 获取自定义特性组。
        /// </summary>
        /// <typeparam name="T">自定义特性类型。</typeparam>
        /// <param name="provider">要搜索的特性类型。</param>
        /// <param name="inherit">指定是否搜索该成员的继承链以查找这些特性。</param>
        /// <returns></returns>
        public static IEnumerable<T> GetCustomAttributes<T>(this ICustomAttributeProvider provider, bool inherit = false) where T : Attribute
        {
            return provider.GetCustomAttributes(typeof(T), inherit).Cast<T>();
        }

        /// <summary>
        /// 判定是否定义了自定义特性。
        /// </summary>
        /// <typeparam name="T">自定义特性类型。</typeparam>
        /// <param name="provider">要搜索的特性类型。</param>
        /// <param name="inherit">指定是否搜索该成员的继承链以查找这些特性。</param>
        /// <returns></returns>
        public static bool IsDefined<T>(this ICustomAttributeProvider provider, bool inherit = false) where T : Attribute
        {
            return provider.IsDefined(typeof(T), inherit);
        }

        public static bool IsValidStructuralPropertyType(this Type type)
        {
            return ((!type.IsGenericTypeDefinition && !type.IsPointer) && !(type == typeof(object)));
        }

        public static bool IsValidStructuralType(this Type type)
        {
            return ((((!type.IsGenericType && !type.IsValueType) && (!type.IsPrimitive && !type.IsInterface)) && (!type.IsArray && !(type == typeof(string)))) && type.IsValidStructuralPropertyType());
        }

        /// <summary>
        /// 构造一个实现类。
        /// </summary>
        /// <param name="definedType">定义的接口或父类。</param>
        /// <returns></returns>
        public static Type BuildImplementType(this Type definedType)
        {
            Guard.ArgumentNull(definedType, nameof(definedType));

            var cacheMgr = MemoryCacheManager.Instance;
            var key = "ImplAssembly_" + definedType.FullName;
            return cacheMgr.TryGet(key, () => InternalBuildImplementType(definedType));
        }

        /// <summary>
        /// 高效地获取指定属性的值。
        /// </summary>
        /// <param name="property">要操作的属性。</param>
        /// <param name="instance">实例对象。</param>
        /// <returns></returns>
        public static object FastGetValue(this PropertyInfo property, object instance)
        {
            Guard.ArgumentNull(property, nameof(property));
            return ReflectionCache.GetAccessor(property).GetValue(instance);
        }

        /// <summary>
        /// 高效地设置指定属性的值。
        /// </summary>
        /// <param name="property">要操作的属性。</param>
        /// <param name="instance">实例对象。</param>
        /// <param name="value">要设置的值。</param>
        public static void FastSetValue(this PropertyInfo property, object instance, object value)
        {
            Guard.ArgumentNull(property, nameof(property));
            ReflectionCache.GetAccessor(property).SetValue(instance, value);
        }

        /// <summary>
        /// 高效地获取指定字段的值。
        /// </summary>
        /// <param name="field">要操作的字段。</param>
        /// <param name="instance">实例对象。</param>
        /// <returns></returns>
        public static object FastGetValue(this FieldInfo field, object instance)
        {
            Guard.ArgumentNull(field, nameof(field));
            return ReflectionCache.GetAccessor(field).GetValue(instance);
        }

        /// <summary>
        /// 高效地执行指定的方法。
        /// </summary>
        /// <param name="method">要操作的方法。</param>
        /// <param name="instance">实例对象。</param>
        /// <param name="arguments">方法的参数。</param>
        /// <returns></returns>
        public static object FastInvoke(this MethodInfo method, object instance, params object[] arguments)
        {
            Guard.ArgumentNull(method, nameof(method));
            return ReflectionCache.GetInvoker(method).Invoke(instance, arguments);
        }

        /// <summary>
        /// 高效地执行指定的构造函数。
        /// </summary>
        /// <param name="constructor">要操作的构造函数。</param>
        /// <param name="arguments">构造函数的参数。</param>
        public static object FastInvoke(this ConstructorInfo constructor, params object[] arguments)
        {
            Guard.ArgumentNull(constructor, nameof(constructor));
            return ReflectionCache.GetInvoker(constructor).Invoke(arguments);
        }

        /// <summary>
        /// 获取接口定义的属性。
        /// </summary>
        /// <param name="interfaceType">接口类型。</param>
        /// <returns></returns>
        public static IEnumerable<PropertyInfo> GetDefinedProperties(this Type interfaceType)
        {
            var binding = BindingFlags.Public | BindingFlags.Instance;
            var properties = interfaceType.GetProperties(binding).AsEnumerable();
            foreach (var implType in interfaceType.GetInterfaces())
            {
                properties = properties.Union(implType.GetProperties(binding));
            }

            return properties;
        }

        private static Type InternalBuildImplementType(Type definedType)
        {
            var assemblyBuilder = new DynamicAssemblyBuilder("ImplAssembly");
            var typeBuilder = assemblyBuilder.DefineType(definedType.Name + "_Impl");

            if (definedType.IsInterface)
            {
                typeBuilder.ImplementInterface(definedType);
            }
            else
            {
                typeBuilder.BaseType = definedType;
            }

            //实现属性
            foreach (var property in definedType.GetDefinedProperties())
            {
                if (property.Name == "Item")
                {
                    continue;
                }

                var getMethod = property.GetGetMethod();
                var setMethod = property.GetSetMethod();
                if (definedType.IsInterface || (getMethod != null && getMethod.IsVirtual) || (setMethod != null && setMethod.IsVirtual))
                {
                    var propertyBuilder = typeBuilder.DefineProperty(property.Name, property.PropertyType).DefineGetSetMethods();
                }
            }

            //实现方法
            foreach (var method in definedType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                if ((definedType.IsInterface || method.IsVirtual) && !Regex.IsMatch(method.Name, @"^set_|get_"))
                {
                    var parameters = method.GetParameters();
                    var methodBuilder = typeBuilder.DefineMethod(method.Name, method.ReturnType, parameters.Select(s => s.ParameterType).ToArray());

                    foreach (var par in parameters)
                    {
                        methodBuilder.DefineParameter(par.Name, par.IsOut, par.DefaultValue != DBNull.Value, par.DefaultValue);
                    }
                }
            }

            return typeBuilder.CreateType();
        }
    }
}
