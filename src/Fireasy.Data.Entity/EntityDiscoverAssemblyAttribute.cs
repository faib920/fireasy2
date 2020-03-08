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
    /// 标识程序集能够发现实体定义。
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class EntityDiscoverAssemblyAttribute : Attribute
    {
    }
}
