// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// 提供自定义函数的绑定特性。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class FunctionBindAttribute : Attribute
    {
        /// <summary>
        /// 初始化 <see cref="FunctionBindAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="funcName">自定义函数名称。</param>
        public FunctionBindAttribute(string funcName)
        {
            FunctionName = funcName;
        }

        /// <summary>
        /// 获取或设置自定义函数名称。
        /// </summary>
        public string FunctionName { get; set; }
    }
}
