// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Data.Schema
{
    /// <summary>
    /// 表示不支持对某类架构信息进行查询的异常。
    /// </summary>
    [Serializable]
    public sealed class SchemaNotSupportedtException : NotSupportedException
    {
        /// <summary>
        /// 初始化 <see cref="SchemaNotSupportedtException"/> 类的新实例。
        /// </summary>
        /// <param name="collectionName">架构类别的名称。</param>
        /// <param name="innerExp">内部异常。</param>
        public SchemaNotSupportedtException(string collectionName, Exception innerExp)
            : base(SR.GetString(SRKind.SchemaNotSupported, collectionName), innerExp)
        {
        }
    }
}
