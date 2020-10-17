// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Fireasy.Common.Serialization
{
    internal class DeserializeBase : DisposableBase
    {
        private readonly SerializeContext _context;
        private readonly SerializeOption _option;

        internal DeserializeBase(SerializeOption option)
        {
            _option = option;
            _context = new SerializeContext { Option = option };
        }

        /// <summary>
        /// 获取指定类型的属性访问缓存。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected Dictionary<string, SerializerPropertyMetadata> GetAccessorMetadataMappers(Type type)
        {
            var cache = _context.GetProperties(type, () => _option.ContractResolver.GetProperties(type));
            return cache.ToDictionary(s => s.PropertyName, StringIgnoreComparer.Default);
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
            foreach (var acc in _context.GetProperties(objType, () => _option.ContractResolver.GetProperties(objType)).Where(s => s.DefaultValue != null))
            {
                acc.Setter(obj, acc.DefaultValue.ToType(acc.PropertyInfo.PropertyType));
            }

            return obj;
        }

        protected bool HasEmptyConstructor(Type type)
        {
            return type.GetConstructor(Type.EmptyTypes) != null;
        }

        protected override bool Dispose(bool disposing)
        {
            _context.Dispose();

            return base.Dispose(disposing);
        }
    }
}
