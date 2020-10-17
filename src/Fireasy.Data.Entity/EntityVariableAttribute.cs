// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 一个标识实体变量的特性。无法继承此类。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class EntityVariableAttribute : Attribute
    {
        /// <summary>
        /// 初始化 <see cref="EntityVariableAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="expression"></param>
        public EntityVariableAttribute(string expression)
        {
            Expression = expression;
        }

        /// <summary>
        /// 获取或设置具有变量的表的名称表达式。格式如 TABLE_&lt;VarName1&gt;_&lt;VarName2&gt;。
        /// </summary>
        public string Expression { get; set; }
    }
}
