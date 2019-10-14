// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Caching;
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Dynamic;
using Fireasy.Common.Emit;
using Fireasy.Common.Linq.Expressions;
using Fireasy.Common.Mapper;
using Fireasy.Common.Serialization;
using Fireasy.Common.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Fireasy.Common.Extensions
{
    /// <summary>
    /// 基本扩展方法。
    /// </summary>
    public static class GenericExtension
    {
        private static ReadWriteLocker locker = new ReadWriteLocker();
        private static MethodInfo MthToType = typeof(GenericExtension).GetMethod(nameof(GenericExtension.ToType));

        /// <summary>
        /// 判断对象是否为空。
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this object source)
        {
            return source == null ||
                source is DBNull ||
                string.IsNullOrEmpty(source.ToString());
        }

        /// <summary>
        /// 检查可空类型的值是否为空，如果不为空，则调用委托函数返回结果。
        /// </summary>
        /// <typeparam name="TSource">对象类型。</typeparam>
        /// <typeparam name="TReturn">返回的类型</typeparam>
        /// <param name="value">要查检的值。</param>
        /// <param name="func">值不为空时调用的函数。</param>
        public static TReturn AssertNotNull<TSource, TReturn>(this TSource? value, Func<TSource, TReturn> func) where TSource : struct
        {
            if (value != null && func != null)
            {
                return func(value.Value);
            }

            return default(TReturn);
        }

        /// <summary>
        /// 检查对象是否为空，如果不为空，则调用委托方法。
        /// </summary>
        /// <typeparam name="TSource">结构类型。</typeparam>
        /// <param name="value">要查检的值。</param>
        /// <param name="action">值不为空时调用的方法。</param>
        public static void AssertNotNull<TSource>(this TSource value, Action<TSource> action)
        {
            if (value != null && action != null)
            {
                action(value);
            }
        }

        /// <summary>
        /// 检查对象是否为空，如果不为空，则调用委托函数返回结果。
        /// </summary>
        /// <typeparam name="TSource">结构类型。</typeparam>
        /// <typeparam name="TReturn">返回的类型</typeparam>
        /// <param name="value">要查检的值。</param>
        /// <param name="func">值不为空时调用的函数。</param>
        public static TReturn AssertNotNull<TSource, TReturn>(this TSource value, Func<TSource, TReturn> func) where TSource : class
        {
            if (value != null && func != null)
            {
                return func(value);
            }

            return default(TReturn);
        }

        /// <summary>
        /// 尝试释放对象占用的资源。
        /// </summary>
        /// <param name="disobj"></param>
        public static void TryDispose(this object disobj)
        {
            var p = disobj as IDisposable;
            if (p != null)
            {
                p.Dispose();
            }
        }

        /// <summary>
        /// 将对象转换为 T 类型，如果对象不支持转换，则返回 null。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T As<T>(this object obj) where T : class
        {
            return obj as T;
        }

        /// <summary>
        /// 将对象转换为 T 类型，如果对象不支持转换，则返回 null。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="action"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static T As<T>(this object obj, Action<T> action, Action other = null) where T : class
        {
            var item = obj as T;
            if (item != null && action != null)
            {
                action(item);
            }
            else if (other != null)
            {
                other();
            }
            return item;
        }

        /// <summary>
        /// 将对象转换为 T 类型，如果对象不支持转换，则返回 null。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="obj"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static V As<T, V>(this object obj, Func<T, V> func) where T : class
        {
            var item = obj as T;
            if (item != null && func != null)
            {
                return func(item);
            }

            return default(V);
        }

        /// <summary>
        /// 判断对象是否可以转换为 T 类型。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool Is<T>(this object obj)
        {
            return obj is T;
        }

        /// <summary>
        /// 输出对象的字符串表示方法，对象为 null 时仍然返回一个字符串。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToStringSafely(this object value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return value.ToString();
        }

        /// <summary>
        /// 判断值是否在范围内。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">要判断的值。</param>
        /// <param name="lowerBound">下标。</param>
        /// <param name="upperBound">上标。</param>
        /// <param name="comparer"><typeparamref name="T"/> 的比较器。</param>
        /// <param name="includeLowerBound">是否包含下标。</param>
        /// <param name="includeUpperBound">是否包含上标。</param>
        /// <returns></returns>
        public static bool IsBetween<T>(this T value, T lowerBound, T upperBound, IComparer<T> comparer, bool includeLowerBound = false, bool includeUpperBound = false)
        {
            Guard.ArgumentNull(comparer, nameof(comparer));
            var lowerCompareResult = comparer.Compare(value, lowerBound);
            var upperCompareResult = comparer.Compare(value, upperBound);

            return (includeLowerBound && lowerCompareResult == 0) ||
                (includeUpperBound && upperCompareResult == 0) ||
                (lowerCompareResult > 0 && upperCompareResult < 0);
        }

        #region To

        /// <summary>
        /// 将对象转换为指定的类型。
        /// </summary>
        /// <typeparam name="TSource">对象的类型。</typeparam>
        /// <typeparam name="TTarget">要转换的类型。</typeparam>
        /// <param name="source">源对象。</param>
        /// <param name="defaultValue">转换失败后返回的默认值。</param>
        /// <returns></returns>
        public static TTarget To<TSource, TTarget>(this TSource source, TTarget defaultValue = default(TTarget))
        {
            return (TTarget)ToType(source, typeof(TTarget), defaultValue);
        }

        /// <summary>
        /// 将对象转换为指定的类型。
        /// </summary>
        /// <typeparam name="TSource">对象的类型。</typeparam>
        /// <typeparam name="TTarget">要转换的类型。</typeparam>
        /// <param name="source">源对象。</param>
        /// <param name="mapper">转换器。</param>
        /// <returns></returns>
        public static TTarget To<TSource, TTarget>(this TSource source, ConvertMapper<TSource, TTarget> mapper)
        {
            return (TTarget)ToType(source, typeof(TTarget), default(TTarget), mapper);
        }

        /// <summary>
        /// 将对象转换为指定的类型。
        /// </summary>
        /// <typeparam name="TTarget">要转换的对象类型。</typeparam>
        /// <param name="value">源对象。</param>
        /// <param name="defaultValue">转换失败后返回的默认值。</param>
        /// <returns></returns>
        public static TTarget To<TTarget>(this object value, TTarget defaultValue = default(TTarget))
        {
            return (TTarget)value.ToType(typeof(TTarget), defaultValue);
        }

        /// <summary>
        /// 将对象转换为指定的类型。
        /// </summary>
        /// <param name="value">源对象。</param>
        /// <param name="conversionType">要转换的对象类型。</param>
        /// <param name="defaultValue">转换失败后返回的默认值。</param>
        /// <param name="mapper">转换器。</param>
        /// <returns></returns>
        public static object ToType(this object value, Type conversionType, object defaultValue = null, ConvertMapper mapper = null)
        {
            Guard.ArgumentNull(conversionType, nameof(conversionType));
            if (value.IsNullOrEmpty())
            {
                return conversionType.IsNullableType() ? null : (defaultValue ?? conversionType.GetDefaultValue());
            }
            if (value.GetType() == conversionType)
            {
                return value;
            }

            try
            {
                if (conversionType.IsEnum)
                {
                    return Enum.Parse(conversionType, value.ToString(), true);
                }
                if (conversionType == typeof(bool?) && Convert.ToInt32(value) == -1)
                {
                    return null;
                }

                if (conversionType.IsNullableType())
                {
                    return conversionType.New(new[] { value.ToType(conversionType.GetGenericArguments()[0]) });
                }
                if (conversionType == typeof(bool))
                {
                    if (value is string)
                    {
                        var lower = ((string)value).ToLower();
                        return lower == "true" || lower == "t" || lower == "1" || lower == "yes" || lower == "on";
                    }
                    return Convert.ToInt32(value) == 1;
                }
                if (value is bool)
                {
                    if (conversionType == typeof(string))
                    {
                        return Convert.ToBoolean(value) ? "true" : "false";
                    }
                    return Convert.ToBoolean(value) ? 1 : 0;
                }
                if (conversionType == typeof(Type))
                {
                    return Type.GetType(value.ToString(), false, true);
                }
                if (value is Type && conversionType == typeof(string))
                {
                    return ((Type)value).FullName;
                }
                if (typeof(IConvertible).IsAssignableFrom(conversionType))
                {
                    return Convert.ChangeType(value, conversionType, null);
                }

                return value.CloneTo(conversionType, mapper);
            }
            catch (Exception exp)
            {
                return defaultValue;
            }
        }

        #endregion

        /// <summary>
        /// 使用另一个对象对源对象进行扩展，这类似于 jQuery 中的 extend 方法。
        /// </summary>
        /// <param name="source">源对象。</param>
        /// <param name="other">用于扩展的另一个对象。</param>
        /// <returns></returns>
        public static object Extend(this object source, object other)
        {
            if (source == null)
            {
                return other;
            }

            if (other == null)
            {
                return source;
            }

            TypeDescriptorUtility.AddDefaultDynamicProvider();
            var sourceProperties = TypeDescriptor.GetProperties(source);
            var otherProperties = TypeDescriptor.GetProperties(other);
            var expando = new ExpandoObject();
            var dictionary = (IDictionary<string, object>)expando;

            var sourceLazy = source as ILazyManager;
            var otherLazy = other as ILazyManager;
            foreach (PropertyDescriptor p in sourceProperties)
            {
                if (sourceLazy != null && !sourceLazy.IsValueCreated(p.Name))
                {
                    continue;
                }

                dictionary.Add(p.Name, p.GetValue(source));
            }

            foreach (PropertyDescriptor p in otherProperties)
            {
                if (otherLazy != null && !otherLazy.IsValueCreated(p.Name))
                {
                    continue;
                }

                if (!dictionary.ContainsKey(p.Name))
                {
                    dictionary.Add(p.Name, p.GetValue(other));
                }
            }

            return expando;
        }

        /// <summary>
        /// 将 <paramref name="other"/> 的属性复制到 <paramref name="source"/> 中。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static TSource CloneFrom<TSource>(this TSource source, object other)
        {
            if (other == null)
            {
                return source;
            }

            TypeDescriptorUtility.AddDefaultDynamicProvider();

            var sourceProperties = TypeDescriptor.GetProperties(source);
            var otherProperties = TypeDescriptor.GetProperties(other);

            var sourceLazy = source as ILazyManager;
            var otherLazy = other as ILazyManager;
            foreach (PropertyDescriptor p in otherProperties)
            {
                if (otherLazy != null && !otherLazy.IsValueCreated(p.Name))
                {
                    continue;
                }

                var sp = sourceProperties.Find(p.Name, true);
                if (sp != null)
                {
                    var value = p.GetValue(other);
                    if (value != null)
                    {
                        sp.SetValue(source, value.ToType(sp.PropertyType));
                    }
                }
            }

            return source;
        }

        /// <summary>
        /// 使用另一个对象对源对象进行扩展，生成类型 <typeparamref name="TTarget"/> 的对象。
        /// </summary>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="source">源对象。</param>
        /// <param name="other">用于扩展的另一个对象。</param>
        /// <returns></returns>
        public static TTarget ExtendAs<TTarget>(this object source, object other)
        {
            var target = source.To<TTarget>();
            TypeDescriptorUtility.AddDefaultDynamicProvider();
            var sourceProperties = TypeDescriptor.GetProperties(target);
            var otherProperties = TypeDescriptor.GetProperties(other);
            foreach (PropertyDescriptor p in otherProperties)
            {
                var targetProperty = sourceProperties[p.Name];
                if (targetProperty != null && !targetProperty.IsReadOnly &&
                    targetProperty.PropertyType == p.PropertyType)
                {
                    var value = p.GetValue(other);
                    targetProperty.SetValue(target, value);
                }
            }

            return target;
        }

        /// <summary>
        /// 将一个匿名类型的对象转换为类型为 <see cref="ExpandoObject"/> 的动态对象。使用 <see cref="DynamicObjectTypeDescriptionProvider"/> 类型进行元数据补充。
        /// </summary>
        /// <param name="source">一个匿名类型对象。</param>
        /// <returns>如果 <paramref name="source"/> 为非匿名类型对象，则为其自身。</returns>
        public static dynamic ToDynamic(this object source)
        {
            if (source == null || source is ExpandoObject)
            {
                return source;
            }

            var type = source.GetType();
            if (!type.IsAnonymousType())
            {
                return source;
            }

            var expando = new ExpandoObject();
            var dictionary = (IDictionary<string, object>)expando;
            TypeDescriptorUtility.AddDefaultDynamicProvider();

            foreach (PropertyDescriptor pd in TypeDescriptor.GetProperties(source))
            {
                dictionary.Add(pd.Name, pd.GetValue(source));
            }

            return expando;
        }

        /// <summary>
        /// 比较对象与它默认实例具有差异的属性。
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="action"></param>
        public static void Compare(this object obj, Action<PropertyInfo, object> action)
        {
            Guard.ArgumentNull(obj, nameof(obj));
            Guard.ArgumentNull(action, nameof(action));

            var type = obj.GetType();
            var comobj = type.New();

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!property.CanRead || !property.CanWrite)
                {
                    continue;
                }

                var v1 = property.GetValue(obj, null);
                var v2 = property.GetValue(comobj, null);
                if (v1 != null && !v1.Equals(v2))
                {
                    action(property, v1);
                }
            }
        }

        /// <summary>
        /// 将对象序列化为 Json 字符串。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public static string ToJson<T>(this T obj, JsonSerializeOption option = null)
        {
            var serializer = new JsonSerializer(option);
            return serializer.Serialize(obj);
        }

        /// <summary>
        /// 从 Json 字符串。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public static T FromJson<T>(this string json, JsonSerializeOption option = null)
        {
            var serializer = new JsonSerializer(option);
            return serializer.Deserialize<T>(json);
        }

        /// <summary>
        /// 使用指定的属性集合创建新的对象类型。
        /// </summary>
        /// <param name="newTypeName"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        private static Type BuildNewObjectType(string newTypeName, IEnumerable<PropertyInfo> properties)
        {
            var assembly = new DynamicAssemblyBuilder("__DynamicAssembly_ExtendTypes");
            var typeBuilder = assembly.DefineType(newTypeName);
            //构造方法的参数类型集合
            var constructorParTypes = new List<Type>();
            //属性set方法的集合
            var setMethodBuilders = new List<DynamicMethodBuilder>();

            foreach (var pro in properties)
            {
                if (pro.CanRead)
                {
                    var pbuilder = typeBuilder.DefineProperty(pro.Name, pro.PropertyType);

                    //定义get和set方法
                    pbuilder.DefineGetMethod();
                    setMethodBuilders.Add(pbuilder.DefineSetMethod(pro.CanWrite ? VisualDecoration.Public : VisualDecoration.Internal));

                    constructorParTypes.Add(pro.PropertyType);
                }
            }

            //定义一个构造方法，使用两个类型中的所有属性作为方法的参数
            if (constructorParTypes.Count > 0)
            {
                typeBuilder.DefineConstructor(constructorParTypes.ToArray()).AppendCode(e =>
                    {
                        //在构造方法中，对每一个属性进行赋值
                        e.Each(setMethodBuilders, (e1, b, i) =>
                            e1.ldarg_0.ldarg(i + 1).call(b.MethodBuilder)
                            ).ret();
                    });
            }

            return typeBuilder.CreateType();
        }

        /// <summary>
        /// 将对象克隆为指定类型的实例。
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="conversionType"></param>
        /// <returns></returns>
        private static object CloneTo(this object obj, Type conversionType, ConvertMapper mapper)
        {
            Guard.ArgumentNull(obj, nameof(obj));
            var sourceType = obj.GetType();

            if (conversionType.IsAssignableFrom(sourceType))
            {
                return obj;
            }

            //接口或抽象类，生成一个实现类
            if (conversionType.IsInterface || conversionType.IsAbstract)
            {
                conversionType = conversionType.BuildImplementType();
            }

            //如果可枚举
            var enumerable = obj as IEnumerable;
            if (enumerable != null && typeof(IEnumerable).IsAssignableFrom(conversionType))
            {
                if (typeof(IDictionary).IsAssignableFrom(conversionType))
                {
                    return ConvertToDictionary(enumerable, conversionType);
                }
                else
                {
                    return ConvertToEnumerable(enumerable, conversionType);
                }
            }

            var cacheMgr = MemoryCacheManager.Instance;
            var func = cacheMgr.TryGet(sourceType.FullName + "-" + conversionType.FullName, () => BuildCloneToDelegate(obj, conversionType, mapper));
            return func.DynamicInvoke(obj);
        }

        private static Delegate BuildCloneToDelegate(object obj, Type conversionType, ConvertMapper mapper)
        {
            var sourceType = obj.GetType();
            var bindings = new List<MemberBinding>();
            var parExp = Expression.Parameter(sourceType, "s");

            TypeDescriptorUtility.AddDefaultDynamicProvider();
            var @dynamic = obj as IDynamicMetaObjectProvider;
            if (@dynamic != null)
            {
                GetDynamicMemberBindings(@dynamic, conversionType, parExp, bindings, mapper);
            }
            else
            {
                GetGeneralMemberBindings(obj, sourceType, conversionType, parExp, bindings, mapper);
            }

            var expExp = Expression.New(conversionType);
            var mbrInitExp = Expression.MemberInit(expExp, bindings);
            var funcType = typeof(Func<,>).MakeGenericType(sourceType, conversionType);
            var lambda = Expression.Lambda(funcType, mbrInitExp, parExp);
            return lambda.Compile();
        }

        private static void GetDynamicMemberBindings(IDynamicMetaObjectProvider @dynamic, Type conversionType, ParameterExpression parExp, List<MemberBinding> bindings, ConvertMapper mapper)
        {
            var method = typeof(DynamicManager).GetMethod(nameof(DynamicManager.GetMember), BindingFlags.Instance | BindingFlags.Public);
            var metaObject = @dynamic.GetMetaObject(Expression.Constant(@dynamic));
            var metaObjExp = Expression.TypeAs(parExp, typeof(IDynamicMetaObjectProvider));
            foreach (var name in metaObject.GetDynamicMemberNames())
            {
                try
                {
                    var descProperty = conversionType.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
                    if (descProperty == null || !descProperty.CanWrite)
                    {
                        continue;
                    }

                    var mgrExp = Expression.New(typeof(DynamicManager));
                    var exp = (Expression)Expression.Call(mgrExp, method, metaObjExp, Expression.Constant(name));
                    exp = Expression.Call(null, MthToType, exp, Expression.Constant(descProperty.PropertyType), Expression.Constant(null), Expression.Constant(null, typeof(ConvertMapper)));
                    exp = (Expression)Expression.Convert(exp, descProperty.PropertyType);
                    bindings.Add(Expression.Bind(descProperty, exp));
                }
                catch
                {
                    continue;
                }
            }
        }

        private static void GetGeneralMemberBindings(object obj, Type sourceType, Type conversionType, ParameterExpression parExp, List<MemberBinding> bindings, ConvertMapper mapper)
        {
            var lazyMgr = obj as ILazyManager;
            foreach (var property in conversionType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                try
                {
                    if (lazyMgr != null && !lazyMgr.IsValueCreated(property.Name))
                    {
                        continue;
                    }

                    Expression descExp = null;

                    //在映射器里查找转换表达式
                    if (mapper != null)
                    {
                        descExp = mapper.GetMapExpression(property);
                        if (descExp != null)
                        {
                            descExp = (ExpressionReplacer.Replace(descExp, parExp) as LambdaExpression).Body;
                        }
                    }

                    if (descExp == null)
                    {
                        var sourceProperty = sourceType.GetProperty(property.Name, BindingFlags.Public | BindingFlags.Instance);
                        if (sourceProperty == null || !sourceProperty.CanRead || !property.CanWrite)
                        {
                            continue;
                        }

                        descExp = Expression.MakeMemberAccess(parExp, sourceProperty);
                        if (property.PropertyType != sourceProperty.PropertyType)
                        {
                            descExp = Expression.Call(null, MthToType,
                                Expression.Convert(descExp, typeof(object)),
                                Expression.Constant(property.PropertyType),
                                Expression.Constant(null),
                                Expression.Constant(null, typeof(ConvertMapper)));
                            descExp = Expression.Convert(descExp, property.PropertyType);
                        }
                    }

                    bindings.Add(Expression.Bind(property, descExp));
                }
                catch
                {
                    continue;
                }
            }
        }

        private static object ConvertToEnumerable(IEnumerable enumerable, Type conversionType)
        {
            var elementType = conversionType.GetEnumerableElementType();
            var result = typeof(List<>).MakeGenericType(elementType).New<IList>();
            foreach (var item in enumerable)
            {
                result.Add(item.ToType(elementType));
            }

            if (conversionType.IsArray)
            {
                var toArrayMethod = typeof(Enumerable).GetMethod("ToArray").MakeGenericMethod(elementType);
                return toArrayMethod.Invoke(null, new[] { result });
            }

            return result;
        }

        private static object ConvertToDictionary(IEnumerable enumerable, Type conversionType)
        {
            var result = conversionType.New<IDictionary>();
            var argumentTypes = conversionType.GetGenericArguments();

            foreach (var item in enumerable)
            {
                var properties = TypeDescriptor.GetProperties(item);
                var key = properties["Key"].GetValue(item);
                var value = properties["Value"].GetValue(item);

                result.Add(key.ToType(argumentTypes[0]), value.ToType(argumentTypes[1]));
            }

            return result;
        }

        /// <summary>
        /// 读取两个对象的属性值。
        /// </summary>
        /// <typeparam name="TS"></typeparam>
        /// <typeparam name="TO"></typeparam>
        /// <param name="source"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        private static object[] ReadObjectValues<TS, TO>(TS source, TO other, IEnumerable<PropertyInfo> sourceProperties, IEnumerable<PropertyInfo> otherProperties)
        {
            var values = new ArrayList();

            foreach (var property in sourceProperties)
            {
                values.Add(property.GetValue(source, null));
            }

            foreach (var property in otherProperties)
            {
                values.Add(property.GetValue(other, null));
            }

            return values.ToArray();
        }

        private static IEnumerable<string> FilterProperties(IEnumerable<string> properties, object obj)
        {
            var lazyMgr = obj as ILazyManager;
            if (lazyMgr != null)
            {
                return properties.Where(s => lazyMgr.IsValueCreated(s));
            }

            return properties;
        }
    }
}
