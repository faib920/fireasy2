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
    /// 对于已经持久化的实体对象，在修改其主要属性时将引发此异常。无法继承此类。
    /// </summary>
    public sealed class PrimaryPropertyUpdateException : Exception
    {
        /// <summary>
        /// 初始化 <see cref="PrimaryPropertyUpdateException"/> 类的新实例。
        /// </summary>
        /// <param name="property">指定引发修改异常的主要属性。</param>
        public PrimaryPropertyUpdateException(IProperty property)
            : base(SR.GetString(SRKind.DisUpdatePrimaryProperty))
        {
            Property = property;
        }

        /// <summary>
        /// 获取引发修改异常的主要属性。
        /// </summary>
        public IProperty Property { get; }
    }
}
