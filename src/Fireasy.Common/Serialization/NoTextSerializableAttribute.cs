// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace Fireasy.Common.Serialization
{
    /// <summary>
    /// 表示此属性不被序列化。此类无法继承。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class NoTextSerializableAttribute : Attribute
    {
    }
}
