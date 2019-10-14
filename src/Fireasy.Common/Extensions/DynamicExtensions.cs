// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Dynamic;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;

namespace Fireasy.Common.Extensions
{
    /// <summary>
    /// 动态类型的扩展。
    /// </summary>
    public static class DynamicExtensions
    {
        private static object ErrorResult = new object();

        /// <summary>
        /// 获取动态对象中成员名称的枚举。
        /// </summary>
        /// <param name="dynamicProvider">一个动态对象。</param>
        /// <returns></returns>
        public static IEnumerable<string> GetDynamicMemberNames(this IDynamicMetaObjectProvider dynamicProvider)
        {
            var metaObject = dynamicProvider.GetMetaObject(Expression.Constant(dynamicProvider));
            return metaObject.GetDynamicMemberNames();
        }

        /// <summary>
        /// 尝试获取动态对象中指定名称的属性值。
        /// </summary>
        /// <param name="dynamicProvider">一个动态对象。</param>
        /// <param name="name">属性的名称。</param>
        /// <param name="value">返回值。</param>
        /// <returns></returns>
        public static bool TryGetMember(this IDynamicMetaObjectProvider dynamicProvider, string name, out object value)
        {
            if (dynamicProvider is IDictionary<string, object> dict)
            {
                return dict.TryGetValue(name, out value);
            }

            return new DynamicManager().TryGetMember(dynamicProvider, name, out value);
        }

        /// <summary>
        /// 尝试设置动态对象中指定名称的属性值。
        /// </summary>
        /// <param name="dynamicProvider">一个动态对象。</param>
        /// <param name="name">属性的名称。</param>
        /// <param name="value">设置值。</param>
        /// <returns></returns>
        public static bool TrySetMember(this IDynamicMetaObjectProvider dynamicProvider, string name, object value)
        {
            if (dynamicProvider is IDictionary<string, object> dict)
            {
                dict.AddOrReplace(name, value);
                return true;
            }

            return new DynamicManager().TrySetMember(dynamicProvider, name, value);
        }
    }
}
