// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Common.Ioc
{
    /// <summary>
    /// 表示在解析实例时，该属性将被忽略，容器不会解析此属性类型的对象来设置属性值。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class IgnoreInjectPropertyAttribute : Attribute
    {
    }
}
