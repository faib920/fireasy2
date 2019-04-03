// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Common.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Fireasy.Common.Serialization
{
    internal class DeserializeBase : IDisposable
    {
        private bool isDisposed;
        private SerializeContext context;
        private SerializeOption option;

        internal DeserializeBase(SerializeOption option)
        {
            this.option = option;
            context = new SerializeContext { Option = option };
        }

        /// <summary>
        /// 获取指定类型的属性访问缓存。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected Dictionary<string, PropertyAccessor> GetAccessorCache(Type type)
        {
            return context.SetAccessors.TryGetValue(type, () =>
                {
                    return type.GetProperties()
                        .Where(s => s.CanWrite && !SerializerUtil.IsNoSerializable(option, s))
                        .Select(s =>
                            {
                                var ele = s.GetCustomAttributes<TextSerializeElementAttribute>().FirstOrDefault();
                                //如果使用Camel命名，则名称第一位小写
                                var name = ele != null ?
                                    ele.Name : (option.CamelNaming ?
                                        char.ToLower(s.Name[0]) + s.Name.Substring(1) : s.Name);

                                return new { name, p = s };
                            })
                        .ToDictionary(s => s.name, s => ReflectionCache.GetAccessor(s.p));
                });
        }

        /// <summary>
        /// 创建一个 <see cref="IList"/> 对象。
        /// </summary>
        /// <param name="listType"></param>
        /// <param name="elementType"></param>
        /// <param name="container"></param>
        protected void CreateListContainer(Type listType, out Type elementType, out IList container)
        {
            if (listType.IsArray)
            {
                elementType = listType.GetElementType();
                container = typeof(List<>).MakeGenericType(elementType).New<IList>();
            }
            else if (listType.IsInterface && !listType.IsGenericType)
            {
                elementType = null;
                container = new ArrayList();
            }
            else if (listType.IsInterface && listType.IsGenericType)
            {
                elementType = listType.GetEnumerableElementType();
                container = (IList)typeof(List<>).MakeGenericType(elementType).New();
            }
            else
            {
                elementType = listType.GetGenericImplementType(typeof(IList<>)).GetGenericArguments()[0];
                container = listType.New<IList>();
            }
        }

        /// <summary>
        /// 创建一个 <see cref="IDictionary"/> 对象。
        /// </summary>
        /// <param name="dictType"></param>
        /// <param name="keyValueTypes"></param>
        /// <param name="container"></param>
        protected void CreateDictionaryContainer(Type dictType, out Type[] keyValueTypes, out IDictionary container)
        {
            if (dictType.IsInterface)
            {
                keyValueTypes = dictType.GetGenericArguments();
                container = typeof(Dictionary<,>).MakeGenericType(keyValueTypes).New<IDictionary>();
            }
            else
            {
                keyValueTypes = dictType.GetGenericImplementType(typeof(IDictionary<,>)).GetGenericArguments();
                container = dictType.New<IDictionary>();
            }
        }

        /// <summary>
        /// 使用标注的缺省值创建一个新对象。
        /// </summary>
        /// <param name="objType"></param>
        /// <returns></returns>
        protected object CreateGeneralObject(Type objType)
        {
            var obj = objType.New();
            foreach (var acc in context.GetAccessorCache(objType).Where(s => s.DefaultValue != null))
            {
                acc.Accessor.SetValue(obj, acc.DefaultValue.ToType(acc.PropertyInfo.PropertyType));
            }

            return obj;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed)
            {
                return;
            }

            if (disposing)
            {
                context.Dispose();
            }

            isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
