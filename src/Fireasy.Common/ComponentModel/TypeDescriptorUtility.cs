// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Dynamic;
using System.ComponentModel;
using System.Dynamic;

namespace Fireasy.Common.ComponentModel
{
    public sealed class TypeDescriptorUtility
    {
        /// <summary>
        /// 添加一个动态类型的自定义类型说明。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void AddDynamicProvider<T>() where T : IDynamicMetaObjectProvider
        {
            TypeDescriptor.AddProvider(new DynamicObjectTypeDescriptionProvider(), typeof(T));
        }

        /// <summary>
        /// 添加默认的动态类型的自定义类型说明。
        /// </summary>
        public static void AddDefaultDynamicProvider()
        {
            AddDynamicProvider<DynamicObject>();
            AddDynamicProvider<ExpandoObject>();
            AddDynamicProvider<DynamicExpandoObject>();
        }
    }
}
