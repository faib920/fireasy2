// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Fireasy.Common.Extensions
{
    public static class ComponentExtension
    {
        /// <summary>
        /// 判定是否定义了自定义特性。
        /// </summary>
        /// <typeparam name="T">自定义特性类型。</typeparam>
        /// <param name="member">要搜索的成员定义。</param>
        /// <returns></returns>
        public static bool IsDefined<T>(this MemberDescriptor member) where T : Attribute
        {
            return member.Attributes[typeof(T)] != null;
        }

        /// <summary>
        /// 获取自定义特性组。
        /// </summary>
        /// <typeparam name="T">自定义特性类型。</typeparam>
        /// <param name="member">要搜索的成员定义。</param>
        /// <returns></returns>
        public static IEnumerable<T> GetCustomAttributes<T>(this MemberDescriptor member) where T : Attribute
        {
            yield return (T)member.Attributes[typeof(T)];
        }
    }
}
