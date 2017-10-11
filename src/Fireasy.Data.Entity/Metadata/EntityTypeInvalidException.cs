// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Data.Entity.Metadata
{
    /// <summary>
    /// 指定的类型不属于实体类型时引发此异常，即类型未派生自 <see cref="IEntity"/> 接口。无法继承此类。
    /// </summary>
    public sealed class EntityTypeInvalidException : Exception
    {
    }
}
