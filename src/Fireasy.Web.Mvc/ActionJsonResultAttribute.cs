// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Web.Mvc
{
    /// <summary>
    /// 用于为 action 标记其 json 的返回类型。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ActionJsonResultAttribute : Attribute
    {
        /// <summary>
        /// 初始化 <see cref="ActionJsonResultAttribute"/> 类的新实例。
        /// </summary>
        public ActionJsonResultAttribute()
        {
        }

        /// <summary>
        /// 初始化 <see cref="ActionJsonResultAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="resultType">返回的数据类型。</param>
        public ActionJsonResultAttribute(Type resultType)
        {
            ResultType = resultType;
        }

        /// <summary>
        /// 获取或设置返回的数据类型。
        /// </summary>
        public Type ResultType { get; set; }
    }
}
